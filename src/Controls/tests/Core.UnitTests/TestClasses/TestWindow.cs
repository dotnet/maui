using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;


using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Primitives;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class TestWindow : Window
    {
        // Because the relationship from Window => Application is a weakreference
        // we need to retain a reference to the Application so it doesn't get GC'd
        TestApp _app;

        public TestWindow()
        {

        }

        public TestWindow(Page page) : base(page)
        {
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == PageProperty.PropertyName &&
                Parent == null)
            {
                _app = new TestApp(this);
                _ = (_app as IApplication).CreateWindow(null);
            }
        }
    }


    public static class TestWindowExtensions
    {
        public static T AddToTestWindow<T>(this T page)
            where T : Page
        {
            return (T)new TestWindow(page).Page;
        }
    }

    public partial class WindowTests
    {
        /// <summary>
        /// Tests that MaximumHeight getter returns the default value when no explicit value has been set.
        /// This test covers the not-covered getter line by verifying it correctly retrieves the default value.
        /// </summary>
        [Fact]
        public void MaximumHeight_Get_ReturnsDefaultValueWhenNotSet()
        {
            // Arrange
            var window = new TestWindow();

            // Act
            var result = window.MaximumHeight;

            // Assert
            Assert.Equal(Dimension.Maximum, result);
        }

        /// <summary>
        /// Tests that MaximumHeight getter returns the correct value after setting it.
        /// This test covers the not-covered getter line by verifying it correctly retrieves set values.
        /// </summary>
        /// <param name="value">The value to set and retrieve</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(100.0)]
        [InlineData(1920.0)]
        [InlineData(-1.0)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        public void MaximumHeight_Get_ReturnsSetValue(double value)
        {
            // Arrange
            var window = new TestWindow();

            // Act
            window.MaximumHeight = value;
            var result = window.MaximumHeight;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that MaximumHeight getter correctly handles special double values.
        /// This test covers the not-covered getter line with edge case values.
        /// </summary>
        /// <param name="value">The special double value to test</param>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void MaximumHeight_Get_HandlesSpecialDoubleValues(double value)
        {
            // Arrange
            var window = new TestWindow();

            // Act
            window.MaximumHeight = value;
            var result = window.MaximumHeight;

            // Assert
            if (double.IsNaN(value))
            {
                Assert.True(double.IsNaN(result));
            }
            else
            {
                Assert.Equal(value, result);
            }
        }

        /// <summary>
        /// Tests that MaximumHeight setter correctly stores positive values.
        /// This test verifies the setter works correctly with valid positive values.
        /// </summary>
        /// <param name="value">The positive value to set</param>
        [Theory]
        [InlineData(1.0)]
        [InlineData(100.0)]
        [InlineData(1920.0)]
        [InlineData(3840.0)]
        public void MaximumHeight_Set_AcceptsValidPositiveValues(double value)
        {
            // Arrange
            var window = new TestWindow();

            // Act
            window.MaximumHeight = value;

            // Assert
            Assert.Equal(value, window.MaximumHeight);
        }

        /// <summary>
        /// Tests that MaximumHeight setter correctly stores zero value.
        /// This test verifies the setter works correctly with zero.
        /// </summary>
        [Fact]
        public void MaximumHeight_Set_AcceptsZeroValue()
        {
            // Arrange
            var window = new TestWindow();

            // Act
            window.MaximumHeight = 0.0;

            // Assert
            Assert.Equal(0.0, window.MaximumHeight);
        }

        /// <summary>
        /// Tests that MaximumHeight setter correctly stores negative values.
        /// This test verifies the setter behavior with negative values.
        /// </summary>
        /// <param name="value">The negative value to set</param>
        [Theory]
        [InlineData(-1.0)]
        [InlineData(-100.0)]
        [InlineData(-1920.0)]
        public void MaximumHeight_Set_AcceptsNegativeValues(double value)
        {
            // Arrange
            var window = new TestWindow();

            // Act
            window.MaximumHeight = value;

            // Assert
            Assert.Equal(value, window.MaximumHeight);
        }

        /// <summary>
        /// Tests that MaximumHeight property can be set multiple times with different values.
        /// This test covers the not-covered getter line by verifying it returns updated values.
        /// </summary>
        [Fact]
        public void MaximumHeight_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var window = new TestWindow();

            // Act & Assert - Set and verify first value
            window.MaximumHeight = 100.0;
            Assert.Equal(100.0, window.MaximumHeight);

            // Act & Assert - Set and verify second value
            window.MaximumHeight = 200.0;
            Assert.Equal(200.0, window.MaximumHeight);

            // Act & Assert - Set and verify third value
            window.MaximumHeight = 0.0;
            Assert.Equal(0.0, window.MaximumHeight);
        }

        /// <summary>
        /// Tests that MaximumHeight property preserves precision for decimal values.
        /// This test covers the not-covered getter line with precise decimal values.
        /// </summary>
        /// <param name="value">The decimal value to test precision with</param>
        [Theory]
        [InlineData(123.456789)]
        [InlineData(0.123456789)]
        [InlineData(999.999999)]
        public void MaximumHeight_SetGet_PreservesDecimalPrecision(double value)
        {
            // Arrange
            var window = new TestWindow();

            // Act
            window.MaximumHeight = value;
            var result = window.MaximumHeight;

            // Assert
            Assert.Equal(value, result, precision: 15);
        }

        /// <summary>
        /// Tests that MinimumHeight getter returns the default value when no value has been set.
        /// Input: New Window instance with no MinimumHeight set.
        /// Expected: Returns Dimension.Minimum (default value).
        /// </summary>
        [Fact]
        public void MinimumHeight_DefaultValue_ReturnsDimensionMinimum()
        {
            // Arrange
            var window = new Window();

            // Act
            var result = window.MinimumHeight;

            // Assert
            Assert.Equal(Dimension.Minimum, result);
        }

        /// <summary>
        /// Tests that MinimumHeight getter returns the correct value after setting various valid values.
        /// Input: Various valid double values including boundary values.
        /// Expected: Getter returns the exact value that was set.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(100.5)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(-1.0)]
        [InlineData(double.Epsilon)]
        public void MinimumHeight_SetValidValue_GetterReturnsSetValue(double value)
        {
            // Arrange
            var window = new Window();

            // Act
            window.MinimumHeight = value;
            var result = window.MinimumHeight;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that MinimumHeight getter handles special double values correctly.
        /// Input: Special double values like NaN and Infinity.
        /// Expected: Getter returns the exact special value that was set.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void MinimumHeight_SetSpecialDoubleValue_GetterReturnsSetValue(double value)
        {
            // Arrange
            var window = new Window();

            // Act
            window.MinimumHeight = value;
            var result = window.MinimumHeight;

            // Assert
            if (double.IsNaN(value))
            {
                Assert.True(double.IsNaN(result));
            }
            else
            {
                Assert.Equal(value, result);
            }
        }

        /// <summary>
        /// Tests that MinimumHeight getter works correctly after setting the value multiple times.
        /// Input: Set MinimumHeight to different values sequentially.
        /// Expected: Getter always returns the most recently set value.
        /// </summary>
        [Fact]
        public void MinimumHeight_SetMultipleTimes_GetterReturnsLatestValue()
        {
            // Arrange
            var window = new Window();

            // Act & Assert
            window.MinimumHeight = 100.0;
            Assert.Equal(100.0, window.MinimumHeight);

            window.MinimumHeight = 200.5;
            Assert.Equal(200.5, window.MinimumHeight);

            window.MinimumHeight = 0.0;
            Assert.Equal(0.0, window.MinimumHeight);
        }
    }
}