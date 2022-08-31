using System;
using Microsoft.Maui.Converters;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class EasingTests : BaseTestFixture
	{
		[Theory, MemberData(nameof(TestDataHelpers.Range), 0, 10, 1, MemberType = typeof(TestDataHelpers))]
		public void Linear(double input)
		{
			Assert.Equal(input, Easing.Linear.Ease(input));
		}

		[Theory]
		[InlineData(0.0)]
		[InlineData(1.0)]
		public void AllRunFromZeroToOne(double val)
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

		[Fact]
		public void TestEasingTypeConverter()
		{
			var converter = new EasingTypeConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));
			Assert.Null(converter.ConvertFromInvariantString(null));
			Assert.Null(converter.ConvertFromInvariantString(string.Empty));
			Assert.Equal(Easing.Linear, converter.ConvertFromInvariantString("Linear"));
			Assert.Equal(Easing.Linear, converter.ConvertFromInvariantString("linear"));
			Assert.Equal(Easing.Linear, converter.ConvertFromInvariantString("Easing.Linear"));
			Assert.Equal(Easing.SinOut, converter.ConvertFromInvariantString("SinOut"));
			Assert.Equal(Easing.SinOut, converter.ConvertFromInvariantString("sinout"));
			Assert.Equal(Easing.SinOut, converter.ConvertFromInvariantString("Easing.SinOut"));
			Assert.Equal(Easing.SinIn, converter.ConvertFromInvariantString("SinIn"));
			Assert.Equal(Easing.SinIn, converter.ConvertFromInvariantString("sinin"));
			Assert.Equal(Easing.SinIn, converter.ConvertFromInvariantString("Easing.SinIn"));
			Assert.Equal(Easing.SinInOut, converter.ConvertFromInvariantString("SinInOut"));
			Assert.Equal(Easing.SinInOut, converter.ConvertFromInvariantString("sininout"));
			Assert.Equal(Easing.SinInOut, converter.ConvertFromInvariantString("Easing.SinInOut"));
			Assert.Equal(Easing.CubicOut, converter.ConvertFromInvariantString("CubicOut"));
			Assert.Equal(Easing.CubicOut, converter.ConvertFromInvariantString("cubicout"));
			Assert.Equal(Easing.CubicOut, converter.ConvertFromInvariantString("Easing.CubicOut"));
			Assert.Equal(Easing.CubicIn, converter.ConvertFromInvariantString("CubicIn"));
			Assert.Equal(Easing.CubicIn, converter.ConvertFromInvariantString("cubicin"));
			Assert.Equal(Easing.CubicIn, converter.ConvertFromInvariantString("Easing.CubicIn"));
			Assert.Equal(Easing.CubicInOut, converter.ConvertFromInvariantString("CubicInOut"));
			Assert.Equal(Easing.CubicInOut, converter.ConvertFromInvariantString("cubicinout"));
			Assert.Equal(Easing.CubicInOut, converter.ConvertFromInvariantString("Easing.CubicInOut"));
			Assert.Equal(Easing.BounceOut, converter.ConvertFromInvariantString("BounceOut"));
			Assert.Equal(Easing.BounceOut, converter.ConvertFromInvariantString("bounceout"));
			Assert.Equal(Easing.BounceOut, converter.ConvertFromInvariantString("Easing.BounceOut"));
			Assert.Equal(Easing.BounceIn, converter.ConvertFromInvariantString("BounceIn"));
			Assert.Equal(Easing.BounceIn, converter.ConvertFromInvariantString("bouncein"));
			Assert.Equal(Easing.BounceIn, converter.ConvertFromInvariantString("Easing.BounceIn"));
			Assert.Equal(Easing.SpringOut, converter.ConvertFromInvariantString("SpringOut"));
			Assert.Equal(Easing.SpringOut, converter.ConvertFromInvariantString("springout"));
			Assert.Equal(Easing.SpringOut, converter.ConvertFromInvariantString("Easing.SpringOut"));
			Assert.Equal(Easing.SpringIn, converter.ConvertFromInvariantString("SpringIn"));
			Assert.Equal(Easing.SpringIn, converter.ConvertFromInvariantString("springin"));
			Assert.Equal(Easing.SpringIn, converter.ConvertFromInvariantString("Easing.SpringIn"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("WrongEasingName"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("Easing.Linear.SinInOut"));
		}
	}
}
