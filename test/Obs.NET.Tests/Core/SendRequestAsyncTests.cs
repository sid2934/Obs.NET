#region

using System.Text.Json;
using Obs.NET.Requests;

#endregion

namespace Obs.NET.Tests.Core;

public class SendRequestAsyncTests
{
    [Test]
    public async Task SendRequestAsync_GenericConstraints_CompileCorrectly()
    {
        // This test verifies that the generic constraints work correctly at compile time
        // TRequest : IObsWsRequestData
        // TResponse : IObsWsRequestData

        // Arrange - These should compile without issues
        GetProfileParameterRequest request = new()
        {
            ParameterCategory = "Video",
            ParameterName = "BaseCX"
        };

        // Act & Assert - This should compile, demonstrating the constraints work
        await Assert.That(GetProfileParameterRequest.RequestType).IsEqualTo("GetProfileParameter");
        await Assert.That(GetProfileParameterResponse.RequestType).IsEqualTo("GetProfileParameter");
        await Assert.That(request).IsNotNull();
        await Assert.That(request.ParameterCategory).IsEqualTo("Video");
        await Assert.That(request.ParameterName).IsEqualTo("BaseCX");
    }

    [Test]
    public async Task RequestResponseTypes_ImplementIObsWsRequestData()
    {
        // Verify that our request/response types correctly implement the interface
        await Assert.That(typeof(GetProfileParameterRequest).IsAssignableTo(typeof(IObsWsRequestData))).IsTrue();
        await Assert.That(typeof(GetProfileParameterResponse).IsAssignableTo(typeof(IObsWsRequestData))).IsTrue();
    }

    [Test]
    public async Task JsonDeserialization_WorksForResponseTypes()
    {
        // Arrange - Simulate a response JSON from OBS WebSocket
        var responseJson = """
                           {
                               "parameterValue": "1920",
                               "defaultParameterValue": "1920"
                           }
                           """;

        // Act
        var response = JsonSerializer.Deserialize<GetProfileParameterResponse>(responseJson);

        // Assert
        await Assert.That(response).IsNotNull();
        await Assert.That(response!.ParameterValue).IsEqualTo("1920");
        await Assert.That(response.DefaultParameterValue).IsEqualTo("1920");
    }

    [Test]
    public async Task JsonSerialization_WorksForRequestTypes()
    {
        // Arrange
        GetProfileParameterRequest request = new()
        {
            ParameterCategory = "Video",
            ParameterName = "BaseCX"
        };

        // Act
        var json = JsonSerializer.Serialize(request);

        // Assert
        await Assert.That(json).Contains("\"parameterCategory\":");
        await Assert.That(json).Contains("\"Video\"");
        await Assert.That(json).Contains("\"parameterName\":");
        await Assert.That(json).Contains("\"BaseCX\"");
    }

    [Test]
    public async Task RequestTypes_HaveCorrectStaticRequestType()
    {
        // This verifies that the static RequestType property is consistent
        // between request and response types (required for type discriminator pattern)

        await Assert.That(GetProfileParameterRequest.RequestType).IsEqualTo("GetProfileParameter");
        await Assert.That(GetProfileParameterResponse.RequestType).IsEqualTo("GetProfileParameter");

        // Both request and response should have the same RequestType for proper routing
        await Assert.That(GetProfileParameterRequest.RequestType).IsEqualTo(GetProfileParameterResponse.RequestType);
    }
}