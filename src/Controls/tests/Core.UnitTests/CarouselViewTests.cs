#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class CarouselViewTests : BaseTestFixture
    {
        public CarouselViewTests()
        {
            DeviceDisplay.SetCurrent(new MockDeviceDisplay());
        }

        [Fact]
        public void TestConstructorAndDefaults()
        {
            var carouselView = new CarouselView();
            Assert.Null(carouselView.ItemsSource);
            Assert.Null(carouselView.ItemTemplate);
            Assert.NotNull(carouselView.ItemsLayout);
            Assert.True(carouselView.Position == 0);
        }

        [Fact]
        public void TestPositionChangedCommand()
        {
            var source = new List<string> { "1", "2", "3" };
            var carouselView = new CarouselView
            {
                ItemsSource = source
            };

            int countFired = 0;
            carouselView.PositionChangedCommand = new Command(() =>
            {
                countFired = countFired + 1;
            });
            Assert.Same(source, carouselView.ItemsSource);
            carouselView.Position = 1;
            Assert.True(countFired == 1);
        }

        [Fact]
        public void TestPositionChangedEvent()
        {
            var gotoPosition = 1;
            var source = new List<string> { "1", "2", "3" };
            var carouselView = new CarouselView
            {
                ItemsSource = source
            };

            int countFired = 0;
            carouselView.PositionChanged += (s, e) =>
            {
                countFired += 1;
            };
            Assert.Same(source, carouselView.ItemsSource);
            carouselView.Position = gotoPosition;
            Assert.True(countFired == 1);
        }

        [Fact]
        public void TestCurrentItemChangedCommand()
        {
            var gotoPosition = 1;
            var source = new List<string> { "1", "2", "3" };
            var carouselView = new CarouselView
            {
                ItemsSource = source
            };

            int countFired = 0;
            carouselView.CurrentItemChangedCommand = new Command(() =>
            {
                countFired += 1;
            });
            Assert.Same(source, carouselView.ItemsSource);
            carouselView.CurrentItem = source[gotoPosition];
            Assert.True(countFired == 1);
        }

        [Fact]
        public void TestCurrentItemChangedEvent()
        {
            var gotoPosition = 1;
            var source = new List<string> { "1", "2", "3" };
            var carouselView = new CarouselView
            {
                ItemsSource = source
            };

            int countFired = 0;
            carouselView.CurrentItemChanged += (s, e) =>
            {
                countFired += 1;
            };
            Assert.Same(source, carouselView.ItemsSource);
            carouselView.CurrentItem = source[gotoPosition];
            Assert.True(countFired == 1);
        }

        [Fact]
        public void TestAddRemoveItems()
        {
            var source = new List<string>();

            var carouselView = new CarouselView
            {
                ItemsSource = source
            };

            source.Add("1");
            source.Add("2");

            carouselView.ScrollTo(1, position: ScrollToPosition.Center, animate: false);
            source.Remove("2");

            Assert.Equal(0, carouselView.Position);
        }

        /// <summary>
        /// Tests that VisibleViews property returns a non-null ObservableCollection of View objects.
        /// Verifies the property is properly initialized and accessible.
        /// </summary>
        [Fact]
        public void VisibleViews_DefaultState_ReturnsNonNullObservableCollection()
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            var visibleViews = carouselView.VisibleViews;

            // Assert
            Assert.NotNull(visibleViews);
            Assert.IsType<ObservableCollection<View>>(visibleViews);
        }

        /// <summary>
        /// Tests that VisibleViews property returns an empty collection by default.
        /// Verifies no views are present in the collection when CarouselView is first created.
        /// </summary>
        [Fact]
        public void VisibleViews_DefaultState_ReturnsEmptyCollection()
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            var visibleViews = carouselView.VisibleViews;

            // Assert
            Assert.Empty(visibleViews);
        }

        /// <summary>
        /// Tests that multiple accesses to VisibleViews property return the same instance.
        /// Verifies property consistency and that the underlying collection is stable.
        /// </summary>
        [Fact]
        public void VisibleViews_MultipleAccess_ReturnsSameInstance()
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            var visibleViews1 = carouselView.VisibleViews;
            var visibleViews2 = carouselView.VisibleViews;

            // Assert
            Assert.Same(visibleViews1, visibleViews2);
        }

        /// <summary>
        /// Tests that AnimatePositionChanges returns the default value of true when IsScrollAnimated is at its default.
        /// Verifies the default behavior of the property delegation.
        /// </summary>
        [Fact]
        public void AnimatePositionChanges_DefaultValue_ReturnsTrue()
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            bool result = carouselView.AnimatePositionChanges;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that AnimatePositionChanges returns false when IsScrollAnimated is set to false.
        /// Verifies the property correctly delegates to IsScrollAnimated when set to false.
        /// </summary>
        [Fact]
        public void AnimatePositionChanges_IsScrollAnimatedFalse_ReturnsFalse()
        {
            // Arrange
            var carouselView = new CarouselView();
            carouselView.IsScrollAnimated = false;

            // Act
            bool result = carouselView.AnimatePositionChanges;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that AnimatePositionChanges returns true when IsScrollAnimated is explicitly set to true.
        /// Verifies the property correctly delegates to IsScrollAnimated when explicitly set to true.
        /// </summary>
        [Fact]
        public void AnimatePositionChanges_IsScrollAnimatedTrue_ReturnsTrue()
        {
            // Arrange
            var carouselView = new CarouselView();
            carouselView.IsScrollAnimated = true;

            // Act
            bool result = carouselView.AnimatePositionChanges;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsDragging property returns the default value of false when CarouselView is newly created.
        /// Verifies the initial state of the dragging indicator.
        /// </summary>
        [Fact]
        public void IsDragging_WhenCarouselViewCreated_ReturnsFalse()
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            bool isDragging = carouselView.IsDragging;

            // Assert
            Assert.False(isDragging);
        }

        /// <summary>
        /// Tests that IsDragging property returns true after SetIsDragging(true) is called.
        /// Verifies the property correctly reflects the dragging state when set to true.
        /// </summary>
        [Fact]
        public void IsDragging_WhenSetToTrue_ReturnsTrue()
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            carouselView.SetIsDragging(true);
            bool isDragging = carouselView.IsDragging;

            // Assert
            Assert.True(isDragging);
        }

        /// <summary>
        /// Tests that IsDragging property returns false after SetIsDragging(false) is called.
        /// Verifies the property correctly reflects the dragging state when explicitly set to false.
        /// </summary>
        [Fact]
        public void IsDragging_WhenSetToFalse_ReturnsFalse()
        {
            // Arrange
            var carouselView = new CarouselView();
            carouselView.SetIsDragging(true); // First set to true

            // Act
            carouselView.SetIsDragging(false);
            bool isDragging = carouselView.IsDragging;

            // Assert
            Assert.False(isDragging);
        }

        /// <summary>
        /// Tests that IsDragging property can be toggled multiple times and returns correct values.
        /// Verifies the property state management across multiple state changes.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsDragging_WhenSetToValue_ReturnsExpectedValue(bool expectedValue)
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            carouselView.SetIsDragging(expectedValue);
            bool actualValue = carouselView.IsDragging;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests the IsBounceEnabled property getter and setter functionality.
        /// Verifies the default value is true and that the property can be set to both true and false values.
        /// </summary>
        [Fact]
        public void TestIsBounceEnabled()
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act & Assert - Test default value
            Assert.True(carouselView.IsBounceEnabled);

            // Act & Assert - Test setting to false
            carouselView.IsBounceEnabled = false;
            Assert.False(carouselView.IsBounceEnabled);

            // Act & Assert - Test setting to true
            carouselView.IsBounceEnabled = true;
            Assert.True(carouselView.IsBounceEnabled);
        }

        /// <summary>
        /// Tests that the IsSwipeEnabled property returns the default value of true when CarouselView is constructed.
        /// Verifies that the getter correctly retrieves the default value from the underlying BindableProperty.
        /// </summary>
        [Fact]
        public void IsSwipeEnabled_DefaultValue_ReturnsTrue()
        {
            // Arrange & Act
            var carouselView = new CarouselView();

            // Assert
            Assert.True(carouselView.IsSwipeEnabled);
        }

        /// <summary>
        /// Tests that the IsSwipeEnabled property can be set to false and retrieved correctly.
        /// Verifies the getter returns the correct value after setting through the setter.
        /// </summary>
        [Fact]
        public void IsSwipeEnabled_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            carouselView.IsSwipeEnabled = false;

            // Assert
            Assert.False(carouselView.IsSwipeEnabled);
        }

        /// <summary>
        /// Tests that the IsSwipeEnabled property can be set to true and retrieved correctly.
        /// Verifies the getter returns the correct value after setting through the setter.
        /// </summary>
        [Fact]
        public void IsSwipeEnabled_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            carouselView.IsSwipeEnabled = true;

            // Assert
            Assert.True(carouselView.IsSwipeEnabled);
        }

        /// <summary>
        /// Tests that the IsSwipeEnabled property can be toggled multiple times and correctly retrieves each value.
        /// Verifies the consistency of get/set operations for the property.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        public void IsSwipeEnabled_SetMultipleValues_ReturnsCorrectValue(bool expectedValue)
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            carouselView.IsSwipeEnabled = expectedValue;

            // Assert
            Assert.Equal(expectedValue, carouselView.IsSwipeEnabled);
        }

        /// <summary>
        /// Tests that the IsScrollAnimated property returns the correct default value.
        /// </summary>
        [Fact]
        public void IsScrollAnimated_DefaultValue_ReturnsTrue()
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            var result = carouselView.IsScrollAnimated;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the IsScrollAnimated property can be set to false and retrieved correctly.
        /// </summary>
        [Fact]
        public void IsScrollAnimated_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            carouselView.IsScrollAnimated = false;
            var result = carouselView.IsScrollAnimated;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the IsScrollAnimated property can be set to true and retrieved correctly.
        /// </summary>
        [Fact]
        public void IsScrollAnimated_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            carouselView.IsScrollAnimated = true;
            var result = carouselView.IsScrollAnimated;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the IsScrollAnimated property can be set multiple times with different values.
        /// </summary>
        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void IsScrollAnimated_SetMultipleTimes_ReturnsCorrectValue(bool firstValue, bool secondValue)
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            carouselView.IsScrollAnimated = firstValue;
            carouselView.IsScrollAnimated = secondValue;
            var result = carouselView.IsScrollAnimated;

            // Assert
            Assert.Equal(secondValue, result);
        }

        /// <summary>
        /// Tests that CurrentItem property getter returns the correct value after setting it.
        /// Tests the basic get/set functionality of the CurrentItem property.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("test string")]
        [InlineData(42)]
        [InlineData(true)]
        public void CurrentItem_SetValue_ReturnsCorrectValue(object expectedValue)
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            carouselView.CurrentItem = expectedValue;
            var actualValue = carouselView.CurrentItem;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that CurrentItem property can be set to null and returns null when retrieved.
        /// Validates null handling for the CurrentItem property.
        /// </summary>
        [Fact]
        public void CurrentItem_SetNull_ReturnsNull()
        {
            // Arrange
            var carouselView = new CarouselView();
            carouselView.CurrentItem = "initial value";

            // Act
            carouselView.CurrentItem = null;
            var result = carouselView.CurrentItem;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that CurrentItem property returns null by default when no value has been set.
        /// Validates the default state of the CurrentItem property.
        /// </summary>
        [Fact]
        public void CurrentItem_DefaultValue_ReturnsNull()
        {
            // Arrange & Act
            var carouselView = new CarouselView();
            var result = carouselView.CurrentItem;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that CurrentItem property can handle complex object types including collections.
        /// Validates that the property works with reference types and collections.
        /// </summary>
        [Fact]
        public void CurrentItem_SetComplexObject_ReturnsCorrectValue()
        {
            // Arrange
            var carouselView = new CarouselView();
            var complexObject = new ObservableCollection<string> { "item1", "item2" };

            // Act
            carouselView.CurrentItem = complexObject;
            var result = carouselView.CurrentItem;

            // Assert
            Assert.Same(complexObject, result);
        }

        /// <summary>
        /// Tests that setting the same value multiple times to CurrentItem property works correctly.
        /// Validates that the property handles repeated assignments properly.
        /// </summary>
        [Fact]
        public void CurrentItem_SetSameValueMultipleTimes_ReturnsCorrectValue()
        {
            // Arrange
            var carouselView = new CarouselView();
            var testValue = "test value";

            // Act
            carouselView.CurrentItem = testValue;
            carouselView.CurrentItem = testValue;
            carouselView.CurrentItem = testValue;
            var result = carouselView.CurrentItem;

            // Assert
            Assert.Equal(testValue, result);
        }

        /// <summary>
        /// Tests that CurrentItem property can be changed from one value to another.
        /// Validates that the property correctly updates when different values are assigned.
        /// </summary>
        [Fact]
        public void CurrentItem_ChangeFromOneValueToAnother_ReturnsNewValue()
        {
            // Arrange
            var carouselView = new CarouselView();
            var initialValue = "initial";
            var newValue = "new value";

            // Act
            carouselView.CurrentItem = initialValue;
            carouselView.CurrentItem = newValue;
            var result = carouselView.CurrentItem;

            // Assert
            Assert.Equal(newValue, result);
        }

        /// <summary>
        /// Tests that CurrentItemChangedCommandParameter returns null by default.
        /// This test validates the default value behavior of the bindable property.
        /// </summary>
        [Fact]
        public void CurrentItemChangedCommandParameter_DefaultValue_ReturnsNull()
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            var result = carouselView.CurrentItemChangedCommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that CurrentItemChangedCommandParameter can be set and retrieved correctly with various object types.
        /// This test validates the property getter and setter functionality with different parameter types.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("string parameter")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void CurrentItemChangedCommandParameter_SetAndGet_ReturnsCorrectValue(object parameter)
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            carouselView.CurrentItemChangedCommandParameter = parameter;
            var result = carouselView.CurrentItemChangedCommandParameter;

            // Assert
            Assert.Equal(parameter, result);
        }

        /// <summary>
        /// Tests that CurrentItemChangedCommandParameter can store complex objects.
        /// This test validates the property with custom object types and collections.
        /// </summary>
        [Fact]
        public void CurrentItemChangedCommandParameter_SetComplexObject_ReturnsCorrectValue()
        {
            // Arrange
            var carouselView = new CarouselView();
            var complexParameter = new { Id = 1, Name = "Test", Items = new List<string> { "Item1", "Item2" } };

            // Act
            carouselView.CurrentItemChangedCommandParameter = complexParameter;
            var result = carouselView.CurrentItemChangedCommandParameter;

            // Assert
            Assert.Same(complexParameter, result);
        }

        /// <summary>
        /// Tests that CurrentItemChangedCommandParameter can be set multiple times with different values.
        /// This test validates that the property correctly updates when set to different values.
        /// </summary>
        [Fact]
        public void CurrentItemChangedCommandParameter_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var carouselView = new CarouselView();
            var firstParameter = "first";
            var secondParameter = 100;
            var thirdParameter = new DateTime(2023, 1, 1);

            // Act & Assert
            carouselView.CurrentItemChangedCommandParameter = firstParameter;
            Assert.Equal(firstParameter, carouselView.CurrentItemChangedCommandParameter);

            carouselView.CurrentItemChangedCommandParameter = secondParameter;
            Assert.Equal(secondParameter, carouselView.CurrentItemChangedCommandParameter);

            carouselView.CurrentItemChangedCommandParameter = thirdParameter;
            Assert.Equal(thirdParameter, carouselView.CurrentItemChangedCommandParameter);
        }

        /// <summary>
        /// Tests that the Loop property returns the correct default value.
        /// The default value should be true as specified in LoopProperty definition.
        /// </summary>
        [Fact]
        public void Loop_DefaultValue_ReturnsTrue()
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            var result = carouselView.Loop;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the Loop property getter and setter work correctly for both boolean values.
        /// This verifies that the property correctly delegates to the underlying BindableProperty.
        /// </summary>
        /// <param name="value">The boolean value to test setting and getting</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Loop_SetAndGet_ReturnsExpectedValue(bool value)
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            carouselView.Loop = value;
            var result = carouselView.Loop;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that the Loop property setter correctly updates the underlying BindableProperty
        /// by verifying the value can be retrieved through GetValue method.
        /// </summary>
        /// <param name="value">The boolean value to test</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Loop_SetValue_UpdatesBindableProperty(bool value)
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            carouselView.Loop = value;
            var bindableValue = (bool)carouselView.GetValue(CarouselView.LoopProperty);

            // Assert
            Assert.Equal(value, bindableValue);
        }

        /// <summary>
        /// Tests that AnimateCurrentItemChanges property returns the correct value based on IsScrollAnimated property.
        /// Tests both true and false values of IsScrollAnimated to ensure the property correctly delegates to IsScrollAnimated.
        /// Expected result: AnimateCurrentItemChanges should return the same value as IsScrollAnimated.
        /// </summary>
        /// <param name="isScrollAnimated">The value to set for IsScrollAnimated property</param>
        /// <param name="expectedResult">The expected return value from AnimateCurrentItemChanges</param>
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void AnimateCurrentItemChanges_ReturnsIsScrollAnimatedValue_ExpectedResult(bool isScrollAnimated, bool expectedResult)
        {
            // Arrange
            var carouselView = new CarouselView();
            carouselView.IsScrollAnimated = isScrollAnimated;

            // Act
            bool result = carouselView.AnimateCurrentItemChanges;

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }

    /// <summary>
    /// Unit tests for CarouselView PeekAreaInsets property
    /// </summary>
    public partial class CarouselViewPeekAreaInsetsTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that PeekAreaInsets returns the default thickness value when not explicitly set.
        /// Input: New CarouselView instance with default PeekAreaInsets
        /// Expected: Returns default(Thickness) which is zero thickness on all edges
        /// </summary>
        [Fact]
        public void PeekAreaInsets_DefaultValue_ReturnsDefaultThickness()
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            var result = carouselView.PeekAreaInsets;

            // Assert
            Assert.Equal(default(Thickness), result);
            Assert.Equal(0, result.Left);
            Assert.Equal(0, result.Top);
            Assert.Equal(0, result.Right);
            Assert.Equal(0, result.Bottom);
        }

        /// <summary>
        /// Tests that PeekAreaInsets getter returns the correct thickness after setting a uniform value.
        /// Input: Thickness with uniform value of 10
        /// Expected: Returns Thickness with all edges set to 10
        /// </summary>
        [Fact]
        public void PeekAreaInsets_SetUniformThickness_ReturnsCorrectValue()
        {
            // Arrange
            var carouselView = new CarouselView();
            var expectedThickness = new Thickness(10);

            // Act
            carouselView.PeekAreaInsets = expectedThickness;
            var result = carouselView.PeekAreaInsets;

            // Assert
            Assert.Equal(expectedThickness, result);
            Assert.Equal(10, result.Left);
            Assert.Equal(10, result.Top);
            Assert.Equal(10, result.Right);
            Assert.Equal(10, result.Bottom);
        }

        /// <summary>
        /// Tests that PeekAreaInsets getter returns the correct thickness after setting individual edge values.
        /// Input: Thickness with different values for each edge (Left=1, Top=2, Right=3, Bottom=4)
        /// Expected: Returns Thickness with the exact values set for each edge
        /// </summary>
        [Fact]
        public void PeekAreaInsets_SetIndividualEdgeValues_ReturnsCorrectValues()
        {
            // Arrange
            var carouselView = new CarouselView();
            var expectedThickness = new Thickness(1, 2, 3, 4);

            // Act
            carouselView.PeekAreaInsets = expectedThickness;
            var result = carouselView.PeekAreaInsets;

            // Assert
            Assert.Equal(expectedThickness, result);
            Assert.Equal(1, result.Left);
            Assert.Equal(2, result.Top);
            Assert.Equal(3, result.Right);
            Assert.Equal(4, result.Bottom);
        }

        /// <summary>
        /// Tests that PeekAreaInsets getter handles edge case values correctly including extreme double values.
        /// Input: Various edge case thickness values
        /// Expected: Returns the exact thickness values without loss of precision
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(-1, -2, -3, -4)]
        [InlineData(double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue, double.MinValue, double.MinValue)]
        [InlineData(1.5, 2.7, 3.14159, 4.999999)]
        public void PeekAreaInsets_EdgeCaseValues_ReturnsCorrectValues(double left, double top, double right, double bottom)
        {
            // Arrange
            var carouselView = new CarouselView();
            var expectedThickness = new Thickness(left, top, right, bottom);

            // Act
            carouselView.PeekAreaInsets = expectedThickness;
            var result = carouselView.PeekAreaInsets;

            // Assert
            Assert.Equal(expectedThickness, result);
            Assert.Equal(left, result.Left);
            Assert.Equal(top, result.Top);
            Assert.Equal(right, result.Right);
            Assert.Equal(bottom, result.Bottom);
        }

        /// <summary>
        /// Tests that PeekAreaInsets getter handles NaN values correctly.
        /// Input: Thickness with NaN values for all edges
        /// Expected: Returns Thickness with NaN values preserved
        /// </summary>
        [Fact]
        public void PeekAreaInsets_NaNValues_ReturnsNaNValues()
        {
            // Arrange
            var carouselView = new CarouselView();
            var expectedThickness = new Thickness(double.NaN, double.NaN, double.NaN, double.NaN);

            // Act
            carouselView.PeekAreaInsets = expectedThickness;
            var result = carouselView.PeekAreaInsets;

            // Assert
            Assert.Equal(expectedThickness, result);
            Assert.True(double.IsNaN(result.Left));
            Assert.True(double.IsNaN(result.Top));
            Assert.True(double.IsNaN(result.Right));
            Assert.True(double.IsNaN(result.Bottom));
        }

        /// <summary>
        /// Tests that PeekAreaInsets getter handles infinity values correctly.
        /// Input: Thickness with positive and negative infinity values
        /// Expected: Returns Thickness with infinity values preserved
        /// </summary>
        [Fact]
        public void PeekAreaInsets_InfinityValues_ReturnsInfinityValues()
        {
            // Arrange
            var carouselView = new CarouselView();
            var expectedThickness = new Thickness(double.PositiveInfinity, double.NegativeInfinity,
                                                 double.PositiveInfinity, double.NegativeInfinity);

            // Act
            carouselView.PeekAreaInsets = expectedThickness;
            var result = carouselView.PeekAreaInsets;

            // Assert
            Assert.Equal(expectedThickness, result);
            Assert.True(double.IsPositiveInfinity(result.Left));
            Assert.True(double.IsNegativeInfinity(result.Top));
            Assert.True(double.IsPositiveInfinity(result.Right));
            Assert.True(double.IsNegativeInfinity(result.Bottom));
        }

        /// <summary>
        /// Tests that PeekAreaInsets getter returns updated values after multiple property changes.
        /// Input: Multiple different thickness values set sequentially
        /// Expected: Returns the most recently set thickness value each time
        /// </summary>
        [Fact]
        public void PeekAreaInsets_MultipleChanges_ReturnsLatestValue()
        {
            // Arrange
            var carouselView = new CarouselView();
            var firstThickness = new Thickness(1, 2, 3, 4);
            var secondThickness = new Thickness(5, 6, 7, 8);
            var thirdThickness = new Thickness(10);

            // Act & Assert
            carouselView.PeekAreaInsets = firstThickness;
            Assert.Equal(firstThickness, carouselView.PeekAreaInsets);

            carouselView.PeekAreaInsets = secondThickness;
            Assert.Equal(secondThickness, carouselView.PeekAreaInsets);

            carouselView.PeekAreaInsets = thirdThickness;
            Assert.Equal(thirdThickness, carouselView.PeekAreaInsets);
        }
    }


    /// <summary>
    /// Unit tests for the PositionChangedCommandParameter property of CarouselView.
    /// </summary>
    public partial class CarouselViewPositionChangedCommandParameterTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that PositionChangedCommandParameter can be set to null and retrieved as null.
        /// Validates the getter returns null when the property is set to null.
        /// Expected result: Property returns null.
        /// </summary>
        [Fact]
        public void PositionChangedCommandParameter_SetToNull_ReturnsNull()
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            carouselView.PositionChangedCommandParameter = null;
            var result = carouselView.PositionChangedCommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that PositionChangedCommandParameter can be set to a string value and retrieved correctly.
        /// Validates the getter returns the exact same string object that was set.
        /// Expected result: Property returns the string value that was set.
        /// </summary>
        [Fact]
        public void PositionChangedCommandParameter_SetToString_ReturnsString()
        {
            // Arrange
            var carouselView = new CarouselView();
            var testString = "test parameter";

            // Act
            carouselView.PositionChangedCommandParameter = testString;
            var result = carouselView.PositionChangedCommandParameter;

            // Assert
            Assert.Equal(testString, result);
        }

        /// <summary>
        /// Tests that PositionChangedCommandParameter can be set to an integer value and retrieved correctly.
        /// Validates the getter returns the exact same integer object that was set.
        /// Expected result: Property returns the integer value that was set.
        /// </summary>
        [Fact]
        public void PositionChangedCommandParameter_SetToInteger_ReturnsInteger()
        {
            // Arrange
            var carouselView = new CarouselView();
            var testInteger = 42;

            // Act
            carouselView.PositionChangedCommandParameter = testInteger;
            var result = carouselView.PositionChangedCommandParameter;

            // Assert
            Assert.Equal(testInteger, result);
        }

        /// <summary>
        /// Tests that PositionChangedCommandParameter can be set to a custom object and retrieved correctly.
        /// Validates the getter returns the exact same object reference that was set.
        /// Expected result: Property returns the same object reference that was set.
        /// </summary>
        [Fact]
        public void PositionChangedCommandParameter_SetToCustomObject_ReturnsCustomObject()
        {
            // Arrange
            var carouselView = new CarouselView();
            var testObject = new CustomParameterObject { Id = 123, Name = "Test" };

            // Act
            carouselView.PositionChangedCommandParameter = testObject;
            var result = carouselView.PositionChangedCommandParameter;

            // Assert
            Assert.Same(testObject, result);
        }

        /// <summary>
        /// Tests that PositionChangedCommandParameter can be set to an empty string and retrieved correctly.
        /// Validates the getter returns an empty string when set to an empty string.
        /// Expected result: Property returns an empty string.
        /// </summary>
        [Fact]
        public void PositionChangedCommandParameter_SetToEmptyString_ReturnsEmptyString()
        {
            // Arrange
            var carouselView = new CarouselView();
            var emptyString = string.Empty;

            // Act
            carouselView.PositionChangedCommandParameter = emptyString;
            var result = carouselView.PositionChangedCommandParameter;

            // Assert
            Assert.Equal(emptyString, result);
        }

        /// <summary>
        /// Tests that PositionChangedCommandParameter returns the default value when not explicitly set.
        /// Validates the getter returns the default value for the property after construction.
        /// Expected result: Property returns the default value (null for object type).
        /// </summary>
        [Fact]
        public void PositionChangedCommandParameter_DefaultValue_ReturnsDefault()
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            var result = carouselView.PositionChangedCommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that PositionChangedCommandParameter can be overwritten with different values.
        /// Validates that setting a new value replaces the previous value completely.
        /// Expected result: Property returns the most recently set value.
        /// </summary>
        [Fact]
        public void PositionChangedCommandParameter_OverwriteValue_ReturnsNewValue()
        {
            // Arrange
            var carouselView = new CarouselView();
            var firstValue = "first";
            var secondValue = 100;

            // Act
            carouselView.PositionChangedCommandParameter = firstValue;
            carouselView.PositionChangedCommandParameter = secondValue;
            var result = carouselView.PositionChangedCommandParameter;

            // Assert
            Assert.Equal(secondValue, result);
        }

        /// <summary>
        /// Helper class for testing custom object parameter scenarios.
        /// </summary>
        private class CustomParameterObject
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }


    /// <summary>
    /// Unit tests for CarouselView SetIsDragging method
    /// </summary>
    public partial class CarouselViewSetIsDraggingTests : BaseTestFixture
    {
        public CarouselViewSetIsDraggingTests()
        {
            DeviceDisplay.SetCurrent(new MockDeviceDisplay());
        }

        /// <summary>
        /// Tests that SetIsDragging with true value sets IsDragging property to true
        /// </summary>
        [Fact]
        public void SetIsDragging_WithTrue_SetsIsDraggingToTrue()
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            carouselView.SetIsDragging(true);

            // Assert
            Assert.True(carouselView.IsDragging);
        }

        /// <summary>
        /// Tests that SetIsDragging with false value sets IsDragging property to false
        /// </summary>
        [Fact]
        public void SetIsDragging_WithFalse_SetsIsDraggingToFalse()
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act
            carouselView.SetIsDragging(false);

            // Assert
            Assert.False(carouselView.IsDragging);
        }

        /// <summary>
        /// Tests that SetIsDragging can toggle between true and false values multiple times
        /// </summary>
        [Fact]
        public void SetIsDragging_ToggleValues_UpdatesIsDraggingCorrectly()
        {
            // Arrange
            var carouselView = new CarouselView();

            // Act & Assert - Set to true
            carouselView.SetIsDragging(true);
            Assert.True(carouselView.IsDragging);

            // Act & Assert - Set to false
            carouselView.SetIsDragging(false);
            Assert.False(carouselView.IsDragging);

            // Act & Assert - Set to true again
            carouselView.SetIsDragging(true);
            Assert.True(carouselView.IsDragging);
        }

        /// <summary>
        /// Tests that IsDragging has the correct default value before calling SetIsDragging
        /// </summary>
        [Fact]
        public void SetIsDragging_DefaultValue_IsFalse()
        {
            // Arrange & Act
            var carouselView = new CarouselView();

            // Assert
            Assert.False(carouselView.IsDragging);
        }
    }
}