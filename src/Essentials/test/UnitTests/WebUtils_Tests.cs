#nullable enable
using System;
using Xunit;

namespace Tests
{
	public class WebUtils_Tests
	{
		// ============================================================
		// ResolveRelativePath — valid cases
		// ============================================================

		[Fact]
		public void ResolveRelativePath_ValidRelative_ReturnsPath()
		{
			var origin = new Uri("https://0.0.0.0/");
			var request = new Uri("https://0.0.0.0/index.html");

			var result = Microsoft.Maui.WebUtils.ResolveRelativePath(origin, request);

			Assert.NotNull(result);
			Assert.Equal("index.html", result);
		}

		[Fact]
		public void ResolveRelativePath_SubPath_ReturnsPath()
		{
			var origin = new Uri("https://0.0.0.0/");
			var request = new Uri("https://0.0.0.0/sub/file.txt");

			var result = Microsoft.Maui.WebUtils.ResolveRelativePath(origin, request);

			Assert.NotNull(result);
			Assert.Equal("sub/file.txt", result);
		}

		[Fact]
		public void ResolveRelativePath_RootRequest_ReturnsEmpty()
		{
			var origin = new Uri("https://0.0.0.0/");
			var request = new Uri("https://0.0.0.0/");

			var result = Microsoft.Maui.WebUtils.ResolveRelativePath(origin, request);

			Assert.NotNull(result);
			Assert.Equal(string.Empty, result);
		}

		// ============================================================
		// ResolveRelativePath — invalid cases
		// ============================================================

		[Fact]
		public void ResolveRelativePath_DifferentOrigin_ReturnsNull()
		{
			var origin = new Uri("https://0.0.0.0/");
			var request = new Uri("https://other.example.com/file.txt");

			var result = Microsoft.Maui.WebUtils.ResolveRelativePath(origin, request);

			Assert.Null(result);
		}

		[Fact]
		public void ResolveRelativePath_DoubleSlash_MakeRelativeUri_ProducesRooted_ReturnsNull()
		{
			// When the request has a double-slash path like //images/logo.png,
			// MakeRelativeUri can produce a path that starts with a separator,
			// which should be rejected as rooted.
			var origin = new Uri("https://0.0.0.0/");
			var request = new Uri("https://0.0.0.0//images/logo.png");

			var result = Microsoft.Maui.WebUtils.ResolveRelativePath(origin, request);

			// Either null (rejected) or a valid non-rooted path — never a rooted path
			if (result is not null)
			{
				Assert.False(System.IO.Path.IsPathRooted(result),
					$"ResolveRelativePath should not return a rooted path, got: '{result}'");
			}
		}

		// ============================================================
		// ResolveRelativePath — encoded paths
		// ============================================================

		[Fact]
		public void ResolveRelativePath_EncodedDotDot_HandledCorrectly()
		{
			var origin = new Uri("https://0.0.0.0/");
			// %2e%2e is URL-encoded ".." — Uri class may decode this
			var request = new Uri("https://0.0.0.0/%2e%2e/secret.txt");

			var result = Microsoft.Maui.WebUtils.ResolveRelativePath(origin, request);

			// Should be null (invalid) or if Uri decoded it, the result should be valid
			if (result is not null)
			{
				Assert.DoesNotContain("..", result, StringComparison.Ordinal);
			}
		}

		// ============================================================
		// RemovePossibleQueryString
		// ============================================================

		[Theory]
		[InlineData(null, "")]
		[InlineData("", "")]
		[InlineData("https://example.com", "https://example.com")]
		[InlineData("https://example.com?foo=bar", "https://example.com")]
		[InlineData("https://example.com/path?query=1&other=2", "https://example.com/path")]
		public void RemovePossibleQueryString_ReturnsExpected(string? input, string expected)
		{
			var result = Microsoft.Maui.WebUtils.RemovePossibleQueryString(input);
			Assert.Equal(expected, result);
		}
	}
}
