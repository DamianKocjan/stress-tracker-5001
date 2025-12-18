using Xunit;
using StressTracker5001Server.Common;

namespace StressTracker5001Server.Tests.Helpers;

public static class AssertExtensions
{
  /// <summary>
  /// Asserts that a Result is successful
  /// </summary>
  public static T AssertSuccess<T>(this Result<T> result, string? message = null)
  {
    Assert.True(result.IsSuccess, message ?? $"Expected success but got: {result.Error}");
    Assert.NotNull(result.Value);
    return result.Value;
  }

  /// <summary>
  /// Asserts that a Result failed with a specific status code
  /// </summary>
  public static void AssertFailure<T>(this Result<T> result, int expectedStatusCode, string? expectedErrorSubstring = null)
  {
    Assert.False(result.IsSuccess, "Expected failure but result was successful");
    Assert.Equal(expectedStatusCode, result.StatusCode);

    if (!string.IsNullOrEmpty(expectedErrorSubstring))
    {
      Assert.Contains(expectedErrorSubstring, result.Error ?? string.Empty);
    }
  }

  /// <summary>
  /// Asserts that a Result returned Forbidden (403)
  /// </summary>
  public static void AssertForbidden<T>(this Result<T> result, string? expectedErrorSubstring = null)
  {
    Assert.False(result.IsSuccess, "Expected failure but result was successful");
    Assert.Equal(403, result.StatusCode);

    if (!string.IsNullOrEmpty(expectedErrorSubstring))
    {
      Assert.Contains(expectedErrorSubstring, result.Error ?? string.Empty);
    }
  }

  /// <summary>
  /// Asserts that a Result returned NotFound (404)
  /// </summary>
  public static void AssertNotFound<T>(this Result<T> result, string? expectedErrorSubstring = null)
  {
    Assert.False(result.IsSuccess, "Expected failure but result was successful");
    Assert.Equal(404, result.StatusCode);

    if (!string.IsNullOrEmpty(expectedErrorSubstring))
    {
      Assert.Contains(expectedErrorSubstring, result.Error ?? string.Empty);
    }
  }

  /// <summary>
  /// Asserts that a Result returned BadRequest (400)
  /// </summary>
  public static void AssertBadRequest<T>(this Result<T> result, string? expectedErrorSubstring = null)
  {
    Assert.False(result.IsSuccess, "Expected failure but result was successful");
    Assert.Equal(400, result.StatusCode);

    if (!string.IsNullOrEmpty(expectedErrorSubstring))
    {
      Assert.Contains(expectedErrorSubstring, result.Error ?? string.Empty);
    }
  }

  /// <summary>
  /// Helper to verify boolean Result success
  /// </summary>
  public static void AssertSuccessful(this Result<bool> result, string? message = null)
  {
    Assert.True(result.IsSuccess, message ?? $"Expected success but got: {result.Error}");
    Assert.True(result.Value, "Expected true value");
  }
}
