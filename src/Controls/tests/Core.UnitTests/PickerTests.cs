#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    /// <summary>
    /// Tests for the Picker.FontSize property.
    /// </summary>
    public partial class PickerFontSizeTests
    {
        /// <summary>
        /// Tests that FontSize getter returns the correct value when set to a normal positive value.
        /// </summary>
        [Theory]
        [InlineData(12.0)]
        [InlineData(14.5)]
        [InlineData(16.0)]
        [InlineData(18.0)]
        [InlineData(20.5)]
        public void FontSize_SetPositiveValue_ReturnsCorrectValue(double expectedFontSize)
        {
            // Arrange
            var picker = new Picker();

            // Act
            picker.FontSize = expectedFontSize;
            var actualFontSize = picker.FontSize;

            // Assert
            Assert.Equal(expectedFontSize, actualFontSize);
        }

        /// <summary>
        /// Tests that FontSize can be set and retrieved with zero value.
        /// </summary>
        [Fact]
        public void FontSize_SetZero_ReturnsZero()
        {
            // Arrange
            var picker = new Picker();

            // Act
            picker.FontSize = 0.0;
            var actualFontSize = picker.FontSize;

            // Assert
            Assert.Equal(0.0, actualFontSize);
        }

        /// <summary>
        /// Tests that FontSize can be set and retrieved with negative values.
        /// </summary>
        [Theory]
        [InlineData(-1.0)]
        [InlineData(-5.5)]
        [InlineData(-10.0)]
        public void FontSize_SetNegativeValue_ReturnsNegativeValue(double expectedFontSize)
        {
            // Arrange
            var picker = new Picker();

            // Act
            picker.FontSize = expectedFontSize;
            var actualFontSize = picker.FontSize;

            // Assert
            Assert.Equal(expectedFontSize, actualFontSize);
        }

        /// <summary>
        /// Tests that FontSize handles double.MinValue correctly.
        /// </summary>
        [Fact]
        public void FontSize_SetMinValue_ReturnsMinValue()
        {
            // Arrange
            var picker = new Picker();

            // Act
            picker.FontSize = double.MinValue;
            var actualFontSize = picker.FontSize;

            // Assert
            Assert.Equal(double.MinValue, actualFontSize);
        }

        /// <summary>
        /// Tests that FontSize handles double.MaxValue correctly.
        /// </summary>
        [Fact]
        public void FontSize_SetMaxValue_ReturnsMaxValue()
        {
            // Arrange
            var picker = new Picker();

            // Act
            picker.FontSize = double.MaxValue;
            var actualFontSize = picker.FontSize;

            // Assert
            Assert.Equal(double.MaxValue, actualFontSize);
        }

        /// <summary>
        /// Tests that FontSize handles double.NaN correctly.
        /// </summary>
        [Fact]
        public void FontSize_SetNaN_ReturnsNaN()
        {
            // Arrange
            var picker = new Picker();

            // Act
            picker.FontSize = double.NaN;
            var actualFontSize = picker.FontSize;

            // Assert
            Assert.True(double.IsNaN(actualFontSize));
        }

        /// <summary>
        /// Tests that FontSize handles double.PositiveInfinity correctly.
        /// </summary>
        [Fact]
        public void FontSize_SetPositiveInfinity_ReturnsPositiveInfinity()
        {
            // Arrange
            var picker = new Picker();

            // Act
            picker.FontSize = double.PositiveInfinity;
            var actualFontSize = picker.FontSize;

            // Assert
            Assert.Equal(double.PositiveInfinity, actualFontSize);
        }

        /// <summary>
        /// Tests that FontSize handles double.NegativeInfinity correctly.
        /// </summary>
        [Fact]
        public void FontSize_SetNegativeInfinity_ReturnsNegativeInfinity()
        {
            // Arrange
            var picker = new Picker();

            // Act
            picker.FontSize = double.NegativeInfinity;
            var actualFontSize = picker.FontSize;

            // Assert
            Assert.Equal(double.NegativeInfinity, actualFontSize);
        }

        /// <summary>
        /// Tests that FontSize handles very small positive values correctly.
        /// </summary>
        [Fact]
        public void FontSize_SetEpsilon_ReturnsEpsilon()
        {
            // Arrange
            var picker = new Picker();

            // Act
            picker.FontSize = double.Epsilon;
            var actualFontSize = picker.FontSize;

            // Assert
            Assert.Equal(double.Epsilon, actualFontSize);
        }

        /// <summary>
        /// Tests that FontSize returns the default value when not explicitly set.
        /// </summary>
        [Fact]
        public void FontSize_DefaultValue_ReturnsExpectedDefault()
        {
            // Arrange
            var picker = new Picker();

            // Act
            var actualFontSize = picker.FontSize;

            // Assert
            // The default value should be retrieved from the FontSizeProperty default value
            Assert.True(actualFontSize >= 0 || double.IsNaN(actualFontSize), "Default FontSize should be non-negative or NaN");
        }

        /// <summary>
        /// Tests that multiple consecutive sets and gets work correctly.
        /// </summary>
        [Fact]
        public void FontSize_MultipleSetAndGet_WorksCorrectly()
        {
            // Arrange
            var picker = new Picker();
            var values = new double[] { 12.0, 16.0, 0.0, -5.0, 24.5 };

            foreach (var expectedValue in values)
            {
                // Act
                picker.FontSize = expectedValue;
                var actualValue = picker.FontSize;

                // Assert
                Assert.Equal(expectedValue, actualValue);
            }
        }

        /// <summary>
        /// Tests that FontSize property works correctly with decimal precision.
        /// </summary>
        [Theory]
        [InlineData(12.123456789)]
        [InlineData(16.999999999)]
        [InlineData(0.000000001)]
        [InlineData(1234.56789)]
        public void FontSize_SetHighPrecisionValue_RetainsPrecision(double expectedFontSize)
        {
            // Arrange
            var picker = new Picker();

            // Act
            picker.FontSize = expectedFontSize;
            var actualFontSize = picker.FontSize;

            // Assert
            Assert.Equal(expectedFontSize, actualFontSize);
        }
    }
}