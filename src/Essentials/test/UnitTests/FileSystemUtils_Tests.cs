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

		// ============================================================
		// EnsureFileName
		// ============================================================

		[Theory]
		[InlineData("photo.jpg", null, "photo.jpg")]
		[InlineData("document.pdf", ".pdf", "document.pdf")]
		[InlineData("readme.txt", ".doc", "readme.txt")]
		public void EnsureFileName_SimpleNameWithExtension_ReturnsUnchanged(string displayName, string? extension, string expected)
		{
			var result = FileSystemUtils.EnsureFileName(displayName, extension);
			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData("screenshot_12345", ".png", "screenshot_12345.png")]
		[InlineData("download", ".pdf", "download.pdf")]
		[InlineData("photo", "jpg", "photo.jpg")]
		public void EnsureFileName_NoExtension_AppendsExtension(string displayName, string extension, string expected)
		{
			var result = FileSystemUtils.EnsureFileName(displayName, extension);
			Assert.Equal(expected, result);
		}

		[Fact]
		public void EnsureFileName_NoExtensionAndNoKnownExtension_ReturnsAsIs()
		{
			var result = FileSystemUtils.EnsureFileName("screenshot_12345", null);
			Assert.Equal("screenshot_12345", result);
		}

		[Theory]
		[InlineData("/storage/emulated/0/DCIM/photo.jpg", null, "photo.jpg")]
		[InlineData("/data/user/0/com.app/files/doc.pdf", ".pdf", "doc.pdf")]
		[InlineData("some/relative/path/image.png", null, "image.png")]
		public void EnsureFileName_PathWithDirectories_StripsToFilename(string displayName, string? extension, string expected)
		{
			var result = FileSystemUtils.EnsureFileName(displayName, extension);
			Assert.Equal(expected, result);
		}

		[Fact]
		public void EnsureFileName_PathWithDirectoriesNoExtension_AppendsExtension()
		{
			var result = FileSystemUtils.EnsureFileName("/storage/emulated/0/DCIM/screenshot", ".png");
			Assert.Equal("screenshot.png", result);
		}

		[Fact]
		public void EnsureFileName_Null_GeneratesGuid()
		{
			var result = FileSystemUtils.EnsureFileName(null, ".jpg");
			Assert.NotNull(result);
			Assert.EndsWith(".jpg", result, StringComparison.Ordinal);
			// GUID "N" format is 32 hex chars; with extension it's 32 + ".jpg" = 36
			Assert.True(result.Length > 4);
		}

		[Theory]
		[InlineData("")]
		[InlineData("   ")]
		public void EnsureFileName_EmptyOrWhitespace_GeneratesGuid(string displayName)
		{
			var result = FileSystemUtils.EnsureFileName(displayName, ".pdf");
			Assert.NotNull(result);
			Assert.EndsWith(".pdf", result, StringComparison.Ordinal);
			Assert.True(result.Length > 4);
		}

		[Fact]
		public void EnsureFileName_TrailingSeparator_GeneratesGuid()
		{
			// Path.GetFileName("foo/") returns "" — guard against that
			var result = FileSystemUtils.EnsureFileName("foo/", ".txt");
			Assert.NotNull(result);
			Assert.EndsWith(".txt", result, StringComparison.Ordinal);
			Assert.True(result.Length > 4);
		}

		[Theory]
		[InlineData(".")]
		[InlineData("..")]
		public void EnsureFileName_DotOrDotDot_GeneratesGuid(string displayName)
		{
			// "." and ".." would resolve to parent dirs in Java.IO.File(parent, child)
			var result = FileSystemUtils.EnsureFileName(displayName, ".jpg");
			Assert.NotNull(result);
			Assert.EndsWith(".jpg", result, StringComparison.Ordinal);
			Assert.NotEqual(".", result);
			Assert.NotEqual("..", result);
			Assert.True(result.Length > 4);
		}

		[Fact]
		public void EnsureFileName_EmptyExtension_DoesNotAppend()
		{
			var result = FileSystemUtils.EnsureFileName("noext", "");
			Assert.Equal("noext", result);
		}
	}
}
