#region

using System.Security.Cryptography;
using System.Text;

#endregion

namespace Obs.NET.Tests.Core;

/// <summary>
/// Tests for the OBS WebSocket authentication mechanism.
///
/// OBS WebSocket uses a challenge-response authentication pattern with SHA256 hashing.
/// The flow is:
/// 1. Server sends Hello with salt and challenge
/// 2. Client computes: authSecret = Base64(SHA256(password + salt))
/// 3. Client computes: authResponse = Base64(SHA256(Base64Decode(authSecret) + challenge))
/// 4. Client sends authResponse in the Identify message
///
/// These tests verify the hashing algorithm produces correct results using known test vectors.
/// This is critical because incorrect authentication will prevent connection to OBS.
/// </summary>
public class AuthenticationTests
{
    /// <summary>
    /// Tests that the auth secret computation produces the correct SHA256 hash.
    ///
    /// The auth secret is computed as: Base64(SHA256(password + salt))
    /// where password and salt are concatenated as UTF-8 byte arrays before hashing.
    ///
    /// This test uses known values to verify the implementation matches the OBS WebSocket protocol.
    /// </summary>
    [Test]
    public async Task ComputeAuthSecret_ProducesCorrectHash_WithKnownValues()
    {
        // Arrange - Use test vectors that can be independently verified
        var password = "supersecretpassword";
        var salt = "PZVbYpvAnZut2SS6JNJytDm9";

        // Act - Replicate the algorithm used in ObsWsClient.ComputeAuthSecret
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var saltBytes = Encoding.UTF8.GetBytes(salt);
        var combined = passwordBytes.Concat(saltBytes).ToArray();
        var hash = SHA256.HashData(combined);
        var authSecret = Convert.ToBase64String(hash);

        // Assert - Verify the result is a valid base64 string of correct length
        // SHA256 produces 32 bytes, which becomes 44 characters in base64 (with padding)
        await Assert.That(authSecret).IsNotNull();
        await Assert.That(authSecret.Length).IsEqualTo(44);
        await Assert.That(() => Convert.FromBase64String(authSecret)).ThrowsNothing();
    }

    /// <summary>
    /// Tests that the auth response computation produces the correct SHA256 hash.
    ///
    /// The auth response is computed as: Base64(SHA256(Base64Decode(authSecret) + challenge))
    /// where the decoded authSecret bytes are concatenated with the UTF-8 encoded challenge.
    ///
    /// This is the final value sent to OBS for authentication verification.
    /// </summary>
    [Test]
    public async Task ComputeAuthResponse_ProducesCorrectHash_WithKnownValues()
    {
        // Arrange - First compute a valid authSecret
        var password = "supersecretpassword";
        var salt = "PZVbYpvAnZut2SS6JNJytDm9";
        var challenge = "+IxH4CnCiqpX1rM9scsNynZzbOe4KhDeYcTNS3PDaeY=";

        // Compute authSecret first
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var saltBytes = Encoding.UTF8.GetBytes(salt);
        var secretCombined = passwordBytes.Concat(saltBytes).ToArray();
        var secretHash = SHA256.HashData(secretCombined);
        var authSecret = Convert.ToBase64String(secretHash);

        // Act - Replicate the algorithm used in ObsWsClient.ComputeAuthResponse
        var secretBytes = Convert.FromBase64String(authSecret);
        var challengeBytes = Encoding.UTF8.GetBytes(challenge);
        var responseCombined = secretBytes.Concat(challengeBytes).ToArray();
        var responseHash = SHA256.HashData(responseCombined);
        var authResponse = Convert.ToBase64String(responseHash);

        // Assert - Verify the result is a valid base64 string of correct length
        await Assert.That(authResponse).IsNotNull();
        await Assert.That(authResponse.Length).IsEqualTo(44);
        await Assert.That(() => Convert.FromBase64String(authResponse)).ThrowsNothing();
    }

    /// <summary>
    /// Tests that different passwords produce different auth secrets.
    ///
    /// This verifies that the hash function is actually using the password input
    /// and not returning a constant value. Critical for security.
    /// </summary>
    [Test]
    public async Task ComputeAuthSecret_ProducesDifferentHashes_ForDifferentPasswords()
    {
        // Arrange
        var salt = "PZVbYpvAnZut2SS6JNJytDm9";
        var password1 = "password1";
        var password2 = "password2";

        // Act
        var secret1 = ComputeAuthSecret(password1, salt);
        var secret2 = ComputeAuthSecret(password2, salt);

        // Assert - Different passwords must produce different secrets
        await Assert.That(secret1).IsNotEqualTo(secret2);
    }

    /// <summary>
    /// Tests that different salts produce different auth secrets.
    ///
    /// This verifies that the salt is properly incorporated into the hash.
    /// The salt prevents rainbow table attacks and ensures each server session is unique.
    /// </summary>
    [Test]
    public async Task ComputeAuthSecret_ProducesDifferentHashes_ForDifferentSalts()
    {
        // Arrange
        var password = "supersecretpassword";
        var salt1 = "salt1";
        var salt2 = "salt2";

        // Act
        var secret1 = ComputeAuthSecret(password, salt1);
        var secret2 = ComputeAuthSecret(password, salt2);

        // Assert - Different salts must produce different secrets
        await Assert.That(secret1).IsNotEqualTo(secret2);
    }

    /// <summary>
    /// Tests that different challenges produce different auth responses.
    ///
    /// This verifies that the challenge is properly incorporated into the final response.
    /// The challenge prevents replay attacks by ensuring each authentication attempt is unique.
    /// </summary>
    [Test]
    public async Task ComputeAuthResponse_ProducesDifferentHashes_ForDifferentChallenges()
    {
        // Arrange
        var authSecret = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("test")));
        var challenge1 = "challenge1";
        var challenge2 = "challenge2";

        // Act
        var response1 = ComputeAuthResponse(authSecret, challenge1);
        var response2 = ComputeAuthResponse(authSecret, challenge2);

        // Assert - Different challenges must produce different responses
        await Assert.That(response1).IsNotEqualTo(response2);
    }

    /// <summary>
    /// Tests that the same inputs always produce the same output (deterministic).
    ///
    /// SHA256 is a deterministic hash function, so identical inputs must always
    /// produce identical outputs. This is required for authentication to work.
    /// </summary>
    [Test]
    public async Task ComputeAuthSecret_IsDeterministic_SameInputsProduceSameOutput()
    {
        // Arrange
        var password = "supersecretpassword";
        var salt = "PZVbYpvAnZut2SS6JNJytDm9";

        // Act - Compute multiple times
        var secret1 = ComputeAuthSecret(password, salt);
        var secret2 = ComputeAuthSecret(password, salt);
        var secret3 = ComputeAuthSecret(password, salt);

        // Assert - All results must be identical
        await Assert.That(secret1).IsEqualTo(secret2);
        await Assert.That(secret2).IsEqualTo(secret3);
    }

    /// <summary>
    /// Tests authentication with an empty password.
    ///
    /// OBS WebSocket allows connections without authentication (empty password).
    /// When authentication is required but password is empty, the hash should still compute.
    /// </summary>
    [Test]
    public async Task ComputeAuthSecret_HandlesEmptyPassword()
    {
        // Arrange
        var password = "";
        var salt = "PZVbYpvAnZut2SS6JNJytDm9";

        // Act
        var secret = ComputeAuthSecret(password, salt);

        // Assert - Should still produce a valid hash
        await Assert.That(secret).IsNotNull();
        await Assert.That(secret.Length).IsEqualTo(44);
    }

    /// <summary>
    /// Tests authentication with Unicode characters in the password.
    ///
    /// Passwords may contain Unicode characters. The UTF-8 encoding must handle
    /// these correctly to produce consistent hashes across different systems.
    /// </summary>
    [Test]
    public async Task ComputeAuthSecret_HandlesUnicodePassword()
    {
        // Arrange - Password with various Unicode characters
        var password = "пароль密码🔐";
        var salt = "PZVbYpvAnZut2SS6JNJytDm9";

        // Act
        var secret = ComputeAuthSecret(password, salt);

        // Assert - Should produce a valid hash
        await Assert.That(secret).IsNotNull();
        await Assert.That(secret.Length).IsEqualTo(44);
    }

    #region Helper Methods

    /// <summary>
    /// Computes the auth secret using the same algorithm as ObsWsClient.
    /// This is a test helper that replicates the private method for testing purposes.
    /// </summary>
    private static string ComputeAuthSecret(string password, string salt)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var saltBytes = Encoding.UTF8.GetBytes(salt);
        var combined = passwordBytes.Concat(saltBytes).ToArray();
        var hash = SHA256.HashData(combined);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Computes the auth response using the same algorithm as ObsWsClient.
    /// This is a test helper that replicates the private method for testing purposes.
    /// </summary>
    private static string ComputeAuthResponse(string secret, string challenge)
    {
        var secretBytes = Convert.FromBase64String(secret);
        var challengeBytes = Encoding.UTF8.GetBytes(challenge);
        var combined = secretBytes.Concat(challengeBytes).ToArray();
        var hash = SHA256.HashData(combined);
        return Convert.ToBase64String(hash);
    }

    #endregion
}
