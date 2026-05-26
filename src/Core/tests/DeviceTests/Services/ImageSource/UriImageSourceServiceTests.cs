using System;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ImageSource)]
	public partial class UriImageSourceServiceTests : BaseImageSourceServiceTests
	{
		[Fact]
		// Regression test for runtime issue https://github.com/dotnet/runtime/issues/111506
		// Ensures that String.Replace() with StringComparison.OrdinalIgnoreCase works correctly in .NET 10.0
		// This issue affected the CachedFilenameIsCorrectAndValid test on all platforms.
		public void StringReplaceWithOrdinalIgnoreCaseWorksCorrectly()
		{
			// This specific operation was causing "IndexOf with matchLength" errors in earlier .NET versions
			var template = "test{hash}file.png";
			var hash = "abc123";

			// This should not throw an exception
			var result = template.Replace("{hash}", hash, StringComparison.OrdinalIgnoreCase);

			Assert.Equal("testabc123file.png", result);
		}

		[Fact]
		// Tests String.Replace with multiple occurrences of the search string
		public void StringReplaceWithMultipleOccurrences()
		{
			var template = "{HASH}test{HASH}file{HASH}.png";
			var replacement = "abc123";
			var result = template.Replace("{hash}", replacement, StringComparison.OrdinalIgnoreCase);
			Assert.Equal("abc123testabc123fileabc123.png", result);
		}

		[Fact]
		// Tests String.Replace with special characters in template and replacement
		public void StringReplaceWithSpecialCharacters()
		{
			var template = "test{HASH}@special#file.png";
			var replacement = "!@#$%^";
			var result = template.Replace("{hash}", replacement, StringComparison.OrdinalIgnoreCase);
			Assert.Equal("test!@#$%^@special#file.png", result);
		}

		[Fact]
		// Tests String.Replace with a long template and replacement
		public void StringReplaceWithLongStrings()
		{
			var template = "start{HASH}" + new string('x', 1000) + "end.png";
			var replacement = new string('y', 500);
			var result = template.Replace("{hash}", replacement, StringComparison.OrdinalIgnoreCase);
			Assert.Equal("start" + new string('y', 500) + new string('x', 1000) + "end.png", result);
		}
	}
}