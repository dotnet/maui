using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Xamarin.Forms.Core.UnitTests
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
			var color = new Color(1, 1, 1, 1);
			color = color.MultiplyAlpha(0.25f);
			Assert.Equal(.25, color.Alpha);

			color = Color.FromHsla(1, 1, 1, 1);
			color = color.MultiplyAlpha(0.25f);
			Assert.Equal(.25, color.Alpha);
		}

		[Fact]
		public void TestClamping()
		{
			var color = new Color(2, 2, 2, 2);

			Assert.Equal(1, color.Red);
			Assert.Equal(1, color.Green);
			Assert.Equal(1, color.Blue);
			Assert.Equal(1, color.Alpha);

			color = new Color(-1, -1, -1, -1);

			Assert.Equal(0, color.Red);
			Assert.Equal(0, color.Green);
			Assert.Equal(0, color.Blue);
			Assert.Equal(0, color.Alpha);
		}

		[Fact]
		public void TestRGBToHSL()
		{
			var color = new Color(.5f, .1f, .1f);

			Assert.Equal(1, color.GetHue(), 3);
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
			Assert.Equal(brighter.GetLuminosity(), color.GetLuminosity() + 0.2,3);
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

			Assert.Equal(0.6f, color.Red, 3);
			Assert.Equal(0.7f, color.Green, 3);
			Assert.Equal(0.2f, color.Blue, 3);
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
			Assert.Equal("[Color: Red=1, Green=1, Blue=1, Alpha=0.5]", color.ToString());
		}

		[Fact]
		public void TestFromHex()
		{
			var color = Color.FromRgb(138, 43, 226);
			Assert.Equal(color, new Color("8a2be2"));

            Assert.Equal(Color.FromRgba(138, 43, 226, 128), new Color("#8a2be280"));
			Assert.Equal(Color.FromHex("#aabbcc"), new Color("#abc"));
			Assert.Equal(Color.FromHex("#aabbccdd"), new Color("#abcd"));
		}

		[Fact]
		public void TestToHex()
		{
			var colorRgb = Color.FromRgb(138, 43, 226);
			Assert.Equal(Color.FromHex(colorRgb.ToHex()), colorRgb);
			var colorRgba = Color.FromRgba(138, 43, 226, .2);
			Assert.Equal(Color.FromHex(colorRgba.ToHex()), colorRgba);
			var colorHsl = Color.FromHsla(240, 1, 1);
			Assert.Equal(Color.FromHex(colorHsl.ToHex()), colorHsl);
			var colorHsla = Color.FromHsla(240, 1, 1, .1f);
			var hexFromHsla = new Color(colorHsla.ToHex());
			Assert.Equal(hexFromHsla.Alpha, colorHsla.Alpha,2);
			Assert.Equal(hexFromHsla.Red, colorHsla.Red,3);
			Assert.Equal(hexFromHsla.Green, colorHsla.Green,3);
			Assert.Equal(hexFromHsla.Blue, colorHsla.Blue,3);
		}

		[Fact]
		public void TestFromHsv()
		{
			var color = Color.FromRgb(1, .29f, .752f);
			var colorHsv = Color.FromHsv(321, 71, 100);
			Assert.Equal(color.Red, colorHsv.Red,3);
			Assert.Equal(color.Green, colorHsv.Green,3);
			Assert.Equal(color.Blue, colorHsv.Blue,3);
		}

		[Fact]
		public void TestFromHsva()
		{
			var color = Color.FromRgba(1, .29, .752, .5);
			var colorHsv = Color.FromHsva(321, 71, 100, 50);
			Assert.Equal(color.Red, colorHsv.Red, 3);
			Assert.Equal(color.Green, colorHsv.Green,3);
			Assert.Equal(color.Blue, colorHsv.Blue,3);
			Assert.Equal(color.Alpha, colorHsv.Alpha,3);
		}

		[Fact]
		public void TestFromHsvDouble()
		{
			var color = Color.FromRgb(1, .29f, .758f);
			var colorHsv = Color.FromHsv(.89f, .71f, 1);
			Assert.Equal(color.Red, colorHsv.Red,2);
			Assert.Equal(color.Green, colorHsv.Green,2);
			Assert.Equal(color.Blue, colorHsv.Blue,2);
		}

		[Fact]
		public void TestFromHsvaDouble()
		{
			var color = Color.FromRgba(1, .29, .758, .5);
			var colorHsv = Color.FromHsva(.89f, .71f, 1f, .5f);
			Assert.Equal(color.Red, colorHsv.Red,2);
			Assert.Equal(color.Green, colorHsv.Green,2);
			Assert.Equal(color.Blue, colorHsv.Blue,2);
			Assert.Equal(color.Alpha, colorHsv.Alpha,2);
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
	}
}