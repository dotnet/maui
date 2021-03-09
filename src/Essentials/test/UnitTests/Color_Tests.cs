using System;
using System.Drawing;
using Microsoft.Maui.Essentials;
using Xunit;

namespace Tests
{
	public class Color_Tests
	{
		float h = 204;
		float s = 69.9f;
		float l = 53.1f;
		int r = 52;
		int g = 152;
		int b = 219;

		[Fact]
		public void FromHsl()
		{
			var color = ColorConverters.FromHsl(h, s, l);
			Assert.Equal(255, color.A);
			Assert.Equal(r, color.R);
			Assert.Equal(g, color.G);
			Assert.Equal(b, color.B);
		}

		[Fact]
		public void FromUint()
		{
			var color = ColorConverters.FromUInt(4294933007);
			Assert.Equal(255, color.A);
			Assert.Equal(255, color.R);
			Assert.Equal(122, color.G);
			Assert.Equal(15, color.B);
		}

		[Fact]
		public void ToUInt()
		{
			var color = Color.FromArgb(255, 255, 122, 15);
			var i = color.ToUInt();
			Assert.Equal(4294933007U, i);
		}

		[Theory]
		[InlineData("#3498DB", 255, 52, 152, 219)]
		[InlineData("#C0C0C0", 255, 192, 192, 192)]
		[InlineData("3498DB", 255, 52, 152, 219)]
		[InlineData("C0C0C0", 255, 192, 192, 192)]
		[InlineData("#903498DB", 144, 52, 152, 219)]
		[InlineData("#90C0C0C0", 144, 192, 192, 192)]
		public void FromHex(string hex, int a, int r, int g, int b)
		{
			var color = ColorConverters.FromHex(hex);
			Assert.Equal(a, color.A);
			Assert.Equal(r, color.R);
			Assert.Equal(g, color.G);
			Assert.Equal(b, color.B);
		}

		[Theory]
		[InlineData("#FF0000", "#00FFFF")] // Red & Cyan
		[InlineData("#00FF00", "#FF00FF")] // Green & Fuchsia
		[InlineData("#0000FF", "#FFFF00")] // Blue & Yellow
		[InlineData("#0AF56C", "#F50A93")] // Lime green & bright purple (but with no limit values)
		public void GetComplementary(string original, string expected)
		{
			var orig = ColorConverters.FromHex(original);
			var expectedComplement = ColorConverters.FromHex(expected);

			Assert.Equal(expectedComplement, orig.GetComplementary());
		}

		[Fact]
		public void FromHsla()
		{
			var a = 186;
			var color = ColorConverters.FromHsla(h, s, l, 186);
			Assert.Equal(a, color.A);
			Assert.Equal(r, color.R);
			Assert.Equal(g, color.G);
			Assert.Equal(b, color.B);
		}

		[Fact]
		public void MuliplyAlpha()
		{
			var color = Color.FromArgb(r, g, b);
			color = color.MultiplyAlpha(.5f);
			Assert.Equal((int)(255 * .5f), color.A);
			Assert.Equal(r, color.R);
			Assert.Equal(g, color.G);
			Assert.Equal(b, color.B);
		}

		[Fact]
		public void WithHue()
		{
			var color = Color.FromArgb(r, g, b);
			color = color.WithHue(10);
			Assert.Equal(255, color.A);
			Assert.Equal(219, color.R);
			Assert.Equal(80, color.G);
			Assert.Equal(52, color.B);
		}

		[Fact]
		public void WithAlpha()
		{
			var color = Color.FromArgb(r, g, b);
			color = color.WithAlpha(10);
			Assert.Equal(10, color.A);
			Assert.Equal(r, color.R);
			Assert.Equal(g, color.G);
			Assert.Equal(b, color.B);
		}

		[Fact]
		public void WithSaturation()
		{
			var color = Color.FromArgb(r, g, b);
			color = color.WithSaturation(10);
			Assert.Equal(255, color.A);
			Assert.Equal(124, color.R);
			Assert.Equal(138, color.G);
			Assert.Equal(147, color.B);
		}

		[Fact]
		public void WithLuminosity()
		{
			var color = Color.FromArgb(r, g, b);
			color = color.WithLuminosity(10);
			Assert.Equal(255, color.A);
			Assert.Equal(8, color.R);
			Assert.Equal(29, color.G);
			Assert.Equal(43, color.B);
		}

		[Theory]
		[InlineData("black", 0, 0, 0)]
		[InlineData("red", 0, 100, 100)]
		[InlineData("white", 0, 0, 100)]
		[InlineData("green", 120, 100, 50.2)]
		[InlineData("lime", 120, 100, 100)]
		[InlineData("yellow", 60, 100, 100)]
		[InlineData("magenta", 300, 100, 100)]
		[InlineData("cyan", 180, 100, 100)]
		public void RgbToHsv(string name, double hTest, double sTest, double vTest)
		{
			var color = Color.FromName(name);
			var (h, s, v) = color.ToHsv();
			Assert.Equal(hTest, h);
			Assert.Equal(sTest, s);
			Assert.Equal(vTest, Math.Round(v, 1));
		}

		[Theory]
		[InlineData("black", 0, 0, 0)]
		[InlineData("red", 0, 100, 100)]
		[InlineData("white", 0, 0, 100)]
		[InlineData("green", 120, 100, 50.2)]
		[InlineData("lime", 120, 100, 100)]
		[InlineData("yellow", 60, 100, 100)]
		[InlineData("magenta", 300, 100, 100)]
		[InlineData("cyan", 180, 100, 100)]
		public void HsvToRgba(string name, double hTest, double sTest, double vTest)
		{
			var colorExpected = Color.FromName(name);
			var color = ColorExtensions.FromHsva(hTest, sTest, vTest, 50);
			Assert.Equal(colorExpected.R, color.R);
			Assert.Equal(colorExpected.G, color.G);
			Assert.Equal(colorExpected.B, color.B);
			Assert.Equal(50, color.A);
		}
	}
}
