#region

using System.Text.Json;
using Obs.NET.Events;

#endregion

namespace Obs.NET.Tests.Events;

/// <summary>
/// Tests for event deserialization from JSON.
///
/// OBS WebSocket sends events as JSON messages. These tests verify that our generated
/// event classes correctly deserialize the JSON payloads sent by OBS.
///
/// Correct event deserialization is critical because:
/// 1. Event handlers depend on properly populated event objects
/// 2. Missing or incorrect data could cause runtime errors in user code
/// 3. The event classes are auto-generated, so we need to verify the generator output
/// </summary>
public class EventDeserializationTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Tests deserialization of SceneCreatedEvent.
    ///
    /// This event is fired when a new scene is created in OBS.
    /// It includes the scene name, UUID, and whether it's a group.
    /// </summary>
    [Test]
    public async Task SceneCreatedEvent_DeserializesCorrectly()
    {
        // Arrange - JSON payload as sent by OBS WebSocket
        var json = """
            {
                "sceneName": "My New Scene",
                "sceneUuid": "abc123-def456-ghi789",
                "isGroup": false
            }
            """;

        // Act
        var eventData = JsonSerializer.Deserialize<SceneCreatedEvent>(json, _jsonOptions);

        // Assert
        await Assert.That(eventData).IsNotNull();
        await Assert.That(eventData!.SceneName).IsEqualTo("My New Scene");
        await Assert.That(eventData.SceneUuid).IsEqualTo("abc123-def456-ghi789");
        await Assert.That(eventData.IsGroup).IsFalse();
    }

    /// <summary>
    /// Tests deserialization of SceneCreatedEvent when it's a group.
    ///
    /// Scene groups are special scenes that contain other scene items.
    /// The isGroup flag distinguishes them from regular scenes.
    /// </summary>
    [Test]
    public async Task SceneCreatedEvent_DeserializesCorrectly_WhenIsGroup()
    {
        // Arrange
        var json = """
            {
                "sceneName": "My Scene Group",
                "sceneUuid": "group-uuid-123",
                "isGroup": true
            }
            """;

        // Act
        var eventData = JsonSerializer.Deserialize<SceneCreatedEvent>(json, _jsonOptions);

        // Assert
        await Assert.That(eventData).IsNotNull();
        await Assert.That(eventData!.IsGroup).IsTrue();
    }

    /// <summary>
    /// Tests that SceneCreatedEvent has the correct static EventType property.
    ///
    /// The EventType property is used by the event dispatcher to route events
    /// to the correct handler. It must match the OBS WebSocket protocol exactly.
    /// </summary>
    [Test]
    public async Task SceneCreatedEvent_HasCorrectEventType()
    {
        // Assert
        await Assert.That(SceneCreatedEvent.EventType).IsEqualTo("SceneCreated");
    }

    /// <summary>
    /// Tests deserialization of CurrentProfileChangedEvent.
    ///
    /// This event is fired when the user switches to a different profile in OBS.
    /// Profiles contain settings like video resolution, encoder settings, etc.
    /// </summary>
    [Test]
    public async Task CurrentProfileChangedEvent_DeserializesCorrectly()
    {
        // Arrange
        var json = """
            {
                "profileName": "Streaming Profile"
            }
            """;

        // Act
        var eventData = JsonSerializer.Deserialize<CurrentProfileChangedEvent>(json, _jsonOptions);

        // Assert
        await Assert.That(eventData).IsNotNull();
        await Assert.That(eventData!.ProfileName).IsEqualTo("Streaming Profile");
    }

    /// <summary>
    /// Tests that CurrentProfileChangedEvent has the correct static EventType property.
    /// </summary>
    [Test]
    public async Task CurrentProfileChangedEvent_HasCorrectEventType()
    {
        // Assert
        await Assert.That(CurrentProfileChangedEvent.EventType).IsEqualTo("CurrentProfileChanged");
    }

    /// <summary>
    /// Tests deserialization of CurrentProgramSceneChangedEvent.
    ///
    /// This event is fired when the program (live/output) scene changes.
    /// This is one of the most commonly used events for scene tracking.
    /// </summary>
    [Test]
    public async Task CurrentProgramSceneChangedEvent_DeserializesCorrectly()
    {
        // Arrange
        var json = """
            {
                "sceneName": "Gaming Scene",
                "sceneUuid": "scene-uuid-456"
            }
            """;

        // Act
        var eventData = JsonSerializer.Deserialize<CurrentProgramSceneChangedEvent>(json, _jsonOptions);

        // Assert
        await Assert.That(eventData).IsNotNull();
        await Assert.That(eventData!.SceneName).IsEqualTo("Gaming Scene");
        await Assert.That(eventData.SceneUuid).IsEqualTo("scene-uuid-456");
    }

    /// <summary>
    /// Tests deserialization of InputCreatedEvent.
    ///
    /// This event is fired when a new input (source) is created.
    /// Inputs include video captures, images, text sources, etc.
    /// </summary>
    [Test]
    public async Task InputCreatedEvent_DeserializesCorrectly()
    {
        // Arrange
        var json = """
            {
                "inputName": "Webcam",
                "inputUuid": "input-uuid-789",
                "inputKind": "dshow_input",
                "unversionedInputKind": "dshow_input",
                "inputKindCaps": 1234,
                "inputSettings": {
                    "video_device_id": "device123"
                },
                "defaultInputSettings": {}
            }
            """;

        // Act
        var eventData = JsonSerializer.Deserialize<InputCreatedEvent>(json, _jsonOptions);

        // Assert
        await Assert.That(eventData).IsNotNull();
        await Assert.That(eventData!.InputName).IsEqualTo("Webcam");
        await Assert.That(eventData.InputUuid).IsEqualTo("input-uuid-789");
        await Assert.That(eventData.InputKind).IsEqualTo("dshow_input");
        await Assert.That(eventData.UnversionedInputKind).IsEqualTo("dshow_input");
        await Assert.That(eventData.InputKindCaps).IsEqualTo(1234);
    }

    /// <summary>
    /// Tests deserialization of RecordStateChangedEvent.
    ///
    /// This event is fired when recording state changes (starting, started, stopping, stopped).
    /// The outputPath is only populated when recording has started or stopped.
    /// </summary>
    [Test]
    public async Task RecordStateChangedEvent_DeserializesCorrectly_WhenStarted()
    {
        // Arrange
        var json = """
            {
                "outputActive": true,
                "outputState": "OBS_WEBSOCKET_OUTPUT_STARTED",
                "outputPath": "/recordings/recording_2024-01-15.mkv"
            }
            """;

        // Act
        var eventData = JsonSerializer.Deserialize<RecordStateChangedEvent>(json, _jsonOptions);

        // Assert
        await Assert.That(eventData).IsNotNull();
        await Assert.That(eventData!.OutputActive).IsTrue();
        await Assert.That(eventData.OutputState).IsEqualTo("OBS_WEBSOCKET_OUTPUT_STARTED");
        await Assert.That(eventData.OutputPath).IsEqualTo("/recordings/recording_2024-01-15.mkv");
    }

    /// <summary>
    /// Tests deserialization of RecordStateChangedEvent when stopping.
    ///
    /// When recording is in the "stopping" state, outputActive is still true
    /// but the state indicates the transition is in progress.
    /// </summary>
    [Test]
    public async Task RecordStateChangedEvent_DeserializesCorrectly_WhenStopping()
    {
        // Arrange
        var json = """
            {
                "outputActive": true,
                "outputState": "OBS_WEBSOCKET_OUTPUT_STOPPING",
                "outputPath": null
            }
            """;

        // Act
        var eventData = JsonSerializer.Deserialize<RecordStateChangedEvent>(json, _jsonOptions);

        // Assert
        await Assert.That(eventData).IsNotNull();
        await Assert.That(eventData!.OutputActive).IsTrue();
        await Assert.That(eventData.OutputState).IsEqualTo("OBS_WEBSOCKET_OUTPUT_STOPPING");
    }

    /// <summary>
    /// Tests deserialization of StreamStateChangedEvent.
    ///
    /// This event is fired when streaming state changes.
    /// Similar to RecordStateChangedEvent but for streaming output.
    /// </summary>
    [Test]
    public async Task StreamStateChangedEvent_DeserializesCorrectly()
    {
        // Arrange
        var json = """
            {
                "outputActive": true,
                "outputState": "OBS_WEBSOCKET_OUTPUT_STARTED"
            }
            """;

        // Act
        var eventData = JsonSerializer.Deserialize<StreamStateChangedEvent>(json, _jsonOptions);

        // Assert
        await Assert.That(eventData).IsNotNull();
        await Assert.That(eventData!.OutputActive).IsTrue();
        await Assert.That(eventData.OutputState).IsEqualTo("OBS_WEBSOCKET_OUTPUT_STARTED");
    }

    /// <summary>
    /// Tests that event classes implement IObsWsEventData interface.
    ///
    /// All event classes must implement this interface for the event dispatcher
    /// to work correctly. The interface provides the static EventType property.
    /// </summary>
    [Test]
    public async Task EventClasses_ImplementIObsWsEventData()
    {
        // Assert - Verify several event types implement the interface
        await Assert.That(typeof(SceneCreatedEvent).IsAssignableTo(typeof(IObsWsEventData))).IsTrue();
        await Assert.That(typeof(CurrentProfileChangedEvent).IsAssignableTo(typeof(IObsWsEventData))).IsTrue();
        await Assert.That(typeof(CurrentProgramSceneChangedEvent).IsAssignableTo(typeof(IObsWsEventData))).IsTrue();
        await Assert.That(typeof(InputCreatedEvent).IsAssignableTo(typeof(IObsWsEventData))).IsTrue();
        await Assert.That(typeof(RecordStateChangedEvent).IsAssignableTo(typeof(IObsWsEventData))).IsTrue();
        await Assert.That(typeof(StreamStateChangedEvent).IsAssignableTo(typeof(IObsWsEventData))).IsTrue();
    }

    /// <summary>
    /// Tests that deserialization handles missing optional fields gracefully.
    ///
    /// Some event fields are optional and may not be present in all messages.
    /// The deserializer should handle these cases without throwing exceptions.
    /// </summary>
    [Test]
    public async Task EventDeserialization_RequiresAllFields()
    {
        // Arrange - JSON with all required fields
        var json = """
            {
                "sceneName": "Test",
                "sceneUuid": "uuid-12345",
                "isGroup": false
            }
            """;

        // Act
        var eventData = JsonSerializer.Deserialize<SceneCreatedEvent>(json, _jsonOptions);

        // Assert - All fields should be populated
        await Assert.That(eventData).IsNotNull();
        await Assert.That(eventData!.SceneName).IsEqualTo("Test");
        await Assert.That(eventData.SceneUuid).IsEqualTo("uuid-12345");
        await Assert.That(eventData.IsGroup).IsFalse();
    }

    /// <summary>
    /// Tests that deserialization ignores unknown fields.
    ///
    /// OBS may add new fields in future versions. The deserializer should
    /// ignore unknown fields to maintain forward compatibility.
    /// </summary>
    [Test]
    public async Task EventDeserialization_IgnoresUnknownFields()
    {
        // Arrange - JSON with extra fields not in the class
        var json = """
            {
                "sceneName": "Test Scene",
                "sceneUuid": "uuid-123",
                "isGroup": false,
                "futureField": "some value",
                "anotherNewField": 42
            }
            """;

        // Act - Should not throw despite unknown fields
        var eventData = JsonSerializer.Deserialize<SceneCreatedEvent>(json, _jsonOptions);

        // Assert
        await Assert.That(eventData).IsNotNull();
        await Assert.That(eventData!.SceneName).IsEqualTo("Test Scene");
    }
}
