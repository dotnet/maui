using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Graphics.Tests
{
	public class ColorTypeConverterTests
	{

		[Theory]
		[MemberData(nameof(ColorConvertData))]
		public void ConvertsFromString(string from, Color expected)
		{
			Assert.Equal(expected, Color.Parse(from));
		}

		// Supported inputs
		// HEX		#rgb, #argb, #rrggbb, #aarrggbb
		// RGB		rgb(255,0,0), rgb(100%,0%,0%)					values in range 0-255 or 0%-100%
		// RGBA		rgba(255, 0, 0, 0.8), rgba(100%, 0%, 0%, 0.8)	opacity is 0.0-1.0
		// HSL		hsl(120, 100%, 50%)								h is 0-360, s and l are 0%-100%
		// HSLA		hsla(120, 100%, 50%, .8)						opacity is 0.0-1.0
		// HSV		hsv(120, 100%, 50%)								h is 0-360, s and v are 0%-100%
		// HSVA		hsva(120, 100%, 50%, .8)						opacity is 0.0-1.0
		// Predefined color											case insensitive
		public static IEnumerable<object[]> ColorConvertData()
			=> new List<object[]>
			{
				new object[] { "#000000", new Color() },
				new object[] { "#ff000000", new Color() },
				new object[] { "Black", new Color() },
				new object[] { "black", new Color() },
				new object[] { "rgb(0,0,0)", new Color() },
				new object[] { "rgba(0,0,0,255)", new Color() },
				new object[] { "rgba(0,0,0,0)", Colors.Transparent },
				new object[] { "hsl(0,0,0)", new Color() },
				new object[] { "hsla(0,0,0,1)", new Color() },
				new object[] { "hsla(0,0,0,0)", Colors.Transparent },
				new object[] { "hsv(0,0,0)", new Color() },
				new object[] { "hsva(0,0,0,1)", new Color() },
				new object[] { "hsva(0,0,0,0)", Colors.Transparent },

				new object[] { "hsl(253,66,50)", Color.FromArgb("#512BD4") },
				new object[] { "hsv(253,80,83)", Color.FromArgb("#512BD4") },
				new object[] { "rgb(81,43,212)", Color.FromArgb("#512BD4") },
			};

		// Ensures all the named color fields are represented in the color type converter
		// which uses hard coded strings for better perf
		// So let's use a test that does reflection to make sure we don't forget to add
		// any to the hard coded list if we add more fields
		[Fact]
		public void ConvertStandardValuesAreComplete()
		{
			var colorTypeConverter = new Converters.ColorTypeConverter();

			var standardValues = colorTypeConverter.GetStandardValues().Cast<string>().ToList();

			var namedColors = typeof(Colors)
				.GetFields()
				.Where(f => f.FieldType == typeof(Color))
				.Select(f => f.Name);

			foreach (var namedColor in namedColors)
				Assert.Contains(standardValues, c => c.Equals(namedColor, StringComparison.OrdinalIgnoreCase));
		}
	}
}
