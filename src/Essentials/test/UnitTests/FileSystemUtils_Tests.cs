#nullable enable
using System;
using System.IO;
using Microsoft.Maui.Storage;
using Xunit;

namespace Tests
{
	public class FileSystemUtils_Tests
	{
		// ============================================================
		// IsValidRelativePath
		// ============================================================

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		public void IsValidRelativePath_NullOrEmpty_ReturnsTrue(string? path)
		{
			Assert.True(FileSystemUtils.IsValidRelativePath(path));
		}

		[Theory]
		[InlineData("file.txt")]
		[InlineData("sub/file.txt")]
		[InlineData("sub/deep/file.txt")]
		[InlineData("./file.txt")]
		[InlineData("sub/./file.txt")]
		public void IsValidRelativePath_ValidRelative_ReturnsTrue(string path)
		{
			Assert.True(FileSystemUtils.IsValidRelativePath(path));
		}

		[Theory]
		[InlineData("foo..bar.js")]
		[InlineData("image..png")]
		[InlineData("name...ext")]
		[InlineData("a..b/c..d")]
		public void IsValidRelativePath_DoubleDotInFilename_ReturnsTrue(string path)
		{
			// ".." inside a filename is not a path segment — should be allowed
			Assert.True(FileSystemUtils.IsValidRelativePath(path));
		}

		[Theory]
		[InlineData("../file.txt")]
		[InlineData("../../file.txt")]
		[InlineData("sub/../file.txt")]
		[InlineData("sub/../../file.txt")]
		[InlineData("..\\file.txt")]
		[InlineData("sub\\..\\file.txt")]
		public void IsValidRelativePath_DotDotSegment_ReturnsFalse(string path)
		{
			Assert.False(FileSystemUtils.IsValidRelativePath(path));
		}

		[Theory]
		[InlineData("/file.txt")]
		[InlineData("//file.txt")]
		[InlineData("///file.txt")]
		public void IsValidRelativePath_RootedPath_ReturnsFalse(string path)
		{
			Assert.False(FileSystemUtils.IsValidRelativePath(path));
		}

		[Fact]
		public void IsValidRelativePath_WindowsDriveLetter_ReturnsFalse()
		{
			if (!OperatingSystem.IsWindows())
				return; // Path.IsPathRooted behaves differently on non-Windows

			Assert.False(FileSystemUtils.IsValidRelativePath("C:\\file.txt"));
			Assert.False(FileSystemUtils.IsValidRelativePath("D:/file.txt"));
		}

		// ============================================================
		// Combine — with absolute root directory
		// ============================================================

		[Fact]
		public void Combine_AbsoluteRoot_ValidRelative_ReturnsFullPath()
		{
			var root = Path.GetTempPath();
			var result = FileSystemUtils.Combine(root, "sub/file.txt");

			Assert.NotNull(result);
			Assert.True(Path.IsPathRooted(result));
			Assert.StartsWith(root, result, StringComparison.Ordinal);
			Assert.EndsWith("file.txt", result, StringComparison.Ordinal);
		}

		[Theory]
		[InlineData("../file.txt")]
		[InlineData("../../file.txt")]
		[InlineData("sub/../../file.txt")]
		public void Combine_AbsoluteRoot_DotDotAboveRoot_ReturnsNull(string relativePath)
		{
			var root = Path.Combine(Path.GetTempPath(), "testroot", "content");
			var result = FileSystemUtils.Combine(root, relativePath);

			Assert.Null(result);
		}

		[Theory]
		[InlineData("/etc/config.txt")]
		[InlineData("//images/logo.png")]
		[InlineData("///data/readme.txt")]
		public void Combine_AbsoluteRoot_RootedRelative_ReturnsNull(string relativePath)
		{
			var root = Path.Combine(Path.GetTempPath(), "testroot");
			var result = FileSystemUtils.Combine(root, relativePath);

			Assert.Null(result);
		}

		[Fact]
		public void Combine_AbsoluteRoot_EmptyRelative_ReturnsRoot()
		{
			var root = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);
			var result = FileSystemUtils.Combine(root, "");

			// Empty relative path combined with root should return a path within root
			Assert.NotNull(result);
		}

		[Fact]
		public void Combine_AbsoluteRoot_SingleDot_ReturnsPathWithinRoot()
		{
			var root = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);
			var result = FileSystemUtils.Combine(root, "./file.txt");

			Assert.NotNull(result);
			Assert.EndsWith("file.txt", result, StringComparison.Ordinal);
		}

		[Fact]
		public void Combine_AbsoluteRoot_WindowsCaseSensitivity()
		{
			if (!OperatingSystem.IsWindows())
				return;

			// On Windows, paths are case-insensitive, so even if GetFullPath returns
			// a different case, the boundary check should still pass.
			var root = Path.Combine(Path.GetTempPath(), "TestRoot");
			var result = FileSystemUtils.Combine(root, "sub/file.txt");

			Assert.NotNull(result);
		}

		// ============================================================
		// Combine — with relative root directory (Android/package paths)
		// ============================================================

		[Fact]
		public void Combine_RelativeRoot_ValidRelative_ReturnsRelativePath()
		{
			var result = FileSystemUtils.Combine("wwwroot", "index.html");

			Assert.NotNull(result);
			// Should NOT be absolute — should stay relative for package/asset APIs
			Assert.False(Path.IsPathRooted(result));
			Assert.Contains("wwwroot", result, StringComparison.Ordinal);
			Assert.Contains("index.html", result, StringComparison.Ordinal);
		}

		[Fact]
		public void Combine_RelativeRoot_SubPath_PreservesRelativeStructure()
		{
			var result = FileSystemUtils.Combine("HybridTestRoot", "sub/file.txt");

			Assert.NotNull(result);
			Assert.False(Path.IsPathRooted(result));
		}

		[Theory]
		[InlineData("../file.txt")]
		[InlineData("../../file.txt")]
		[InlineData("sub/../../file.txt")]
		public void Combine_RelativeRoot_DotDotAboveRoot_ReturnsNull(string relativePath)
		{
			var result = FileSystemUtils.Combine("wwwroot", relativePath);

			Assert.Null(result);
		}

		[Theory]
		[InlineData("/etc/config.txt")]
		[InlineData("//images/logo.png")]
		public void Combine_RelativeRoot_RootedRelative_ReturnsNull(string relativePath)
		{
			var result = FileSystemUtils.Combine("wwwroot", relativePath);

			Assert.Null(result);
		}

		[Fact]
		public void Combine_RelativeRoot_NormalizesSlashes()
		{
			var result = FileSystemUtils.Combine("wwwroot", "sub/file.txt");

			Assert.NotNull(result);
			// Verify slashes are normalized for current platform
			if (Path.DirectorySeparatorChar == '/')
				Assert.DoesNotContain("\\", result, StringComparison.Ordinal);
			else
				Assert.DoesNotContain("/", result, StringComparison.Ordinal);
		}

		// ============================================================
		// Combine — additional edge cases
		// ============================================================

		[Theory]
		[InlineData("..\\readme.txt")]
		[InlineData("..\\..\\data\\config.txt")]
		[InlineData("subfolder\\..\\..\\readme.txt")]
		public void Combine_BackslashDotDot_ReturnsNull(string relativePath)
		{
			var root = Path.Combine(Path.GetTempPath(), "testroot");
			var result = FileSystemUtils.Combine(root, relativePath);

			Assert.Null(result);
		}

		[Theory]
		[InlineData("///127.0.0.1/share/data.txt")]
		[InlineData("///localhost/C$/data.txt")]
		public void Combine_NetworkStylePath_ReturnsNull(string relativePath)
		{
			var root = Path.Combine(Path.GetTempPath(), "testroot");
			var result = FileSystemUtils.Combine(root, relativePath);

			Assert.Null(result);
		}

		[Fact]
		public void Combine_WindowsDriveLetter_ReturnsNull()
		{
			if (!OperatingSystem.IsWindows())
				return;

			var root = Path.Combine(Path.GetTempPath(), "testroot");
			Assert.Null(FileSystemUtils.Combine(root, "C:\\Windows\\notepad.exe"));
			Assert.Null(FileSystemUtils.Combine(root, "D:/projects/readme.md"));
		}

		// ============================================================
		// Combine — filenames containing ".." that are NOT path segments
		// ============================================================

		[Theory]
		[InlineData("foo..bar.js")]
		[InlineData("image..png")]
		[InlineData("a..b/c..d.txt")]
		public void Combine_DoubleDotInFilename_Succeeds(string relativePath)
		{
			var root = Path.Combine(Path.GetTempPath(), "testroot");
			var result = FileSystemUtils.Combine(root, relativePath);

			Assert.NotNull(result);
		}

		// ============================================================
		// NormalizePath
		// ============================================================

		[Fact]
		public void NormalizePath_ReplacesForwardAndBackSlashes()
		{
			var result = FileSystemUtils.NormalizePath("a/b\\c");
			var expected = $"a{Path.DirectorySeparatorChar}b{Path.DirectorySeparatorChar}c";
			Assert.Equal(expected, result);
		}
	}
}
