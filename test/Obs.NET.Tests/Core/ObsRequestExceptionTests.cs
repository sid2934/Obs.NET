#region

using System;

#endregion

namespace Obs.NET.Tests.Core;

/// <summary>
/// Tests for the ObsRequestException class.
///
/// ObsRequestException is thrown when an OBS WebSocket request fails. This can happen when:
/// 1. The requested resource doesn't exist (e.g., scene not found)
/// 2. The operation is invalid (e.g., trying to delete an active scene)
/// 3. The client doesn't have permission for the operation
/// 4. Internal OBS errors occur
///
/// These tests verify that the exception properly preserves error messages and inner exceptions,
/// which is critical for debugging failed requests in user applications.
/// </summary>
public class ObsRequestExceptionTests
{
    /// <summary>
    /// Tests that ObsRequestException preserves the error message.
    ///
    /// The error message typically contains details from OBS about why the request
    /// failed, such as "Scene 'NonExistent' not found" or "Cannot remove active output".
    /// This message must be preserved for debugging purposes.
    /// </summary>
    [Test]
    public async Task Constructor_WithMessage_PreservesMessage()
    {
        // Arrange
        const string errorMessage = "Scene 'NonExistent' not found";

        // Act
        var exception = new ObsRequestException(errorMessage);

        // Assert
        await Assert.That(exception.Message).IsEqualTo(errorMessage);
    }

    /// <summary>
    /// Tests that ObsRequestException preserves both message and inner exception.
    ///
    /// When an OBS request fails due to an underlying exception (e.g., network timeout,
    /// JSON parsing error), the inner exception provides crucial debugging information.
    /// Both the custom message and the original exception must be preserved.
    /// </summary>
    [Test]
    public async Task Constructor_WithMessageAndInnerException_PreservesBoth()
    {
        // Arrange
        const string errorMessage = "Failed to parse OBS response";
        var innerException = new InvalidOperationException("JSON deserialization failed");

        // Act
        var exception = new ObsRequestException(errorMessage, innerException);

        // Assert
        await Assert.That(exception.Message).IsEqualTo(errorMessage);
        await Assert.That(exception.InnerException).IsNotNull();
        await Assert.That(exception.InnerException).IsEqualTo(innerException);
    }

    /// <summary>
    /// Tests that ObsRequestException is derived from Exception.
    ///
    /// ObsRequestException must inherit from System.Exception to be properly
    /// catchable in try-catch blocks and to follow .NET exception conventions.
    /// </summary>
    [Test]
    public async Task ObsRequestException_InheritsFromException()
    {
        // Arrange & Act
        var exception = new ObsRequestException("Test");

        // Assert
        await Assert.That(exception).IsAssignableTo<Exception>();
    }

    /// <summary>
    /// Tests that ObsRequestException can be caught as a generic Exception.
    ///
    /// Users may catch exceptions at different levels of specificity. This test
    /// verifies that ObsRequestException works correctly in the exception hierarchy
    /// and can be caught by both specific and general catch blocks.
    /// </summary>
    [Test]
    public async Task ObsRequestException_CanBeCaughtAsException()
    {
        // Arrange
        Exception? caughtException = null;

        // Act
        try
        {
            throw new ObsRequestException("Request failed");
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        await Assert.That(caughtException).IsNotNull();
        await Assert.That(caughtException).IsTypeOf<ObsRequestException>();
    }

    /// <summary>
    /// Tests exception handling with the specific ObsRequestException type.
    ///
    /// Applications should be able to catch ObsRequestException specifically
    /// to handle OBS-related failures differently from other exceptions.
    /// For example, a "scene not found" error might be handled by creating the scene.
    /// </summary>
    [Test]
    public async Task ObsRequestException_CanBeCaughtSpecifically()
    {
        // Arrange
        ObsRequestException? caughtException = null;

        // Act
        try
        {
            throw new ObsRequestException("Scene not found");
        }
        catch (ObsRequestException ex)
        {
            caughtException = ex;
        }

        // Assert
        await Assert.That(caughtException).IsNotNull();
        await Assert.That(caughtException!.Message).IsEqualTo("Scene not found");
    }

    /// <summary>
    /// Tests that the inner exception chain is preserved through multiple levels.
    ///
    /// Complex failure scenarios may involve multiple levels of exceptions.
    /// For example: Network error -> JSON parsing error -> ObsRequestException.
    /// The full chain must be preserved for proper debugging.
    /// </summary>
    [Test]
    public async Task Constructor_WithNestedInnerExceptions_PreservesChain()
    {
        // Arrange - Create a chain of exceptions
        var rootCause = new TimeoutException("Connection timed out");
        var intermediateException = new InvalidOperationException("WebSocket send failed", rootCause);
        var obsException = new ObsRequestException("Request to OBS failed", intermediateException);

        // Assert - Verify the chain is intact
        await Assert.That(obsException.InnerException).IsNotNull();
        await Assert.That(obsException.InnerException).IsEqualTo(intermediateException);
        await Assert.That(obsException.InnerException!.InnerException).IsNotNull();
        await Assert.That(obsException.InnerException!.InnerException).IsEqualTo(rootCause);
    }

    /// <summary>
    /// Tests ObsRequestException with an empty message.
    ///
    /// While not recommended, empty messages should not cause exceptions.
    /// The exception should still be functional with an empty message.
    /// </summary>
    [Test]
    public async Task Constructor_WithEmptyMessage_DoesNotThrow()
    {
        // Arrange & Act
        var exception = new ObsRequestException(string.Empty);

        // Assert
        await Assert.That(exception.Message).IsEqualTo(string.Empty);
    }

    /// <summary>
    /// Tests that ToString() includes the message for debugging output.
    ///
    /// The ToString() method is commonly used in logging. It should include
    /// the exception type name and message at minimum, following .NET conventions.
    /// </summary>
    [Test]
    public async Task ToString_IncludesMessageAndTypeName()
    {
        // Arrange
        const string errorMessage = "Input 'TestInput' not found";
        var exception = new ObsRequestException(errorMessage);

        // Act
        var toStringResult = exception.ToString();

        // Assert - Should include type name and message
        await Assert.That(toStringResult.Contains("ObsRequestException")).IsTrue();
        await Assert.That(toStringResult.Contains(errorMessage)).IsTrue();
    }

    /// <summary>
    /// Tests that ToString() includes inner exception details when present.
    ///
    /// When debugging, the full exception chain is important. The ToString()
    /// method should include inner exception information for complete debugging output.
    /// </summary>
    [Test]
    public async Task ToString_IncludesInnerExceptionDetails()
    {
        // Arrange
        var innerException = new ArgumentException("Invalid parameter");
        var exception = new ObsRequestException("Request validation failed", innerException);

        // Act
        var toStringResult = exception.ToString();

        // Assert - Should include inner exception info
        await Assert.That(toStringResult.Contains("ArgumentException")).IsTrue();
        await Assert.That(toStringResult.Contains("Invalid parameter")).IsTrue();
    }

    /// <summary>
    /// Tests common OBS error scenarios to ensure exceptions work correctly.
    ///
    /// These represent real-world error messages that OBS WebSocket returns.
    /// The test verifies that various message formats are handled correctly.
    /// </summary>
    [Test]
    [Arguments("No scene was found by the name of `NonExistent`.")]
    [Arguments("Unable to perform request. Recording is not active.")]
    [Arguments("Unable to remove the input. Cannot remove an input in use by a scene.")]
    [Arguments("RequestType field `requestType` is missing or invalid.")]
    public async Task Constructor_WithRealWorldErrorMessages_PreservesMessage(string errorMessage)
    {
        // Act
        var exception = new ObsRequestException(errorMessage);

        // Assert
        await Assert.That(exception.Message).IsEqualTo(errorMessage);
    }
}
