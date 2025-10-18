#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;


using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    /// <summary>
    /// Unit tests for the RefreshView.On method.
    /// </summary>
    public partial class RefreshViewOnTests
    {
        /// <summary>
        /// Test platform that implements IConfigPlatform for testing purposes.
        /// </summary>
        private class TestPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Another test platform that implements IConfigPlatform for testing purposes.
        /// </summary>
        private class AnotherTestPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Tests that the On method returns a non-null IPlatformElementConfiguration instance.
        /// This verifies the basic functionality and that the method properly delegates to the platform registry.
        /// Expected result: A valid IPlatformElementConfiguration instance is returned.
        /// </summary>
        [Fact]
        public void On_WithValidPlatformType_ReturnsNonNullConfiguration()
        {
            // Arrange
            var refreshView = new RefreshView();

            // Act
            var result = refreshView.On<TestPlatform>();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, RefreshView>>(result);
        }

        /// <summary>
        /// Tests that the On method returns the same instance when called multiple times with the same platform type.
        /// This verifies the caching behavior of the underlying platform configuration registry.
        /// Expected result: The same configuration instance is returned for repeated calls.
        /// </summary>
        [Fact]
        public void On_CalledMultipleTimesWithSamePlatform_ReturnsSameInstance()
        {
            // Arrange
            var refreshView = new RefreshView();

            // Act
            var first = refreshView.On<TestPlatform>();
            var second = refreshView.On<TestPlatform>();

            // Assert
            Assert.Same(first, second);
        }

        /// <summary>
        /// Tests that the On method returns different instances for different platform types.
        /// This verifies that the platform registry correctly manages separate configurations per platform.
        /// Expected result: Different configuration instances are returned for different platform types.
        /// </summary>
        [Fact]
        public void On_CalledWithDifferentPlatforms_ReturnsDifferentInstances()
        {
            // Arrange
            var refreshView = new RefreshView();

            // Act
            var testPlatformConfig = refreshView.On<TestPlatform>();
            var anotherPlatformConfig = refreshView.On<AnotherTestPlatform>();

            // Assert
            Assert.NotSame(testPlatformConfig, anotherPlatformConfig);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, RefreshView>>(testPlatformConfig);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<AnotherTestPlatform, RefreshView>>(anotherPlatformConfig);
        }

        /// <summary>
        /// Tests that the On method works correctly with the lazy initialization of the platform registry.
        /// This verifies that the first call properly initializes the lazy registry and subsequent calls use the same registry.
        /// Expected result: The method works correctly regardless of when the registry is first accessed.
        /// </summary>
        [Fact]
        public void On_FirstCall_InitializesLazyRegistryCorrectly()
        {
            // Arrange
            var refreshView = new RefreshView();

            // Act
            var firstResult = refreshView.On<TestPlatform>();
            var secondResult = refreshView.On<TestPlatform>();

            // Assert
            Assert.NotNull(firstResult);
            Assert.Same(firstResult, secondResult);
        }

        /// <summary>
        /// Tests that multiple RefreshView instances maintain separate platform configurations.
        /// This verifies that each RefreshView has its own platform configuration registry.
        /// Expected result: Different RefreshView instances have separate platform configurations.
        /// </summary>
        [Fact]
        public void On_DifferentRefreshViewInstances_HaveSeparateConfigurations()
        {
            // Arrange
            var refreshView1 = new RefreshView();
            var refreshView2 = new RefreshView();

            // Act
            var config1 = refreshView1.On<TestPlatform>();
            var config2 = refreshView2.On<TestPlatform>();

            // Assert
            Assert.NotNull(config1);
            Assert.NotNull(config2);
            Assert.NotSame(config1, config2);
        }
    }


    /// <summary>
    /// Unit tests for the RefreshColor property of RefreshView class.
    /// </summary>
    public partial class RefreshColorTests : BaseTestFixture
    {
        /// <summary>
        /// Verifies that RefreshColor property returns null when not set (default value).
        /// Tests the default state and ensures the getter retrieves the correct default value.
        /// </summary>
        [Fact]
        public void RefreshColor_DefaultValue_ReturnsNull()
        {
            // Arrange
            var refreshView = new RefreshView();

            // Act
            var result = refreshView.RefreshColor;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that RefreshColor property can be set to null and retrieved correctly.
        /// Tests explicit null assignment and getter behavior with null values.
        /// </summary>
        [Fact]
        public void RefreshColor_SetToNull_ReturnsNull()
        {
            // Arrange
            var refreshView = new RefreshView();

            // Act
            refreshView.RefreshColor = null;
            var result = refreshView.RefreshColor;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that RefreshColor property can be set to various Color values and retrieved correctly.
        /// Tests the round-trip behavior of setting and getting different Color instances.
        /// </summary>
        [Theory]
        [InlineData(0f, 0f, 0f, 1f)] // Black
        [InlineData(1f, 1f, 1f, 1f)] // White  
        [InlineData(1f, 0f, 0f, 1f)] // Red
        [InlineData(0f, 1f, 0f, 1f)] // Green
        [InlineData(0f, 0f, 1f, 1f)] // Blue
        [InlineData(0.5f, 0.5f, 0.5f, 1f)] // Gray
        [InlineData(1f, 0f, 1f, 0.5f)] // Magenta with alpha
        [InlineData(0f, 0f, 0f, 0f)] // Transparent
        public void RefreshColor_SetValidColor_ReturnsSetValue(float red, float green, float blue, float alpha)
        {
            // Arrange
            var refreshView = new RefreshView();
            var color = new Color(red, green, blue, alpha);

            // Act
            refreshView.RefreshColor = color;
            var result = refreshView.RefreshColor;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(color.Red, result.Red);
            Assert.Equal(color.Green, result.Green);
            Assert.Equal(color.Blue, result.Blue);
            Assert.Equal(color.Alpha, result.Alpha);
        }

        /// <summary>
        /// Verifies that RefreshColor property handles Color boundary values correctly.
        /// Tests edge cases with minimum and maximum component values.
        /// </summary>
        [Theory]
        [InlineData(0f, 0f, 0f, 0f)] // All minimum values
        [InlineData(1f, 1f, 1f, 1f)] // All maximum values
        [InlineData(0f, 1f, 0f, 1f)] // Mixed boundary values
        [InlineData(1f, 0f, 1f, 0f)] // Mixed boundary values with transparent
        public void RefreshColor_SetBoundaryValues_ReturnsSetValue(float red, float green, float blue, float alpha)
        {
            // Arrange
            var refreshView = new RefreshView();
            var color = new Color(red, green, blue, alpha);

            // Act
            refreshView.RefreshColor = color;
            var result = refreshView.RefreshColor;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(color.Red, result.Red);
            Assert.Equal(color.Green, result.Green);
            Assert.Equal(color.Blue, result.Blue);
            Assert.Equal(color.Alpha, result.Alpha);
        }

        /// <summary>
        /// Verifies that RefreshColor property can be set multiple times with different values.
        /// Tests the behavior of overwriting previously set values and ensures proper state management.
        /// </summary>
        [Fact]
        public void RefreshColor_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var refreshView = new RefreshView();
            var firstColor = new Color(1f, 0f, 0f, 1f); // Red
            var secondColor = new Color(0f, 1f, 0f, 1f); // Green
            var thirdColor = new Color(0f, 0f, 1f, 1f); // Blue

            // Act & Assert - First set
            refreshView.RefreshColor = firstColor;
            var firstResult = refreshView.RefreshColor;
            Assert.NotNull(firstResult);
            Assert.Equal(firstColor.Red, firstResult.Red);
            Assert.Equal(firstColor.Green, firstResult.Green);
            Assert.Equal(firstColor.Blue, firstResult.Blue);
            Assert.Equal(firstColor.Alpha, firstResult.Alpha);

            // Act & Assert - Second set
            refreshView.RefreshColor = secondColor;
            var secondResult = refreshView.RefreshColor;
            Assert.NotNull(secondResult);
            Assert.Equal(secondColor.Red, secondResult.Red);
            Assert.Equal(secondColor.Green, secondResult.Green);
            Assert.Equal(secondColor.Blue, secondResult.Blue);
            Assert.Equal(secondColor.Alpha, secondResult.Alpha);

            // Act & Assert - Third set
            refreshView.RefreshColor = thirdColor;
            var thirdResult = refreshView.RefreshColor;
            Assert.NotNull(thirdResult);
            Assert.Equal(thirdColor.Red, thirdResult.Red);
            Assert.Equal(thirdColor.Green, thirdResult.Green);
            Assert.Equal(thirdColor.Blue, thirdResult.Blue);
            Assert.Equal(thirdColor.Alpha, thirdResult.Alpha);
        }

        /// <summary>
        /// Verifies that RefreshColor property can be set to a Color and then set back to null.
        /// Tests the transition from a valid Color value back to the default null state.
        /// </summary>
        [Fact]
        public void RefreshColor_SetColorThenNull_ReturnsNull()
        {
            // Arrange
            var refreshView = new RefreshView();
            var color = new Color(0.5f, 0.5f, 0.5f, 1f);

            // Act - Set to color first
            refreshView.RefreshColor = color;
            var colorResult = refreshView.RefreshColor;

            // Assert - Color is set
            Assert.NotNull(colorResult);
            Assert.Equal(color.Red, colorResult.Red);

            // Act - Set back to null
            refreshView.RefreshColor = null;
            var nullResult = refreshView.RefreshColor;

            // Assert - Back to null
            Assert.Null(nullResult);
        }

        /// <summary>
        /// Verifies that RefreshColor property works correctly with Color created from various constructors.
        /// Tests compatibility with different Color instantiation methods.
        /// </summary>
        [Fact]
        public void RefreshColor_SetColorsFromDifferentConstructors_ReturnsCorrectValues()
        {
            // Arrange
            var refreshView = new RefreshView();

            // Act & Assert - Default constructor (black)
            var defaultColor = new Color();
            refreshView.RefreshColor = defaultColor;
            var defaultResult = refreshView.RefreshColor;
            Assert.NotNull(defaultResult);
            Assert.Equal(0f, defaultResult.Red);
            Assert.Equal(0f, defaultResult.Green);
            Assert.Equal(0f, defaultResult.Blue);
            Assert.Equal(1f, defaultResult.Alpha);

            // Act & Assert - RGB constructor
            var rgbColor = new Color(0.8f, 0.6f, 0.4f);
            refreshView.RefreshColor = rgbColor;
            var rgbResult = refreshView.RefreshColor;
            Assert.NotNull(rgbResult);
            Assert.Equal(0.8f, rgbResult.Red);
            Assert.Equal(0.6f, rgbResult.Green);
            Assert.Equal(0.4f, rgbResult.Blue);
            Assert.Equal(1f, rgbResult.Alpha);

            // Act & Assert - RGBA constructor
            var rgbaColor = new Color(0.2f, 0.4f, 0.6f, 0.8f);
            refreshView.RefreshColor = rgbaColor;
            var rgbaResult = refreshView.RefreshColor;
            Assert.NotNull(rgbaResult);
            Assert.Equal(0.2f, rgbaResult.Red);
            Assert.Equal(0.4f, rgbaResult.Green);
            Assert.Equal(0.6f, rgbaResult.Blue);
            Assert.Equal(0.8f, rgbaResult.Alpha);
        }
    }
}