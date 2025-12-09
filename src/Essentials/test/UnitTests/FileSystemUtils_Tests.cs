using System;
using System.IO;
using Microsoft.Maui.Storage;
using Xunit;

namespace Tests
{
	public class FileSystemUtils_Tests
	{
		[Theory]
		[InlineData("test.txt")]
		[InlineData("subfolder/test.txt")]
		[InlineData("subfolder\\test.txt")]
		[InlineData("deep/nested/folder/test.txt")]
		public void GetSecurePath_ValidRelativePath_ReturnsFullPath(string relativePath)
		{
			// Arrange
			var basePath = Path.GetTempPath();

			// Act
			var result = FileSystemUtils.GetSecurePath(basePath, relativePath);

			// Assert
			Assert.StartsWith(Path.GetFullPath(basePath), result, StringComparison.OrdinalIgnoreCase);
		}

		[Theory]
		[InlineData("../test.txt")]
		[InlineData("..\\test.txt")]
		[InlineData("../../test.txt")]
		[InlineData("subfolder/../../test.txt")]
		[InlineData("subfolder\\..\\..\\test.txt")]
		[InlineData("../../../../../../../etc/passwd")]
		[InlineData("..\\..\\..\\..\\..\\..\\..\\Windows\\System32\\config\\SAM")]
		public void GetSecurePath_PathTraversal_ThrowsUnauthorizedAccessException(string maliciousPath)
		{
			// Arrange
			var basePath = Path.Combine(Path.GetTempPath(), "app_data");

			// Act & Assert
			Assert.Throws<UnauthorizedAccessException>(() => FileSystemUtils.GetSecurePath(basePath, maliciousPath));
		}

		[Fact]
		public void GetSecurePath_NullBasePath_ThrowsArgumentNullException()
		{
			// Act & Assert
			var ex = Assert.Throws<ArgumentNullException>(() => FileSystemUtils.GetSecurePath(null!, "test.txt"));
			Assert.Equal("basePath", ex.ParamName);
		}

		[Fact]
		public void GetSecurePath_NullRelativePath_ThrowsArgumentNullException()
		{
			// Arrange
			var basePath = Path.GetTempPath();

			// Act & Assert
			var ex = Assert.Throws<ArgumentNullException>(() => FileSystemUtils.GetSecurePath(basePath, null!));
			Assert.Equal("relativePath", ex.ParamName);
		}

		[Theory]
		[InlineData("test.txt")]
		[InlineData("test/file.txt")]
		[InlineData("test\\file.txt")]
		public void GetSecurePath_NormalizesPathSeparators(string relativePath)
		{
			// Arrange
			var basePath = Path.GetTempPath();

			// Act
			var result = FileSystemUtils.GetSecurePath(basePath, relativePath);

			// Assert
			// Path should be fully normalized with platform-specific separators
			Assert.DoesNotContain(Path.DirectorySeparatorChar == '/' ? "\\" : "/", result, StringComparison.Ordinal);
		}

		[Fact]
		public void GetSecurePath_PathWithinBaseDirectory_ButSimilarPrefix_ThrowsException()
		{
			// This tests that "/app/data_other" doesn't match when base is "/app/data"
			// because we need to ensure proper directory boundary checking
			var basePath = Path.Combine(Path.GetTempPath(), "app", "data");
			var maliciousPath = "../data_other/secrets.txt";

			// Act & Assert
			Assert.Throws<UnauthorizedAccessException>(() => FileSystemUtils.GetSecurePath(basePath, maliciousPath));
		}

		[Fact]
		public void GetSecurePath_SubdirectoryPath_ReturnsValidPath()
		{
			// Testing that a valid subdirectory path works correctly
			var basePath = Path.GetTempPath();
			var subPath = "myapp/data/file.txt";
			
			// Act
			var result = FileSystemUtils.GetSecurePath(basePath, subPath);

			// Assert
			var expectedPath = Path.GetFullPath(Path.Combine(basePath, subPath));
			Assert.Equal(expectedPath, result);
		}

		[Theory]
		[InlineData("path/to/file.txt")]
		[InlineData("path\\to\\file.txt")]
		[InlineData("path/to\\mixed/slashes\\file.txt")]
		public void NormalizePath_ConvertsAllSlashesToPlatformSpecific(string input)
		{
			// Act
			var result = FileSystemUtils.NormalizePath(input);

			// Assert
			Assert.DoesNotContain(Path.DirectorySeparatorChar == '/' ? "\\" : "/", result, StringComparison.Ordinal);
		}
	}
}
