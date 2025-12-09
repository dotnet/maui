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
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("\t")]
		[InlineData("   ")]
		public void GetSecurePath_EmptyOrWhitespaceRelativePath_ThrowsArgumentException(string relativePath)
		{
			// Arrange
			var basePath = Path.GetTempPath();

			// Act & Assert
			var ex = Assert.Throws<ArgumentException>(() => FileSystemUtils.GetSecurePath(basePath, relativePath));
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

		[Fact]
		public void GetSecurePath_AbsolutePath_OutsideBase_ThrowsUnauthorizedAccessException()
		{
			// Arrange
			var basePath = Path.Combine(Path.GetTempPath(), "app_data");
			
			// On Windows, use a different drive or root; on Unix, use root
			var absolutePath = OperatingSystem.IsWindows() 
				? @"C:\Windows\System32\config.sys" 
				: "/etc/passwd";

			// Act & Assert
			Assert.Throws<UnauthorizedAccessException>(() => FileSystemUtils.GetSecurePath(basePath, absolutePath));
		}

		[Theory]
		[InlineData("folder with spaces/file.txt")]
		[InlineData("folder/./file.txt")]
		[InlineData("folder/../folder/file.txt")]
		[InlineData("folder//file.txt")]
		public void GetSecurePath_SpecialPathCases_HandlesCorrectly(string relativePath)
		{
			// Arrange
			var basePath = Path.GetTempPath();

			// Act
			var result = FileSystemUtils.GetSecurePath(basePath, relativePath);

			// Assert
			Assert.StartsWith(Path.GetFullPath(basePath), result, StringComparison.OrdinalIgnoreCase);
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

		[Fact]
		public void GetSecurePath_UsesPlatformAppropriateCaseSensitivity()
		{
			// Arrange
			var basePath = Path.GetTempPath();
			var subPath = "TestFolder/file.txt";
			
			// Act
			var result = FileSystemUtils.GetSecurePath(basePath, subPath);

			// Assert
			// On Windows, paths are case-insensitive; on Unix, they are case-sensitive
			// The result should always be within the base path using platform-appropriate comparison
			var expectedComparison = OperatingSystem.IsWindows()
				? StringComparison.OrdinalIgnoreCase
				: StringComparison.Ordinal;
			
			Assert.StartsWith(Path.GetFullPath(basePath), result, expectedComparison);
		}

		[Fact]
		public void GetSecurePath_CaseVariations_WorksWithinSameDirectory()
		{
			// This test verifies that case variations in the path are handled correctly
			// On Windows, "Folder" and "folder" are the same; on Unix, they're different
			var basePath = Path.GetTempPath();
			
			// Act - both should succeed as they stay within base directory
			var result1 = FileSystemUtils.GetSecurePath(basePath, "folder/file.txt");
			var result2 = FileSystemUtils.GetSecurePath(basePath, "Folder/file.txt");

			// Assert - both paths should be valid and within base
			Assert.StartsWith(Path.GetFullPath(basePath), result1, StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith(Path.GetFullPath(basePath), result2, StringComparison.OrdinalIgnoreCase);
		}
	}
}
