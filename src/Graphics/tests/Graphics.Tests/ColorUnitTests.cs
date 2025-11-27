using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Graphics.Tests
{
	public class ColorUnitTests
	{
		[Fact]
		public void TestHSLPostSetEquality()
		{
			var color = new Color(1, 0.5f, 0.2f);
			var color2 = color;

			color2 = color.WithLuminosity(.2f);
			Assert.False(color == color2);
		}

		[Fact]
		public void TestHSLPostSetInequality()
		{
			var color = new Color(1, 0.5f, 0.2f);
			var color2 = color;

			color2 = color.WithLuminosity(.2f);

			Assert.True(color != color2);
		}

		[Fact]
		public void TestHSLSetToDefaultValue()
		{
			var color = new Color(0.2f, 0.5f, 0.8f);

			// saturation is initialized to 0, make sure we still update
			color = color.WithSaturation(0);

			Assert.Equal(color.Red, color.Green);
			Assert.Equal(color.Red, color.Blue);
		}

		[Fact]
		public void TestHSLModifiers()
		{
			var color = Color.FromHsla(.8f, .6f, .2f);
			Assert.Equal(Color.FromHsla(.1f, .6f, .2f), color.WithHue(.1f));
			Assert.Equal(Color.FromHsla(.8f, .1f, .2f), color.WithSaturation(.1f));
			Assert.Equal(Color.FromHsla(.8f, .6f, .1f), color.WithLuminosity(.1f));
		}

		[Fact]
		public void TestMultiplyAlpha()
		{
			var color = new Color(1f, 1f, 1f, 1f);
			color = color.MultiplyAlpha(0.25f);
			Assert.Equal(.25, color.Alpha);

			color = Color.FromHsla(1f, 1f, 1f, 1f);
			color = color.MultiplyAlpha(0.25f);
			Assert.Equal(.25, color.Alpha);
		}

		[Fact]
		public void TestClamping()
		{
			var color = new Color(2f, 2f, 2f, 2f);

			Assert.Equal(1, color.Red);
			Assert.Equal(1, color.Green);
			Assert.Equal(1, color.Blue);
			Assert.Equal(1, color.Alpha);

			color = new Color(-1f, -1f, -1f, -1f);

			Assert.Equal(0, color.Red);
			Assert.Equal(0, color.Green);
			Assert.Equal(0, color.Blue);
			Assert.Equal(0, color.Alpha);
		}

		[Fact]
		public void TestRGBToHSL()
		{
			var color = new Color(.5f, .1f, .1f);

			Assert.Equal((float)1, color.GetHue(), (float)3);
			Assert.Equal(0.662, color.GetSaturation(), 1);
			Assert.Equal(0.302, color.GetLuminosity(), 1);
		}

		[Fact]
		public void TestHSLToRGB()
		{
			var color = Color.FromHsla(0, .662, .302);

			Assert.Equal(0.5, color.Red, 2);
			Assert.Equal(0.1, color.Green, 2);
			Assert.Equal(0.1, color.Blue, 2);
		}

		[Fact]
		public void TestColorFromValue()
		{
			var color = new Color(0.2f);

			Assert.Equal(new Color(0.2f, 0.2f, 0.2f, 1), color);
		}

		[Fact]
		public void TestAddLuminosity()
		{
			var color = new Color(0.2f);
			var brighter = color.AddLuminosity(0.2f);
			Assert.Equal(brighter.GetLuminosity(), color.GetLuminosity() + 0.2, 3);
		}

		[Fact]
		public void TestZeroLuminosity()
		{
			var color = new Color(0.1f, 0.2f, 0.3f);
			color = color.AddLuminosity(-1);

			Assert.Equal(0, color.GetLuminosity());
			Assert.Equal(0, color.Red);
			Assert.Equal(0, color.Green);
			Assert.Equal(0, color.Blue);
		}

		[Fact]
		public void TestHashCode()
		{
			var color1 = new Color(0.1f);
			var color2 = new Color(0.1f);

			Assert.True(color1.GetHashCode() == color2.GetHashCode());
		}

		[Fact]
		public void TestHashCodeNamedColors()
		{
			Color red = Colors.Red; //R=1, G=0, B=0, A=1
			int hashRed = red.GetHashCode();

			Color blue = Colors.Blue; //R=0, G=0, B=1, A=1
			int hashBlue = blue.GetHashCode();

			Assert.False(hashRed == hashBlue);
		}

		[Fact]
		public void TestHashCodeAll()
		{
			Dictionary<int, Color> colorsAndHashes = new Dictionary<int, Color>();
			colorsAndHashes.Add(Colors.Transparent.GetHashCode(), Colors.Transparent);
			colorsAndHashes.Add(Colors.Aqua.GetHashCode(), Colors.Aqua);
			colorsAndHashes.Add(Colors.Black.GetHashCode(), Colors.Black);
			colorsAndHashes.Add(Colors.Blue.GetHashCode(), Colors.Blue);
			colorsAndHashes.Add(Colors.Fuchsia.GetHashCode(), Colors.Fuchsia);
			colorsAndHashes.Add(Colors.Gray.GetHashCode(), Colors.Gray);
			colorsAndHashes.Add(Colors.Green.GetHashCode(), Colors.Green);
			colorsAndHashes.Add(Colors.Lime.GetHashCode(), Colors.Lime);
			colorsAndHashes.Add(Colors.Maroon.GetHashCode(), Colors.Maroon);
			colorsAndHashes.Add(Colors.Navy.GetHashCode(), Colors.Navy);
			colorsAndHashes.Add(Colors.Olive.GetHashCode(), Colors.Olive);
			colorsAndHashes.Add(Colors.Purple.GetHashCode(), Colors.Purple);
			colorsAndHashes.Add(Colors.Pink.GetHashCode(), Colors.Pink);
			colorsAndHashes.Add(Colors.Red.GetHashCode(), Colors.Red);
			colorsAndHashes.Add(Colors.Silver.GetHashCode(), Colors.Silver);
			colorsAndHashes.Add(Colors.Teal.GetHashCode(), Colors.Teal);
			colorsAndHashes.Add(Colors.Yellow.GetHashCode(), Colors.Yellow);
		}

		[Fact]
		public void TestSetHue()
		{
			var color = new Color(0.2f, 0.5f, 0.7f);
			color = Color.FromHsla(.2f, color.GetSaturation(), color.GetLuminosity());

			Assert.Equal(0.6f, color.Red, 3f);
			Assert.Equal(0.7f, color.Green, 3f);
			Assert.Equal(0.2f, color.Blue, 3f);
		}

		[Fact]
		public void ZeroLuminToRGB()
		{
			var color = new Color(0);
			Assert.Equal(0, color.GetLuminosity());
			Assert.Equal(0, color.GetHue());
			Assert.Equal(0, color.GetSaturation());
		}

		[Fact]
		public void TestToString()
		{
			var color = new Color(1, 1, 1, 0.5f);
			Assert.Equal($"[Color: Red=1, Green=1, Blue=1, Alpha=0.5]", color.ToString());
		}

		[Fact]
		public void TestFromHex()
		{
			var color = Color.FromRgb(138, 43, 226);
			Assert.Equal(color, Color.FromArgb("8a2be2"));

			Assert.Equal(Color.FromRgba(138, 43, 226, 128), Color.FromArgb("#808a2be2"));
			Assert.Equal(Color.FromArgb("#aabbcc"), Color.FromArgb("#abc"));
			Assert.Equal(Color.FromArgb("#aabbccdd"), Color.FromArgb("#abcd"));
		}

		[Fact]
		public void TestToHex()
		{
			var colorRgb = Color.FromRgb(138, 43, 226);
			Assert.Equal(Color.FromArgb(colorRgb.ToArgbHex()), colorRgb);
			var colorRgba = Color.FromRgba(138, 43, 226, .2);
			Assert.Equal(Color.FromArgb(colorRgba.ToArgbHex()), colorRgba);
			var colorHsl = Color.FromHsla(240, 1, 1);
			Assert.Equal(Color.FromArgb(colorHsl.ToArgbHex()), colorHsl);
			var colorHsla = Color.FromHsla(240, 1, 1, .1f);
			var hexFromHsla = Color.FromArgb(colorHsla.ToArgbHex());
			Assert.Equal(hexFromHsla.Alpha, colorHsla.Alpha, 2f);
			Assert.Equal(hexFromHsla.Red, colorHsla.Red, 3f);
			Assert.Equal(hexFromHsla.Green, colorHsla.Green, 3f);
			Assert.Equal(hexFromHsla.Blue, colorHsla.Blue, 3f);
		}

		[Fact]
		public void TestFromHsv()
		{
			var color = Color.FromRgb(1, .29f, .752f);
			var colorHsv = Color.FromHsv(321, 71, 100);
			Assert.Equal(color.Red, colorHsv.Red, 3f);
			Assert.Equal(color.Green, colorHsv.Green, 3f);
			Assert.Equal(color.Blue, colorHsv.Blue, 3f);
		}

		[Fact]
		public void TestFromHsva()
		{
			var color = Color.FromRgba(1, .29, .752, .5);
			var colorHsv = Color.FromHsva(321, 71, 100, 50);
			Assert.Equal(color.Red, colorHsv.Red, 3f);
			Assert.Equal(color.Green, colorHsv.Green, 3f);
			Assert.Equal(color.Blue, colorHsv.Blue, 3f);
			Assert.Equal(color.Alpha, colorHsv.Alpha, 3f);
		}

		[Fact]
		public void TestFromHsvDouble()
		{
			var color = Color.FromRgb(1, .29f, .758f);
			var colorHsv = Color.FromHsv(.89f, .71f, 1);
			Assert.Equal(color.Red, colorHsv.Red, 2f);
			Assert.Equal(color.Green, colorHsv.Green, 2f);
			Assert.Equal(color.Blue, colorHsv.Blue, 2f);
		}

		[Fact]
		public void TestFromHsvaDouble()
		{
			var color = Color.FromRgba(1, .29, .758, .5);
			var colorHsv = Color.FromHsva(.89f, .71f, 1f, .5f);
			Assert.Equal(color.Red, colorHsv.Red, 2f);
			Assert.Equal(color.Green, colorHsv.Green, 2f);
			Assert.Equal(color.Blue, colorHsv.Blue, 2f);
			Assert.Equal(color.Alpha, colorHsv.Alpha, 2f);
		}

		[Fact]
		public void FromRGBDouble()
		{
			var color = Color.FromRgb(0.2, 0.3, 0.4);

			Assert.Equal(new Color(0.2f, 0.3f, 0.4f), color);
		}

		[Fact]
		public void FromRGBADouble()
		{
			var color = Color.FromRgba(0.2, 0.3, 0.4, 0.5);

			Assert.Equal(new Color(0.2f, 0.3f, 0.4f, 0.5f), color);
		}

		[Fact]
		public void DefaultColorsMatch()
		{
			//This spot-checks a few of the fields in Color
			Assert.Equal(Colors.CornflowerBlue, Color.FromRgb(100, 149, 237));
			Assert.Equal(Colors.DarkSalmon, Color.FromRgb(233, 150, 122));
			Assert.Equal(Colors.Transparent, Color.FromRgba(0, 0, 0, 0));
			Assert.Equal(Colors.Wheat, Color.FromRgb(245, 222, 179));
			Assert.Equal(Colors.White, Color.FromRgb(255, 255, 255));
		}

		[Fact]
		public void TestFromUint()
		{
			var expectedColor = new Color(1, 0.65f, 0, 1);

			// Convert the expected color to a uint (argb)
			var blue = (int)(expectedColor.Blue * 255);
			var red = (int)(expectedColor.Red * 255);
			var green = (int)(expectedColor.Green * 255);
			var alpha = (int)(expectedColor.Alpha * 255);

			uint argb = (uint)(blue | (green << 8) | (red << 16) | (alpha << 24));

			// Create a new color from the uint
			var fromUint = Color.FromUint(argb);

			// Verify the components
			Assert.Equal(expectedColor.Alpha, fromUint.Alpha, 2f);
			Assert.Equal(expectedColor.Red, fromUint.Red, 2f);
			Assert.Equal(expectedColor.Green, fromUint.Green, 2f);
			Assert.Equal(expectedColor.Blue, fromUint.Blue, 2f);
		}

		[Fact]
		public void ToUInt()
		{
			var color = Color.FromRgba(255, 122, 15, 255);
			var i = color.ToUint();
			Assert.Equal(4294933007U, i);
		}

		[Theory]
		[InlineData("#FF0000", "#00FFFF")] // Red & Cyan
		[InlineData("#00FF00", "#FF00FF")] // Green & Fuchsia
		[InlineData("#0000FF", "#FFFF00")] // Blue & Yellow
		[InlineData("#0AF56C", "#F50A93")] // Lime green & bright purple (but with no limit values)
		public void GetComplementary(string original, string expected)
		{
			var orig = Color.FromArgb(original);
			var expectedComplement = Color.FromArgb(expected);

			var comp = orig.GetComplementary();

			Assert.Equal(expectedComplement.Alpha, comp.Alpha, 3f);
			Assert.Equal(expectedComplement.Red, comp.Red, 3f);
			Assert.Equal(expectedComplement.Green, comp.Green, 3f);
			Assert.Equal(expectedComplement.Blue, comp.Blue, 3f);
		}

		public static IEnumerable<object[]> TestFromRgbaValues()
		{
			yield return new object[] { "#111", Color.FromRgb(0x11, 0x11, 0x11) };
			yield return new object[] { "#a222", Color.FromRgba(0xaa, 0x22, 0x22, 0x22) };
			yield return new object[] { "#F2E2D2", Color.FromRgb(0xF2, 0xE2, 0xD2) };
			yield return new object[] { "#C2F2E2D2", Color.FromRgba(0xC2, 0xF2, 0xE2, 0xD2) };
			yield return new object[] { "111", Color.FromRgb(0x11, 0x11, 0x11) };
			yield return new object[] { "a222", Color.FromRgba(0xaa, 0x22, 0x22, 0x22) };
			yield return new object[] { "F2E2D2", Color.FromRgb(0xF2, 0xE2, 0xD2) };
			yield return new object[] { "C2F2E2D2", Color.FromRgba(0xC2, 0xF2, 0xE2, 0xD2) };
		}

		[Theory]
		[MemberData(nameof(TestFromRgbaValues))]
		public void TestFromRgba(string value, Color expected)
		{
			Color actual = Color.FromRgba(value);
			Assert.Equal(expected, actual);
		}

		public static IEnumerable<object[]> TestFromArgbValuesHash()
		{
			yield return new object[] { "#111", Color.FromRgb(0x11, 0x11, 0x11) };
			yield return new object[] { "#a222", Color.FromRgba(0x22, 0x22, 0x22, 0xaa) };
			yield return new object[] { "#F2E2D2", Color.FromRgb(0xF2, 0xE2, 0xD2) };
			yield return new object[] { "#C2F2E2D2", Color.FromRgba(0xF2, 0xE2, 0xD2, 0xC2) };
			yield return new object[] { "#000000", Color.FromRgba(0x00, 0x00, 0x00, 0xFF) };
			yield return new object[] { "#000", Color.FromRgba(0x00, 0x00, 0x00, 0xFF) };
			yield return new object[] { "#00FFff 40%", Color.FromRgba(0f, 0f, 0f, 1f) }; // unsupported syntax, but should not throw and fall back to the default black
		}

		public static IEnumerable<object[]> TestFromArgbValuesNoHash()
		{
			yield return new object[] { "111", Color.FromRgb(0x11, 0x11, 0x11) };
			yield return new object[] { "a222", Color.FromRgba(0x22, 0x22, 0x22, 0xaa) };
			yield return new object[] { "F2E2D2", Color.FromRgb(0xF2, 0xE2, 0xD2) };
			yield return new object[] { "C2F2E2D2", Color.FromRgba(0xF2, 0xE2, 0xD2, 0xC2) };
		}

		[Theory]
		[MemberData(nameof(TestFromArgbValuesHash))]
		[MemberData(nameof(TestFromArgbValuesNoHash))]
		public void TestFromArgb(string value, Color expected)
		{
			Color actual = Color.FromArgb(value);
			Assert.Equal(expected, actual);
		}

		public static IEnumerable<object[]> TestParseValidValues()
		{
			foreach (object[] argb in TestFromArgbValuesHash())
			{
				yield return argb;
			}

			yield return new object[] { "rgb(255,0,0)", Color.FromRgb(255, 0, 0) };
			yield return new object[] { "rgb(100%, 0%, 0%)", Color.FromRgb(255, 0, 0) };

			yield return new object[] { "rgba(0, 255, 0, 0.7)", Color.FromRgba(0, 255, 0, 0.7f) };
			yield return new object[] { "rgba(0%, 100%, 0%, 0.7)", Color.FromRgba(0, 255, 0, 0.7f) };

			yield return new object[] { "hsl(120, 100%, 50%)", Color.FromHsla(120f / 360f, 1.0f, .5f) };
			yield return new object[] { "hsl(120, 75, 20%)", Color.FromHsla(120f / 360f, .75f, .2f) };

			yield return new object[] { "hsla(160, 100%, 50%, .4)", Color.FromHsla(160f / 360f, 1.0f, .5f, .4f) };
			yield return new object[] { "hsla(160,100%,50%,.6)", Color.FromHsla(160f / 360f, 1.0f, .5f, .6f) };

			yield return new object[] { "hsv(120, 85%, 35%)", Color.FromHsv(120f / 360f, .85f, .35f) };
			yield return new object[] { "hsv(120, 85, 35)", Color.FromHsv(120f / 360f, .85f, .35f) };

			yield return new object[] { "hsva(120, 100%, 50%, .8)", Color.FromHsva(120f / 360f, 1.0f, .5f, .8f) };
			yield return new object[] { "hsva(120, 100, 50, .8)", Color.FromHsva(120f / 360f, 1.0f, .5f, .8f) };
		}

		[Theory]
		[MemberData(nameof(TestParseValidValues))]
		public void TestParseValid(string value, Color expected)
		{
			Assert.True(Color.TryParse(value, out Color actual));
			Assert.Equal(expected, actual);

			actual = Color.Parse(value);
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("default")]
		[InlineData("notAColor")]
		[InlineData("#ZZZ")]
		[InlineData("#12g")]
		[InlineData("#1g3")]
		[InlineData("#zyxv")]
		[InlineData("222")]
		[InlineData("rgb)255,0,0(")]
		[InlineData("rgb255,0,0")]
		[InlineData("rgba(255, 0, 0, 0.8")]
		[InlineData("hsv(120, 100#, 50#)")]
		[InlineData("hsv(120%, 100%, 50%)")]
		[InlineData("hsva(120, 120%, 50%, a)")]
		public void TestParseBad(string badValue)
		{
			Assert.False(Color.TryParse(badValue, out Color actual));
			Assert.Throws<InvalidOperationException>(() => Color.Parse(badValue));
		}

		[Fact]
		public void TestParseAllBuiltInColors()
		{
			var fields = typeof(Colors).GetFields(BindingFlags.Public | BindingFlags.Static);
			Assert.True(fields.Length > 100, "we should have some Color fields");

			foreach (FieldInfo field in fields)
			{
				string colorName = field.Name;
				Color actual = Color.Parse(colorName);
				Color expected = (Color)field.GetValue(null);

				Assert.Equal(expected, actual);
			}
		}
	}
}
