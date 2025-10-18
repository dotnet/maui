#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class FrameUnitTests : BaseTestFixture
    {
        [Fact]
        public void TestConstructor()
        {
            Frame frame = new Frame();

            Assert.Null(frame.Content);
            Assert.Equal(new Thickness(20, 20, 20, 20), frame.Padding);
        }

        [Fact]
        public void TestSetChild()
        {
            Frame frame = new Frame();

            var child1 = new Label();

            bool added = false;

            frame.ChildAdded += (sender, e) => added = true;

            frame.Content = child1;

            Assert.True(added);
            Assert.Equal(child1, frame.Content);
            Assert.Equal(child1.Parent, frame);

            added = false;
            frame.Content = child1;

            Assert.False(added);
        }

        [Fact]
        public void TestReplaceChild()
        {
            Frame frame = new Frame();

            var child1 = new Label();
            var child2 = new Label();

            frame.Content = child1;

            bool removed = false;
            bool added = false;

            frame.ChildRemoved += (sender, e) => removed = true;
            frame.ChildAdded += (sender, e) => added = true;

            frame.Content = child2;
            Assert.Null(child1.Parent);

            Assert.True(removed);
            Assert.True(added);
            Assert.Equal(child2, frame.Content);
        }

        [Fact]
        public void TestFrameLayout()
        {
            View child;
            var mockViewHandler = Substitute.For<IViewHandler>();

            // Set up the mock handler to properly participate in the layout system
            mockViewHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo =>
            {
                // Extract the constraints passed to GetDesiredSize
                var width = (double)callInfo[0];
                var height = (double)callInfo[1];

                // Return the child's requested size
                return new Size(100, 200);
            });

            var frame = new Frame
            {
                Content = child = new View
                {
                    WidthRequest = 100,
                    HeightRequest = 200,
                    IsPlatformEnabled = true,
                    Handler = mockViewHandler
                },
                IsPlatformEnabled = true,
            };

            ICrossPlatformLayout crossPlatformLayout = frame;

            Assert.Equal(new Size(140, 240), crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity));

            crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 300, 300));

            Assert.Equal(new Rect(100, 50, 100, 200), child.Bounds);

            // Verify the PlatformArrange method was called on the handler with the correct frame
            mockViewHandler.Received().PlatformArrange(new Rect(100, 50, 100, 200));
        }

        [Fact]
        public void TestDoesNotThrowOnSetNullChild()
        {
            _ = new Frame { Content = null };
        }

        [Fact]
        public void WidthRequest()
        {
            View child;
            var mockViewHandler = Substitute.For<IViewHandler>();

            // Set up the mock handler to properly participate in the layout system
            mockViewHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo =>
            {
                // Extract the constraints passed to GetDesiredSize
                var width = (double)callInfo[0];
                var height = (double)callInfo[1];

                // Return the child's requested size
                return new Size(100, 200);
            });

            var frame = new Frame
            {
                Content = child = new View
                {
                    WidthRequest = 100,
                    HeightRequest = 200,
                    IsPlatformEnabled = true,
                    Handler = mockViewHandler
                },
                IsPlatformEnabled = true,
                WidthRequest = 20
            };

            // Get the ICrossPlatformLayout implementation
            ICrossPlatformLayout crossPlatformLayout = frame;

            Assert.Equal(new Size(140, 240), crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity));
        }

        [Fact]
        public void HeightRequest()
        {
            View child;
            var mockViewHandler = Substitute.For<IViewHandler>();

            // Set up the mock handler to properly participate in the layout system
            mockViewHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo =>
            {
                // Extract the constraints passed to GetDesiredSize
                var width = (double)callInfo[0];
                var height = (double)callInfo[1];

                // Return the child's requested size
                return new Size(100, 200);
            });

            var frame = new Frame
            {
                Content = child = new View
                {
                    WidthRequest = 100,
                    HeightRequest = 200,
                    IsPlatformEnabled = true,
                    Handler = mockViewHandler
                },
                IsPlatformEnabled = true,
                HeightRequest = 20
            };

            // Get the ICrossPlatformLayout implementation
            ICrossPlatformLayout crossPlatformLayout = frame;

            Assert.Equal(new Size(140, 240), crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity));
        }

        [Fact]
        public void LayoutVerticallyCenter()
        {
            View child;
            var mockViewHandler = Substitute.For<IViewHandler>();

            // Set up the mock handler to properly participate in the layout system
            mockViewHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo =>
            {
                // Extract the constraints passed to GetDesiredSize
                var width = (double)callInfo[0];
                var height = (double)callInfo[1];

                // Return the child's requested size
                return new Size(100, 100);
            });

            var frame = new Frame
            {
                Content = child = new View
                {
                    WidthRequest = 100,
                    HeightRequest = 100,
                    IsPlatformEnabled = true,
                    VerticalOptions = LayoutOptions.Center,
                    Handler = mockViewHandler
                },
                IsPlatformEnabled = true,
            };

            ICrossPlatformLayout crossPlatformLayout = frame;
            crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity);
            crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 200, 200));

            Assert.Equal(new Rect(50, 50, 100, 100), child.Bounds);

            // Verify the PlatformArrange method was called on the handler with the correct frame
            mockViewHandler.Received().PlatformArrange(new Rect(50, 50, 100, 100));
        }

        [Fact]
        public void LayoutVerticallyBegin()
        {
            View child;
            var mockViewHandler = Substitute.For<IViewHandler>();

            // Set up the mock handler to properly participate in the layout system
            mockViewHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo =>
            {
                // Extract the constraints passed to GetDesiredSize
                var width = (double)callInfo[0];
                var height = (double)callInfo[1];

                // Return the child's requested size
                return new Size(100, 100);
            });

            var frame = new Frame
            {
                Content = child = new View
                {
                    WidthRequest = 100,
                    HeightRequest = 100,
                    IsPlatformEnabled = true,
                    VerticalOptions = LayoutOptions.Start,
                    Handler = mockViewHandler
                },
                IsPlatformEnabled = true,
            };

            ICrossPlatformLayout crossPlatformLayout = frame;
            crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity);
            crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 200, 200));

            Assert.Equal(new Rect(50, 20, 100, 100), child.Bounds);

            // Verify the PlatformArrange method was called on the handler with the correct frame
            mockViewHandler.Received().PlatformArrange(new Rect(50, 20, 100, 100));
        }

        [Fact]
        public void LayoutVerticallyEnd()
        {
            View child;
            var mockViewHandler = Substitute.For<IViewHandler>();

            // Set up the mock handler to properly participate in the layout system
            mockViewHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo =>
            {
                // Extract the constraints passed to GetDesiredSize
                var width = (double)callInfo[0];
                var height = (double)callInfo[1];

                // Return the child's requested size
                return new Size(100, 100);
            });

            var frame = new Frame
            {
                Content = child = new View
                {
                    WidthRequest = 100,
                    HeightRequest = 100,
                    IsPlatformEnabled = true,
                    VerticalOptions = LayoutOptions.End,
                    Handler = mockViewHandler
                },
                IsPlatformEnabled = true,
            };

            ICrossPlatformLayout crossPlatformLayout = frame;
            crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity);
            crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 200, 200));

            Assert.Equal(new Rect(50, 80, 100, 100), child.Bounds);

            // Verify the PlatformArrange method was called on the handler with the correct frame
            mockViewHandler.Received().PlatformArrange(new Rect(50, 80, 100, 100));
        }

        [Fact]
        public void LayoutHorizontallyCenter()
        {
            View child;
            var mockViewHandler = Substitute.For<IViewHandler>();

            // Set up the mock handler to properly participate in the layout system
            mockViewHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo =>
            {
                // Extract the constraints passed to GetDesiredSize
                var width = (double)callInfo[0];
                var height = (double)callInfo[1];

                // Return the child's requested size
                return new Size(100, 100);
            });

            var frame = new Frame
            {
                Content = child = new View
                {
                    WidthRequest = 100,
                    HeightRequest = 100,
                    IsPlatformEnabled = true,
                    HorizontalOptions = LayoutOptions.Center,
                    Handler = mockViewHandler
                },
                IsPlatformEnabled = true,
            };

            ICrossPlatformLayout crossPlatformLayout = frame;
            crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity);
            crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 200, 200));

            Assert.Equal(new Rect(50, 50, 100, 100), child.Bounds);

            // Verify the PlatformArrange method was called on the handler with the correct frame
            mockViewHandler.Received().PlatformArrange(new Rect(50, 50, 100, 100));
        }

        [Fact]
        public void LayoutHorizontallyBegin()
        {
            View child;
            var mockViewHandler = Substitute.For<IViewHandler>();

            // Set up the mock handler to properly participate in the layout system
            mockViewHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo =>
            {
                // Extract the constraints passed to GetDesiredSize
                var width = (double)callInfo[0];
                var height = (double)callInfo[1];

                // Return the child's requested size
                return new Size(100, 100);
            });

            var frame = new Frame
            {
                Content = child = new View
                {
                    WidthRequest = 100,
                    HeightRequest = 100,
                    IsPlatformEnabled = true,
                    HorizontalOptions = LayoutOptions.Start,
                    Handler = mockViewHandler
                },
                IsPlatformEnabled = true,
            };

            ICrossPlatformLayout crossPlatformLayout = frame;
            crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity);
            crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 200, 200));

            Assert.Equal(new Rect(20, 50, 100, 100), child.Bounds);

            // Verify the PlatformArrange method was called on the handler with the correct frame
            mockViewHandler.Received().PlatformArrange(new Rect(20, 50, 100, 100));
        }

        [Fact]
        public void LayoutHorizontallyEnd()
        {
            View child;
            var mockViewHandler = Substitute.For<IViewHandler>();

            // Set up the mock handler to properly participate in the layout system
            mockViewHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo =>
            {
                // Extract the constraints passed to GetDesiredSize
                var width = (double)callInfo[0];
                var height = (double)callInfo[1];

                // Return the child's requested size
                return new Size(100, 100);
            });

            var frame = new Frame
            {
                Content = child = new View
                {
                    WidthRequest = 100,
                    HeightRequest = 100,
                    IsPlatformEnabled = true,
                    HorizontalOptions = LayoutOptions.End,
                    Handler = mockViewHandler
                },
                IsPlatformEnabled = true,
            };

            ICrossPlatformLayout crossPlatformLayout = frame;
            crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity);
            crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 200, 200));

            Assert.Equal(new Rect(80, 50, 100, 100), child.Bounds);

            // Verify the PlatformArrange method was called on the handler with the correct frame
            mockViewHandler.Received().PlatformArrange(new Rect(80, 50, 100, 100));
        }

        [Fact]
        public void SettingPaddingThroughStyle()
        {
            var frame = new Frame
            {
                Style = new Style(typeof(Frame))
                {
                    Setters = {
                        new Setter {Property = Layout.PaddingProperty, Value = 0}
                    }
                }
            };

            Assert.Equal(new Thickness(0), frame.Padding);

        }
    }

    public partial class FrameTests
    {
        /// <summary>
        /// Tests that the CornerRadius property returns the default value of -1.0f when not explicitly set.
        /// </summary>
        [Fact]
        public void CornerRadius_DefaultValue_ReturnsNegativeOne()
        {
            // Arrange
            var frame = new Frame();

            // Act
            var cornerRadius = frame.CornerRadius;

            // Assert
            Assert.Equal(-1.0f, cornerRadius);
        }

        /// <summary>
        /// Tests that the CornerRadius property can be set to and retrieve valid positive values.
        /// </summary>
        /// <param name="value">The corner radius value to test</param>
        /// <param name="description">Description of the test case</param>
        [Theory]
        [InlineData(-1.0f, "explicit default value")]
        [InlineData(0f, "zero boundary value")]
        [InlineData(0.1f, "small positive value")]
        [InlineData(1.0f, "unit value")]
        [InlineData(10.5f, "decimal positive value")]
        [InlineData(100.0f, "large positive value")]
        [InlineData(3.4028235E+38f, "float max value")]
        public void CornerRadius_SetValidValue_ReturnsSetValue(float value, string description)
        {
            // Arrange
            var frame = new Frame();

            // Act
            frame.CornerRadius = value;
            var actualValue = frame.CornerRadius;

            // Assert
            Assert.Equal(value, actualValue);
        }

        /// <summary>
        /// Tests that setting CornerRadius to invalid negative values (other than -1.0f) throws an ArgumentException.
        /// </summary>
        /// <param name="value">The invalid corner radius value to test</param>
        /// <param name="description">Description of the test case</param>
        [Theory]
        [InlineData(-0.1f, "small negative value")]
        [InlineData(-1.1f, "just below default")]
        [InlineData(-10.0f, "large negative value")]
        [InlineData(-3.4028235E+38f, "float min value")]
        public void CornerRadius_SetInvalidNegativeValue_ThrowsArgumentException(float value, string description)
        {
            // Arrange
            var frame = new Frame();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => frame.CornerRadius = value);
            Assert.NotNull(exception);
        }

        /// <summary>
        /// Tests that setting CornerRadius to special float values throws an ArgumentException.
        /// </summary>
        /// <param name="value">The special float value to test</param>
        /// <param name="description">Description of the test case</param>
        [Theory]
        [InlineData(float.NaN, "NaN value")]
        [InlineData(float.PositiveInfinity, "positive infinity")]
        [InlineData(float.NegativeInfinity, "negative infinity")]
        public void CornerRadius_SetSpecialFloatValue_ThrowsArgumentException(float value, string description)
        {
            // Arrange
            var frame = new Frame();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => frame.CornerRadius = value);
            Assert.NotNull(exception);
        }

        /// <summary>
        /// Tests that the CornerRadius getter retrieves the value from the underlying BindableProperty system.
        /// </summary>
        [Fact]
        public void CornerRadius_GetterBehavior_RetrievesFromBindableProperty()
        {
            // Arrange
            var frame = new Frame();
            var testValue = 25.5f;

            // Act - Set through property
            frame.CornerRadius = testValue;

            // Get through property to test getter path
            var retrievedValue = frame.CornerRadius;

            // Assert
            Assert.Equal(testValue, retrievedValue);
        }

        /// <summary>
        /// Tests the CornerRadius property behavior across multiple set/get operations.
        /// </summary>
        [Fact]
        public void CornerRadius_MultipleSetGet_MaintainsValueConsistency()
        {
            // Arrange
            var frame = new Frame();

            // Act & Assert - Test sequence of different valid values
            frame.CornerRadius = 0f;
            Assert.Equal(0f, frame.CornerRadius);

            frame.CornerRadius = 15.75f;
            Assert.Equal(15.75f, frame.CornerRadius);

            frame.CornerRadius = -1.0f;
            Assert.Equal(-1.0f, frame.CornerRadius);

            frame.CornerRadius = 200f;
            Assert.Equal(200f, frame.CornerRadius);
        }

        /// <summary>
        /// Tests that On method returns a valid IPlatformElementConfiguration for a valid IConfigPlatform implementation.
        /// This test verifies the basic functionality of the platform configuration system.
        /// </summary>
        [Fact]
        public void On_ValidPlatformType_ReturnsConfiguration()
        {
            // Arrange
            var frame = new Frame();

            // Act
            var result = frame.On<TestPlatform>();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, Frame>>(result);
        }

        /// <summary>
        /// Tests that On method returns the same configuration instance when called multiple times with the same platform type.
        /// This verifies the caching behavior of the platform configuration registry.
        /// </summary>
        [Fact]
        public void On_SamePlatformTypeCalledTwice_ReturnsSameInstance()
        {
            // Arrange
            var frame = new Frame();

            // Act
            var result1 = frame.On<TestPlatform>();
            var result2 = frame.On<TestPlatform>();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that On method returns different configuration instances for different platform types.
        /// This verifies that the platform configuration registry properly separates configurations by platform type.
        /// </summary>
        [Fact]
        public void On_DifferentPlatformTypes_ReturnsDifferentInstances()
        {
            // Arrange
            var frame = new Frame();

            // Act
            var result1 = frame.On<TestPlatform>();
            var result2 = frame.On<AnotherTestPlatform>();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotSame(result1, result2);
        }

        /// <summary>
        /// Tests that On method works correctly when called on multiple Frame instances.
        /// This verifies that each Frame maintains its own platform configuration registry.
        /// </summary>
        [Fact]
        public void On_DifferentFrameInstances_ReturnsDifferentConfigurations()
        {
            // Arrange
            var frame1 = new Frame();
            var frame2 = new Frame();

            // Act
            var result1 = frame1.On<TestPlatform>();
            var result2 = frame2.On<TestPlatform>();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotSame(result1, result2);
        }

        /// <summary>
        /// Tests that On method maintains configuration state across multiple calls with mixed platform types.
        /// This verifies complex caching scenarios with multiple platform types.
        /// </summary>
        [Fact]
        public void On_MixedPlatformTypeCalls_MaintainsCorrectCaching()
        {
            // Arrange
            var frame = new Frame();

            // Act
            var testPlatform1 = frame.On<TestPlatform>();
            var anotherPlatform1 = frame.On<AnotherTestPlatform>();
            var testPlatform2 = frame.On<TestPlatform>();
            var anotherPlatform2 = frame.On<AnotherTestPlatform>();

            // Assert
            Assert.NotNull(testPlatform1);
            Assert.NotNull(anotherPlatform1);
            Assert.NotNull(testPlatform2);
            Assert.NotNull(anotherPlatform2);

            // Same platform type should return same instance
            Assert.Same(testPlatform1, testPlatform2);
            Assert.Same(anotherPlatform1, anotherPlatform2);

            // Different platform types should return different instances
            Assert.NotSame(testPlatform1, anotherPlatform1);
        }

        // Test platform implementations as inner classes
        internal class TestPlatform : IConfigPlatform
        {
        }

        internal class AnotherTestPlatform : IConfigPlatform
        {
        }
    }


    /// <summary>
    /// Tests for the FrameExtensions class
    /// </summary>
    public class FrameExtensionsTests
    {
        /// <summary>
        /// Tests that IsClippedToBoundsSet returns the frame's IsClippedToBounds value when the property is set to true
        /// </summary>
        [Fact]
        public void IsClippedToBoundsSet_WhenPropertyIsSetAndIsClippedToBoundsIsTrue_ReturnsTrue()
        {
            // Arrange
            var frame = Substitute.For<Frame>();
            frame.IsSet(Layout.IsClippedToBoundsProperty).Returns(true);
            frame.IsClippedToBounds.Returns(true);
            var defaultValue = false;

            // Act
            var result = FrameExtensions.IsClippedToBoundsSet(frame, defaultValue);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsClippedToBoundsSet returns the frame's IsClippedToBounds value when the property is set to false
        /// </summary>
        [Fact]
        public void IsClippedToBoundsSet_WhenPropertyIsSetAndIsClippedToBoundsIsFalse_ReturnsFalse()
        {
            // Arrange
            var frame = Substitute.For<Frame>();
            frame.IsSet(Layout.IsClippedToBoundsProperty).Returns(true);
            frame.IsClippedToBounds.Returns(false);
            var defaultValue = true;

            // Act
            var result = FrameExtensions.IsClippedToBoundsSet(frame, defaultValue);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsClippedToBoundsSet returns the default value when the property is not set and defaultValue is true
        /// </summary>
        [Fact]
        public void IsClippedToBoundsSet_WhenPropertyIsNotSetAndDefaultValueIsTrue_ReturnsTrue()
        {
            // Arrange
            var frame = Substitute.For<Frame>();
            frame.IsSet(Layout.IsClippedToBoundsProperty).Returns(false);
            frame.IsClippedToBounds.Returns(false);
            var defaultValue = true;

            // Act
            var result = FrameExtensions.IsClippedToBoundsSet(frame, defaultValue);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsClippedToBoundsSet returns the default value when the property is not set and defaultValue is false
        /// </summary>
        [Fact]
        public void IsClippedToBoundsSet_WhenPropertyIsNotSetAndDefaultValueIsFalse_ReturnsFalse()
        {
            // Arrange
            var frame = Substitute.For<Frame>();
            frame.IsSet(Layout.IsClippedToBoundsProperty).Returns(false);
            frame.IsClippedToBounds.Returns(true);
            var defaultValue = false;

            // Act
            var result = FrameExtensions.IsClippedToBoundsSet(frame, defaultValue);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsClippedToBoundsSet throws ArgumentNullException when frame parameter is null
        /// </summary>
        [Fact]
        public void IsClippedToBoundsSet_WhenFrameIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            Frame frame = null;
            var defaultValue = true;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => FrameExtensions.IsClippedToBoundsSet(frame, defaultValue));
        }

        /// <summary>
        /// Tests the behavior with both edge case boolean values for comprehensive coverage
        /// </summary>
        [Theory]
        [InlineData(true, true, false, true)]   // Property set, IsClippedToBounds=true, defaultValue=false -> returns true
        [InlineData(true, false, true, false)]  // Property set, IsClippedToBounds=false, defaultValue=true -> returns false
        [InlineData(false, true, true, true)]   // Property not set, IsClippedToBounds=true, defaultValue=true -> returns true
        [InlineData(false, false, false, false)] // Property not set, IsClippedToBounds=false, defaultValue=false -> returns false
        public void IsClippedToBoundsSet_VariousInputCombinations_ReturnsExpectedValue(
            bool isPropertySet, bool isClippedToBounds, bool defaultValue, bool expectedResult)
        {
            // Arrange
            var frame = Substitute.For<Frame>();
            frame.IsSet(Layout.IsClippedToBoundsProperty).Returns(isPropertySet);
            frame.IsClippedToBounds.Returns(isClippedToBounds);

            // Act
            var result = FrameExtensions.IsClippedToBoundsSet(frame, defaultValue);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }


    /// <summary>
    /// Tests for the Frame.BorderColor property to ensure proper getter and setter functionality.
    /// </summary>
    public partial class FrameBorderColorTests
    {
        /// <summary>
        /// Tests that BorderColor property returns null by default when no value has been set.
        /// This verifies the default value behavior and exercises the getter.
        /// </summary>
        [Fact]
        public void BorderColor_DefaultValue_ReturnsNull()
        {
            // Arrange
            var frame = new Frame();

            // Act
            var borderColor = frame.BorderColor;

            // Assert
            Assert.Null(borderColor);
        }

        /// <summary>
        /// Tests setting and getting the BorderColor property with various color values.
        /// This verifies that the property correctly stores and retrieves Color objects,
        /// exercising both getter and setter functionality.
        /// </summary>
        /// <param name="expectedRed">The expected red component (0.0 to 1.0)</param>
        /// <param name="expectedGreen">The expected green component (0.0 to 1.0)</param>
        /// <param name="expectedBlue">The expected blue component (0.0 to 1.0)</param>
        /// <param name="expectedAlpha">The expected alpha component (0.0 to 1.0)</param>
        [Theory]
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Green
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Blue
        [InlineData(0.0f, 0.0f, 0.0f, 1.0f)] // Black
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // White
        [InlineData(0.5f, 0.5f, 0.5f, 0.5f)] // Semi-transparent gray
        [InlineData(1.0f, 0.0f, 0.0f, 0.0f)] // Transparent red
        public void BorderColor_SetAndGet_ReturnsExpectedColor(float expectedRed, float expectedGreen, float expectedBlue, float expectedAlpha)
        {
            // Arrange
            var frame = new Frame();
            var expectedColor = new Color(expectedRed, expectedGreen, expectedBlue, expectedAlpha);

            // Act
            frame.BorderColor = expectedColor;
            var actualColor = frame.BorderColor;

            // Assert
            Assert.NotNull(actualColor);
            Assert.Equal(expectedColor.Red, actualColor.Red);
            Assert.Equal(expectedColor.Green, actualColor.Green);
            Assert.Equal(expectedColor.Blue, actualColor.Blue);
            Assert.Equal(expectedColor.Alpha, actualColor.Alpha);
        }

        /// <summary>
        /// Tests setting BorderColor to null and verifying it can be retrieved as null.
        /// This ensures the property properly handles null values and exercises the getter.
        /// </summary>
        [Fact]
        public void BorderColor_SetToNull_ReturnsNull()
        {
            // Arrange
            var frame = new Frame();
            frame.BorderColor = new Color(1.0f, 0.0f, 0.0f, 1.0f); // Set to red first

            // Act
            frame.BorderColor = null;
            var actualColor = frame.BorderColor;

            // Assert
            Assert.Null(actualColor);
        }

        /// <summary>
        /// Tests setting BorderColor using Color factory methods.
        /// This verifies that colors created through different factory methods
        /// are properly stored and retrieved, exercising the getter functionality.
        /// </summary>
        [Fact]
        public void BorderColor_SetUsingFactoryMethods_ReturnsExpectedColor()
        {
            // Arrange
            var frame = new Frame();
            var rgbColor = Color.FromRgb(255, 128, 64); // Orange-ish color
            var rgbaColor = Color.FromRgba(128, 64, 192, 128); // Purple with transparency

            // Act & Assert - RGB color
            frame.BorderColor = rgbColor;
            var actualRgbColor = frame.BorderColor;
            Assert.NotNull(actualRgbColor);
            Assert.Equal(rgbColor.Red, actualRgbColor.Red);
            Assert.Equal(rgbColor.Green, actualRgbColor.Green);
            Assert.Equal(rgbColor.Blue, actualRgbColor.Blue);
            Assert.Equal(1.0f, actualRgbColor.Alpha); // RGB factory method sets alpha to 1

            // Act & Assert - RGBA color
            frame.BorderColor = rgbaColor;
            var actualRgbaColor = frame.BorderColor;
            Assert.NotNull(actualRgbaColor);
            Assert.Equal(rgbaColor.Red, actualRgbaColor.Red);
            Assert.Equal(rgbaColor.Green, actualRgbaColor.Green);
            Assert.Equal(rgbaColor.Blue, actualRgbaColor.Blue);
            Assert.Equal(rgbaColor.Alpha, actualRgbaColor.Alpha);
        }

        /// <summary>
        /// Tests setting BorderColor using the default Color constructor.
        /// This verifies that the default black color is properly stored and retrieved,
        /// exercising the getter functionality.
        /// </summary>
        [Fact]
        public void BorderColor_SetToDefaultConstructorColor_ReturnsBlackColor()
        {
            // Arrange
            var frame = new Frame();
            var blackColor = new Color(); // Default constructor creates black (0,0,0,1)

            // Act
            frame.BorderColor = blackColor;
            var actualColor = frame.BorderColor;

            // Assert
            Assert.NotNull(actualColor);
            Assert.Equal(0.0f, actualColor.Red);
            Assert.Equal(0.0f, actualColor.Green);
            Assert.Equal(0.0f, actualColor.Blue);
            Assert.Equal(1.0f, actualColor.Alpha);
        }

        /// <summary>
        /// Tests setting BorderColor multiple times and verifying each retrieval.
        /// This ensures the property correctly updates and the getter returns
        /// the most recently set value.
        /// </summary>
        [Fact]
        public void BorderColor_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var frame = new Frame();
            var firstColor = new Color(1.0f, 0.0f, 0.0f, 1.0f); // Red
            var secondColor = new Color(0.0f, 1.0f, 0.0f, 1.0f); // Green
            var thirdColor = new Color(0.0f, 0.0f, 1.0f, 1.0f); // Blue

            // Act & Assert - First color
            frame.BorderColor = firstColor;
            var actual1 = frame.BorderColor;
            Assert.Equal(firstColor.Red, actual1.Red);
            Assert.Equal(firstColor.Green, actual1.Green);
            Assert.Equal(firstColor.Blue, actual1.Blue);

            // Act & Assert - Second color
            frame.BorderColor = secondColor;
            var actual2 = frame.BorderColor;
            Assert.Equal(secondColor.Red, actual2.Red);
            Assert.Equal(secondColor.Green, actual2.Green);
            Assert.Equal(secondColor.Blue, actual2.Blue);

            // Act & Assert - Third color
            frame.BorderColor = thirdColor;
            var actual3 = frame.BorderColor;
            Assert.Equal(thirdColor.Red, actual3.Red);
            Assert.Equal(thirdColor.Green, actual3.Green);
            Assert.Equal(thirdColor.Blue, actual3.Blue);
        }
    }
}