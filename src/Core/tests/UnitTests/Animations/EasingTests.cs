using System;

using Microsoft.Maui;
using Microsoft.Maui.Converters;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
    [Category(TestCategory.Animations)]
    public class EasingTests
    {
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(2.0)]
        [InlineData(5.0)]
        [InlineData(8.0)]
        [InlineData(9.0)]
        [InlineData(10.0)]
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
        public void CanConvert()
        {
            var converter = new EasingTypeConverter();

            Assert.True(converter.CanConvertFrom(typeof(string)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void NonTextEasingsAreNull(string input)
        {
            var converter = new EasingTypeConverter();

            Assert.Null(converter.ConvertFromInvariantString(input));
        }

        [Fact]
        public void CanConvertFromEasingNameToEasing()
        {
            var converter = new EasingTypeConverter();

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
        }

        [Fact]
        public void CanConvertFromEasingToEasingName()
        {
            var converter = new EasingTypeConverter();

            Assert.Equal("Linear", converter.ConvertToInvariantString(Easing.Linear));
            Assert.Equal("SinOut", converter.ConvertToInvariantString(Easing.SinOut));
            Assert.Equal("SinIn", converter.ConvertToInvariantString(Easing.SinIn));
            Assert.Equal("SinInOut", converter.ConvertToInvariantString(Easing.SinInOut));
            Assert.Equal("CubicOut", converter.ConvertToInvariantString(Easing.CubicOut));
            Assert.Equal("CubicIn", converter.ConvertToInvariantString(Easing.CubicIn));
            Assert.Equal("CubicInOut", converter.ConvertToInvariantString(Easing.CubicInOut));
            Assert.Equal("BounceOut", converter.ConvertToInvariantString(Easing.BounceOut));
            Assert.Equal("BounceIn", converter.ConvertToInvariantString(Easing.BounceIn));
            Assert.Equal("SpringOut", converter.ConvertToInvariantString(Easing.SpringOut));
            Assert.Equal("SpringIn", converter.ConvertToInvariantString(Easing.SpringIn));
        }

        [Fact]
        public void InvalidEasingNamesThrow()
        {
            var converter = new EasingTypeConverter();

            Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("WrongEasingName"));
            Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("Easing.Linear.SinInOut"));
        }
    }

    public class EasingDefaultPropertyTests
    {
        /// <summary>
        /// Tests that the Default property returns a non-null Easing instance.
        /// Verifies the basic contract that Default provides a valid easing function.
        /// Expected result: Default property returns a non-null Easing instance.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Default_Always_ReturnsNonNullInstance()
        {
            // Act
            var result = Easing.Default;

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that the Default property returns the same instance as CubicInOut.
        /// Verifies that Default is correctly configured to use the CubicInOut easing function as documented.
        /// Expected result: Default property returns the exact same instance as CubicInOut.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Default_Always_ReturnsCubicInOutInstance()
        {
            // Arrange
            var cubicInOut = Easing.CubicInOut;

            // Act
            var result = Easing.Default;

            // Assert
            Assert.Same(cubicInOut, result);
        }

        /// <summary>
        /// Tests that the Default property returns the same instance on multiple calls.
        /// Verifies the property provides consistent behavior and maintains reference equality.
        /// Expected result: Multiple accesses to Default return the same object reference.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Default_MultipleAccesses_ReturnsSameInstance()
        {
            // Act
            var first = Easing.Default;
            var second = Easing.Default;

            // Assert
            Assert.Same(first, second);
        }
    }
}