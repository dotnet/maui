using System.Drawing;
using Xunit;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class UtilsTests
	{
		public class ParseColorString
		{
			[Theory]
			[InlineData("#12345678")]
			[InlineData("12345678")]
			public void ParsesHexValues(string hex)
			{
				var parsed = Utils.ParseColorString(hex);

				Assert.NotNull(parsed);
				Assert.Equal(0x12345678, parsed?.ToArgb());
			}

			[Fact]
			public void ParsesNamedColors()
			{
				var parsed = Utils.ParseColorString("Red");

				Assert.NotNull(parsed);
				Assert.Equal(0xFFFF0000, unchecked((uint)parsed?.ToArgb()));
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
				Assert.Equal(new Size(1, 2), parsed);
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
	}
}
