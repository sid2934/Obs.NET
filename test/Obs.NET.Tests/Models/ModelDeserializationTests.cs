#region

using System.Text.Json;
using Obs.NET.Models;

#endregion

namespace Obs.NET.Tests.Models;

/// <summary>
/// Tests for model class deserialization from JSON.
///
/// These models (ObsScene, ObsInput, ObsFilter, etc.) use [JsonExtensionData] to capture
/// any additional properties not explicitly defined in the model. This provides forward
/// compatibility with future OBS versions that may add new fields.
///
/// These tests verify that:
/// 1. Known properties are correctly deserialized
/// 2. Unknown properties are captured in the AdditionalData dictionary
/// 3. Models handle missing optional properties gracefully
/// 4. The JsonPropertyName attributes map correctly to OBS WebSocket JSON
/// </summary>
public class ModelDeserializationTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    #region ObsScene Tests

    /// <summary>
    /// Tests that ObsScene deserializes all known properties correctly.
    ///
    /// ObsScene represents a scene in OBS Studio. The key properties are:
    /// - sceneName: Human-readable name shown in OBS UI
    /// - sceneUuid: Unique identifier for the scene
    /// - sceneIndex: Position in the scene list (0-based)
    /// </summary>
    [Test]
    public async Task ObsScene_DeserializesKnownProperties()
    {
        // Arrange - Standard scene JSON from OBS WebSocket
        var json = """
            {
                "sceneName": "Gaming Scene",
                "sceneUuid": "abc123-def456",
                "sceneIndex": 2
            }
            """;

        // Act
        var scene = JsonSerializer.Deserialize<ObsScene>(json, _jsonOptions);

        // Assert
        await Assert.That(scene).IsNotNull();
        await Assert.That(scene!.SceneName).IsEqualTo("Gaming Scene");
        await Assert.That(scene.SceneUuid).IsEqualTo("abc123-def456");
        await Assert.That(scene.SceneIndex).IsEqualTo(2);
    }

    /// <summary>
    /// Tests that ObsScene captures unknown properties in AdditionalData.
    ///
    /// OBS may add new properties in future versions. The [JsonExtensionData] attribute
    /// ensures these are captured rather than silently ignored, allowing users to access
    /// new data without waiting for library updates.
    /// </summary>
    [Test]
    public async Task ObsScene_CapturesUnknownPropertiesInAdditionalData()
    {
        // Arrange - JSON with extra properties that might be added in future OBS versions
        var json = """
            {
                "sceneName": "Test Scene",
                "sceneUuid": "uuid-123",
                "sceneIndex": 0,
                "newFutureProperty": "some value",
                "anotherNewField": 42,
                "nestedObject": { "key": "value" }
            }
            """;

        // Act
        var scene = JsonSerializer.Deserialize<ObsScene>(json, _jsonOptions);

        // Assert - Known properties should be populated
        await Assert.That(scene).IsNotNull();
        await Assert.That(scene!.SceneName).IsEqualTo("Test Scene");

        // Assert - Unknown properties should be in AdditionalData
        await Assert.That(scene.AdditionalData).IsNotNull();
        await Assert.That(scene.AdditionalData!.ContainsKey("newFutureProperty")).IsTrue();
        await Assert.That(scene.AdditionalData["newFutureProperty"].GetString()).IsEqualTo("some value");
        await Assert.That(scene.AdditionalData["anotherNewField"].GetInt32()).IsEqualTo(42);
        await Assert.That(scene.AdditionalData.ContainsKey("nestedObject")).IsTrue();
    }

    /// <summary>
    /// Tests that ObsScene handles minimal JSON with default values.
    ///
    /// In some API responses, not all properties may be present. The model should
    /// initialize missing string properties to empty strings rather than null
    /// to provide safer defaults for consuming code.
    /// </summary>
    [Test]
    public async Task ObsScene_HandlesMinimalJson()
    {
        // Arrange - JSON with only sceneName
        var json = """
            {
                "sceneName": "Minimal Scene"
            }
            """;

        // Act
        var scene = JsonSerializer.Deserialize<ObsScene>(json, _jsonOptions);

        // Assert - Should have defaults for missing properties
        await Assert.That(scene).IsNotNull();
        await Assert.That(scene!.SceneName).IsEqualTo("Minimal Scene");
        await Assert.That(scene.SceneUuid).IsEqualTo(string.Empty);
        await Assert.That(scene.SceneIndex).IsEqualTo(0);
    }

    #endregion

    #region ObsInput Tests

    /// <summary>
    /// Tests that ObsInput deserializes all known properties correctly.
    ///
    /// ObsInput represents a source/input in OBS Studio. Inputs include:
    /// - Video captures (webcam, game capture)
    /// - Images and videos
    /// - Text sources
    /// - Browser sources
    ///
    /// The inputKind identifies the type (e.g., "browser_source", "dshow_input").
    /// </summary>
    [Test]
    public async Task ObsInput_DeserializesKnownProperties()
    {
        // Arrange - Standard input JSON from OBS WebSocket
        var json = """
            {
                "inputName": "Webcam",
                "inputUuid": "input-uuid-789",
                "inputKind": "dshow_input",
                "unversionedInputKind": "dshow_input"
            }
            """;

        // Act
        var input = JsonSerializer.Deserialize<ObsInput>(json, _jsonOptions);

        // Assert
        await Assert.That(input).IsNotNull();
        await Assert.That(input!.InputName).IsEqualTo("Webcam");
        await Assert.That(input.InputUuid).IsEqualTo("input-uuid-789");
        await Assert.That(input.InputKind).IsEqualTo("dshow_input");
        await Assert.That(input.UnversionedInputKind).IsEqualTo("dshow_input");
    }

    /// <summary>
    /// Tests that ObsInput captures unknown properties for forward compatibility.
    ///
    /// New input properties may be added in future OBS versions. For example,
    /// OBS might add a "inputCategory" or "inputCapabilities" field. These should
    /// be accessible through AdditionalData.
    /// </summary>
    [Test]
    public async Task ObsInput_CapturesUnknownPropertiesInAdditionalData()
    {
        // Arrange - JSON with hypothetical future properties
        var json = """
            {
                "inputName": "Browser Source",
                "inputUuid": "browser-123",
                "inputKind": "browser_source",
                "inputCategory": "web",
                "inputCapabilities": ["audio", "video"]
            }
            """;

        // Act
        var input = JsonSerializer.Deserialize<ObsInput>(json, _jsonOptions);

        // Assert
        await Assert.That(input).IsNotNull();
        await Assert.That(input!.AdditionalData).IsNotNull();
        await Assert.That(input.AdditionalData!.ContainsKey("inputCategory")).IsTrue();
        await Assert.That(input.AdditionalData["inputCategory"].GetString()).IsEqualTo("web");
        await Assert.That(input.AdditionalData.ContainsKey("inputCapabilities")).IsTrue();
    }

    /// <summary>
    /// Tests that ObsInput handles null unversionedInputKind gracefully.
    ///
    /// The unversionedInputKind property is nullable because some API responses
    /// may not include it. The model should handle this without throwing.
    /// </summary>
    [Test]
    public async Task ObsInput_HandlesNullUnversionedInputKind()
    {
        // Arrange - JSON without unversionedInputKind
        var json = """
            {
                "inputName": "Image",
                "inputUuid": "img-uuid",
                "inputKind": "image_source"
            }
            """;

        // Act
        var input = JsonSerializer.Deserialize<ObsInput>(json, _jsonOptions);

        // Assert
        await Assert.That(input).IsNotNull();
        await Assert.That(input!.InputName).IsEqualTo("Image");
        await Assert.That(input.UnversionedInputKind).IsNull();
    }

    #endregion

    #region ObsFilter Tests

    /// <summary>
    /// Tests that ObsFilter deserializes all known properties correctly.
    ///
    /// ObsFilter represents a filter applied to a source. Filters include:
    /// - Color correction
    /// - Chroma key (green screen)
    /// - Masks
    /// - Audio filters (compressor, noise gate)
    ///
    /// The filterSettings property contains filter-specific configuration.
    /// </summary>
    [Test]
    public async Task ObsFilter_DeserializesKnownProperties()
    {
        // Arrange - Color correction filter JSON
        var json = """
            {
                "filterName": "Color Correction",
                "filterKind": "color_filter_v2",
                "filterIndex": 0,
                "filterEnabled": true,
                "filterSettings": {
                    "brightness": 0.1,
                    "contrast": 0.2
                }
            }
            """;

        // Act
        var filter = JsonSerializer.Deserialize<ObsFilter>(json, _jsonOptions);

        // Assert
        await Assert.That(filter).IsNotNull();
        await Assert.That(filter!.FilterName).IsEqualTo("Color Correction");
        await Assert.That(filter.FilterKind).IsEqualTo("color_filter_v2");
        await Assert.That(filter.FilterIndex).IsEqualTo(0);
        await Assert.That(filter.FilterEnabled).IsTrue();
        await Assert.That(filter.FilterSettings).IsNotNull();
    }

    /// <summary>
    /// Tests that ObsFilter correctly parses nested filterSettings.
    ///
    /// Filter settings vary widely based on filter kind. The filterSettings
    /// property uses JsonElement to preserve the raw JSON structure, allowing
    /// users to access any settings regardless of filter type.
    /// </summary>
    [Test]
    public async Task ObsFilter_ParsesFilterSettingsAsJsonElement()
    {
        // Arrange - Chroma key filter with complex settings
        var json = """
            {
                "filterName": "Green Screen",
                "filterKind": "chroma_key_filter_v2",
                "filterIndex": 1,
                "filterEnabled": true,
                "filterSettings": {
                    "key_color_type": "green",
                    "similarity": 400,
                    "smoothness": 80,
                    "spill": 100
                }
            }
            """;

        // Act
        var filter = JsonSerializer.Deserialize<ObsFilter>(json, _jsonOptions);

        // Assert - Verify settings can be accessed
        await Assert.That(filter).IsNotNull();
        await Assert.That(filter!.FilterSettings).IsNotNull();

        var settings = filter.FilterSettings!.Value;
        await Assert.That(settings.GetProperty("key_color_type").GetString()).IsEqualTo("green");
        await Assert.That(settings.GetProperty("similarity").GetInt32()).IsEqualTo(400);
    }

    /// <summary>
    /// Tests that ObsFilter handles disabled filters correctly.
    ///
    /// Filters can be temporarily disabled without removing them. The filterEnabled
    /// property should accurately reflect this state for both enabled and disabled filters.
    /// </summary>
    [Test]
    public async Task ObsFilter_HandlesDisabledFilter()
    {
        // Arrange - Disabled filter
        var json = """
            {
                "filterName": "Disabled Filter",
                "filterKind": "noise_gate_filter",
                "filterIndex": 2,
                "filterEnabled": false,
                "filterSettings": {}
            }
            """;

        // Act
        var filter = JsonSerializer.Deserialize<ObsFilter>(json, _jsonOptions);

        // Assert
        await Assert.That(filter).IsNotNull();
        await Assert.That(filter!.FilterEnabled).IsFalse();
    }

    /// <summary>
    /// Tests that ObsFilter captures unknown properties for forward compatibility.
    ///
    /// Future OBS versions might add filter metadata like creation time,
    /// filter version, or filter-specific capabilities.
    /// </summary>
    [Test]
    public async Task ObsFilter_CapturesUnknownPropertiesInAdditionalData()
    {
        // Arrange - JSON with hypothetical future properties
        var json = """
            {
                "filterName": "Test Filter",
                "filterKind": "test_filter",
                "filterIndex": 0,
                "filterEnabled": true,
                "filterSettings": {},
                "filterVersion": 2,
                "filterCategory": "video"
            }
            """;

        // Act
        var filter = JsonSerializer.Deserialize<ObsFilter>(json, _jsonOptions);

        // Assert
        await Assert.That(filter).IsNotNull();
        await Assert.That(filter!.AdditionalData).IsNotNull();
        await Assert.That(filter.AdditionalData!.ContainsKey("filterVersion")).IsTrue();
        await Assert.That(filter.AdditionalData["filterVersion"].GetInt32()).IsEqualTo(2);
        await Assert.That(filter.AdditionalData["filterCategory"].GetString()).IsEqualTo("video");
    }

    #endregion

    #region Round-Trip Serialization Tests

    /// <summary>
    /// Tests that ObsScene can be serialized and deserialized without data loss.
    ///
    /// Round-trip serialization is important because applications may need to
    /// cache or persist model objects. This test verifies that known properties
    /// survive the round trip accurately.
    /// </summary>
    [Test]
    public async Task ObsScene_RoundTripSerialization_PreservesData()
    {
        // Arrange - Create an ObsScene object
        var original = new ObsScene
        {
            SceneName = "Round Trip Scene",
            SceneUuid = "uuid-round-trip",
            SceneIndex = 5
        };

        // Act - Serialize and deserialize
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<ObsScene>(json, _jsonOptions);

        // Assert
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.SceneName).IsEqualTo(original.SceneName);
        await Assert.That(deserialized.SceneUuid).IsEqualTo(original.SceneUuid);
        await Assert.That(deserialized.SceneIndex).IsEqualTo(original.SceneIndex);
    }

    /// <summary>
    /// Tests that AdditionalData is preserved during round-trip serialization.
    ///
    /// When unknown properties are captured in AdditionalData, they should be
    /// included in serialization output. This ensures that data from future
    /// OBS versions is not lost when the model is serialized.
    /// </summary>
    [Test]
    public async Task ObsScene_RoundTripSerialization_PreservesAdditionalData()
    {
        // Arrange - JSON with future properties
        var originalJson = """
            {
                "sceneName": "Test",
                "sceneUuid": "uuid",
                "sceneIndex": 0,
                "futureProperty": "preserved"
            }
            """;
        var original = JsonSerializer.Deserialize<ObsScene>(originalJson, _jsonOptions)!;

        // Act - Serialize back to JSON
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<ObsScene>(json, _jsonOptions);

        // Assert - Future property should survive round trip
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.AdditionalData).IsNotNull();
        await Assert.That(deserialized.AdditionalData!.ContainsKey("futureProperty")).IsTrue();
        await Assert.That(deserialized.AdditionalData["futureProperty"].GetString()).IsEqualTo("preserved");
    }

    #endregion

    #region Empty JSON Tests

    /// <summary>
    /// Tests that models handle completely empty JSON objects.
    ///
    /// While unusual, APIs sometimes return empty objects in edge cases.
    /// The model should not throw and should have sensible defaults.
    /// </summary>
    [Test]
    public async Task Models_HandleEmptyJsonObject()
    {
        // Arrange
        var json = "{}";

        // Act
        var scene = JsonSerializer.Deserialize<ObsScene>(json, _jsonOptions);
        var input = JsonSerializer.Deserialize<ObsInput>(json, _jsonOptions);
        var filter = JsonSerializer.Deserialize<ObsFilter>(json, _jsonOptions);

        // Assert - All should have default values
        await Assert.That(scene).IsNotNull();
        await Assert.That(scene!.SceneName).IsEqualTo(string.Empty);

        await Assert.That(input).IsNotNull();
        await Assert.That(input!.InputName).IsEqualTo(string.Empty);

        await Assert.That(filter).IsNotNull();
        await Assert.That(filter!.FilterName).IsEqualTo(string.Empty);
    }

    #endregion
}
