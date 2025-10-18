using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Maui;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests;

public partial class PlatformWebViewWebResourceRequestedEventArgsTests
{
#if !WINDOWS && !IOS && !MACCATALYST && !ANDROID
    /// <summary>
    /// Verifies that GetRequestMethod returns null in the fallback implementation
    /// when no platform-specific preprocessor directives are active.
    /// Tests the default behavior for unsupported platforms.
    /// </summary>
    [Fact]
    public void GetRequestMethod_FallbackImplementation_ReturnsNull()
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var platformEventArgs = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);

        // Act
        var result = platformEventArgs.GetRequestMethod();

        // Assert
        Assert.Null(result);
    }
#endif

#if !(WINDOWS || IOS || MACCATALYST || ANDROID)
    /// <summary>
    /// Tests that GetRequestHeaders returns null in the fallback implementation.
    /// This test verifies that when no platform-specific implementation is available,
    /// the method returns null as expected.
    /// </summary>
    [Fact]
    public void GetRequestHeaders_FallbackImplementation_ReturnsNull()
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var platformArgs = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);

        // Act
        var result = platformArgs.GetRequestHeaders();

        // Assert
        Assert.Null(result);
    }
#endif

    /// <summary>
    /// Tests that SetResponse executes without throwing exceptions for various parameter combinations.
    /// This tests the fallback implementation which is a no-op method.
    /// </summary>
    /// <param name="code">HTTP status code to test</param>
    /// <param name="reason">HTTP reason phrase to test</param>
    /// <param name="hasHeaders">Whether to include headers dictionary</param>
    /// <param name="hasContent">Whether to include content stream</param>
    [Theory]
    [InlineData(200, "OK", true, true)]
    [InlineData(404, "Not Found", false, false)]
    [InlineData(500, "Internal Server Error", true, false)]
    [InlineData(0, "", false, true)]
    [InlineData(-1, "Invalid", true, true)]
    [InlineData(int.MaxValue, "Max Value", false, false)]
    [InlineData(int.MinValue, "Min Value", true, false)]
    public void SetResponse_VariousParameters_DoesNotThrow(int code, string reason, bool hasHeaders, bool hasContent)
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var platformArgs = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);

        IReadOnlyDictionary<string, string> headers = hasHeaders
            ? new Dictionary<string, string> { { "Content-Type", "text/html" }, { "Cache-Control", "no-cache" } }
            : null;

        Stream content = hasContent ? new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }) : null;

        // Act & Assert - Should not throw any exceptions
        var exception = Record.Exception(() => platformArgs.SetResponse(code, reason, headers, content));
        Assert.Null(exception);

        // Clean up
        content?.Dispose();
    }

    /// <summary>
    /// Tests SetResponse with null reason parameter.
    /// The fallback implementation should handle null values gracefully.
    /// </summary>
    [Fact]
    public void SetResponse_NullReason_DoesNotThrow()
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var platformArgs = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);

        // Act & Assert
        var exception = Record.Exception(() => platformArgs.SetResponse(200, null, null, null));
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests SetResponse with empty reason string.
    /// The fallback implementation should handle empty strings gracefully.
    /// </summary>
    [Fact]
    public void SetResponse_EmptyReason_DoesNotThrow()
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var platformArgs = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);

        // Act & Assert
        var exception = Record.Exception(() => platformArgs.SetResponse(200, string.Empty, null, null));
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests SetResponse with whitespace-only reason string.
    /// The fallback implementation should handle whitespace strings gracefully.
    /// </summary>
    [Fact]
    public void SetResponse_WhitespaceReason_DoesNotThrow()
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var platformArgs = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);

        // Act & Assert
        var exception = Record.Exception(() => platformArgs.SetResponse(200, "   \t\n\r   ", null, null));
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests SetResponse with very long reason string.
    /// The fallback implementation should handle long strings gracefully.
    /// </summary>
    [Fact]
    public void SetResponse_VeryLongReason_DoesNotThrow()
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var platformArgs = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);
        var longReason = new string('a', 10000);

        // Act & Assert
        var exception = Record.Exception(() => platformArgs.SetResponse(200, longReason, null, null));
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests SetResponse with reason containing special characters.
    /// The fallback implementation should handle special characters gracefully.
    /// </summary>
    [Fact]
    public void SetResponse_ReasonWithSpecialCharacters_DoesNotThrow()
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var platformArgs = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);
        var reasonWithSpecialChars = "Status: OK! @#$%^&*()_+-=[]{}|;':\",./<>?`~";

        // Act & Assert
        var exception = Record.Exception(() => platformArgs.SetResponse(200, reasonWithSpecialChars, null, null));
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests SetResponse with empty headers dictionary.
    /// The fallback implementation should handle empty collections gracefully.
    /// </summary>
    [Fact]
    public void SetResponse_EmptyHeaders_DoesNotThrow()
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var platformArgs = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);
        var emptyHeaders = new Dictionary<string, string>();

        // Act & Assert
        var exception = Record.Exception(() => platformArgs.SetResponse(200, "OK", emptyHeaders, null));
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests SetResponse with headers containing null values.
    /// The fallback implementation should handle null header values gracefully.
    /// </summary>
    [Fact]
    public void SetResponse_HeadersWithNullValues_DoesNotThrow()
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var platformArgs = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);
        var headersWithNulls = new Dictionary<string, string>
            {
                { "Valid-Header", "valid-value" },
                { "Null-Header", null }
            };

        // Act & Assert
        var exception = Record.Exception(() => platformArgs.SetResponse(200, "OK", headersWithNulls, null));
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests SetResponse with empty stream content.
    /// The fallback implementation should handle empty streams gracefully.
    /// </summary>
    [Fact]
    public void SetResponse_EmptyStream_DoesNotThrow()
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var platformArgs = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);
        using var emptyStream = new MemoryStream();

        // Act & Assert
        var exception = Record.Exception(() => platformArgs.SetResponse(200, "OK", null, emptyStream));
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests SetResponse with closed/disposed stream.
    /// The fallback implementation should handle disposed streams gracefully.
    /// </summary>
    [Fact]
    public void SetResponse_DisposedStream_DoesNotThrow()
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var platformArgs = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        stream.Dispose();

        // Act & Assert
        var exception = Record.Exception(() => platformArgs.SetResponse(200, "OK", null, stream));
        Assert.Null(exception);
    }
#if !WINDOWS && !IOS && !MACCATALYST && !ANDROID

    /// <summary>
    /// Tests that SetResponseAsync returns a completed task with valid parameters.
    /// Verifies the method always returns Task.CompletedTask regardless of input values.
    /// </summary>
    [Fact]
    public void SetResponseAsync_ValidParameters_ReturnsCompletedTask()
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var target = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);
        var headers = new Dictionary<string, string> { { "Content-Type", "text/html" } };
        var contentTask = Task.FromResult<Stream?>(new MemoryStream());

        // Act
        var result = target.SetResponseAsync(200, "OK", headers, contentTask);

        // Assert
        Assert.True(result.IsCompletedSuccessfully);
        Assert.Equal(Task.CompletedTask, result);
    }

    /// <summary>
    /// Tests SetResponseAsync with boundary values for the code parameter.
    /// Verifies the method handles extreme integer values without throwing exceptions.
    /// </summary>
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(200)]
    [InlineData(404)]
    [InlineData(500)]
    public void SetResponseAsync_BoundaryCodeValues_ReturnsCompletedTask(int code)
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var target = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);
        var contentTask = Task.FromResult<Stream?>(null);

        // Act
        var result = target.SetResponseAsync(code, "Test", null, contentTask);

        // Assert
        Assert.True(result.IsCompletedSuccessfully);
        Assert.Equal(Task.CompletedTask, result);
    }

    /// <summary>
    /// Tests SetResponseAsync with various reason string values.
    /// Verifies the method handles null, empty, whitespace, and long strings without throwing exceptions.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("OK")]
    [InlineData("Internal Server Error")]
    public void SetResponseAsync_VariousReasonStrings_ReturnsCompletedTask(string reason)
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var target = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);
        var contentTask = Task.FromResult<Stream?>(null);
        var longReason = reason == null ? null : new string('A', 10000);

        // Act
        var result = target.SetResponseAsync(200, reason ?? longReason, null, contentTask);

        // Assert
        Assert.True(result.IsCompletedSuccessfully);
        Assert.Equal(Task.CompletedTask, result);
    }

    /// <summary>
    /// Tests SetResponseAsync with null headers parameter.
    /// Verifies the method handles null headers without throwing exceptions.
    /// </summary>
    [Fact]
    public void SetResponseAsync_NullHeaders_ReturnsCompletedTask()
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var target = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);
        var contentTask = Task.FromResult<Stream?>(null);

        // Act
        var result = target.SetResponseAsync(200, "OK", null, contentTask);

        // Assert
        Assert.True(result.IsCompletedSuccessfully);
        Assert.Equal(Task.CompletedTask, result);
    }

    /// <summary>
    /// Tests SetResponseAsync with empty headers dictionary.
    /// Verifies the method handles empty headers collection without throwing exceptions.
    /// </summary>
    [Fact]
    public void SetResponseAsync_EmptyHeaders_ReturnsCompletedTask()
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var target = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);
        var headers = new Dictionary<string, string>();
        var contentTask = Task.FromResult<Stream?>(null);

        // Act
        var result = target.SetResponseAsync(200, "OK", headers, contentTask);

        // Assert
        Assert.True(result.IsCompletedSuccessfully);
        Assert.Equal(Task.CompletedTask, result);
    }

    /// <summary>
    /// Tests SetResponseAsync with populated headers dictionary.
    /// Verifies the method handles multiple headers without throwing exceptions.
    /// </summary>
    [Fact]
    public void SetResponseAsync_PopulatedHeaders_ReturnsCompletedTask()
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var target = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);
        var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Authorization", "Bearer token123" },
                { "Cache-Control", "no-cache" }
            };
        var contentTask = Task.FromResult<Stream?>(new MemoryStream());

        // Act
        var result = target.SetResponseAsync(201, "Created", headers, contentTask);

        // Assert
        Assert.True(result.IsCompletedSuccessfully);
        Assert.Equal(Task.CompletedTask, result);
    }

    /// <summary>
    /// Tests SetResponseAsync with completed content task containing null stream.
    /// Verifies the method handles null content without throwing exceptions.
    /// </summary>
    [Fact]
    public void SetResponseAsync_CompletedTaskWithNullContent_ReturnsCompletedTask()
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var target = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);
        var contentTask = Task.FromResult<Stream?>(null);

        // Act
        var result = target.SetResponseAsync(204, "No Content", null, contentTask);

        // Assert
        Assert.True(result.IsCompletedSuccessfully);
        Assert.Equal(Task.CompletedTask, result);
    }

    /// <summary>
    /// Tests SetResponseAsync with completed content task containing valid stream.
    /// Verifies the method handles valid content streams without throwing exceptions.
    /// </summary>
    [Fact]
    public void SetResponseAsync_CompletedTaskWithValidContent_ReturnsCompletedTask()
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var target = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);
        var stream = new MemoryStream();
        var contentTask = Task.FromResult<Stream?>(stream);

        // Act
        var result = target.SetResponseAsync(200, "OK", null, contentTask);

        // Assert
        Assert.True(result.IsCompletedSuccessfully);
        Assert.Equal(Task.CompletedTask, result);
    }

    /// <summary>
    /// Tests SetResponseAsync with faulted content task.
    /// Verifies the method returns completed task even when content task is faulted.
    /// </summary>
    [Fact]
    public void SetResponseAsync_FaultedContentTask_ReturnsCompletedTask()
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var target = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);
        var tcs = new TaskCompletionSource<Stream?>();
        tcs.SetException(new InvalidOperationException("Test exception"));
        var contentTask = tcs.Task;

        // Act
        var result = target.SetResponseAsync(500, "Internal Server Error", null, contentTask);

        // Assert
        Assert.True(result.IsCompletedSuccessfully);
        Assert.Equal(Task.CompletedTask, result);
    }

    /// <summary>
    /// Tests SetResponseAsync with cancelled content task.
    /// Verifies the method returns completed task even when content task is cancelled.
    /// </summary>
    [Fact]
    public void SetResponseAsync_CancelledContentTask_ReturnsCompletedTask()
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var target = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var contentTask = Task.FromCanceled<Stream?>(cts.Token);

        // Act
        var result = target.SetResponseAsync(408, "Request Timeout", null, contentTask);

        // Assert
        Assert.True(result.IsCompletedSuccessfully);
        Assert.Equal(Task.CompletedTask, result);
    }

    /// <summary>
    /// Tests SetResponseAsync with null content task parameter.
    /// Verifies the method throws ArgumentNullException when contentTask is null.
    /// </summary>
    [Fact]
    public void SetResponseAsync_NullContentTask_ThrowsArgumentNullException()
    {
        // Arrange
        var mockArgs = Substitute.For<WebResourceRequestedEventArgs>();
        var target = new PlatformWebViewWebResourceRequestedEventArgs(mockArgs);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => target.SetResponseAsync(200, "OK", null, null));
    }

#endif

    /// <summary>
    /// Tests that the constructor accepts a valid WebResourceRequestedEventArgs parameter and successfully creates an instance.
    /// </summary>
    [Fact]
    public void Constructor_ValidWebResourceRequestedEventArgs_CreatesInstance()
    {
        // Arrange
        var args = new WebResourceRequestedEventArgs();

        // Act
        var result = new PlatformWebViewWebResourceRequestedEventArgs(args);

        // Assert
        Assert.NotNull(result);
    }

    /// <summary>
    /// Tests that the constructor accepts a null WebResourceRequestedEventArgs parameter and successfully creates an instance.
    /// </summary>
    [Fact]
    public void Constructor_NullWebResourceRequestedEventArgs_CreatesInstance()
    {
        // Arrange
        WebResourceRequestedEventArgs args = null;

        // Act
        var result = new PlatformWebViewWebResourceRequestedEventArgs(args);

        // Assert
        Assert.NotNull(result);
    }

    /// <summary>
    /// Tests that multiple instances can be created with different WebResourceRequestedEventArgs parameters.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Constructor_MultipleInstances_CreatesDistinctInstances(bool useNullArgs)
    {
        // Arrange
        var args = useNullArgs ? null : new WebResourceRequestedEventArgs();

        // Act
        var instance1 = new PlatformWebViewWebResourceRequestedEventArgs(args);
        var instance2 = new PlatformWebViewWebResourceRequestedEventArgs(args);

        // Assert
        Assert.NotNull(instance1);
        Assert.NotNull(instance2);
        Assert.NotSame(instance1, instance2);
    }
}