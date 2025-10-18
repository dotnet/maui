#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using NSubstitute;
using Xunit;

using AbsoluteLayoutFlags = Microsoft.Maui.Layouts.AbsoluteLayoutFlags;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class AbsoluteLayoutTests : BaseTestFixture
    {
        public AbsoluteLayoutTests()
        {
            DeviceDisplay.SetCurrent(new MockDeviceDisplay());
        }

        [Fact]
        public void Constructor()
        {
            var abs = new AbsoluteLayout
            {
                IsPlatformEnabled = true
            };

            Assert.Empty(abs.Children);

            var sizeReq = abs.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);
            Assert.Equal(Size.Zero, sizeReq.Request);
            Assert.Equal(Size.Zero, sizeReq.Minimum);
        }

        [Fact]
        public void AbsolutePositionAndSizeUsingRectangle()
        {
            var abs = new Compatibility.AbsoluteLayout
            {
                IsPlatformEnabled = true
            };

            var child = MockPlatformSizeService.Sub<View>();

            abs.Children.Add(child, new Rect(10, 20, 30, 40));

            abs.Layout(new Rect(0, 0, 100, 100));

            Assert.Equal(new Rect(10, 20, 30, 40), child.Bounds);
        }


        [Fact]
        public void AbsolutePositionRelativeSize()
        {
            var abs = new Compatibility.AbsoluteLayout
            {
                IsPlatformEnabled = true
            };

            var child = MockPlatformSizeService.Sub<View>();

            abs.Children.Add(child, new Rect(10, 20, 0.4, 0.5), AbsoluteLayoutFlags.SizeProportional);

            abs.Layout(new Rect(0, 0, 100, 100));

            Assert.Equal(10, child.X);
            Assert.Equal(20, child.Y);
            Assert.Equal(40, child.Width, 4);
            Assert.Equal(50, child.Height, 4);
        }

        [Theory]
        [InlineData(30, 40, 0.2, 0.3)]
        [InlineData(35, 45, 0.5, 0.5)]
        [InlineData(35, 45, 0, 0)]
        [InlineData(35, 45, 1, 1)]
        public void RelativePositionAbsoluteSize(double width, double height, double relX, double relY)
        {
            var abs = new Compatibility.AbsoluteLayout
            {
                IsPlatformEnabled = true
            };

            var child = MockPlatformSizeService.Sub<View>();

            abs.Children.Add(child, new Rect(relX, relY, width, height), AbsoluteLayoutFlags.PositionProportional);

            abs.Layout(new Rect(0, 0, 100, 100));

            double expectedX = Math.Round((100 - width) * relX);
            double expectedY = Math.Round((100 - height) * relY);
            Assert.Equal(expectedX, child.X, 4);
            Assert.Equal(expectedY, child.Y, 4);
            Assert.Equal(width, child.Width);
            Assert.Equal(height, child.Height);
        }

        public static IEnumerable<object[]> RelativeData()
        {
            return TestDataHelpers.Combinations(new List<double>() { 0.0, 0.2, 0.5, 1.0 });
        }

        [Theory]
        [MemberData(nameof(RelativeData))]
        public void RelativePositionRelativeSize(double relX, double relY, double relHeight, double relWidth)
        {
            var abs = new Compatibility.AbsoluteLayout
            {
                IsPlatformEnabled = true
            };

            var child = MockPlatformSizeService.Sub<View>();
            abs.Children.Add(child, new Rect(relX, relY, relWidth, relHeight), AbsoluteLayoutFlags.All);
            abs.Layout(new Rect(0, 0, 100, 100));

            double expectedWidth = Math.Round(100 * relWidth);
            double expectedHeight = Math.Round(100 * relHeight);
            double expectedX = Math.Round((100 - expectedWidth) * relX);
            double expectedY = Math.Round((100 - expectedHeight) * relY);

            Assert.Equal(expectedX, child.X, 4);
            Assert.Equal(expectedY, child.Y, 4);
            Assert.Equal(expectedWidth, child.Width, 4);
            Assert.Equal(expectedHeight, child.Height, 4);
        }

        [Fact]
        public void SizeRequestWithNormalChild()
        {
            var abs = new Compatibility.AbsoluteLayout
            {
                IsPlatformEnabled = true
            };

            var child = MockPlatformSizeService.Sub<View>();

            // ChildSizeReq == 100x20
            abs.Children.Add(child, new Rect(10, 20, 30, 40));

            var sizeReq = abs.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);

            Assert.Equal(new Size(40, 60), sizeReq.Request);
            Assert.Equal(new Size(40, 60), sizeReq.Minimum);
        }

        [Fact]
        public void SizeRequestWithRelativePositionChild()
        {
            var abs = new Compatibility.AbsoluteLayout
            {
                IsPlatformEnabled = true
            };

            var child = MockPlatformSizeService.Sub<View>();

            // ChildSizeReq == 100x20
            abs.Children.Add(child, new Rect(0.5, 0.5, 30, 40), AbsoluteLayoutFlags.PositionProportional);

            var sizeReq = abs.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);

            Assert.Equal(new Size(30, 40), sizeReq.Request);
            Assert.Equal(new Size(30, 40), sizeReq.Minimum);
        }

        [Fact]
        public void SizeRequestWithRelativeChild()
        {
            var abs = new Compatibility.AbsoluteLayout
            {
                IsPlatformEnabled = true
            };

            var child = MockPlatformSizeService.Sub<View>();

            // ChildSizeReq == 100x20
            abs.Children.Add(child, new Rect(0.5, 0.5, 0.5, 0.5), AbsoluteLayoutFlags.All);

            var sizeReq = abs.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);

            Assert.Equal(new Size(200, 40), sizeReq.Request);
            Assert.Equal(new Size(0, 0), sizeReq.Minimum);
        }

        [Fact]
        public void SizeRequestWithRelativeSizeChild()
        {
            var abs = new Compatibility.AbsoluteLayout
            {
                IsPlatformEnabled = true
            };

            var child = MockPlatformSizeService.Sub<View>();

            // ChildSizeReq == 100x20
            abs.Children.Add(child, new Rect(10, 20, 0.5, 0.5), AbsoluteLayoutFlags.SizeProportional);

            var sizeReq = abs.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);

            Assert.Equal(new Size(210, 60), sizeReq.Request);
            Assert.Equal(new Size(10, 20), sizeReq.Minimum);
        }

        [Fact]
        public void MeasureInvalidatedFiresWhenFlagsChanged()
        {
            var abs = new Compatibility.AbsoluteLayout
            {
                IsPlatformEnabled = true
            };

            var child = MockPlatformSizeService.Sub<View>();

            abs.Children.Add(child, new Rect(1, 1, 100, 100));

            bool fired = false;
            abs.MeasureInvalidated += (sender, args) => fired = true;

            AbsoluteLayout.SetLayoutFlags(child, AbsoluteLayoutFlags.PositionProportional);

            Assert.True(fired);
        }

        [Fact]
        public void MeasureInvalidatedFiresWhenBoundsChanged()
        {
            var abs = new Compatibility.AbsoluteLayout
            {
                IsPlatformEnabled = true
            };

            var child = MockPlatformSizeService.Sub<View>();

            abs.Children.Add(child, new Rect(1, 1, 100, 100));

            bool fired = false;
            abs.MeasureInvalidated += (sender, args) => fired = true;

            AbsoluteLayout.SetLayoutBounds(child, new Rect(2, 2, 200, 200));

            Assert.True(fired);
        }

        [Theory]
        [InlineData("en-US"), InlineData("tr-TR")]
        public void TestBoundsTypeConverter(string culture)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);

            var converter = new BoundsTypeConverter();

            Assert.True(converter.CanConvertFrom(typeof(string)));
            Assert.Equal(new Rect(3, 4, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), converter.ConvertFromInvariantString("3, 4"));
            Assert.Equal(new Rect(3, 4, 20, 30), converter.ConvertFromInvariantString("3, 4, 20, 30"));
            Assert.Equal(new Rect(3, 4, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), converter.ConvertFromInvariantString("3, 4, AutoSize, AutoSize"));
            Assert.Equal(new Rect(3, 4, AbsoluteLayout.AutoSize, 30), converter.ConvertFromInvariantString("3, 4, AutoSize, 30"));
            Assert.Equal(new Rect(3, 4, 20, AbsoluteLayout.AutoSize), converter.ConvertFromInvariantString("3, 4, 20, AutoSize"));

            var autoSize = "AutoSize";
            Assert.Equal(new Rect(3.3, 4.4, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), converter.ConvertFromInvariantString("3.3, 4.4, " + autoSize + ", AutoSize"));
            Assert.Equal(new Rect(3.3, 4.4, 5.5, 6.6), converter.ConvertFromInvariantString("3.3, 4.4, 5.5, 6.6"));
        }

        /// <summary>
        /// Tests that GetLayoutFlags throws NullReferenceException when bindable parameter is null.
        /// Verifies proper null parameter handling and expected exception behavior.
        /// </summary>
        [Fact]
        public void GetLayoutFlags_NullBindable_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject nullBindable = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => AbsoluteLayout.GetLayoutFlags(nullBindable));
        }

        /// <summary>
        /// Tests that GetLayoutFlags returns the default value (None) when no layout flags have been explicitly set.
        /// Verifies correct retrieval of default BindableProperty values.
        /// </summary>
        [Fact]
        public void GetLayoutFlags_DefaultValue_ReturnsNone()
        {
            // Arrange
            var bindable = new Label();

            // Act
            var result = AbsoluteLayout.GetLayoutFlags(bindable);

            // Assert
            Assert.Equal(AbsoluteLayoutFlags.None, result);
        }

        /// <summary>
        /// Tests that GetLayoutFlags correctly retrieves various AbsoluteLayoutFlags values that have been set.
        /// Verifies proper casting and value retrieval for different flag combinations.
        /// </summary>
        /// <param name="expectedFlags">The AbsoluteLayoutFlags value to set and retrieve</param>
        [Theory]
        [InlineData(AbsoluteLayoutFlags.None)]
        [InlineData(AbsoluteLayoutFlags.XProportional)]
        [InlineData(AbsoluteLayoutFlags.YProportional)]
        [InlineData(AbsoluteLayoutFlags.WidthProportional)]
        [InlineData(AbsoluteLayoutFlags.HeightProportional)]
        [InlineData(AbsoluteLayoutFlags.PositionProportional)]
        [InlineData(AbsoluteLayoutFlags.SizeProportional)]
        [InlineData(AbsoluteLayoutFlags.All)]
        [InlineData(AbsoluteLayoutFlags.XProportional | AbsoluteLayoutFlags.WidthProportional)]
        [InlineData(AbsoluteLayoutFlags.YProportional | AbsoluteLayoutFlags.HeightProportional)]
        public void GetLayoutFlags_SetValues_ReturnsCorrectFlags(AbsoluteLayoutFlags expectedFlags)
        {
            // Arrange
            var bindable = new Label();
            AbsoluteLayout.SetLayoutFlags(bindable, expectedFlags);

            // Act
            var result = AbsoluteLayout.GetLayoutFlags(bindable);

            // Assert
            Assert.Equal(expectedFlags, result);
        }

        /// <summary>
        /// Tests that GetLayoutFlags returns the attached property value when the view is a BindableObject.
        /// Input: A BindableObject view with LayoutFlags attached property set.
        /// Expected: Returns the attached property value.
        /// </summary>
        [Theory]
        [InlineData(AbsoluteLayoutFlags.None)]
        [InlineData(AbsoluteLayoutFlags.XProportional)]
        [InlineData(AbsoluteLayoutFlags.YProportional)]
        [InlineData(AbsoluteLayoutFlags.WidthProportional)]
        [InlineData(AbsoluteLayoutFlags.HeightProportional)]
        [InlineData(AbsoluteLayoutFlags.PositionProportional)]
        [InlineData(AbsoluteLayoutFlags.SizeProportional)]
        [InlineData(AbsoluteLayoutFlags.All)]
        public void GetLayoutFlags_BindableObjectView_ReturnsAttachedPropertyValue(AbsoluteLayoutFlags expectedFlags)
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var bindableView = new Label();
            AbsoluteLayout.SetLayoutFlags(bindableView, expectedFlags);

            // Act
            var result = layout.GetLayoutFlags(bindableView);

            // Assert
            Assert.Equal(expectedFlags, result);
        }

        /// <summary>
        /// Tests that GetLayoutFlags returns the stored flags from _viewInfo when the view is not a BindableObject.
        /// Input: A non-BindableObject view that has been added to the layout with specific flags.
        /// Expected: Returns the flags stored in the internal _viewInfo dictionary.
        /// </summary>
        [Theory]
        [InlineData(AbsoluteLayoutFlags.None)]
        [InlineData(AbsoluteLayoutFlags.XProportional)]
        [InlineData(AbsoluteLayoutFlags.YProportional)]
        [InlineData(AbsoluteLayoutFlags.WidthProportional)]
        [InlineData(AbsoluteLayoutFlags.HeightProportional)]
        [InlineData(AbsoluteLayoutFlags.PositionProportional)]
        [InlineData(AbsoluteLayoutFlags.SizeProportional)]
        [InlineData(AbsoluteLayoutFlags.All)]
        public void GetLayoutFlags_NonBindableObjectView_ReturnsViewInfoFlags(AbsoluteLayoutFlags expectedFlags)
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var mockView = Substitute.For<IView>();
            layout.Add(mockView);
            layout.SetLayoutFlags(mockView, expectedFlags);

            // Act
            var result = layout.GetLayoutFlags(mockView);

            // Assert
            Assert.Equal(expectedFlags, result);
        }

        /// <summary>
        /// Tests that GetLayoutFlags returns default flags for a non-BindableObject view when no flags have been explicitly set.
        /// Input: A non-BindableObject view that has been added to the layout without setting flags.
        /// Expected: Returns the default flags (AbsoluteLayoutFlags.None).
        /// </summary>
        [Fact]
        public void GetLayoutFlags_NonBindableObjectViewWithDefaultFlags_ReturnsNone()
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var mockView = Substitute.For<IView>();
            layout.Add(mockView);

            // Act
            var result = layout.GetLayoutFlags(mockView);

            // Assert
            Assert.Equal(AbsoluteLayoutFlags.None, result);
        }

        /// <summary>
        /// Tests that GetLayoutFlags returns default flags for a BindableObject view when no attached property has been set.
        /// Input: A BindableObject view without any LayoutFlags attached property set.
        /// Expected: Returns the default flags (AbsoluteLayoutFlags.None).
        /// </summary>
        [Fact]
        public void GetLayoutFlags_BindableObjectViewWithDefaultFlags_ReturnsNone()
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var bindableView = new Label();

            // Act
            var result = layout.GetLayoutFlags(bindableView);

            // Assert
            Assert.Equal(AbsoluteLayoutFlags.None, result);
        }

        /// <summary>
        /// Tests that GetLayoutFlags throws KeyNotFoundException when called with a non-BindableObject view that hasn't been added to the layout.
        /// Input: A non-BindableObject view that is not in the layout's _viewInfo dictionary.
        /// Expected: Throws KeyNotFoundException.
        /// </summary>
        [Fact]
        public void GetLayoutFlags_NonBindableObjectViewNotInLayout_ThrowsKeyNotFoundException()
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var mockView = Substitute.For<IView>();

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => layout.GetLayoutFlags(mockView));
        }

        /// <summary>
        /// Tests that GetLayoutFlags throws ArgumentNullException when called with a null view.
        /// Input: Null view parameter.
        /// Expected: Throws ArgumentNullException.
        /// </summary>
        [Fact]
        public void GetLayoutFlags_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            var layout = new AbsoluteLayout();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => layout.GetLayoutFlags(null));
        }

        /// <summary>
        /// Tests that GetLayoutFlags correctly handles complex flag combinations for BindableObject views.
        /// Input: A BindableObject view with combined layout flags.
        /// Expected: Returns the exact combined flags that were set.
        /// </summary>
        [Theory]
        [InlineData(AbsoluteLayoutFlags.XProportional | AbsoluteLayoutFlags.YProportional)]
        [InlineData(AbsoluteLayoutFlags.WidthProportional | AbsoluteLayoutFlags.HeightProportional)]
        [InlineData(AbsoluteLayoutFlags.XProportional | AbsoluteLayoutFlags.WidthProportional)]
        [InlineData(AbsoluteLayoutFlags.YProportional | AbsoluteLayoutFlags.HeightProportional)]
        public void GetLayoutFlags_BindableObjectViewWithCombinedFlags_ReturnsCombinedFlags(AbsoluteLayoutFlags combinedFlags)
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var bindableView = new Label();
            AbsoluteLayout.SetLayoutFlags(bindableView, combinedFlags);

            // Act
            var result = layout.GetLayoutFlags(bindableView);

            // Assert
            Assert.Equal(combinedFlags, result);
        }

        /// <summary>
        /// Tests that GetLayoutFlags correctly handles complex flag combinations for non-BindableObject views.
        /// Input: A non-BindableObject view with combined layout flags.
        /// Expected: Returns the exact combined flags that were set.
        /// </summary>
        [Theory]
        [InlineData(AbsoluteLayoutFlags.XProportional | AbsoluteLayoutFlags.YProportional)]
        [InlineData(AbsoluteLayoutFlags.WidthProportional | AbsoluteLayoutFlags.HeightProportional)]
        [InlineData(AbsoluteLayoutFlags.XProportional | AbsoluteLayoutFlags.WidthProportional)]
        [InlineData(AbsoluteLayoutFlags.YProportional | AbsoluteLayoutFlags.HeightProportional)]
        public void GetLayoutFlags_NonBindableObjectViewWithCombinedFlags_ReturnsCombinedFlags(AbsoluteLayoutFlags combinedFlags)
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var mockView = Substitute.For<IView>();
            layout.Add(mockView);
            layout.SetLayoutFlags(mockView, combinedFlags);

            // Act
            var result = layout.GetLayoutFlags(mockView);

            // Assert
            Assert.Equal(combinedFlags, result);
        }

        /// <summary>
        /// Tests that SetLayoutFlags correctly sets the LayoutFlags property on a BindableObject view.
        /// This test verifies that when a BindableObject is passed, the method uses the attached property system.
        /// Expected result: The LayoutFlags should be set via the attached property mechanism.
        /// </summary>
        [Theory]
        [InlineData(AbsoluteLayoutFlags.None)]
        [InlineData(AbsoluteLayoutFlags.XProportional)]
        [InlineData(AbsoluteLayoutFlags.YProportional)]
        [InlineData(AbsoluteLayoutFlags.WidthProportional)]
        [InlineData(AbsoluteLayoutFlags.HeightProportional)]
        [InlineData(AbsoluteLayoutFlags.PositionProportional)]
        [InlineData(AbsoluteLayoutFlags.SizeProportional)]
        [InlineData(AbsoluteLayoutFlags.All)]
        public void SetLayoutFlags_BindableObjectView_SetsAttachedProperty(AbsoluteLayoutFlags flags)
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var bindableView = new Label(); // BindableObject

            // Act
            layout.SetLayoutFlags(bindableView, flags);

            // Assert
            var actualFlags = AbsoluteLayout.GetLayoutFlags(bindableView);
            Assert.Equal(flags, actualFlags);
        }

        /// <summary>
        /// Tests that SetLayoutFlags correctly sets the LayoutFlags property on a non-BindableObject view that exists in the layout.
        /// This test verifies that when a non-BindableObject view is added to the layout first, SetLayoutFlags uses the internal dictionary.
        /// Expected result: The LayoutFlags should be set in the internal _viewInfo dictionary.
        /// </summary>
        [Theory]
        [InlineData(AbsoluteLayoutFlags.None)]
        [InlineData(AbsoluteLayoutFlags.XProportional)]
        [InlineData(AbsoluteLayoutFlags.YProportional)]
        [InlineData(AbsoluteLayoutFlags.WidthProportional)]
        [InlineData(AbsoluteLayoutFlags.HeightProportional)]
        [InlineData(AbsoluteLayoutFlags.PositionProportional)]
        [InlineData(AbsoluteLayoutFlags.SizeProportional)]
        [InlineData(AbsoluteLayoutFlags.All)]
        public void SetLayoutFlags_NonBindableObjectViewInLayout_SetsInternalDictionary(AbsoluteLayoutFlags flags)
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var nonBindableView = Substitute.For<IView>(); // Non-BindableObject
            layout.Add(nonBindableView); // This adds it to _viewInfo

            // Act
            layout.SetLayoutFlags(nonBindableView, flags);

            // Assert
            var actualFlags = layout.GetLayoutFlags(nonBindableView);
            Assert.Equal(flags, actualFlags);
        }

        /// <summary>
        /// Tests that SetLayoutFlags throws KeyNotFoundException when called with a non-BindableObject view that is not in the layout.
        /// This test verifies the error condition when trying to set flags on a view that hasn't been added to the layout.
        /// Expected result: KeyNotFoundException should be thrown.
        /// </summary>
        [Fact]
        public void SetLayoutFlags_NonBindableObjectViewNotInLayout_ThrowsKeyNotFoundException()
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var nonBindableView = Substitute.For<IView>(); // Non-BindableObject not added to layout
            var flags = AbsoluteLayoutFlags.XProportional;

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => layout.SetLayoutFlags(nonBindableView, flags));
        }

        /// <summary>
        /// Tests that SetLayoutFlags throws ArgumentNullException when called with a null view parameter.
        /// This test verifies the error condition when a null view is passed to the method.
        /// Expected result: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void SetLayoutFlags_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            var layout = new AbsoluteLayout();
            IView nullView = null;
            var flags = AbsoluteLayoutFlags.XProportional;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => layout.SetLayoutFlags(nullView, flags));
        }

        /// <summary>
        /// Tests that SetLayoutFlags correctly handles combined flag values on BindableObject views.
        /// This test verifies that complex flag combinations are properly set via the attached property system.
        /// Expected result: The combined flags should be set correctly.
        /// </summary>
        [Theory]
        [InlineData(AbsoluteLayoutFlags.XProportional | AbsoluteLayoutFlags.YProportional)]
        [InlineData(AbsoluteLayoutFlags.WidthProportional | AbsoluteLayoutFlags.HeightProportional)]
        [InlineData(AbsoluteLayoutFlags.XProportional | AbsoluteLayoutFlags.WidthProportional)]
        [InlineData(AbsoluteLayoutFlags.YProportional | AbsoluteLayoutFlags.HeightProportional)]
        public void SetLayoutFlags_BindableObjectView_HandlesCombinedFlags(AbsoluteLayoutFlags flags)
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var bindableView = new Label();

            // Act
            layout.SetLayoutFlags(bindableView, flags);

            // Assert
            var actualFlags = AbsoluteLayout.GetLayoutFlags(bindableView);
            Assert.Equal(flags, actualFlags);
        }

        /// <summary>
        /// Tests that SetLayoutFlags correctly handles combined flag values on non-BindableObject views in the layout.
        /// This test verifies that complex flag combinations are properly set in the internal dictionary.
        /// Expected result: The combined flags should be set correctly in the internal storage.
        /// </summary>
        [Theory]
        [InlineData(AbsoluteLayoutFlags.XProportional | AbsoluteLayoutFlags.YProportional)]
        [InlineData(AbsoluteLayoutFlags.WidthProportional | AbsoluteLayoutFlags.HeightProportional)]
        [InlineData(AbsoluteLayoutFlags.XProportional | AbsoluteLayoutFlags.WidthProportional)]
        [InlineData(AbsoluteLayoutFlags.YProportional | AbsoluteLayoutFlags.HeightProportional)]
        public void SetLayoutFlags_NonBindableObjectViewInLayout_HandlesCombinedFlags(AbsoluteLayoutFlags flags)
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var nonBindableView = Substitute.For<IView>();
            layout.Add(nonBindableView);

            // Act
            layout.SetLayoutFlags(nonBindableView, flags);

            // Assert
            var actualFlags = layout.GetLayoutFlags(nonBindableView);
            Assert.Equal(flags, actualFlags);
        }

        /// <summary>
        /// Tests that SetLayoutFlags handles invalid enum values by casting from integer.
        /// This test verifies that the method can handle enum values outside the normal range.
        /// Expected result: The invalid enum value should be set without throwing an exception.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SetLayoutFlags_BindableObjectView_HandlesInvalidEnumValues(int invalidEnumValue)
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var bindableView = new Label();
            var invalidFlags = (AbsoluteLayoutFlags)invalidEnumValue;

            // Act
            layout.SetLayoutFlags(bindableView, invalidFlags);

            // Assert
            var actualFlags = AbsoluteLayout.GetLayoutFlags(bindableView);
            Assert.Equal(invalidFlags, actualFlags);
        }

        /// <summary>
        /// Tests that SetLayoutFlags updates existing flag values on BindableObject views.
        /// This test verifies that subsequent calls to SetLayoutFlags properly override previous values.
        /// Expected result: The new flag value should replace the old one.
        /// </summary>
        [Fact]
        public void SetLayoutFlags_BindableObjectView_UpdatesExistingFlags()
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var bindableView = new Label();
            var initialFlags = AbsoluteLayoutFlags.XProportional;
            var updatedFlags = AbsoluteLayoutFlags.YProportional;

            // Act
            layout.SetLayoutFlags(bindableView, initialFlags);
            layout.SetLayoutFlags(bindableView, updatedFlags);

            // Assert
            var actualFlags = AbsoluteLayout.GetLayoutFlags(bindableView);
            Assert.Equal(updatedFlags, actualFlags);
        }

        /// <summary>
        /// Tests that SetLayoutFlags updates existing flag values on non-BindableObject views in the layout.
        /// This test verifies that subsequent calls to SetLayoutFlags properly override previous values in the internal dictionary.
        /// Expected result: The new flag value should replace the old one in the internal storage.
        /// </summary>
        [Fact]
        public void SetLayoutFlags_NonBindableObjectViewInLayout_UpdatesExistingFlags()
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var nonBindableView = Substitute.For<IView>();
            layout.Add(nonBindableView);
            var initialFlags = AbsoluteLayoutFlags.XProportional;
            var updatedFlags = AbsoluteLayoutFlags.YProportional;

            // Act
            layout.SetLayoutFlags(nonBindableView, initialFlags);
            layout.SetLayoutFlags(nonBindableView, updatedFlags);

            // Assert
            var actualFlags = layout.GetLayoutFlags(nonBindableView);
            Assert.Equal(updatedFlags, actualFlags);
        }

        /// <summary>
        /// Tests that SetLayoutBounds correctly sets bounds on a BindableObject view using the BindableProperty system.
        /// Validates the BindableObject code path (case BindableObject bo:).
        /// </summary>
        [Fact]
        public void SetLayoutBounds_WithBindableObjectView_SetsValueUsingBindableProperty()
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var bindableView = new Label();
            var bounds = new Rect(10, 20, 100, 200);

            // Act
            layout.SetLayoutBounds(bindableView, bounds);

            // Assert
            var actualBounds = AbsoluteLayout.GetLayoutBounds(bindableView);
            Assert.Equal(bounds.X, actualBounds.X);
            Assert.Equal(bounds.Y, actualBounds.Y);
            Assert.Equal(bounds.Width, actualBounds.Width);
            Assert.Equal(bounds.Height, actualBounds.Height);
        }

        /// <summary>
        /// Tests that SetLayoutBounds correctly sets bounds on a non-BindableObject view using the _viewInfo dictionary.
        /// Validates the default code path (_viewInfo[view].LayoutBounds = bounds).
        /// </summary>
        [Fact]
        public void SetLayoutBounds_WithNonBindableObjectView_SetsValueInViewInfoDictionary()
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var nonBindableView = Substitute.For<IView>();
            var bounds = new Rect(15, 25, 150, 250);

            // Add the view to the layout first so it exists in _viewInfo
            layout.Add(nonBindableView);

            // Act
            layout.SetLayoutBounds(nonBindableView, bounds);

            // Assert
            var actualBounds = layout.GetLayoutBounds(nonBindableView);
            Assert.Equal(bounds.X, actualBounds.X);
            Assert.Equal(bounds.Y, actualBounds.Y);
            Assert.Equal(bounds.Width, actualBounds.Width);
            Assert.Equal(bounds.Height, actualBounds.Height);
        }

        /// <summary>
        /// Tests SetLayoutBounds with various boundary values for Rect bounds parameter.
        /// Ensures the method handles edge cases like zero bounds, negative values, and AutoSize.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(-10, -20, 50, 100)]
        [InlineData(5, 10, -1, -1)] // AutoSize values
        [InlineData(double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue, 100, 200)]
        public void SetLayoutBounds_WithBoundaryRectValues_HandlesBoundsCorrectly(double x, double y, double width, double height)
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var bindableView = new Label();
            var bounds = new Rect(x, y, width, height);

            // Act
            layout.SetLayoutBounds(bindableView, bounds);

            // Assert
            var actualBounds = AbsoluteLayout.GetLayoutBounds(bindableView);
            Assert.Equal(bounds.X, actualBounds.X);
            Assert.Equal(bounds.Y, actualBounds.Y);
            Assert.Equal(bounds.Width, actualBounds.Width);
            Assert.Equal(bounds.Height, actualBounds.Height);
        }

        /// <summary>
        /// Tests that SetLayoutBounds throws KeyNotFoundException when called on a non-BindableObject view 
        /// that hasn't been added to the layout (not in _viewInfo dictionary).
        /// </summary>
        [Fact]
        public void SetLayoutBounds_WithNonBindableObjectViewNotInLayout_ThrowsKeyNotFoundException()
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var nonBindableView = Substitute.For<IView>();
            var bounds = new Rect(10, 20, 100, 200);

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => layout.SetLayoutBounds(nonBindableView, bounds));
        }

        /// <summary>
        /// Tests that SetLayoutBounds throws ArgumentNullException when view parameter is null.
        /// Validates proper null parameter handling.
        /// </summary>
        [Fact]
        public void SetLayoutBounds_WithNullView_ThrowsArgumentNullException()
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var bounds = new Rect(10, 20, 100, 200);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => layout.SetLayoutBounds(null, bounds));
        }

        /// <summary>
        /// Tests SetLayoutBounds with AutoSize special values to ensure proper handling of the AutoSize constant.
        /// Verifies both BindableObject and non-BindableObject paths work with AutoSize.
        /// </summary>
        [Fact]
        public void SetLayoutBounds_WithAutoSizeValues_HandlesBoundsCorrectly()
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var bindableView = new Label();
            var nonBindableView = Substitute.For<IView>();
            var autoSizeBounds = new Rect(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);

            layout.Add(nonBindableView);

            // Act
            layout.SetLayoutBounds(bindableView, autoSizeBounds);
            layout.SetLayoutBounds(nonBindableView, autoSizeBounds);

            // Assert
            var bindableBounds = AbsoluteLayout.GetLayoutBounds(bindableView);
            var nonBindableBounds = layout.GetLayoutBounds(nonBindableView);

            Assert.Equal(AbsoluteLayout.AutoSize, bindableBounds.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, bindableBounds.Height);
            Assert.Equal(AbsoluteLayout.AutoSize, nonBindableBounds.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, nonBindableBounds.Height);
        }

        /// <summary>
        /// Tests that OnClear properly clears the internal view info dictionary and calls base OnClear.
        /// Verifies that after clearing, previously added views get default layout bounds when re-added.
        /// </summary>
        [Fact]
        public void OnClear_WithViewsInLayout_ClearsViewInfoAndCallsBase()
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var label = new Label();
            var mockView = Substitute.For<IView>();
            var customBounds = new Rect(10, 20, 100, 200);
            var customFlags = AbsoluteLayoutFlags.PositionProportional;

            // Add views and set custom layout info to populate _viewInfo dictionary
            layout.Add(label);
            layout.Add(mockView);
            layout.SetLayoutBounds(label, customBounds);
            layout.SetLayoutFlags(label, customFlags);
            layout.SetLayoutBounds(mockView, customBounds);
            layout.SetLayoutFlags(mockView, customFlags);

            // Verify views are added and have custom values
            Assert.Equal(2, layout.Count);
            Assert.Equal(customBounds, layout.GetLayoutBounds(label));
            Assert.Equal(customFlags, layout.GetLayoutFlags(label));

            // Act
            layout.Clear(); // This should trigger OnClear()

            // Assert
            Assert.Equal(0, layout.Count);

            // Re-add the same views and verify they get default values (proving _viewInfo was cleared)
            layout.Add(label);
            layout.Add(mockView);

            var expectedDefaultBounds = new Rect(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
            var expectedDefaultFlags = AbsoluteLayoutFlags.None;

            Assert.Equal(expectedDefaultBounds, layout.GetLayoutBounds(label));
            Assert.Equal(expectedDefaultFlags, layout.GetLayoutFlags(label));
            Assert.Equal(expectedDefaultBounds, layout.GetLayoutBounds(mockView));
            Assert.Equal(expectedDefaultFlags, layout.GetLayoutFlags(mockView));
        }

        /// <summary>
        /// Tests that OnClear can be called safely on an empty layout without throwing exceptions.
        /// Verifies the method handles the edge case of clearing an already empty layout.
        /// </summary>
        [Fact]
        public void OnClear_WithEmptyLayout_DoesNotThrowException()
        {
            // Arrange
            var layout = new AbsoluteLayout();

            // Verify layout starts empty
            Assert.Equal(0, layout.Count);

            // Act & Assert - should not throw
            layout.Clear();

            // Verify still empty
            Assert.Equal(0, layout.Count);
        }

        /// <summary>
        /// Tests that OnClear can be called multiple times safely without side effects.
        /// Verifies the method is idempotent and doesn't cause issues when called repeatedly.
        /// </summary>
        [Fact]
        public void OnClear_CalledMultipleTimes_RemainsStable()
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var label = new Label();
            layout.Add(label);
            layout.SetLayoutBounds(label, new Rect(5, 10, 50, 100));

            // Act - clear multiple times
            layout.Clear();
            layout.Clear();
            layout.Clear();

            // Assert - should remain stable
            Assert.Equal(0, layout.Count);

            // Verify can still add views normally after multiple clears
            layout.Add(label);
            Assert.Equal(1, layout.Count);
            var bounds = layout.GetLayoutBounds(label);
            Assert.Equal(new Rect(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), bounds);
        }

        /// <summary>
        /// Tests that OnClear properly handles layouts containing both BindableObject views and IView mocks.
        /// Verifies that the clearing process works correctly for different view types.
        /// </summary>
        [Fact]
        public void OnClear_WithMixedViewTypes_ClearsAllViewInfo()
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var bindableView = new Label();
            var mockView1 = Substitute.For<IView>();
            var mockView2 = Substitute.For<IView>();

            var customBounds1 = new Rect(1, 2, 3, 4);
            var customBounds2 = new Rect(5, 6, 7, 8);
            var customFlags = AbsoluteLayoutFlags.HeightProportional | AbsoluteLayoutFlags.WidthProportional;

            // Add mixed view types with custom layout info
            layout.Add(bindableView);
            layout.Add(mockView1);
            layout.Add(mockView2);

            layout.SetLayoutBounds(bindableView, customBounds1);
            layout.SetLayoutFlags(bindableView, customFlags);
            layout.SetLayoutBounds(mockView1, customBounds2);
            layout.SetLayoutFlags(mockView1, customFlags);

            // Verify initial state
            Assert.Equal(3, layout.Count);

            // Act
            layout.Clear();

            // Assert
            Assert.Equal(0, layout.Count);

            // Re-add and verify all get default values
            layout.Add(bindableView);
            layout.Add(mockView1);
            layout.Add(mockView2);

            var defaultBounds = new Rect(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
            var defaultFlags = AbsoluteLayoutFlags.None;

            Assert.Equal(defaultBounds, layout.GetLayoutBounds(bindableView));
            Assert.Equal(defaultFlags, layout.GetLayoutFlags(bindableView));
            Assert.Equal(defaultBounds, layout.GetLayoutBounds(mockView1));
            Assert.Equal(defaultFlags, layout.GetLayoutFlags(mockView1));
            Assert.Equal(defaultBounds, layout.GetLayoutBounds(mockView2));
            Assert.Equal(defaultFlags, layout.GetLayoutFlags(mockView2));
        }

        /// <summary>
        /// Tests that OnInsert method correctly handles BindableObject views by not creating AbsoluteLayoutInfo entries.
        /// When a BindableObject view is inserted, it should use attached property mechanism instead of _viewInfo dictionary.
        /// Expected result: GetLayoutBounds returns default bounds from attached property without creating _viewInfo entry.
        /// </summary>
        [Fact]
        public void OnInsert_WithBindableObjectView_DoesNotCreateAbsoluteLayoutInfo()
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var bindableObjectView = new Label();
            var index = 0;

            // Act
            layout.Insert(index, bindableObjectView);

            // Assert
            var bounds = layout.GetLayoutBounds(bindableObjectView);
            Assert.Equal(0, bounds.X);
            Assert.Equal(0, bounds.Y);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Height);
        }

        /// <summary>
        /// Tests that OnInsert method correctly handles non-BindableObject views by creating AbsoluteLayoutInfo entries.
        /// When a non-BindableObject view is inserted, it should create an entry in _viewInfo dictionary.
        /// Expected result: GetLayoutBounds returns default bounds from _viewInfo dictionary entry.
        /// </summary>
        [Fact]
        public void OnInsert_WithNonBindableObjectView_CreatesAbsoluteLayoutInfo()
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var nonBindableObjectView = Substitute.For<IView>();
            var index = 0;

            // Act
            layout.Insert(index, nonBindableObjectView);

            // Assert
            var bounds = layout.GetLayoutBounds(nonBindableObjectView);
            Assert.Equal(0, bounds.X);
            Assert.Equal(0, bounds.Y);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Height);
        }

        /// <summary>
        /// Tests that OnInsert method is not called when inserting a null view.
        /// The Insert method has a null check that returns early, preventing OnInsert from being called.
        /// Expected result: No exception thrown, method returns early.
        /// </summary>
        [Fact]
        public void OnInsert_WithNullView_DoesNotThrow()
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var index = 0;

            // Act & Assert
            layout.Insert(index, null); // Should not throw or call OnInsert
        }

        /// <summary>
        /// Tests that OnInsert method works correctly with different valid index values.
        /// The method should handle various index positions for both BindableObject and non-BindableObject views.
        /// Expected result: Views are inserted at correct positions and have expected default layout bounds.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void OnInsert_WithDifferentIndices_InsertsViewsCorrectly(int index)
        {
            // Arrange
            var layout = new AbsoluteLayout();

            // Add some views first to create valid indices
            for (int i = 0; i < 3; i++)
            {
                layout.Add(new Label());
            }

            var bindableObjectView = new Label();
            var nonBindableObjectView = Substitute.For<IView>();

            // Act
            layout.Insert(index, bindableObjectView);
            layout.Insert(index + 1, nonBindableObjectView);

            // Assert
            var bindableBounds = layout.GetLayoutBounds(bindableObjectView);
            var nonBindableBounds = layout.GetLayoutBounds(nonBindableObjectView);

            // Both should have default bounds
            Assert.Equal(0, bindableBounds.X);
            Assert.Equal(0, bindableBounds.Y);
            Assert.Equal(AbsoluteLayout.AutoSize, bindableBounds.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, bindableBounds.Height);

            Assert.Equal(0, nonBindableBounds.X);
            Assert.Equal(0, nonBindableBounds.Y);
            Assert.Equal(AbsoluteLayout.AutoSize, nonBindableBounds.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, nonBindableBounds.Height);

            // Verify views are at expected positions
            Assert.Equal(bindableObjectView, layout.Children[index]);
            Assert.Equal(nonBindableObjectView, layout.Children[index + 1]);
        }

        /// <summary>
        /// Tests that OnInsert method correctly handles the boundary between BindableObject and non-BindableObject behavior.
        /// Verifies that layout flags and bounds can be retrieved correctly for both types after insertion.
        /// Expected result: Both view types should have default layout flags and bounds after insertion.
        /// </summary>
        [Fact]
        public void OnInsert_MixedViewTypes_HandlesBothCorrectly()
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var bindableObjectView = new Label();
            var nonBindableObjectView = Substitute.For<IView>();

            // Act
            layout.Insert(0, bindableObjectView);
            layout.Insert(1, nonBindableObjectView);

            // Assert
            // Test GetLayoutFlags for both types
            var bindableFlags = layout.GetLayoutFlags(bindableObjectView);
            var nonBindableFlags = layout.GetLayoutFlags(nonBindableObjectView);

            Assert.Equal(AbsoluteLayoutFlags.None, bindableFlags);
            Assert.Equal(AbsoluteLayoutFlags.None, nonBindableFlags);

            // Test GetLayoutBounds for both types
            var bindableBounds = layout.GetLayoutBounds(bindableObjectView);
            var nonBindableBounds = layout.GetLayoutBounds(nonBindableObjectView);

            // Both should have the same default bounds values
            Assert.Equal(bindableBounds.X, nonBindableBounds.X);
            Assert.Equal(bindableBounds.Y, nonBindableBounds.Y);
            Assert.Equal(bindableBounds.Width, nonBindableBounds.Width);
            Assert.Equal(bindableBounds.Height, nonBindableBounds.Height);
        }

        /// <summary>
        /// Tests that OnInsert method handles edge case with maximum integer index value.
        /// Verifies that the method doesn't fail with extreme index values that are still valid.
        /// Expected result: View is inserted successfully and has correct default bounds.
        /// </summary>
        [Fact]
        public void OnInsert_WithValidExtremeIndex_InsertsSuccessfully()
        {
            // Arrange
            var layout = new AbsoluteLayout();
            var view = new Label();

            // Add one view first so we have a valid extreme index
            layout.Add(new Label());
            var maxValidIndex = layout.Children.Count;

            // Act
            layout.Insert(maxValidIndex, view);

            // Assert
            var bounds = layout.GetLayoutBounds(view);
            Assert.Equal(0, bounds.X);
            Assert.Equal(0, bounds.Y);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Height);

            // Verify the view was added at the end
            Assert.Equal(view, layout.Children[maxValidIndex]);
        }

        /// <summary>
        /// Helper class to expose protected OnUpdate method for testing.
        /// </summary>
        private class TestableAbsoluteLayout : AbsoluteLayout
        {
            public void TestOnUpdate(int index, IView view, IView oldView)
            {
                OnUpdate(index, view, oldView);
            }
        }

        /// <summary>
        /// Tests OnUpdate when the new view is a BindableObject.
        /// Should remove oldView from internal dictionary and not add new view to dictionary since BindableObjects use attached properties.
        /// Should call base OnUpdate method without throwing exceptions.
        /// </summary>
        [Fact]
        public void OnUpdate_ViewIsBindableObject_RemovesOldViewAndDoesNotAddNewView()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var oldView = Substitute.For<IView>();
            var newView = new Label(); // BindableObject
            var index = 0;

            // Add oldView to layout first so it gets tracked in _viewInfo
            layout.Add(oldView);

            // Act
            layout.TestOnUpdate(index, newView, oldView);

            // Assert
            // For BindableObject, should use attached properties with default values
            var bounds = layout.GetLayoutBounds(newView);
            var flags = layout.GetLayoutFlags(newView);

            Assert.Equal(0, bounds.X);
            Assert.Equal(0, bounds.Y);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Height);
            Assert.Equal(AbsoluteLayoutFlags.None, flags);

            // Verify oldView access throws since it should be removed from _viewInfo
            Assert.ThrowsAny<Exception>(() => layout.GetLayoutBounds(oldView));
        }

        /// <summary>
        /// Tests OnUpdate when the new view is not a BindableObject.
        /// Should remove oldView from internal dictionary and add new view with default AbsoluteLayoutInfo.
        /// Should call base OnUpdate method without throwing exceptions.
        /// </summary>
        [Fact]
        public void OnUpdate_ViewIsNotBindableObject_RemovesOldViewAndAddsNewViewWithDefaults()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var oldView = Substitute.For<IView>();
            var newView = Substitute.For<IView>();
            var index = 1;

            // Add oldView to layout first so it gets tracked in _viewInfo
            layout.Add(oldView);

            // Act
            layout.TestOnUpdate(index, newView, oldView);

            // Assert
            // For non-BindableObject, should have default AbsoluteLayoutInfo values
            var bounds = layout.GetLayoutBounds(newView);
            var flags = layout.GetLayoutFlags(newView);

            Assert.Equal(0, bounds.X);
            Assert.Equal(0, bounds.Y);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Height);
            Assert.Equal(AbsoluteLayoutFlags.None, flags);

            // Verify oldView access throws since it should be removed from _viewInfo
            Assert.ThrowsAny<Exception>(() => layout.GetLayoutBounds(oldView));
        }

        /// <summary>
        /// Tests OnUpdate with null oldView parameter.
        /// Should handle null oldView gracefully without throwing exceptions when removing from dictionary.
        /// Should still process new view appropriately based on its type.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(int.MaxValue)]
        public void OnUpdate_NullOldView_HandlesGracefullyWithVariousIndices(int index)
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var newView = Substitute.For<IView>();
            IView oldView = null;

            // Act & Assert - Should not throw
            layout.TestOnUpdate(index, newView, oldView);

            // Verify newView was added with correct defaults
            var bounds = layout.GetLayoutBounds(newView);
            var flags = layout.GetLayoutFlags(newView);

            Assert.Equal(0, bounds.X);
            Assert.Equal(0, bounds.Y);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Height);
            Assert.Equal(AbsoluteLayoutFlags.None, flags);
        }

        /// <summary>
        /// Tests OnUpdate when oldView is not in the internal dictionary.
        /// Should handle gracefully without throwing exceptions when trying to remove non-existent view.
        /// Should still process new view appropriately.
        /// </summary>
        [Fact]
        public void OnUpdate_OldViewNotInDictionary_HandlesGracefully()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var oldView = Substitute.For<IView>(); // Not added to layout, so not in _viewInfo
            var newView = new Label(); // BindableObject
            var index = 2;

            // Act & Assert - Should not throw
            layout.TestOnUpdate(index, newView, oldView);

            // Verify newView uses attached properties correctly
            var bounds = layout.GetLayoutBounds(newView);
            Assert.Equal(0, bounds.X);
            Assert.Equal(0, bounds.Y);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Height);
        }

        /// <summary>
        /// Tests OnUpdate with boundary index values.
        /// Should accept various valid index values without throwing exceptions.
        /// Should process views appropriately regardless of index value.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(int.MaxValue)]
        public void OnUpdate_VariousIndexValues_ProcessesCorrectly(int index)
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var oldView = Substitute.For<IView>();
            var newView = Substitute.For<IView>();

            // Add oldView first
            layout.Add(oldView);

            // Act & Assert - Should not throw
            layout.TestOnUpdate(index, newView, oldView);

            // Verify processing completed correctly
            var bounds = layout.GetLayoutBounds(newView);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Height);
        }

        /// <summary>
        /// Tests OnUpdate creates AbsoluteLayoutInfo with correct default values.
        /// Should initialize LayoutBounds to (0, 0, AutoSize, AutoSize) and LayoutFlags to None.
        /// Should maintain these defaults for non-BindableObject views.
        /// </summary>
        [Fact]
        public void OnUpdate_NonBindableObjectView_CreatesCorrectDefaultAbsoluteLayoutInfo()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var oldView = Substitute.For<IView>();
            var newView = Substitute.For<IView>();
            var index = 0;

            // Add oldView first
            layout.Add(oldView);

            // Act
            layout.TestOnUpdate(index, newView, oldView);

            // Assert - Verify AbsoluteLayoutInfo defaults match expected values
            var bounds = layout.GetLayoutBounds(newView);
            var flags = layout.GetLayoutFlags(newView);

            Assert.Equal(0, bounds.X);
            Assert.Equal(0, bounds.Y);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Height);
            Assert.Equal(AbsoluteLayoutFlags.None, flags);

            // Verify AutoSize constant value
            Assert.Equal(-1, AbsoluteLayout.AutoSize);
        }

        /// <summary>
        /// Tests that CreateLayoutManager returns a non-null ILayoutManager instance.
        /// Verifies that the method successfully creates and returns a layout manager.
        /// </summary>
        [Fact]
        public void CreateLayoutManager_WhenCalled_ReturnsNonNullLayoutManager()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();

            // Act
            var result = layout.TestCreateLayoutManager();

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that CreateLayoutManager returns an instance of AbsoluteLayoutManager.
        /// Verifies that the correct type of layout manager is created.
        /// </summary>
        [Fact]
        public void CreateLayoutManager_WhenCalled_ReturnsAbsoluteLayoutManager()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();

            // Act
            var result = layout.TestCreateLayoutManager();

            // Assert
            Assert.IsType<AbsoluteLayoutManager>(result);
        }

        /// <summary>
        /// Tests that CreateLayoutManager creates new instances on each call.
        /// Verifies that no caching is performed and each call returns a distinct instance.
        /// </summary>
        [Fact]
        public void CreateLayoutManager_WhenCalledMultipleTimes_ReturnsNewInstances()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();

            // Act
            var result1 = layout.TestCreateLayoutManager();
            var result2 = layout.TestCreateLayoutManager();

            // Assert
            Assert.NotSame(result1, result2);
        }

        /// <summary>
        /// Tests that CreateLayoutManager properly initializes AbsoluteLayoutManager with correct AbsoluteLayout reference.
        /// Verifies that the created layout manager is properly configured with the parent layout.
        /// </summary>
        [Fact]
        public void CreateLayoutManager_WhenCalled_InitializesWithCorrectAbsoluteLayout()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();

            // Act
            var result = layout.TestCreateLayoutManager();

            // Assert
            var absoluteLayoutManager = Assert.IsType<AbsoluteLayoutManager>(result);
            Assert.Same(layout, absoluteLayoutManager.AbsoluteLayout);
        }

    }


    /// <summary>
    /// Unit tests for the AbsoluteLayout.GetLayoutBounds static method.
    /// </summary>
    public partial class AbsoluteLayoutGetLayoutBoundsTests
    {
        /// <summary>
        /// Tests that GetLayoutBounds throws NullReferenceException when bindable parameter is null.
        /// Verifies the method does not handle null input gracefully.
        /// </summary>
        [Fact]
        public void GetLayoutBounds_NullBindableObject_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindable = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => AbsoluteLayout.GetLayoutBounds(bindable));
        }

        /// <summary>
        /// Tests that GetLayoutBounds returns the default layout bounds for a BindableObject that hasn't had custom bounds set.
        /// Verifies the default bounds are Rect(0, 0, AutoSize, AutoSize).
        /// </summary>
        [Fact]
        public void GetLayoutBounds_BindableObjectWithDefaultBounds_ReturnsDefaultRect()
        {
            // Arrange
            var bindable = new Label();
            var expectedBounds = new Rect(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);

            // Act
            var actualBounds = AbsoluteLayout.GetLayoutBounds(bindable);

            // Assert
            Assert.Equal(expectedBounds.X, actualBounds.X);
            Assert.Equal(expectedBounds.Y, actualBounds.Y);
            Assert.Equal(expectedBounds.Width, actualBounds.Width);
            Assert.Equal(expectedBounds.Height, actualBounds.Height);
        }

        /// <summary>
        /// Tests that GetLayoutBounds returns the correct custom bounds when they have been explicitly set on a BindableObject.
        /// Verifies the method retrieves the value that was previously set using SetLayoutBounds.
        /// </summary>
        [Fact]
        public void GetLayoutBounds_BindableObjectWithCustomBounds_ReturnsCustomRect()
        {
            // Arrange
            var bindable = new Label();
            var customBounds = new Rect(10, 20, 100, 200);
            AbsoluteLayout.SetLayoutBounds(bindable, customBounds);

            // Act
            var actualBounds = AbsoluteLayout.GetLayoutBounds(bindable);

            // Assert
            Assert.Equal(customBounds.X, actualBounds.X);
            Assert.Equal(customBounds.Y, actualBounds.Y);
            Assert.Equal(customBounds.Width, actualBounds.Width);
            Assert.Equal(customBounds.Height, actualBounds.Height);
        }

        /// <summary>
        /// Tests GetLayoutBounds with various Rect values including edge cases and boundary conditions.
        /// Verifies the method correctly handles different coordinate and size values.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(-10, -20, -5, -15)]
        [InlineData(1000, 2000, 500, 600)]
        [InlineData(10, -5, -1, 100)] // Using AutoSize value (-1) for width
        [InlineData(-1, -1, -1, -1)] // All AutoSize values
        [InlineData(double.MaxValue, double.MinValue, 0, 1)]
        [InlineData(0.5, 1.5, 2.7, 3.9)] // Fractional values
        public void GetLayoutBounds_BindableObjectWithVariousRectValues_ReturnsCorrectRect(double x, double y, double width, double height)
        {
            // Arrange
            var bindable = new Label();
            var testBounds = new Rect(x, y, width, height);
            AbsoluteLayout.SetLayoutBounds(bindable, testBounds);

            // Act
            var actualBounds = AbsoluteLayout.GetLayoutBounds(bindable);

            // Assert
            Assert.Equal(testBounds.X, actualBounds.X);
            Assert.Equal(testBounds.Y, actualBounds.Y);
            Assert.Equal(testBounds.Width, actualBounds.Width);
            Assert.Equal(testBounds.Height, actualBounds.Height);
        }

        /// <summary>
        /// Tests GetLayoutBounds with special double values including infinity and NaN.
        /// Verifies the method handles extreme floating-point values correctly.
        /// </summary>
        [Theory]
        [InlineData(double.PositiveInfinity, 0, 100, 200)]
        [InlineData(0, double.NegativeInfinity, 100, 200)]
        [InlineData(0, 0, double.PositiveInfinity, 200)]
        [InlineData(0, 0, 100, double.NaN)]
        [InlineData(double.NaN, double.NaN, double.NaN, double.NaN)]
        public void GetLayoutBounds_BindableObjectWithSpecialDoubleValues_ReturnsCorrectRect(double x, double y, double width, double height)
        {
            // Arrange
            var bindable = new Label();
            var testBounds = new Rect(x, y, width, height);
            AbsoluteLayout.SetLayoutBounds(bindable, testBounds);

            // Act
            var actualBounds = AbsoluteLayout.GetLayoutBounds(bindable);

            // Assert
            Assert.Equal(testBounds.X, actualBounds.X);
            Assert.Equal(testBounds.Y, actualBounds.Y);
            Assert.Equal(testBounds.Width, actualBounds.Width);
            Assert.Equal(testBounds.Height, actualBounds.Height);
        }

        /// <summary>
        /// Tests that GetLayoutBounds works correctly with different types of BindableObjects.
        /// Verifies the method is not specific to Label objects and works with other BindableObject derivatives.
        /// </summary>
        [Fact]
        public void GetLayoutBounds_DifferentBindableObjectTypes_ReturnsCorrectRect()
        {
            // Arrange
            var button = new Button();
            var entry = new Entry();
            var testBounds = new Rect(50, 75, 150, 250);

            AbsoluteLayout.SetLayoutBounds(button, testBounds);
            AbsoluteLayout.SetLayoutBounds(entry, testBounds);

            // Act
            var buttonBounds = AbsoluteLayout.GetLayoutBounds(button);
            var entryBounds = AbsoluteLayout.GetLayoutBounds(entry);

            // Assert
            Assert.Equal(testBounds.X, buttonBounds.X);
            Assert.Equal(testBounds.Y, buttonBounds.Y);
            Assert.Equal(testBounds.Width, buttonBounds.Width);
            Assert.Equal(testBounds.Height, buttonBounds.Height);

            Assert.Equal(testBounds.X, entryBounds.X);
            Assert.Equal(testBounds.Y, entryBounds.Y);
            Assert.Equal(testBounds.Width, entryBounds.Width);
            Assert.Equal(testBounds.Height, entryBounds.Height);
        }
    }
}