#region

using System.Text.Json;
using Obs.NET.Models;
using Obs.NET.OperationDataContainers;

#endregion

namespace Obs.NET.Tests.Core.OperationDataContainers;

public class HelloContainerTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    [Test]
    public async Task HelloContainer_DeserializesCorrectly_WithAuthentication()
    {
        // Arrange - Example JSON Hello message from OBS WebSocket (as per protocol documentation)
        var helloJson = """
                        {
                            "obsStudioVersion": "30.2.2",
                            "obsWebSocketVersion": "5.5.2",
                            "rpcVersion": 1,
                            "authentication": {
                                "challenge": "+IxH4CnCiqpX1rM9scsNynZzbOe4KhDeYcTNS3PDaeY=",
                                "salt": "lM1GncleQOaCu9lT1yeUZhFYnqhsLLP1G5lAGo3ixaI="
                            }
                        }
                        """;

        // Act
        var helloContainer = JsonSerializer.Deserialize<HelloContainer>(helloJson, _jsonOptions);

        // Assert
        await Assert.That(helloContainer).IsNotNull();
        await Assert.That(helloContainer!.ObsStudioVersion).IsEqualTo("30.2.2");
        await Assert.That(helloContainer.ObsWebSocketVersion).IsEqualTo("5.5.2");
        await Assert.That(helloContainer.RpcVersion).IsEqualTo(1);
        await Assert.That(helloContainer.Authentication).IsNotNull();
        await Assert.That(helloContainer.Authentication!.Challenge)
            .IsEqualTo("+IxH4CnCiqpX1rM9scsNynZzbOe4KhDeYcTNS3PDaeY=");
        await Assert.That(helloContainer.Authentication.Salt).IsEqualTo("lM1GncleQOaCu9lT1yeUZhFYnqhsLLP1G5lAGo3ixaI=");
    }

    [Test]
    public async Task HelloContainer_DeserializesCorrectly_WithoutAuthentication()
    {
        // Arrange - Example JSON Hello message without authentication
        var helloJson = """
                        {
                            "obsStudioVersion": "30.2.2",
                            "obsWebSocketVersion": "5.5.2",
                            "rpcVersion": 1
                        }
                        """;

        // Act
        var helloContainer = JsonSerializer.Deserialize<HelloContainer>(helloJson, _jsonOptions);

        // Assert
        await Assert.That(helloContainer).IsNotNull();
        await Assert.That(helloContainer!.ObsStudioVersion).IsEqualTo("30.2.2");
        await Assert.That(helloContainer.ObsWebSocketVersion).IsEqualTo("5.5.2");
        await Assert.That(helloContainer.RpcVersion).IsEqualTo(1);
        await Assert.That(helloContainer.Authentication).IsNull();
    }

    [Test]
    public async Task HelloContainer_HasCorrectOpCode()
    {
        // Act & Assert
        await Assert.That(HelloContainer.Op).IsEqualTo(0);
    }

    [Test]
    public async Task HelloContainer_SerializesCorrectly_WithAuthentication()
    {
        // Arrange
        HelloContainer helloContainer = new()
        {
            ObsStudioVersion = "30.2.2",
            ObsWebSocketVersion = "5.5.2",
            RpcVersion = 1,
            Authentication = new HelloAuthenticationBody
            {
                Challenge = "+IxH4CnCiqpX1rM9scsNynZzbOe4KhDeYcTNS3PDaeY=",
                Salt = "lM1GncleQOaCu9lT1yeUZhFYnqhsLLP1G5lAGo3ixaI="
            }
        };

        // Act - Serialize and deserialize to verify round-trip correctness
        var json = JsonSerializer.Serialize(helloContainer, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<HelloContainer>(json, _jsonOptions);

        // Assert - Verify the JSON contains expected keys (values may be escaped)
        await Assert.That(json).Contains("\"obsStudioVersion\":");
        await Assert.That(json).Contains("\"obsWebSocketVersion\":");
        await Assert.That(json).Contains("\"rpcVersion\":");
        await Assert.That(json).Contains("\"authentication\":");
        await Assert.That(json).Contains("\"challenge\":");
        await Assert.That(json).Contains("\"salt\":");

        // Assert - Verify round-trip preserves values
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.ObsStudioVersion).IsEqualTo("30.2.2");
        await Assert.That(deserialized.ObsWebSocketVersion).IsEqualTo("5.5.2");
        await Assert.That(deserialized.RpcVersion).IsEqualTo(1);
        await Assert.That(deserialized.Authentication).IsNotNull();
        await Assert.That(deserialized.Authentication!.Challenge)
            .IsEqualTo("+IxH4CnCiqpX1rM9scsNynZzbOe4KhDeYcTNS3PDaeY=");
        await Assert.That(deserialized.Authentication.Salt)
            .IsEqualTo("lM1GncleQOaCu9lT1yeUZhFYnqhsLLP1G5lAGo3ixaI=");
    }

    [Test]
    public async Task HelloContainer_SerializesCorrectly_WithoutAuthentication()
    {
        // Arrange
        HelloContainer helloContainer = new()
        {
            ObsStudioVersion = "30.2.2",
            ObsWebSocketVersion = "5.5.2",
            RpcVersion = 1,
            Authentication = null
        };

        // Act
        var json = JsonSerializer.Serialize(helloContainer, _jsonOptions);

        // Assert
        await Assert.That(json).Contains("\"obsStudioVersion\": \"30.2.2\"");
        await Assert.That(json).Contains("\"obsWebSocketVersion\": \"5.5.2\"");
        await Assert.That(json).Contains("\"rpcVersion\": 1");
        // Should not contain authentication when null
        await Assert.That(json).DoesNotContain("\"authentication\":");
    }
}