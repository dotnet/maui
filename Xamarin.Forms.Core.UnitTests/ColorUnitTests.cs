using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ColorUnitTests : BaseTestFixture
	{
		[Test]
		public void TestHSLPostSetEquality()
		{
			var color = new Color(1, 0.5, 0.2);
			var color2 = color;

			color2 = color.WithLuminosity(.2);
			Assert.False(color == color2);
		}

		[Test]
		public void TestHSLPostSetInequality()
		{
			var color = new Color(1, 0.5, 0.2);
			var color2 = color;

			color2 = color.WithLuminosity(.2);

			Assert.True(color != color2);
		}

		[Test]
		public void TestHSLSetToDefaultValue()
		{
			var color = new Color(0.2, 0.5, 0.8);

			// saturation is initialized to 0, make sure we still update
			color = color.WithSaturation(0);

			Assert.AreEqual(color.R, color.G);
			Assert.AreEqual(color.R, color.B);
		}

		[Test]
		public void TestHSLModifiers()
		{
			var color = Color.Default;
			Assert.Throws<InvalidOperationException>(() => color.WithHue(.1));
			Assert.Throws<InvalidOperationException>(() => color.WithLuminosity(.1));
			Assert.Throws<InvalidOperationException>(() => color.WithSaturation(.1));

			color = Color.FromHsla(.8, .6, .2);
			Assert.AreEqual(Color.FromHsla(.1, .6, .2), color.WithHue(.1));
			Assert.AreEqual(Color.FromHsla(.8, .1, .2), color.WithSaturation(.1));
			Assert.AreEqual(Color.FromHsla(.8, .6, .1), color.WithLuminosity(.1));
		}

		[Test]
		public void TestMultiplyAlpha()
		{
			var color = new Color(1, 1, 1, 1);
			color = color.MultiplyAlpha(0.25);
			Assert.AreEqual(.25, color.A);

			color = Color.Default;
			Assert.Throws<InvalidOperationException>(() => color = color.MultiplyAlpha(0.25));

			color = Color.FromHsla(1, 1, 1, 1);
			color = color.MultiplyAlpha(0.25);
			Assert.AreEqual(.25, color.A);
		}

		[Test]
		public void TestClamping()
		{
			var color = new Color(2, 2, 2, 2);

			Assert.AreEqual(1, color.R);
			Assert.AreEqual(1, color.G);
			Assert.AreEqual(1, color.B);
			Assert.AreEqual(1, color.A);

			color = new Color(-1, -1, -1, -1);

			Assert.AreEqual(0, color.R);
			Assert.AreEqual(0, color.G);
			Assert.AreEqual(0, color.B);
			Assert.AreEqual(0, color.A);
		}

		[Test]
		public void TestRGBToHSL()
		{
			var color = new Color(.5, .1, .1);

			Assert.That(color.Hue, Is.EqualTo(1).Within(0.001));
			Assert.That(color.Saturation, Is.EqualTo(0.662).Within(0.01));
			Assert.That(color.Luminosity, Is.EqualTo(0.302).Within(0.01));
		}

		[Test]
		public void TestHSLToRGB()
		{
			var color = Color.FromHsla(0, .662, .302);

			Assert.That(color.R, Is.EqualTo(0.5).Within(0.01));
			Assert.That(color.G, Is.EqualTo(0.1).Within(0.01));
			Assert.That(color.B, Is.EqualTo(0.1).Within(0.01));
		}

		[Test]
		public void TestColorFromValue()
		{
			var color = new Color(0.2);

			Assert.AreEqual(new Color(0.2, 0.2, 0.2, 1), color);
		}

		[Test]
		public void TestAddLuminosity()
		{
			var color = new Color(0.2);
			var brighter = color.AddLuminosity(0.2);
			Assert.That(brighter.Luminosity, Is.EqualTo(color.Luminosity + 0.2).Within(0.001));

			color = Color.Default;
			Assert.Throws<InvalidOperationException>(() => color.AddLuminosity(0.2));
		}

		[Test]
		public void TestZeroLuminosity()
		{
			var color = new Color(0.1, 0.2, 0.3);
			color = color.AddLuminosity(-1);

			Assert.AreEqual(0, color.Luminosity);
			Assert.AreEqual(0, color.R);
			Assert.AreEqual(0, color.G);
			Assert.AreEqual(0, color.B);
		}

		[Test]
		public void TestHashCode()
		{
			var color1 = new Color(0.1);
			var color2 = new Color(0.1);

			Assert.True(color1.GetHashCode() == color2.GetHashCode());
			color2 = Color.FromHsla(color2.Hue, color2.Saturation, .5);

			Assert.False(color1.GetHashCode() == color2.GetHashCode());
		}

		[Test]
		public void TestHashCodeNamedColors()
		{
			Color red = Color.Red; //R=1, G=0, B=0, A=1
			int hashRed = red.GetHashCode();

			Color blue = Color.Blue; //R=0, G=0, B=1, A=1
			int hashBlue = blue.GetHashCode();

			Assert.False(hashRed == hashBlue);
		}

		[Test]
		public void TestHashCodeAll()
		{
			Dictionary<int, Color> colorsAndHashes = new Dictionary<int, Color>();
			Assert.DoesNotThrow(() => colorsAndHashes.Add(Color.Transparent.GetHashCode(), Color.Transparent));
			Assert.DoesNotThrow(() => colorsAndHashes.Add(Color.Aqua.GetHashCode(), Color.Aqua));
			Assert.DoesNotThrow(() => colorsAndHashes.Add(Color.Black.GetHashCode(), Color.Black));
			Assert.DoesNotThrow(() => colorsAndHashes.Add(Color.Blue.GetHashCode(), Color.Blue));
			Assert.DoesNotThrow(() => colorsAndHashes.Add(Color.Fuchsia.GetHashCode(), Color.Fuchsia));
			Assert.DoesNotThrow(() => colorsAndHashes.Add(Color.Gray.GetHashCode(), Color.Gray));
			Assert.DoesNotThrow(() => colorsAndHashes.Add(Color.Green.GetHashCode(), Color.Green));
			Assert.DoesNotThrow(() => colorsAndHashes.Add(Color.Lime.GetHashCode(), Color.Lime));
			Assert.DoesNotThrow(() => colorsAndHashes.Add(Color.Maroon.GetHashCode(), Color.Maroon));
			Assert.DoesNotThrow(() => colorsAndHashes.Add(Color.Navy.GetHashCode(), Color.Navy));
			Assert.DoesNotThrow(() => colorsAndHashes.Add(Color.Olive.GetHashCode(), Color.Olive));
			Assert.DoesNotThrow(() => colorsAndHashes.Add(Color.Purple.GetHashCode(), Color.Purple));
			Assert.DoesNotThrow(() => colorsAndHashes.Add(Color.Pink.GetHashCode(), Color.Pink));
			Assert.DoesNotThrow(() => colorsAndHashes.Add(Color.Red.GetHashCode(), Color.Red));
			Assert.DoesNotThrow(() => colorsAndHashes.Add(Color.Silver.GetHashCode(), Color.Silver));
			Assert.DoesNotThrow(() => colorsAndHashes.Add(Color.Teal.GetHashCode(), Color.Teal));
			Assert.DoesNotThrow(() => colorsAndHashes.Add(Color.White.GetHashCode(), Color.White));
			Assert.DoesNotThrow(() => colorsAndHashes.Add(Color.Yellow.GetHashCode(), Color.Yellow));
		}

		[Test]
		public void TestSetHue()
		{
			var color = new Color(0.2, 0.5, 0.7);
			color = Color.FromHsla(.2, color.Saturation, color.Luminosity);

			Assert.That(color.R, Is.EqualTo(0.6).Within(0.001));
			Assert.That(color.G, Is.EqualTo(0.7).Within(0.001));
			Assert.That(color.B, Is.EqualTo(0.2).Within(0.001));
		}

		[Test]
		public void ZeroLuminToRGB()
		{
			var color = new Color(0);
			Assert.AreEqual(0, color.Luminosity);
			Assert.AreEqual(0, color.Hue);
			Assert.AreEqual(0, color.Saturation);
		}

		[Test]
		public void TestToString()
		{
			var color = new Color(1, 1, 1, 0.5);
			Assert.AreEqual("[Color: A=0.5, R=1, G=1, B=1, Hue=0, Saturation=0, Luminosity=1]", color.ToString());
		}

		[Test]
		public void TestFromHex()
		{
			var color = Color.FromRgb(138, 43, 226);
			Assert.AreEqual(color, Color.FromHex("8a2be2"));

			Assert.AreEqual(Color.FromRgba(138, 43, 226, 128), Color.FromHex("#808a2be2"));
			Assert.AreEqual(Color.FromHex("#aabbcc"), Color.FromHex("#abc"));
			Assert.AreEqual(Color.FromHex("#aabbccdd"), Color.FromHex("#abcd"));
		}

		[Test]
		public void TestToHex()
		{
			var colorRgb = Color.FromRgb(138, 43, 226);
			Assert.AreEqual(Color.FromHex(colorRgb.ToHex()), colorRgb);
			var colorRgba = Color.FromRgba(138, 43, 226, .2);
			Assert.AreEqual(Color.FromHex(colorRgba.ToHex()), colorRgba);
			var colorHsl = Color.FromHsla(240, 1, 1);
			Assert.AreEqual(Color.FromHex(colorHsl.ToHex()), colorHsl);
			var colorHsla = Color.FromHsla(240, 1, 1, .1);
			var hexFromHsla = Color.FromHex(colorHsla.ToHex());
			Assert.That(hexFromHsla.A, Is.EqualTo(colorHsla.A).Within(0.002));
			Assert.That(hexFromHsla.R, Is.EqualTo(colorHsla.R).Within(0.001));
			Assert.That(hexFromHsla.G, Is.EqualTo(colorHsla.G).Within(0.001));
			Assert.That(hexFromHsla.B, Is.EqualTo(colorHsla.B).Within(0.001));
		}

		[Test]
		public void TestFromHsv()
		{
			var color = Color.FromRgb(1, .29, .752);
			var colorHsv = Color.FromHsv(321, 71, 100);
			Assert.That(color.R, Is.EqualTo(colorHsv.R).Within(0.001));
			Assert.That(color.G, Is.EqualTo(colorHsv.G).Within(0.001));
			Assert.That(color.B, Is.EqualTo(colorHsv.B).Within(0.001));
		}

		[Test]
		public void TestFromHsva()
		{
			var color = Color.FromRgba(1, .29, .752, .5);
			var colorHsv = Color.FromHsva(321, 71, 100, 50);
			Assert.That(color.R, Is.EqualTo(colorHsv.R).Within(0.001));
			Assert.That(color.G, Is.EqualTo(colorHsv.G).Within(0.001));
			Assert.That(color.B, Is.EqualTo(colorHsv.B).Within(0.001));
			Assert.That(color.A, Is.EqualTo(colorHsv.A).Within(0.001));
		}

		[Test]
		public void TestFromHsvDouble()
		{
			var color = Color.FromRgb(1, .29, .758);
			var colorHsv = Color.FromHsv(.89, .71, 1);
			Assert.That(color.R, Is.EqualTo(colorHsv.R).Within(0.001));
			Assert.That(color.G, Is.EqualTo(colorHsv.G).Within(0.001));
			Assert.That(color.B, Is.EqualTo(colorHsv.B).Within(0.001));
		}

		[Test]
		public void TestFromHsvaDouble()
		{
			var color = Color.FromRgba(1, .29, .758, .5);
			var colorHsv = Color.FromHsva(.89, .71, 1, .5);
			Assert.That(color.R, Is.EqualTo(colorHsv.R).Within(0.001));
			Assert.That(color.G, Is.EqualTo(colorHsv.G).Within(0.001));
			Assert.That(color.B, Is.EqualTo(colorHsv.B).Within(0.001));
			Assert.That(color.A, Is.EqualTo(colorHsv.A).Within(0.001));
		}

		[Test]
		public void FromRGBDouble()
		{
			var color = Color.FromRgb(0.2, 0.3, 0.4);

			Assert.AreEqual(new Color(0.2, 0.3, 0.4), color);
		}

		[Test]
		public void FromRGBADouble()
		{
			var color = Color.FromRgba(0.2, 0.3, 0.4, 0.5);

			Assert.AreEqual(new Color(0.2, 0.3, 0.4, 0.5), color);
		}

		[Test]
		public void TestColorTypeConverter()
		{
			var converter = new ColorTypeConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));
			Assert.AreEqual(Color.Blue, converter.ConvertFromInvariantString("Color.Blue"));
			Assert.AreEqual(Color.Blue, converter.ConvertFromInvariantString("Blue"));
			Assert.AreEqual(Color.Blue, converter.ConvertFromInvariantString("blue"));
			Assert.AreEqual(Color.Blue, converter.ConvertFromInvariantString("#0000ff"));
			Assert.AreEqual(Color.Blue, converter.ConvertFromInvariantString("#00f"));
			Assert.AreEqual(Color.Blue.MultiplyAlpha(2.0 / 3.0), converter.ConvertFromInvariantString("#a00f"));
			Assert.AreEqual(Color.Blue, converter.ConvertFromInvariantString("rgb(0,0, 255)"));
			Assert.AreEqual(Color.Blue, converter.ConvertFromInvariantString("rgb(0,0, 300)"));
			Assert.AreEqual(Color.Blue, converter.ConvertFromInvariantString("rgb(0,0, 300)"));
			Assert.AreEqual(Color.Blue.MultiplyAlpha(.8), converter.ConvertFromInvariantString("rgba(0%,0%, 100%, .8)"));
			Assert.AreEqual(Color.Blue, converter.ConvertFromInvariantString("rgb(0%,0%, 110%)"));
			Assert.AreEqual(Color.Blue, converter.ConvertFromInvariantString("hsl(240,100%, 50%)"));
			Assert.AreEqual(Color.Blue, converter.ConvertFromInvariantString("hsl(240,110%, 50%)"));
			Assert.AreEqual(Color.Blue.MultiplyAlpha(.8), converter.ConvertFromInvariantString("hsla(240,100%, 50%, .8)"));
			Assert.AreEqual(Color.FromHsla(0.66916666666666669, 1, 0.5), converter.ConvertFromInvariantString("hsl(240.9,100%, 50%)"));
			Assert.AreEqual(Color.FromHsva(0.89166666666666669, .71, 1, .8), converter.ConvertFromInvariantString("hsva(321,71%, 100%, .8)"));
			Assert.AreEqual(Color.FromHsv(0.89166666666666669, .71, 1), converter.ConvertFromInvariantString("hsv(321,71%, 100%)"));
			Assert.AreEqual(Color.Default, converter.ConvertFromInvariantString("Color.Default"));
			Assert.AreEqual(Color.Accent, converter.ConvertFromInvariantString("Accent"));
			var hotpink = Color.FromHex("#FF69B4");
			Color.Accent = hotpink;
			Assert.AreEqual(Color.Accent, converter.ConvertFromInvariantString("Accent"));
			Assert.AreEqual(Color.Default, converter.ConvertFromInvariantString("#12345"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString(""));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("rgb(0,0,255"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("hsl(12, 100%)"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("hsv(12, 100%)"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("hsva(12, 100%)"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("rgba(0,0,255)"));
			Assert.AreEqual(Color.FromRgb(0, 122, 255), converter.ConvertFromInvariantString("SystemBlue"));
			Assert.AreEqual(Color.FromHex("#FF767676"), converter.ConvertFromInvariantString("SystemChromeHighColor"));
			Assert.AreEqual(Color.FromHex("#ff00ddff"), converter.ConvertFromInvariantString("HoloBlueBright"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("NonExistentNamedColor"));
		}

		[Test]
		public void TestDefault()
		{
			Assert.AreEqual(Color.Default, default(Color));
			Assert.AreEqual(Color.Default, new Color());
		}

		[Test]
		public void TestImplicitConversionToSystemDrawingColor()
		{
			var color = Color.FromRgba(0.2, 0.3, 0.4, 0.5);
			System.Drawing.Color sdColor = color;
			Assert.AreEqual(51, sdColor.R);
			Assert.AreEqual(76, sdColor.G);
			Assert.AreEqual(102, sdColor.B);
			Assert.AreEqual(127, sdColor.A);
		}

		[Test]
		public void TestDefaultColorToSystemDrawingColorEmpty()
		{
			Assert.AreEqual(System.Drawing.Color.Empty, (System.Drawing.Color)Color.Default);
		}

		[Test]
		public void TestImplicitConversionFromSystemDrawingColor()
		{
			System.Drawing.Color sdColor = System.Drawing.Color.FromArgb(32, 64, 128, 255);
			Color color = sdColor;
			Assert.AreEqual(.125, color.A, .01);
			Assert.AreEqual(.25, color.R, .01);
			Assert.AreEqual(.5, color.G, .01);
			Assert.AreEqual(1, color.B, .01);
		}

		[Test]
		public void TestSystemDrawingColorEmptyToColorDefault()
		{
			Assert.AreEqual(Color.Default, (Color)System.Drawing.Color.Empty);
		}

		[Test]
		public void DefaultColorsMatch()
		{
			//This spot-checks a few of the fields in Color
			Assert.AreEqual(Color.CornflowerBlue, Color.FromRgb(100, 149, 237));
			Assert.AreEqual(Color.DarkSalmon, Color.FromRgb(233, 150, 122));
			Assert.AreEqual(Color.Transparent, Color.FromRgba(255, 255, 255, 0));
			Assert.AreEqual(Color.Wheat, Color.FromRgb(245, 222, 179));
			Assert.AreEqual(Color.White, Color.FromRgb(255, 255, 255));
		}
	}
}