using System.IO;
using SkiaSharp;
using Xunit;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class UtilsTests
	{
		public class ParseColorString
		{
			[Theory]
			[InlineData("#abcdef", 0xffabcdef)]
			[InlineData("#12345678", 0x12345678)]
			[InlineData("12345678", 0x12345678)]
			public void ParsesHexValues(string hex, uint argb)
			{
				var parsed = Utils.ParseColorString(hex);

				Assert.NotNull(parsed);
				Assert.Equal(argb, parsed.Value);
			}

			[Theory]
			[InlineData("Red", 0xFFFF0000)]
			[InlineData("Green", 0xFF008000)]
			[InlineData("Blue", 0xFF0000FF)]
			[InlineData("Grey", 0xFF808080)]
			[InlineData("Gray", 0xFF808080)]
			[InlineData("LightSlateGrey", 0xFF778899)]
			[InlineData("LightSlateGray", 0xFF778899)]
			public void ParsesNamedColors(string name, uint argb)
			{
				var parsed = Utils.ParseColorString(name);

				Assert.NotNull(parsed);
				Assert.Equal(argb, parsed.Value);
			}
		}

		public class ParseSizeString
		{
			[Theory]
			[InlineData("1,2")]
			[InlineData("1;2")]
			public void ParsesHexValues(string hex)
			{
				var parsed = Utils.ParseSizeString(hex);

				Assert.NotNull(parsed);
				Assert.Equal(new SKSize(1, 2), parsed);
			}

			[Theory]
			[InlineData(null)]
			[InlineData("")]
			public void SupportsEmptyNull(string hex)
			{
				var parsed = Utils.ParseSizeString(hex);

				Assert.Null(parsed);
			}
		}

		public class RetryWhileFileIsLocked
		{
			[Fact]
			public void PropagatesOtherIOErrorsWithoutRetrying()
			{
				var calls = 0;
				var error = new DirectoryNotFoundException();

				var thrown = Assert.Throws<DirectoryNotFoundException>(() =>
					Utils.RetryWhileFileIsLocked(() =>
					{
						calls++;
						throw error;
					}, attempts: 3, delay: 0));

				Assert.Same(error, thrown);
				Assert.Equal(1, calls);
			}

			[Fact]
			public void PropagatesLockThatOutlastsAllRetries()
			{
				var calls = 0;

				Assert.Throws<IOException>(() =>
					Utils.RetryWhileFileIsLocked(() =>
					{
						calls++;
						throw new IOException("locked", SkiaSharpTools.ERROR_SHARING_VIOLATION);
					}, attempts: 3, delay: 0));

				Assert.Equal(4, calls);
			}
		}
	}
}
