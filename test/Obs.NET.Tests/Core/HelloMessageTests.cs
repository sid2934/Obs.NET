#region

using Obs.NET.Models;
using Obs.NET.OperationDataContainers;

#endregion

namespace Obs.NET.Tests.Core;

public class HelloMessageTests
{
    [Test]
    public async Task HelloMessage_CreatesCorrectly_FromHelloContainer_WithAuthentication()
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

        // Act
        ObsWsOpMessage<HelloContainer> helloMessage = new() { D = helloContainer };

        // Assert
        await Assert.That(helloMessage.D.ObsStudioVersion).IsEqualTo("30.2.2");
        await Assert.That(helloMessage.D.ObsWebSocketVersion).IsEqualTo("5.5.2");
        await Assert.That(helloMessage.D.RpcVersion).IsEqualTo(1);
        await Assert.That(helloMessage.D.Authentication).IsNotNull();
        await Assert.That(helloMessage.D.Authentication!.Challenge)
            .IsEqualTo("+IxH4CnCiqpX1rM9scsNynZzbOe4KhDeYcTNS3PDaeY=");
        await Assert.That(helloMessage.D.Authentication.Salt).IsEqualTo("lM1GncleQOaCu9lT1yeUZhFYnqhsLLP1G5lAGo3ixaI=");
    }

    [Test]
    public async Task HelloMessage_CreatesCorrectly_FromHelloContainer_WithoutAuthentication()
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
        ObsWsOpMessage<HelloContainer> helloMessage = new() { D = helloContainer };

        // Assert
        await Assert.That(helloMessage.D.ObsStudioVersion).IsEqualTo("30.2.2");
        await Assert.That(helloMessage.D.ObsWebSocketVersion).IsEqualTo("5.5.2");
        await Assert.That(helloMessage.D.RpcVersion).IsEqualTo(1);
        await Assert.That(helloMessage.D.Authentication).IsNull();
    }

    [Test]
    public async Task HelloMessage_DefaultConstructor_InitializesCorrectly()
    {
        // Act
        ObsWsOpMessage<HelloContainer> helloMessage = new()
        {
            D = new HelloContainer
            {
                ObsStudioVersion = string.Empty,
                ObsWebSocketVersion = string.Empty,
                RpcVersion = 0
            }
        };

        // Assert
        await Assert.That(helloMessage.D.ObsStudioVersion).IsEqualTo(string.Empty);
        await Assert.That(helloMessage.D.ObsWebSocketVersion).IsEqualTo(string.Empty);
        await Assert.That(helloMessage.D.RpcVersion).IsEqualTo(0);
        await Assert.That(helloMessage.D.Authentication).IsNull();
    }

    [Test]
    public async Task HelloMessage_Properties_CanBeSetDirectly()
    {
        // Act
        ObsWsOpMessage<HelloContainer> helloMessage = new()
        {
            D = new HelloContainer
            {
                ObsStudioVersion = "29.1.3",
                ObsWebSocketVersion = "5.4.2",
                RpcVersion = 1,
                Authentication = new HelloAuthenticationBody
                {
                    Challenge = "test-challenge",
                    Salt = "test-salt"
                }
            }
        };

        // Assert
        await Assert.That(helloMessage.D.ObsStudioVersion).IsEqualTo("29.1.3");
        await Assert.That(helloMessage.D.ObsWebSocketVersion).IsEqualTo("5.4.2");
        await Assert.That(helloMessage.D.RpcVersion).IsEqualTo(1);
        await Assert.That(helloMessage.D.Authentication).IsNotNull();
        await Assert.That(helloMessage.D.Authentication!.Challenge).IsEqualTo("test-challenge");
        await Assert.That(helloMessage.D.Authentication.Salt).IsEqualTo("test-salt");
    }
}