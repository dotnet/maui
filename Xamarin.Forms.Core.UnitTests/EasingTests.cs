using System;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class EasingTests : BaseTestFixture
	{
		[Test]
		public void Linear([Range(0, 10)] double input)
		{
			Assert.AreEqual(input, Easing.Linear.Ease(input));
		}

		[Test]
		public void AllRunFromZeroToOne([Values(0.0, 1.0)] double val)
		{
			const double epsilon = 0.001;
			Assert.True(Math.Abs(val - Easing.Linear.Ease(val)) < epsilon);
			Assert.True(Math.Abs(val - Easing.BounceIn.Ease(val)) < epsilon);
			Assert.True(Math.Abs(val - Easing.BounceOut.Ease(val)) < epsilon);
			Assert.True(Math.Abs(val - Easing.CubicIn.Ease(val)) < epsilon);
			Assert.True(Math.Abs(val - Easing.CubicInOut.Ease(val)) < epsilon);
			Assert.True(Math.Abs(val - Easing.CubicOut.Ease(val)) < epsilon);
			Assert.True(Math.Abs(val - Easing.SinIn.Ease(val)) < epsilon);
			Assert.True(Math.Abs(val - Easing.SinInOut.Ease(val)) < epsilon);
			Assert.True(Math.Abs(val - Easing.SinOut.Ease(val)) < epsilon);
			Assert.True(Math.Abs(val - Easing.SpringIn.Ease(val)) < epsilon);
			Assert.True(Math.Abs(val - Easing.SpringOut.Ease(val)) < epsilon);
		}

		[Test]
		public void TestEasingTypeConverter()
		{
			var converter = new EasingTypeConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));
			Assert.Null(converter.ConvertFromInvariantString(null));
			Assert.Null(converter.ConvertFromInvariantString(string.Empty));
			Assert.AreEqual(Easing.Linear, converter.ConvertFromInvariantString("Linear"));
			Assert.AreEqual(Easing.Linear, converter.ConvertFromInvariantString("linear"));
			Assert.AreEqual(Easing.Linear, converter.ConvertFromInvariantString("Easing.Linear"));
			Assert.AreEqual(Easing.SinOut, converter.ConvertFromInvariantString("SinOut"));
			Assert.AreEqual(Easing.SinOut, converter.ConvertFromInvariantString("sinout"));
			Assert.AreEqual(Easing.SinOut, converter.ConvertFromInvariantString("Easing.SinOut"));
			Assert.AreEqual(Easing.SinIn, converter.ConvertFromInvariantString("SinIn"));
			Assert.AreEqual(Easing.SinIn, converter.ConvertFromInvariantString("sinin"));
			Assert.AreEqual(Easing.SinIn, converter.ConvertFromInvariantString("Easing.SinIn"));
			Assert.AreEqual(Easing.SinInOut, converter.ConvertFromInvariantString("SinInOut"));
			Assert.AreEqual(Easing.SinInOut, converter.ConvertFromInvariantString("sininout"));
			Assert.AreEqual(Easing.SinInOut, converter.ConvertFromInvariantString("Easing.SinInOut"));
			Assert.AreEqual(Easing.CubicOut, converter.ConvertFromInvariantString("CubicOut"));
			Assert.AreEqual(Easing.CubicOut, converter.ConvertFromInvariantString("cubicout"));
			Assert.AreEqual(Easing.CubicOut, converter.ConvertFromInvariantString("Easing.CubicOut"));
			Assert.AreEqual(Easing.CubicIn, converter.ConvertFromInvariantString("CubicIn"));
			Assert.AreEqual(Easing.CubicIn, converter.ConvertFromInvariantString("cubicin"));
			Assert.AreEqual(Easing.CubicIn, converter.ConvertFromInvariantString("Easing.CubicIn"));
			Assert.AreEqual(Easing.CubicInOut, converter.ConvertFromInvariantString("CubicInOut"));
			Assert.AreEqual(Easing.CubicInOut, converter.ConvertFromInvariantString("cubicinout"));
			Assert.AreEqual(Easing.CubicInOut, converter.ConvertFromInvariantString("Easing.CubicInOut"));
			Assert.AreEqual(Easing.BounceOut, converter.ConvertFromInvariantString("BounceOut"));
			Assert.AreEqual(Easing.BounceOut, converter.ConvertFromInvariantString("bounceout"));
			Assert.AreEqual(Easing.BounceOut, converter.ConvertFromInvariantString("Easing.BounceOut"));
			Assert.AreEqual(Easing.BounceIn, converter.ConvertFromInvariantString("BounceIn"));
			Assert.AreEqual(Easing.BounceIn, converter.ConvertFromInvariantString("bouncein"));
			Assert.AreEqual(Easing.BounceIn, converter.ConvertFromInvariantString("Easing.BounceIn"));
			Assert.AreEqual(Easing.SpringOut, converter.ConvertFromInvariantString("SpringOut"));
			Assert.AreEqual(Easing.SpringOut, converter.ConvertFromInvariantString("springout"));
			Assert.AreEqual(Easing.SpringOut, converter.ConvertFromInvariantString("Easing.SpringOut"));
			Assert.AreEqual(Easing.SpringIn, converter.ConvertFromInvariantString("SpringIn"));
			Assert.AreEqual(Easing.SpringIn, converter.ConvertFromInvariantString("springin"));
			Assert.AreEqual(Easing.SpringIn, converter.ConvertFromInvariantString("Easing.SpringIn"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("WrongEasingName"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("Easing.Linear.SinInOut"));
		}
	}
}