#region

using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Obs.NET.Enums;
using Obs.NET.OperationDataContainers;
using Obs.NET.Requests;

#endregion

namespace Obs.NET;

/// <summary>
///     Modern async/await OBS WebSocket client for OBS Studio WebSocket API v5
/// </summary>
public partial class ObsWsClient(ObsWsClientOptions? options = null) : IAsyncDisposable
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);
    private readonly CancellationTokenSource _lifetimeCts = new();

    private readonly ObsWsClientOptions _options = options ?? new ObsWsClientOptions();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonElement>> _pendingRequests = new();
    private TaskCompletionSource<ObsWsOpMessage<HelloContainer>>? _helloMessageTcs = new();
    private Task? _receiveTask;


    private ClientWebSocket? _webSocket;

    public ConnectionState State { get; private set; } = ConnectionState.Disconnected;

    public async ValueTask DisposeAsync()
    {
        try
        {
            // Attempt clean disconnect first
            await DisconnectAsync().ConfigureAwait(false);
        }
        catch
        {
            // If disconnect fails, force cleanup
        }

        await _lifetimeCts.CancelAsync();

        if (_receiveTask != null)
            try
            {
                await _receiveTask.ConfigureAwait(false);
            }
            catch
            {
                // Ignore exceptions during cleanup
            }

        _webSocket?.Dispose();
        _connectionSemaphore.Dispose();
        _lifetimeCts.Dispose();
    }

    // public event Func<ObsEvent, Task>? EventReceived;
    public event Func<ConnectionState, Task>? ConnectionStateChanged;

    /// <summary>
    ///     Connect to OBS WebSocket server
    /// </summary>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        await ConnectWithRetryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Connect to OBS WebSocket server with retry logic
    /// </summary>
    private async Task ConnectWithRetryAsync(CancellationToken cancellationToken = default)
    {
        var attempts = 0;
        Exception? lastException = null;

        while (attempts < _options.MaxReconnectAttempts)
            try
            {
                _helloMessageTcs = new TaskCompletionSource<ObsWsOpMessage<HelloContainer>>();
                await ConnectOnceAsync(cancellationToken).ConfigureAwait(false);
                return; // Success
            }
            catch (Exception ex)
            {
                lastException = ex;
                attempts++;

                if (attempts >= _options.MaxReconnectAttempts) break;

                // Wait before retry
                await Task.Delay(_options.ReconnectDelay, cancellationToken).ConfigureAwait(false);
            }

        throw new InvalidOperationException($"Failed to connect after {_options.MaxReconnectAttempts} attempts",
            lastException);
    }

    private async Task ConnectOnceAsync(CancellationToken cancellationToken)
    {
        using var combinedCts =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _lifetimeCts.Token);

        await _connectionSemaphore.WaitAsync(combinedCts.Token).ConfigureAwait(false);
        try
        {
            if (State != ConnectionState.Disconnected) return;

            await SetConnectionStateAsync(ConnectionState.Connecting).ConfigureAwait(false);

            _webSocket = new ClientWebSocket();
            Uri uri = new($"ws://{_options.Host}:{_options.Port}");

            await _webSocket.ConnectAsync(uri, combinedCts.Token).ConfigureAwait(false);
            await SetConnectionStateAsync(ConnectionState.Connected).ConfigureAwait(false);

            // Start receive loop
            _receiveTask = ReceiveLoopAsync(_lifetimeCts.Token);

            // Perform identification and authentication
            await IdentifyAsync(combinedCts.Token).ConfigureAwait(false);
            await SetConnectionStateAsync(ConnectionState.Authenticated).ConfigureAwait(false);
        }
        catch
        {
            await SetConnectionStateAsync(ConnectionState.Disconnected).ConfigureAwait(false);
            _webSocket?.Dispose();
            _webSocket = null;
            throw;
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    /// <summary>
    ///     Disconnect from OBS WebSocket server
    /// </summary>
    private async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await _connectionSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (State == ConnectionState.Disconnected) return;

            await SetConnectionStateAsync(ConnectionState.Disconnected).ConfigureAwait(false);

            if (_webSocket?.State == WebSocketState.Open)
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnect", cancellationToken)
                    .ConfigureAwait(false);

            _webSocket?.Dispose();
            _webSocket = null;

            if (_receiveTask != null)
            {
                try
                {
                    await _receiveTask.ConfigureAwait(false);
                }
                catch
                {
                    // Ignore exceptions during cleanup
                }

                _receiveTask = null;
            }
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    /// <summary>
    ///     Send a request to OBS and wait for response
    /// </summary>
    public async Task<TResponse?> SendRequestAsync<TRequest, TResponse>(TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IObsWsRequestData
        where TResponse : IObsWsRequestData
    {
        if (State != ConnectionState.Authenticated)
            throw new InvalidOperationException("Client must be connected and authenticated");

        using var timeoutCts =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _lifetimeCts.Token);
        timeoutCts.CancelAfter(_options.RequestTimeout);

        var requestId = Guid.NewGuid().ToString();
        TaskCompletionSource<JsonElement> tcs = new();

        _pendingRequests[requestId] = tcs;

        try
        {
            // From the request produce the full request object to be serialized
            ObsWsOpMessage<RequestContainer<TRequest>> opMessage = new()
            {
                D = new RequestContainer<TRequest> { RequestId = requestId, RequestData = request }
            };

            await SendMessageAsync(opMessage, timeoutCts.Token).ConfigureAwait(false);
            var responseJsonElement = await tcs.Task.ConfigureAwait(false);

            var fullResponseObject =
                responseJsonElement.Deserialize<ObsWsOpMessage<RequestResponseContainer<TResponse>>>(
                    _jsonSerializerOptions)
                ?? throw new ObsRequestException("Failed to deserialize response data");

            // Check if the request was successful
            var status = fullResponseObject.D.RequestStatus;
            if (!status.Result)
            {
                throw new ObsRequestException(
                    $"OBS request '{fullResponseObject.D.RequestType}' failed with code {status.Code}: {status.Comment ?? "No details provided"}",
                    status.Code,
                    status.Comment);
            }

            return fullResponseObject.D.ResponseData;
        }
        finally
        {
            _pendingRequests.TryRemove(requestId, out _);
        }
    }

    private async Task IdentifyAsync(CancellationToken cancellationToken)
    {
        // Get server info first
        var helloResponse =
            await WaitForHelloMessageAsync(cancellationToken).ConfigureAwait(false);
        var helloResponseContainer = helloResponse.D;

        // Handle authentication if required
        string? authenticationString = null;
        if (helloResponseContainer.Authentication is not null)
        {
            var challenge = helloResponseContainer.Authentication.Challenge;
            var salt = helloResponseContainer.Authentication.Salt;

            authenticationString =
                ComputeAuthResponse(ComputeAuthSecret(_options.Password ?? string.Empty, salt), challenge);
        }

        ObsWsOpMessage<IdentifyContainer> identifyMessage = new()
        {
            D = new IdentifyContainer
            {
                Authentication = authenticationString,
                EventSubscriptions = _options.EventSubscriptions,
                RpcVersion = 1
            }
        };

        await SendMessageAsync(identifyMessage, cancellationToken).ConfigureAwait(false);
    }

    private async Task<ObsWsOpMessage<HelloContainer>> WaitForHelloMessageAsync(CancellationToken cancellationToken)
    {
        if (_helloMessageTcs is null)
            throw new InvalidOperationException("Hello message task completion source was not initialized");

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(10)); // 10 second timeout for Hello message

        var tcs = _helloMessageTcs;
        try
        {
            cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            return await tcs.Task.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested &&
                                                 !cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("Timed out waiting for Hello message from OBS WebSocket server");
        }
    }

    private static string ComputeAuthSecret(string password, string salt)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var saltBytes = Encoding.UTF8.GetBytes(salt);
        var combined = passwordBytes.Concat(saltBytes).ToArray();
        var hash = SHA256.HashData(combined);
        return Convert.ToBase64String(hash);
    }

    private static string ComputeAuthResponse(string secret, string challenge)
    {
        var secretBytes = Convert.FromBase64String(secret);
        var challengeBytes = Encoding.UTF8.GetBytes(challenge);
        var combined = secretBytes.Concat(challengeBytes).ToArray();
        var hash = SHA256.HashData(combined);
        return Convert.ToBase64String(hash);
    }

    private async Task SendMessageAsync(object message, CancellationToken cancellationToken)
    {
        if (_webSocket?.State != WebSocketState.Open) throw new InvalidOperationException("WebSocket is not open");

        var json = JsonSerializer.Serialize(message, _jsonSerializerOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        await _webSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];
        List<byte> messageBuffer = new();

        try
        {
            while (!cancellationToken.IsCancellationRequested && _webSocket?.State == WebSocketState.Open)
            {
                var result = await _webSocket
                    .ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken).ConfigureAwait(false);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    messageBuffer.AddRange(buffer.Take(result.Count));

                    if (result.EndOfMessage)
                    {
                        var json = Encoding.UTF8.GetString(messageBuffer.ToArray());
                        messageBuffer.Clear();
                        await ProcessMessageAsync(json, cancellationToken).ConfigureAwait(false);
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Expected cancellation
        }
        catch (Exception)
        {
            await SetConnectionStateAsync(ConnectionState.Disconnected).ConfigureAwait(false);
        }
    }

    private async Task ProcessMessageAsync(string json, CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Deserialize<JsonElement>(json, _jsonSerializerOptions);


        if (!(message.TryGetProperty("op", out var opProperty) && opProperty.TryGetInt32(out var opCode)))
            throw new ObsRequestException("Invalid response format was missing 'op' property");

        switch (opCode)
        {
            case 0: // Hello
                ProcessHelloMessage(message);
                break;

            case 2: // Identified
                // Connection is ready
                // ToDo: Set connection state?
                break;

            case 5: // Event
                await ProcessEventAsync(message, cancellationToken).ConfigureAwait(false);
                break;

            case 7: // RequestResponse

                // Get d.requestType and d.requestId to avoid deserializing the entire message again
                var messageData = message.GetProperty("d");

                var requestId = messageData.GetProperty("requestId").GetString() ??
                                throw new ObsRequestException(
                                    "Request Response body missing requestId property");

                ProcessRequestResponse(message, requestId);
                break;
        }
    }

    private async Task ProcessEventAsync(JsonElement rawResponseJsonElement, CancellationToken cancellationToken)
    {
        try
        {
            // Extract event type and event data from the message
            var messageData = rawResponseJsonElement.GetProperty("d");
            var eventType = messageData.GetProperty("eventType").GetString();

            if (string.IsNullOrEmpty(eventType)) return;

            var eventData = messageData.GetProperty("eventData");

            // Dispatch to strongly-typed event handlers
            await DispatchEventAsync(eventType, eventData).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Don't let event handler exceptions crash the reception loop
            // Consider logging here in the future
        }
    }


    private void ProcessHelloMessage(JsonElement rawResponseJsonElement)
    {
        try
        {
            // Deserialize the Hello message
            var helloData =
                rawResponseJsonElement.Deserialize<ObsWsOpMessage<HelloContainer>>(_jsonSerializerOptions)
                ?? throw new ArgumentException();

            _helloMessageTcs?.TrySetResult(helloData);
        }
        catch (Exception ex)
        {
            _helloMessageTcs?.TrySetException(ex);
        }
    }

    private void ProcessRequestResponse(JsonElement rawResponseJsonElement, string requestId)
    {
        if (_pendingRequests.TryRemove(
                requestId ??
                throw new ObsRequestException("The Response's requestId did not have a matching outgoing request."),
                out var tcs))
            tcs.SetResult(rawResponseJsonElement);
    }

    /// <summary>
    ///     Set the connection state and raise event if changed
    /// </summary>
    /// <param name="newState">The new value to set _connectionState to.</param>
    private async Task SetConnectionStateAsync(ConnectionState newState)
    {
        if (State == newState) return;

        State = newState;

        if (ConnectionStateChanged != null) await ConnectionStateChanged(newState).ConfigureAwait(false);
    }
}