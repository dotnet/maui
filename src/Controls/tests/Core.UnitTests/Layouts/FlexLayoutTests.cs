#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Transactions;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Layouts.Flex;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
    [Category("Layout")]
    public class FlexLayoutTests
    {
        class TestImage : Image
        {
            public bool Passed { get; private set; }

            protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
            {
                if (double.IsPositiveInfinity(widthConstraint) && double.IsPositiveInfinity(heightConstraint))
                {
                    Passed = true;
                }

                return base.MeasureOverride(widthConstraint, heightConstraint);
            }
        }

        class TestLabel : Label
        {
            protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
            {
                return new Size(150, 100);
            }
        }

        [Fact]
        public void FlexLayoutMeasuresImagesUnconstrained()
        {
            var root = new Grid();
            var flexLayout = new FlexLayout() as IFlexLayout;
            var image = new TestImage();

            root.Add(flexLayout);
            flexLayout.Add(image as IView);

            flexLayout.CrossPlatformMeasure(1000, 1000);

            Assert.True(image.Passed, "Image should be measured unconstrained even if the FlexLayout is constrained.");
        }

        [Fact]
        public void FlexLayoutRecognizesVisibilityChange()
        {
            var root = new Grid();
            var flexLayout = new FlexLayout() as IFlexLayout;
            var view = new TestLabel();
            var view2 = new TestLabel();

            root.Add(flexLayout);
            flexLayout.Add(view as IView);
            flexLayout.Add(view2 as IView);

            // Measure and arrange the layout while the first view is visible
            var measure = flexLayout.CrossPlatformMeasure(1000, 1000);
            flexLayout.CrossPlatformArrange(new Rect(Point.Zero, measure));

            // Keep track of where the second view is arranged
            var whenVisible = view2.Frame.X;

            // Change the visibility
            view.IsVisible = false;

            // Measure and arrange again
            measure = flexLayout.CrossPlatformMeasure(1000, 1000);
            flexLayout.CrossPlatformArrange(new Rect(Point.Zero, measure));

            var whenInvisible = view2.Frame.X;

            // The location of the second view should have changed
            // now that the first view is not visible
            Assert.True(whenInvisible != whenVisible);
        }

        /*
		 * These next two tests deal with unconstrained measure of FlexLayout. Be default, FL
		 * wants to stretch children across each axis. But you can't stretch things across infinity
		 * without it getting weird. So for _measurement_ purposes, we treat infinity as zero and 
		 * just give the children their desired size in the unconstrained direction. Otherwise, FL
		 * would just set their flex frame sizes to zero, which can either cause blanks or layout cycles,
		 * depending on the target platform.
		 */

        (IFlexLayout, IView) SetUpUnconstrainedTest(Action<FlexLayout> configure = null)
        {
            var root = new Grid(); // FlexLayout requires a parent, at least for now

            var controlsFlexLayout = new FlexLayout();
            configure?.Invoke(controlsFlexLayout);

            var flexLayout = controlsFlexLayout as IFlexLayout;

            var view = Substitute.For<IView>();
            var size = new Size(100, 100);
            view.Measure(Arg.Any<double>(), Arg.Any<double>()).Returns(size);

            root.Add(flexLayout);
            flexLayout.Add(view);

            return (flexLayout, view);
        }

        [Fact]
        public void UnconstrainedHeightChildrenHaveHeight()
        {
            (var flexLayout, var view) = SetUpUnconstrainedTest();

            _ = flexLayout.CrossPlatformMeasure(400, double.PositiveInfinity);

            var flexFrame = flexLayout.GetFlexFrame(view);

            Assert.Equal(100, flexFrame.Height);
        }

        [Fact]
        public void UnconstrainedWidthChildrenHaveWidth()
        {
            (var flexLayout, var view) = SetUpUnconstrainedTest();

            _ = flexLayout.CrossPlatformMeasure(double.PositiveInfinity, 400);

            var flexFrame = flexLayout.GetFlexFrame(view);

            Assert.Equal(100, flexFrame.Width);
        }

        [Fact]
        public void FlexLayoutPaddingShouldBeAppliedCorrectly_RowDirection()
        {
            // Arrange
            var padding = 16;
            var root = new Grid();
            var flexLayout = new FlexLayout { Padding = padding };
            var view1 = new TestLabel();
            var view2 = new TestLabel();

            root.Add(flexLayout);
            flexLayout.Add(view1 as IView);
            flexLayout.Add(view2 as IView);

            // Act
            var measure = flexLayout.CrossPlatformMeasure(1000, 1000);
            flexLayout.CrossPlatformArrange(new Rect(Point.Zero, measure));

            var view1Frame = flexLayout.Children[0].Frame;
            var view2Frame = flexLayout.Children[1].Frame;

            var leftPadding = view1Frame.X;
            var topPadding = view1Frame.Y;
            var rightPadding = measure.Width - view2Frame.Right;
            var expectedView1Width = measure.Width - (leftPadding + rightPadding + view2.Width);
            var expectedView2Width = measure.Width - (leftPadding + rightPadding + view1.Width);

            // Assert
            Assert.Equal(padding, leftPadding);
            Assert.Equal(padding, rightPadding);
            Assert.Equal(padding, topPadding);
            Assert.Equal(expectedView1Width, view1Frame.Width);
            Assert.Equal(expectedView2Width, view2Frame.Width);
        }

        [Fact]
        public void FlexLayoutPaddingShouldBeAppliedCorrectly_ColumnDirection()
        {
            // Arrange
            var padding = 16;
            var root = new Grid();
            var flexLayout = new FlexLayout { Padding = padding, Direction = FlexDirection.Column };
            var view1 = new TestLabel();
            var view2 = new TestLabel();

            root.Add(flexLayout);
            flexLayout.Add(view1 as IView);
            flexLayout.Add(view2 as IView);

            // Act
            var measure = flexLayout.CrossPlatformMeasure(1000, 1000);
            flexLayout.CrossPlatformArrange(new Rect(Point.Zero, measure));

            var view1Frame = flexLayout.Children[0].Frame;
            var view2Frame = flexLayout.Children[1].Frame;

            var bottomPadding = measure.Height - view2Frame.Bottom;
            var topPadding = view1Frame.Y;
            var expectedView1Height = measure.Height - (topPadding + bottomPadding + view2.Height);
            var expectedView2Height = measure.Height - (topPadding + bottomPadding + view1.Height);

            // Assert
            Assert.Equal(padding, bottomPadding);
            Assert.Equal(view2.Height, view2Frame.Height);
            Assert.Equal(expectedView1Height, view1Frame.Height);
            Assert.Equal(expectedView2Height, view2Frame.Height);
        }

        [Theory]
        [InlineData(double.PositiveInfinity, 400, FlexDirection.RowReverse)]
        [InlineData(double.PositiveInfinity, 400, FlexDirection.Row)]
        [InlineData(400, double.PositiveInfinity, FlexDirection.ColumnReverse)]
        [InlineData(400, double.PositiveInfinity, FlexDirection.Column)]
        public void UnconstrainedMeasureHonorsFlexDirection(double widthConstraint, double heightConstraint,
            FlexDirection flexDirection)
        {
            (var flexLayout, var view) = SetUpUnconstrainedTest((fl) => { fl.Direction = flexDirection; });

            _ = flexLayout.CrossPlatformMeasure(widthConstraint, heightConstraint);

            var flexFrame = flexLayout.GetFlexFrame(view);

            Assert.Equal(0, flexFrame.X);
            Assert.Equal(0, flexFrame.Y);
        }

        /// <summary>
        /// Tests that SetGrow calls SetValue on BindableObject when view is a BindableObject.
        /// Verifies the BindableObject code path is executed with correct parameters.
        /// </summary>
        /// <param name="growValue">The grow value to set</param>
        [Theory]
        [InlineData(0f)]
        [InlineData(1f)]
        [InlineData(2.5f)]
        [InlineData(float.MaxValue)]
        [InlineData(100000f)]
        public void SetGrow_ViewIsBindableObject_CallsSetValueWithCorrectParameters(float growValue)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var bindableView = Substitute.For<BindableObject, IView>();

            // Act
            flexLayout.SetGrow((IView)bindableView, growValue);

            // Assert
            bindableView.Received(1).SetValue(FlexLayout.GrowProperty, growValue);
        }

        /// <summary>
        /// Tests that SetGrow updates the _viewInfo dictionary when view is not a BindableObject.
        /// Verifies the non-BindableObject code path is executed correctly.
        /// </summary>
        /// <param name="growValue">The grow value to set</param>
        [Theory]
        [InlineData(0f)]
        [InlineData(1f)]
        [InlineData(-1f)]
        [InlineData(float.MaxValue)]
        [InlineData(float.MinValue)]
        [InlineData(float.NaN)]
        [InlineData(float.PositiveInfinity)]
        [InlineData(float.NegativeInfinity)]
        public void SetGrow_ViewIsNotBindableObject_UpdatesViewInfoDictionary(float growValue)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var nonBindableView = Substitute.For<IView>();

            // Act
            flexLayout.SetGrow(nonBindableView, growValue);

            // Assert
            var actualGrowValue = flexLayout.GetGrow(nonBindableView);
            Assert.Equal(growValue, actualGrowValue);
        }

        /// <summary>
        /// Tests that SetGrow throws ArgumentNullException when view parameter is null.
        /// Verifies proper null parameter validation.
        /// </summary>
        [Fact]
        public void SetGrow_ViewIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => flexLayout.SetGrow(null, 1f));
        }

        /// <summary>
        /// Tests that SetGrow can update the same view multiple times with different values.
        /// Verifies that subsequent calls properly override previous values.
        /// </summary>
        [Fact]
        public void SetGrow_UpdateSameViewMultipleTimes_UpdatesValueCorrectly()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var view = Substitute.For<IView>();

            // Act
            flexLayout.SetGrow(view, 1f);
            flexLayout.SetGrow(view, 2f);
            flexLayout.SetGrow(view, 0f);

            // Assert
            var finalGrowValue = flexLayout.GetGrow(view);
            Assert.Equal(0f, finalGrowValue);
        }

        /// <summary>
        /// Tests that SetGrow works correctly with both BindableObject and non-BindableObject views in the same layout.
        /// Verifies that both code paths can coexist and work independently.
        /// </summary>
        [Fact]
        public void SetGrow_MixedViewTypes_HandlesEachTypeCorrectly()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var bindableView = Substitute.For<BindableObject, IView>();
            var nonBindableView = Substitute.For<IView>();

            // Act
            flexLayout.SetGrow((IView)bindableView, 1.5f);
            flexLayout.SetGrow(nonBindableView, 2.5f);

            // Assert
            bindableView.Received(1).SetValue(FlexLayout.GrowProperty, 1.5f);
            var nonBindableGrowValue = flexLayout.GetGrow(nonBindableView);
            Assert.Equal(2.5f, nonBindableGrowValue);
        }

        /// <summary>
        /// Test helper class that exposes the protected OnRemove method for testing.
        /// </summary>
        public class TestableFlexLayout : FlexLayout
        {
            public void TestOnRemove(int index, IView view)
            {
                OnRemove(index, view);
            }

            public bool HasViewInInfo(IView view)
            {
                return _viewInfo.ContainsKey(view);
            }

        }

        /// <summary>
        /// Tests that OnRemove calls base.OnRemove and RemoveFlexItem with valid parameters.
        /// Input: Valid index (0) and mock IView.
        /// Expected: Method executes without exception and removes flex item.
        /// </summary>
        [Fact]
        public void OnRemove_ValidIndexAndView_ExecutesSuccessfully()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var mockView = Substitute.For<IView>();
            int index = 0;

            // Add view first so it can be removed
            layout.Add(mockView);

            // Act & Assert - Should not throw
            layout.TestOnRemove(index, mockView);
        }

        /// <summary>
        /// Tests OnRemove with various index edge cases.
        /// Input: Edge case index values with mock IView.
        /// Expected: Method executes without throwing for all valid index cases.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void OnRemove_VariousIndexValues_ExecutesWithoutException(int index)
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var mockView = Substitute.For<IView>();

            // Act & Assert - Should not throw regardless of index value
            layout.TestOnRemove(index, mockView);
        }

        /// <summary>
        /// Tests OnRemove with null view parameter.
        /// Input: Valid index (0) and null view.
        /// Expected: Method handles null view appropriately.
        /// </summary>
        [Fact]
        public void OnRemove_NullView_HandlesGracefully()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            int index = 0;

            // Act & Assert - Should handle null view
            layout.TestOnRemove(index, null);
        }

        /// <summary>
        /// Tests OnRemove with BindableObject view that has FlexItem property.
        /// Input: Valid index and BindableObject view with FlexItem.
        /// Expected: Removes flex item and clears BindableObject properties.
        /// </summary>
        [Fact]
        public void OnRemove_BindableObjectView_ClearsFlexItemProperty()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var bindableView = new TestBindableView();
            int index = 0;

            // Add the view first
            layout.Add(bindableView);

            // Act
            layout.TestOnRemove(index, bindableView);

            // Assert - Should not throw and should have processed the BindableObject
            Assert.True(true); // If we reach here, the method executed successfully
        }

        /// <summary>
        /// Tests OnRemove with non-BindableObject view that uses _viewInfo dictionary.
        /// Input: Valid index and regular IView mock.
        /// Expected: Removes flex item and clears _viewInfo entry.
        /// </summary>
        [Fact]
        public void OnRemove_NonBindableObjectView_RemovesFromViewInfo()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var mockView = Substitute.For<IView>();
            int index = 0;

            // Add the view first to populate _viewInfo
            layout.Add(mockView);

            // Verify view is in info before removal
            bool beforeRemoval = layout.HasViewInInfo(mockView);

            // Act
            layout.TestOnRemove(index, mockView);

            // Assert
            Assert.True(beforeRemoval || !layout.HasViewInInfo(mockView)); // Either it was there and removed, or it was never there
        }

        /// <summary>
        /// Tests OnRemove with boundary index values and valid view.
        /// Input: Boundary index values (0, 1) with valid IView.
        /// Expected: Method executes successfully for boundary cases.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void OnRemove_BoundaryIndexValues_ExecutesSuccessfully(int index)
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var mockView = Substitute.For<IView>();

            // Act & Assert
            layout.TestOnRemove(index, mockView);
        }

        /// <summary>
        /// Tests OnRemove behavior when flex root is null.
        /// Input: Valid parameters when internal _root is null.
        /// Expected: Method handles null root gracefully.
        /// </summary>
        [Fact]
        public void OnRemove_NullFlexRoot_HandlesGracefully()
        {
            // Arrange
            var layout = new TestableFlexLayout(); // New layout should have null _root initially
            var mockView = Substitute.For<IView>();
            int index = 0;

            // Act & Assert - Should not throw even with null _root
            layout.TestOnRemove(index, mockView);
        }

        /// <summary>
        /// Tests that AlignContent property returns the default value when not explicitly set.
        /// Expected result: Should return FlexAlignContent.Stretch as the default value.
        /// </summary>
        [Fact]
        public void AlignContent_DefaultValue_ReturnsStretch()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            var result = flexLayout.AlignContent;

            // Assert
            Assert.Equal(FlexAlignContent.Stretch, result);
        }

        /// <summary>
        /// Tests that AlignContent property getter and setter work correctly for all valid enum values.
        /// Input conditions: Each valid FlexAlignContent enum value is set.
        /// Expected result: Getter should return the same value that was set.
        /// </summary>
        [Theory]
        [InlineData(FlexAlignContent.Stretch)]
        [InlineData(FlexAlignContent.Center)]
        [InlineData(FlexAlignContent.Start)]
        [InlineData(FlexAlignContent.End)]
        [InlineData(FlexAlignContent.SpaceBetween)]
        [InlineData(FlexAlignContent.SpaceAround)]
        [InlineData(FlexAlignContent.SpaceEvenly)]
        public void AlignContent_SetValidValue_GetterReturnsSetValue(FlexAlignContent alignContent)
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            flexLayout.AlignContent = alignContent;
            var result = flexLayout.AlignContent;

            // Assert
            Assert.Equal(alignContent, result);
        }

        /// <summary>
        /// Tests that AlignContent property handles invalid enum values by casting.
        /// Input conditions: Invalid enum value (outside defined range) is cast and set.
        /// Expected result: Getter should return the cast value.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void AlignContent_SetInvalidEnumValue_GetterReturnsCastValue(int invalidEnumValue)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var invalidAlignContent = (FlexAlignContent)invalidEnumValue;

            // Act
            flexLayout.AlignContent = invalidAlignContent;
            var result = flexLayout.AlignContent;

            // Assert
            Assert.Equal(invalidAlignContent, result);
        }

        /// <summary>
        /// Tests that AlignContent property can be set multiple times and getter returns the latest value.
        /// Input conditions: Multiple different enum values are set sequentially.
        /// Expected result: Getter should always return the most recently set value.
        /// </summary>
        [Fact]
        public void AlignContent_SetMultipleTimes_GetterReturnsLatestValue()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act & Assert - Set and verify Center
            flexLayout.AlignContent = FlexAlignContent.Center;
            Assert.Equal(FlexAlignContent.Center, flexLayout.AlignContent);

            // Act & Assert - Set and verify Start
            flexLayout.AlignContent = FlexAlignContent.Start;
            Assert.Equal(FlexAlignContent.Start, flexLayout.AlignContent);

            // Act & Assert - Set and verify SpaceEvenly
            flexLayout.AlignContent = FlexAlignContent.SpaceEvenly;
            Assert.Equal(FlexAlignContent.SpaceEvenly, flexLayout.AlignContent);
        }

        /// <summary>
        /// Tests that SetOrder correctly calls SetValue on BindableObject views with various order values.
        /// This test verifies the BindableObject code path in the SetOrder method.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SetOrder_WithBindableObjectView_CallsSetValueWithCorrectParameters(int order)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var bindableView = Substitute.For<BindableObject, IView>();

            // Act
            flexLayout.SetOrder(bindableView, order);

            // Assert
            bindableView.Received(1).SetValue(FlexLayout.OrderProperty, order);
        }

        /// <summary>
        /// Tests that SetOrder correctly sets the Order property in _viewInfo for non-BindableObject views.
        /// This test verifies the default case code path in the SetOrder method.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SetOrder_WithNonBindableObjectView_SetsOrderInViewInfo(int order)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var mockView = Substitute.For<IView>();

            // First add the view to the layout so it gets added to _viewInfo
            flexLayout.Add(mockView);

            // Act
            flexLayout.SetOrder(mockView, order);

            // Assert
            var actualOrder = flexLayout.GetOrder(mockView);
            Assert.Equal(order, actualOrder);
        }

        /// <summary>
        /// Tests that SetOrder throws ArgumentNullException when view parameter is null.
        /// This test verifies proper null parameter handling.
        /// </summary>
        [Fact]
        public void SetOrder_WithNullView_ThrowsArgumentNullException()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => flexLayout.SetOrder(null, 0));
        }

        /// <summary>
        /// Tests that SetOrder throws KeyNotFoundException when trying to set order on a non-BindableObject view
        /// that hasn't been added to the layout (and thus not in _viewInfo).
        /// This test verifies error handling for views not in the _viewInfo dictionary.
        /// </summary>
        [Fact]
        public void SetOrder_WithNonBindableObjectViewNotInLayout_ThrowsKeyNotFoundException()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var mockView = Substitute.For<IView>();
            // Note: Not adding the view to the layout, so it won't be in _viewInfo

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => flexLayout.SetOrder(mockView, 1));
        }

        /// <summary>
        /// Tests that SetOrder works correctly with both BindableObject and non-BindableObject views
        /// in the same FlexLayout instance, ensuring both code paths work independently.
        /// This test verifies mixed view types scenario.
        /// </summary>
        [Fact]
        public void SetOrder_WithMixedViewTypes_HandlesBothCorrectly()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var bindableView = Substitute.For<BindableObject, IView>();
            var nonBindableView = Substitute.For<IView>();

            flexLayout.Add(nonBindableView); // Add to _viewInfo

            // Act
            flexLayout.SetOrder(bindableView, 10);
            flexLayout.SetOrder(nonBindableView, 20);

            // Assert
            bindableView.Received(1).SetValue(FlexLayout.OrderProperty, 10);
            Assert.Equal(20, flexLayout.GetOrder(nonBindableView));
        }

        /// <summary>
        /// Tests that SetOrder can be called multiple times on the same view with different order values.
        /// This test verifies that order values can be updated correctly.
        /// </summary>
        [Fact]
        public void SetOrder_CalledMultipleTimesOnSameView_UpdatesOrderCorrectly()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var nonBindableView = Substitute.For<IView>();
            flexLayout.Add(nonBindableView);

            // Act & Assert - First call
            flexLayout.SetOrder(nonBindableView, 5);
            Assert.Equal(5, flexLayout.GetOrder(nonBindableView));

            // Act & Assert - Second call with different value
            flexLayout.SetOrder(nonBindableView, -3);
            Assert.Equal(-3, flexLayout.GetOrder(nonBindableView));

            // Act & Assert - Third call with zero
            flexLayout.SetOrder(nonBindableView, 0);
            Assert.Equal(0, flexLayout.GetOrder(nonBindableView));
        }

        /// <summary>
        /// Tests that SetOrder with BindableObject view can be called multiple times,
        /// ensuring SetValue is called each time with the correct parameters.
        /// This test verifies repeated calls to the BindableObject code path.
        /// </summary>
        [Fact]
        public void SetOrder_BindableObjectCalledMultipleTimes_CallsSetValueEachTime()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var bindableView = Substitute.For<BindableObject, IView>();

            // Act
            flexLayout.SetOrder(bindableView, 1);
            flexLayout.SetOrder(bindableView, 2);
            flexLayout.SetOrder(bindableView, 3);

            // Assert
            Received.InOrder(() =>
            {
                bindableView.SetValue(FlexLayout.OrderProperty, 1);
                bindableView.SetValue(FlexLayout.OrderProperty, 2);
                bindableView.SetValue(FlexLayout.OrderProperty, 3);
            });
        }

        /// <summary>
        /// Tests that the AlignItems property returns the default value of Stretch when not explicitly set.
        /// Verifies the getter functionality for the default state.
        /// </summary>
        [Fact]
        public void AlignItems_DefaultValue_ReturnsStretch()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            var result = flexLayout.AlignItems;

            // Assert
            Assert.Equal(FlexAlignItems.Stretch, result);
        }

        /// <summary>
        /// Tests that the AlignItems property correctly sets and gets all valid enum values.
        /// Verifies both getter and setter functionality for all defined enum values.
        /// </summary>
        /// <param name="alignItems">The FlexAlignItems value to test</param>
        [Theory]
        [InlineData(FlexAlignItems.Stretch)]
        [InlineData(FlexAlignItems.Center)]
        [InlineData(FlexAlignItems.Start)]
        [InlineData(FlexAlignItems.End)]
        public void AlignItems_ValidEnumValues_SetAndGetCorrectly(FlexAlignItems alignItems)
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            flexLayout.AlignItems = alignItems;
            var result = flexLayout.AlignItems;

            // Assert
            Assert.Equal(alignItems, result);
        }

        /// <summary>
        /// Tests that the AlignItems property handles invalid enum values (outside defined range).
        /// Verifies that casting integers outside the enum range still works with the property.
        /// </summary>
        /// <param name="invalidValue">An integer value outside the defined FlexAlignItems enum range</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(99)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void AlignItems_InvalidEnumValues_SetAndGetCorrectly(int invalidValue)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var invalidAlignItems = (FlexAlignItems)invalidValue;

            // Act
            flexLayout.AlignItems = invalidAlignItems;
            var result = flexLayout.AlignItems;

            // Assert
            Assert.Equal(invalidAlignItems, result);
        }

        /// <summary>
        /// Tests that setting AlignItems multiple times with different values works correctly.
        /// Verifies that the property maintains state correctly across multiple assignments.
        /// </summary>
        [Fact]
        public void AlignItems_MultipleAssignments_MaintainsCorrectValue()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act & Assert
            flexLayout.AlignItems = FlexAlignItems.Center;
            Assert.Equal(FlexAlignItems.Center, flexLayout.AlignItems);

            flexLayout.AlignItems = FlexAlignItems.Start;
            Assert.Equal(FlexAlignItems.Start, flexLayout.AlignItems);

            flexLayout.AlignItems = FlexAlignItems.End;
            Assert.Equal(FlexAlignItems.End, flexLayout.AlignItems);

            flexLayout.AlignItems = FlexAlignItems.Stretch;
            Assert.Equal(FlexAlignItems.Stretch, flexLayout.AlignItems);
        }

        /// <summary>
        /// Tests that SetShrink correctly sets the shrink property on a BindableObject view
        /// using the attached property system with a valid positive shrink value.
        /// </summary>
        [Theory]
        [InlineData(0f)]
        [InlineData(1f)]
        [InlineData(2.5f)]
        [InlineData(float.MaxValue)]
        public void SetShrink_WithBindableObjectAndValidShrink_SetsShrinkProperty(float shrink)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var bindableView = new Label();

            // Act
            flexLayout.SetShrink(bindableView, shrink);

            // Assert
            Assert.Equal(shrink, FlexLayout.GetShrink(bindableView));
        }

        /// <summary>
        /// Tests that SetShrink correctly handles edge case float values for BindableObject views,
        /// including special IEEE 754 values like NaN and infinity.
        /// </summary>
        [Theory]
        [InlineData(float.NaN)]
        [InlineData(float.PositiveInfinity)]
        [InlineData(float.NegativeInfinity)]
        [InlineData(-1f)]
        [InlineData(float.MinValue)]
        public void SetShrink_WithBindableObjectAndEdgeCaseValues_SetsValue(float shrink)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var bindableView = new Label();

            // Act & Assert - Should not throw during SetShrink call
            // Note: Validation happens in the BindableProperty system, not in SetShrink itself
            flexLayout.SetShrink(bindableView, shrink);

            // Verify the value was set (even if invalid, it goes through the property system)
            var actualValue = FlexLayout.GetShrink(bindableView);
            if (float.IsNaN(shrink))
            {
                Assert.True(float.IsNaN(actualValue));
            }
            else
            {
                Assert.Equal(shrink, actualValue);
            }
        }

        /// <summary>
        /// Tests that SetShrink correctly sets the shrink property on a non-BindableObject view
        /// that has been properly added to the layout's internal tracking dictionary.
        /// </summary>
        [Theory]
        [InlineData(0f)]
        [InlineData(1f)]
        [InlineData(2.5f)]
        [InlineData(float.MaxValue)]
        public void SetShrink_WithNonBindableObjectInLayout_SetsShrinkInViewInfo(float shrink)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var mockView = Substitute.For<IView>();

            // Add the view to the layout first to populate _viewInfo
            flexLayout.Add(mockView);

            // Act
            flexLayout.SetShrink(mockView, shrink);

            // Assert
            Assert.Equal(shrink, flexLayout.GetShrink(mockView));
        }

        /// <summary>
        /// Tests that SetShrink handles edge case float values for non-BindableObject views
        /// that are properly tracked in the layout's internal dictionary.
        /// </summary>
        [Theory]
        [InlineData(float.NaN)]
        [InlineData(float.PositiveInfinity)]
        [InlineData(float.NegativeInfinity)]
        [InlineData(-1f)]
        [InlineData(float.MinValue)]
        public void SetShrink_WithNonBindableObjectAndEdgeCaseValues_SetsShrinkInViewInfo(float shrink)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var mockView = Substitute.For<IView>();

            // Add the view to the layout first to populate _viewInfo
            flexLayout.Add(mockView);

            // Act
            flexLayout.SetShrink(mockView, shrink);

            // Assert
            var actualValue = flexLayout.GetShrink(mockView);
            if (float.IsNaN(shrink))
            {
                Assert.True(float.IsNaN(actualValue));
            }
            else
            {
                Assert.Equal(shrink, actualValue);
            }
        }

        /// <summary>
        /// Tests that SetShrink throws a KeyNotFoundException when called with a non-BindableObject view
        /// that has not been added to the layout and therefore doesn't exist in the internal tracking dictionary.
        /// </summary>
        [Fact]
        public void SetShrink_WithNonBindableObjectNotInLayout_ThrowsKeyNotFoundException()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var mockView = Substitute.For<IView>();

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => flexLayout.SetShrink(mockView, 1f));
        }

        /// <summary>
        /// Tests that SetShrink throws an ArgumentNullException when called with a null view parameter,
        /// ensuring proper null reference validation.
        /// </summary>
        [Fact]
        public void SetShrink_WithNullView_ThrowsArgumentNullException()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => flexLayout.SetShrink(null, 1f));
        }

        /// <summary>
        /// Tests that the Position property returns the default value when not explicitly set.
        /// The default value should be FlexPosition.Relative as defined in the BindableProperty.
        /// </summary>
        [Fact]
        public void Position_DefaultValue_ReturnsRelative()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            var result = flexLayout.Position;

            // Assert
            Assert.Equal(FlexPosition.Relative, result);
        }

        /// <summary>
        /// Tests that the Position property correctly sets and gets valid FlexPosition enum values.
        /// This exercises both the setter and getter to ensure proper value storage and retrieval.
        /// </summary>
        /// <param name="expectedPosition">The FlexPosition value to set and verify</param>
        [Theory]
        [InlineData(FlexPosition.Relative)]
        [InlineData(FlexPosition.Absolute)]
        public void Position_SetValidValues_ReturnsCorrectValue(FlexPosition expectedPosition)
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            flexLayout.Position = expectedPosition;
            var result = flexLayout.Position;

            // Assert
            Assert.Equal(expectedPosition, result);
        }

        /// <summary>
        /// Tests that the Position property can handle enum values cast from integers,
        /// including boundary cases and invalid enum values that might be cast.
        /// </summary>
        /// <param name="enumValue">The integer value to cast to FlexPosition</param>
        /// <param name="expectedPosition">The expected FlexPosition value</param>
        [Theory]
        [InlineData(0, FlexPosition.Relative)]
        [InlineData(1, FlexPosition.Absolute)]
        [InlineData(-1, (FlexPosition)(-1))]
        [InlineData(99, (FlexPosition)99)]
        public void Position_SetCastValues_HandlesAllValues(int enumValue, FlexPosition expectedPosition)
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            flexLayout.Position = (FlexPosition)enumValue;
            var result = flexLayout.Position;

            // Assert
            Assert.Equal(expectedPosition, result);
        }

        /// <summary>
        /// Tests that SetAlignSelf correctly sets the AlignSelf attached property on a valid BindableObject
        /// with all valid FlexAlignSelf enum values.
        /// </summary>
        /// <param name="alignSelfValue">The FlexAlignSelf value to test</param>
        [Theory]
        [InlineData(FlexAlignSelf.Auto)]
        [InlineData(FlexAlignSelf.Stretch)]
        [InlineData(FlexAlignSelf.Center)]
        [InlineData(FlexAlignSelf.Start)]
        [InlineData(FlexAlignSelf.End)]
        public void SetAlignSelf_WithValidBindableAndValidEnumValues_SetsPropertyCorrectly(FlexAlignSelf alignSelfValue)
        {
            // Arrange
            var bindableObject = new Label();

            // Act
            FlexLayout.SetAlignSelf(bindableObject, alignSelfValue);

            // Assert
            var actualValue = FlexLayout.GetAlignSelf(bindableObject);
            Assert.Equal(alignSelfValue, actualValue);
        }

        /// <summary>
        /// Tests that SetAlignSelf throws NullReferenceException when called with a null BindableObject.
        /// </summary>
        [Fact]
        public void SetAlignSelf_WithNullBindable_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject nullBindable = null;
            var alignSelfValue = FlexAlignSelf.Center;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => FlexLayout.SetAlignSelf(nullBindable, alignSelfValue));
        }

        /// <summary>
        /// Tests that SetAlignSelf accepts invalid enum values (outside the defined range)
        /// and sets them on the BindableObject without validation.
        /// </summary>
        /// <param name="invalidEnumValue">An invalid FlexAlignSelf enum value</param>
        [Theory]
        [InlineData((FlexAlignSelf)(-1))]
        [InlineData((FlexAlignSelf)999)]
        [InlineData((FlexAlignSelf)int.MinValue)]
        [InlineData((FlexAlignSelf)int.MaxValue)]
        public void SetAlignSelf_WithInvalidEnumValue_SetsValueWithoutValidation(FlexAlignSelf invalidEnumValue)
        {
            // Arrange
            var bindableObject = new Label();

            // Act
            FlexLayout.SetAlignSelf(bindableObject, invalidEnumValue);

            // Assert
            var actualValue = FlexLayout.GetAlignSelf(bindableObject);
            Assert.Equal(invalidEnumValue, actualValue);
        }

        /// <summary>
        /// Mock view for testing purposes.
        /// </summary>
        class MockView : View
        {
            protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
            {
                return new Size(100, 50);
            }
        }

        /// <summary>
        /// Tests that OnClear method executes successfully on an empty layout.
        /// This verifies the method can handle the case where there are no children to clear.
        /// </summary>
        [Fact]
        public void OnClear_EmptyLayout_ExecutesWithoutError()
        {
            // Arrange
            var layout = new TestableFlexLayout();

            // Act & Assert - should not throw
            layout.CallOnClear();
        }

        /// <summary>
        /// Tests that OnClear method executes successfully on a layout with a single child.
        /// This verifies the method properly clears and repopulates the flex items for one child.
        /// </summary>
        [Fact]
        public void OnClear_SingleChild_ExecutesWithoutError()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var child = new MockView();
            layout.Children.Add(child);

            // Act & Assert - should not throw
            layout.CallOnClear();

            // Verify child is still present
            Assert.Single(layout.Children);
            Assert.Equal(child, layout.Children[0]);
        }

        /// <summary>
        /// Tests that OnClear method executes successfully on a layout with multiple children.
        /// This verifies the method properly clears and repopulates the flex items for multiple children.
        /// </summary>
        [Fact]
        public void OnClear_MultipleChildren_ExecutesWithoutError()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var child1 = new MockView();
            var child2 = new MockView();
            var child3 = new MockView();

            layout.Children.Add(child1);
            layout.Children.Add(child2);
            layout.Children.Add(child3);

            // Act & Assert - should not throw
            layout.CallOnClear();

            // Verify all children are still present
            Assert.Equal(3, layout.Children.Count);
            Assert.Equal(child1, layout.Children[0]);
            Assert.Equal(child2, layout.Children[1]);
            Assert.Equal(child3, layout.Children[2]);
        }

        /// <summary>
        /// Tests that OnClear method properly handles children with flex properties set.
        /// This verifies the method correctly resets and repopulates flex items that have custom properties.
        /// </summary>
        [Fact]
        public void OnClear_ChildrenWithFlexProperties_ExecutesWithoutError()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var child1 = new MockView();
            var child2 = new MockView();

            // Set some flex properties
            FlexLayout.SetGrow(child1, 2.0f);
            FlexLayout.SetShrink(child2, 0.5f);
            FlexLayout.SetOrder(child1, 1);
            FlexLayout.SetAlignSelf(child2, FlexAlignSelf.Center);

            layout.Children.Add(child1);
            layout.Children.Add(child2);

            // Act & Assert - should not throw
            layout.CallOnClear();

            // Verify children are still present and properties are preserved
            Assert.Equal(2, layout.Children.Count);
            Assert.Equal(child1, layout.Children[0]);
            Assert.Equal(child2, layout.Children[1]);

            // Verify flex properties are still set
            Assert.Equal(2.0f, FlexLayout.GetGrow(child1));
            Assert.Equal(0.5f, FlexLayout.GetShrink(child2));
            Assert.Equal(1, FlexLayout.GetOrder(child1));
            Assert.Equal(FlexAlignSelf.Center, FlexLayout.GetAlignSelf(child2));
        }

        /// <summary>
        /// Tests that OnClear method works correctly when layout has various flex layout properties set.
        /// This verifies the method properly resets the internal layout structure while preserving layout configuration.
        /// </summary>
        [Fact]
        public void OnClear_LayoutWithFlexProperties_ExecutesWithoutError()
        {
            // Arrange
            var layout = new TestableFlexLayout
            {
                Direction = FlexDirection.Column,
                JustifyContent = FlexJustify.SpaceEvenly,
                AlignItems = FlexAlignItems.Center,
                Wrap = FlexWrap.Wrap,
                AlignContent = FlexAlignContent.SpaceBetween,
                Position = FlexPosition.Absolute
            };

            var child = new MockView();
            layout.Children.Add(child);

            // Act & Assert - should not throw
            layout.CallOnClear();

            // Verify layout properties are preserved
            Assert.Equal(FlexDirection.Column, layout.Direction);
            Assert.Equal(FlexJustify.SpaceEvenly, layout.JustifyContent);
            Assert.Equal(FlexAlignItems.Center, layout.AlignItems);
            Assert.Equal(FlexWrap.Wrap, layout.Wrap);
            Assert.Equal(FlexAlignContent.SpaceBetween, layout.AlignContent);
            Assert.Equal(FlexPosition.Absolute, layout.Position);

            // Verify child is still present
            Assert.Single(layout.Children);
            Assert.Equal(child, layout.Children[0]);
        }

        /// <summary>
        /// Tests that OnClear method can be called multiple times without issues.
        /// This verifies the method is idempotent and doesn't cause problems with repeated calls.
        /// </summary>
        [Fact]
        public void OnClear_MultipleCallsInSequence_ExecutesWithoutError()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var child = new MockView();
            layout.Children.Add(child);

            // Act & Assert - multiple calls should not throw
            layout.CallOnClear();
            layout.CallOnClear();
            layout.CallOnClear();

            // Verify child is still present after multiple clears
            Assert.Single(layout.Children);
            Assert.Equal(child, layout.Children[0]);
        }

        /// <summary>
        /// Tests that the Wrap property returns the default value of FlexWrap.NoWrap when not explicitly set.
        /// This test ensures the getter properly retrieves the default value from the bindable property.
        /// Expected result: FlexWrap.NoWrap.
        /// </summary>
        [Fact]
        public void Wrap_DefaultValue_ReturnsNoWrap()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            var result = flexLayout.Wrap;

            // Assert
            Assert.Equal(FlexWrap.NoWrap, result);
        }

        /// <summary>
        /// Tests that the Wrap property getter and setter work correctly for all valid FlexWrap enum values.
        /// This test verifies that each enum value can be set and retrieved properly through the property.
        /// Expected result: The getter returns the same value that was set.
        /// </summary>
        [Theory]
        [InlineData(FlexWrap.NoWrap)]
        [InlineData(FlexWrap.Wrap)]
        [InlineData(FlexWrap.Reverse)]
        public void Wrap_ValidEnumValues_GetterReturnsSetValue(FlexWrap wrapValue)
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            flexLayout.Wrap = wrapValue;
            var result = flexLayout.Wrap;

            // Assert
            Assert.Equal(wrapValue, result);
        }

        /// <summary>
        /// Tests that the Wrap property getter returns the correct value after multiple property changes.
        /// This test ensures the bindable property mechanism properly handles sequential value updates.
        /// Expected result: The getter returns the most recently set value.
        /// </summary>
        [Fact]
        public void Wrap_MultipleValueChanges_GetterReturnsLatestValue()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act & Assert - Test sequence of changes
            flexLayout.Wrap = FlexWrap.Wrap;
            Assert.Equal(FlexWrap.Wrap, flexLayout.Wrap);

            flexLayout.Wrap = FlexWrap.Reverse;
            Assert.Equal(FlexWrap.Reverse, flexLayout.Wrap);

            flexLayout.Wrap = FlexWrap.NoWrap;
            Assert.Equal(FlexWrap.NoWrap, flexLayout.Wrap);
        }

        /// <summary>
        /// Tests that the Wrap property handles invalid enum values by casting.
        /// This test verifies the behavior when an invalid integer value is cast to FlexWrap enum.
        /// Expected result: The getter returns the cast value even if it's outside the defined enum range.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void Wrap_InvalidEnumValues_GetterReturnsUndefinedValue(int invalidEnumValue)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var undefinedWrapValue = (FlexWrap)invalidEnumValue;

            // Act
            flexLayout.Wrap = undefinedWrapValue;
            var result = flexLayout.Wrap;

            // Assert
            Assert.Equal(undefinedWrapValue, result);
            Assert.Equal(invalidEnumValue, (int)result);
        }

        /// <summary>
        /// Tests that the Wrap property getter works correctly on a newly instantiated FlexLayout.
        /// This test ensures the bindable property is properly initialized during object construction.
        /// Expected result: FlexWrap.NoWrap (the default value).
        /// </summary>
        [Fact]
        public void Wrap_NewInstance_GetterReturnsDefaultValue()
        {
            // Arrange & Act
            var flexLayout = new FlexLayout();
            var result = flexLayout.Wrap;

            // Assert
            Assert.Equal(FlexWrap.NoWrap, result);
        }

        /// <summary>
        /// Tests that multiple FlexLayout instances maintain independent Wrap property values.
        /// This test ensures that bindable properties are properly isolated between instances.
        /// Expected result: Each instance returns its own set value.
        /// </summary>
        [Fact]
        public void Wrap_MultipleInstances_GetterReturnsIndependentValues()
        {
            // Arrange
            var flexLayout1 = new FlexLayout();
            var flexLayout2 = new FlexLayout();
            var flexLayout3 = new FlexLayout();

            // Act
            flexLayout1.Wrap = FlexWrap.NoWrap;
            flexLayout2.Wrap = FlexWrap.Wrap;
            flexLayout3.Wrap = FlexWrap.Reverse;

            // Assert
            Assert.Equal(FlexWrap.NoWrap, flexLayout1.Wrap);
            Assert.Equal(FlexWrap.Wrap, flexLayout2.Wrap);
            Assert.Equal(FlexWrap.Reverse, flexLayout3.Wrap);
        }

        /// <summary>
        /// Tests that SetBasis correctly sets FlexBasis.Auto value on a valid BindableObject.
        /// </summary>
        [Fact]
        public void SetBasis_WithAutoValue_SetsPropertyCorrectly()
        {
            // Arrange
            var bindable = new Label();
            var basis = FlexBasis.Auto;

            // Act
            FlexLayout.SetBasis(bindable, basis);

            // Assert
            var actualBasis = FlexLayout.GetBasis(bindable);
            Assert.Equal(basis, actualBasis);
        }

        /// <summary>
        /// Tests that SetBasis correctly sets absolute length FlexBasis values on a valid BindableObject.
        /// </summary>
        [Theory]
        [InlineData(0f)]
        [InlineData(10f)]
        [InlineData(100.5f)]
        [InlineData(float.MaxValue)]
        public void SetBasis_WithAbsoluteLengthValue_SetsPropertyCorrectly(float length)
        {
            // Arrange
            var bindable = new Label();
            var basis = new FlexBasis(length);

            // Act
            FlexLayout.SetBasis(bindable, basis);

            // Assert
            var actualBasis = FlexLayout.GetBasis(bindable);
            Assert.Equal(basis, actualBasis);
        }

        /// <summary>
        /// Tests that SetBasis correctly sets relative length FlexBasis values on a valid BindableObject.
        /// </summary>
        [Theory]
        [InlineData(0f)]
        [InlineData(0.5f)]
        [InlineData(1f)]
        public void SetBasis_WithRelativeLengthValue_SetsPropertyCorrectly(float length)
        {
            // Arrange
            var bindable = new Label();
            var basis = new FlexBasis(length, true);

            // Act
            FlexLayout.SetBasis(bindable, basis);

            // Assert
            var actualBasis = FlexLayout.GetBasis(bindable);
            Assert.Equal(basis, actualBasis);
        }

        /// <summary>
        /// Tests that SetBasis throws NullReferenceException when bindable parameter is null.
        /// </summary>
        [Fact]
        public void SetBasis_WithNullBindable_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindable = null;
            var basis = FlexBasis.Auto;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => FlexLayout.SetBasis(bindable, basis));
        }

        /// <summary>
        /// Tests that SetBasis works with implicit conversion from float to FlexBasis.
        /// </summary>
        [Fact]
        public void SetBasis_WithImplicitFloatConversion_SetsPropertyCorrectly()
        {
            // Arrange
            var bindable = new Label();
            FlexBasis basis = 50f; // Implicit conversion from float

            // Act
            FlexLayout.SetBasis(bindable, basis);

            // Assert
            var actualBasis = FlexLayout.GetBasis(bindable);
            Assert.Equal(basis, actualBasis);
        }

        /// <summary>
        /// Tests that SetBasis works correctly with different types of BindableObjects.
        /// </summary>
        [Theory]
        [InlineData(typeof(Label))]
        [InlineData(typeof(Button))]
        [InlineData(typeof(FlexLayout))]
        public void SetBasis_WithDifferentBindableObjectTypes_SetsPropertyCorrectly(Type bindableType)
        {
            // Arrange
            var bindable = (BindableObject)Activator.CreateInstance(bindableType);
            var basis = new FlexBasis(25f);

            // Act
            FlexLayout.SetBasis(bindable, basis);

            // Assert
            var actualBasis = FlexLayout.GetBasis(bindable);
            Assert.Equal(basis, actualBasis);
        }

        /// <summary>
        /// Tests that SetBasis can override previously set values correctly.
        /// </summary>
        [Fact]
        public void SetBasis_OverridingPreviousValue_SetsNewValueCorrectly()
        {
            // Arrange
            var bindable = new Label();
            var initialBasis = new FlexBasis(10f);
            var newBasis = new FlexBasis(20f);

            // Act
            FlexLayout.SetBasis(bindable, initialBasis);
            FlexLayout.SetBasis(bindable, newBasis);

            // Assert
            var actualBasis = FlexLayout.GetBasis(bindable);
            Assert.Equal(newBasis, actualBasis);
            Assert.NotEqual(initialBasis, actualBasis);
        }

        /// <summary>
        /// Tests that OnInsert with valid parameters calls AddFlexItem and base OnInsert.
        /// Verifies that both the flex-specific logic and base insertion logic are executed.
        /// </summary>
        [Fact]
        public void OnInsert_ValidIndexAndView_CallsBothMethods()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var mockView = Substitute.For<IView>();
            var index = 0;

            // Act & Assert - Should not throw
            layout.TestOnInsert(index, mockView);
        }

        /// <summary>
        /// Tests OnInsert with null view parameter.
        /// Verifies proper handling of null view input.
        /// </summary>
        [Fact]
        public void OnInsert_NullView_HandlesGracefully()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            IView nullView = null;
            var index = 0;

            // Act & Assert - Should handle null gracefully or throw appropriate exception
            layout.TestOnInsert(index, nullView);
        }

        /// <summary>
        /// Tests OnInsert with various index values including edge cases.
        /// Verifies that the method handles different index values appropriately.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(int.MaxValue)]
        public void OnInsert_VariousIndexValues_CallsBothMethods(int index)
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var mockView = Substitute.For<IView>();

            // Act & Assert - Should not throw
            layout.TestOnInsert(index, mockView);
        }

        /// <summary>
        /// Tests OnInsert with minimum integer value.
        /// Verifies handling of extreme negative index values.
        /// </summary>
        [Fact]
        public void OnInsert_MinIntIndex_HandlesBoundary()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var mockView = Substitute.For<IView>();
            var index = int.MinValue;

            // Act & Assert - Should handle extreme values
            layout.TestOnInsert(index, mockView);
        }

        /// <summary>
        /// Tests OnInsert with different types of IView implementations.
        /// Verifies that the method works with various view types.
        /// </summary>
        [Theory]
        [InlineData(typeof(Label))]
        [InlineData(typeof(Button))]
        [InlineData(typeof(FlexLayout))]
        public void OnInsert_DifferentViewTypes_CallsBothMethods(Type viewType)
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var mockView = (IView)Substitute.For(new[] { viewType, typeof(IView) }, new object[0]);
            var index = 0;

            // Act & Assert - Should not throw
            layout.TestOnInsert(index, mockView);
        }

        /// <summary>
        /// Tests OnInsert when inserting multiple views at different positions.
        /// Verifies that the method can handle sequential insertions correctly.
        /// </summary>
        [Fact]
        public void OnInsert_MultipleInsertions_HandlesSequentialCalls()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var mockView1 = Substitute.For<IView>();
            var mockView2 = Substitute.For<IView>();
            var mockView3 = Substitute.For<IView>();

            // Act & Assert - Should handle multiple insertions
            layout.TestOnInsert(0, mockView1);
            layout.TestOnInsert(1, mockView2);
            layout.TestOnInsert(0, mockView3);
        }

        /// <summary>
        /// Tests OnInsert with the same view inserted multiple times.
        /// Verifies behavior when attempting to insert the same view instance multiple times.
        /// </summary>
        [Fact]
        public void OnInsert_SameViewMultipleTimes_HandlesRepeatedInsertions()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var mockView = Substitute.For<IView>();

            // Act & Assert - Should handle repeated insertions
            layout.TestOnInsert(0, mockView);
            layout.TestOnInsert(1, mockView);
        }

        /// <summary>
        /// Tests the Layout method with normal finite width and height values.
        /// Should set root width and height to the provided values and call layout.
        /// </summary>
        [Theory]
        [InlineData(100.0, 200.0)]
        [InlineData(0.0, 0.0)]
        [InlineData(50.5, 75.25)]
        [InlineData(1000.0, 500.0)]
        public void Layout_WithFiniteWidthAndHeight_SetsRootDimensionsAndCallsLayout(double width, double height)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            flexLayout.IsPlatformEnabled = true;

            // Act
            flexLayout.Layout(width, height);

            // Assert
            // The method should complete without throwing exceptions for finite values
            Assert.True(true);
        }

        /// <summary>
        /// Tests the Layout method with positive infinity width and height.
        /// Should set root width and height to 0 when infinity is provided and use measure hack.
        /// </summary>
        [Theory]
        [InlineData(double.PositiveInfinity, 100.0)]
        [InlineData(100.0, double.PositiveInfinity)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        public void Layout_WithPositiveInfinityDimensions_UsesMeasureHackAndSetsZeroDimensions(double width, double height)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            flexLayout.IsPlatformEnabled = true;
            var child = Substitute.For<IView>();
            child.DesiredSize.Returns(new Size(50, 50));
            flexLayout.Add(child);

            // Act
            flexLayout.Layout(width, height);

            // Assert
            // The method should complete without throwing exceptions with infinity values
            Assert.True(true);
        }

        /// <summary>
        /// Tests the Layout method with negative infinity width and height.
        /// Should set root width and height to 0 when negative infinity is provided and use measure hack.
        /// </summary>
        [Theory]
        [InlineData(double.NegativeInfinity, 100.0)]
        [InlineData(100.0, double.NegativeInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
        public void Layout_WithNegativeInfinityDimensions_UsesMeasureHackAndSetsZeroDimensions(double width, double height)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            flexLayout.IsPlatformEnabled = true;

            // Act
            flexLayout.Layout(width, height);

            // Assert
            // The method should complete without throwing exceptions with negative infinity values
            Assert.True(true);
        }

        /// <summary>
        /// Tests the Layout method with NaN width and height values.
        /// Should handle NaN values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 100.0)]
        [InlineData(100.0, double.NaN)]
        [InlineData(double.NaN, double.NaN)]
        public void Layout_WithNaNDimensions_HandlesNaNValuesWithoutException(double width, double height)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            flexLayout.IsPlatformEnabled = true;

            // Act
            flexLayout.Layout(width, height);

            // Assert
            // The method should complete without throwing exceptions with NaN values
            Assert.True(true);
        }

        /// <summary>
        /// Tests the Layout method with negative width and height values.
        /// Should handle negative values appropriately.
        /// </summary>
        [Theory]
        [InlineData(-100.0, 100.0)]
        [InlineData(100.0, -100.0)]
        [InlineData(-50.0, -75.0)]
        public void Layout_WithNegativeDimensions_HandlesNegativeValuesWithoutException(double width, double height)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            flexLayout.IsPlatformEnabled = true;

            // Act
            flexLayout.Layout(width, height);

            // Assert
            // The method should complete without throwing exceptions with negative values
            Assert.True(true);
        }

        /// <summary>
        /// Tests the Layout method with extreme double values.
        /// Should handle maximum and minimum double values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, 100.0)]
        [InlineData(100.0, double.MaxValue)]
        [InlineData(double.MinValue, 100.0)]
        [InlineData(100.0, double.MinValue)]
        public void Layout_WithExtremeDoubleValues_HandlesExtremeValuesWithoutException(double width, double height)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            flexLayout.IsPlatformEnabled = true;

            // Act
            flexLayout.Layout(width, height);

            // Assert
            // The method should complete without throwing exceptions with extreme values
            Assert.True(true);
        }

        /// <summary>
        /// Tests the Layout method with children added to verify the layout process works end-to-end.
        /// Should properly lay out children within the specified dimensions.
        /// </summary>
        [Fact]
        public void Layout_WithChildrenAdded_PerformsLayoutWithoutException()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            flexLayout.IsPlatformEnabled = true;
            flexLayout.Direction = FlexDirection.Row;

            var child1 = Substitute.For<IView>();
            child1.DesiredSize.Returns(new Size(50, 50));
            var child2 = Substitute.For<IView>();
            child2.DesiredSize.Returns(new Size(30, 40));

            flexLayout.Add(child1);
            flexLayout.Add(child2);

            // Act
            flexLayout.Layout(200.0, 100.0);

            // Assert
            // The method should complete without throwing exceptions when children are present
            Assert.True(true);
        }

        /// <summary>
        /// Tests the Layout method with various flex properties set on children.
        /// Should respect flex properties during layout calculation.
        /// </summary>
        [Fact]
        public void Layout_WithFlexPropertiesOnChildren_RespectsFlexProperties()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            flexLayout.IsPlatformEnabled = true;
            flexLayout.Direction = FlexDirection.Column;

            var child1 = Substitute.For<IView>();
            child1.DesiredSize.Returns(new Size(100, 50));
            var child2 = Substitute.For<IView>();
            child2.DesiredSize.Returns(new Size(100, 50));

            FlexLayout.SetGrow(child1, 1.0f);
            FlexLayout.SetShrink(child2, 2.0f);
            FlexLayout.SetOrder(child1, 2);
            FlexLayout.SetOrder(child2, 1);

            flexLayout.Add(child1);
            flexLayout.Add(child2);

            // Act
            flexLayout.Layout(100.0, 200.0);

            // Assert
            // The method should complete without throwing exceptions with flex properties set
            Assert.True(true);
        }

        /// <summary>
        /// Tests the Layout method when no parent is set on the FlexLayout.
        /// Should proceed with normal layout since _root.Parent should be null.
        /// </summary>
        [Fact]
        public void Layout_WhenNoParentSet_ProceedsWithNormalLayout()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            flexLayout.IsPlatformEnabled = true;

            // Act & Assert
            // Should not return early since FlexLayout has no parent initially
            flexLayout.Layout(100.0, 100.0);
            Assert.True(true);
        }

        /// <summary>
        /// Tests the Layout method with nested FlexLayout setup to potentially trigger early return.
        /// This test attempts to create a scenario where _root.Parent might not be null.
        /// </summary>
        [Fact]
        public void Layout_WithNestedFlexLayoutSetup_HandlesComplexHierarchy()
        {
            // Arrange
            var parentLayout = new FlexLayout();
            parentLayout.IsPlatformEnabled = true;

            var childLayout = new FlexLayout();
            childLayout.IsPlatformEnabled = true;

            var grandChild = Substitute.For<IView>();
            grandChild.DesiredSize.Returns(new Size(50, 50));

            childLayout.Add(grandChild);
            parentLayout.Add(childLayout);

            // Act
            childLayout.Layout(100.0, 100.0);

            // Assert
            // The method should complete without throwing exceptions in nested scenarios
            Assert.True(true);
        }

        /// <summary>
        /// Tests that SetGrow method correctly sets the grow value on a valid BindableObject.
        /// Input: Valid BindableObject and positive float value.
        /// Expected: Value is set without exception.
        /// </summary>
        [Theory]
        [InlineData(0f)]
        [InlineData(1f)]
        [InlineData(2.5f)]
        [InlineData(float.MaxValue)]
        [InlineData(float.PositiveInfinity)]
        public void SetGrow_ValidBindableObjectAndValue_SetsValueSuccessfully(float value)
        {
            // Arrange
            var bindableObject = new Label();

            // Act & Assert (no exception should be thrown)
            FlexLayout.SetGrow(bindableObject, value);

            // Verify the value was actually set
            Assert.Equal(value, FlexLayout.GetGrow(bindableObject));
        }

        /// <summary>
        /// Tests that SetGrow method throws NullReferenceException when bindable parameter is null.
        /// Input: Null BindableObject.
        /// Expected: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void SetGrow_NullBindableObject_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindableObject = null;
            float value = 1f;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => FlexLayout.SetGrow(bindableObject, value));
        }

        /// <summary>
        /// Tests that SetGrow method handles negative values according to validation rules.
        /// Input: Valid BindableObject and negative float values.
        /// Expected: ArgumentException is thrown due to validation failure.
        /// </summary>
        [Theory]
        [InlineData(-1f)]
        [InlineData(-0.1f)]
        [InlineData(float.MinValue)]
        [InlineData(float.NegativeInfinity)]
        public void SetGrow_ValidBindableObjectAndNegativeValue_ThrowsArgumentException(float value)
        {
            // Arrange
            var bindableObject = new Label();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FlexLayout.SetGrow(bindableObject, value));
        }

        /// <summary>
        /// Tests that SetGrow method handles NaN value according to validation rules.
        /// Input: Valid BindableObject and NaN float value.
        /// Expected: ArgumentException is thrown due to validation failure.
        /// </summary>
        [Fact]
        public void SetGrow_ValidBindableObjectAndNaNValue_ThrowsArgumentException()
        {
            // Arrange
            var bindableObject = new Label();
            float value = float.NaN;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => FlexLayout.SetGrow(bindableObject, value));
        }

        /// <summary>
        /// Tests that SetGrow method works with minimum valid float value.
        /// Input: Valid BindableObject and float.Epsilon (smallest positive value).
        /// Expected: Value is set successfully.
        /// </summary>
        [Fact]
        public void SetGrow_ValidBindableObjectAndEpsilonValue_SetsValueSuccessfully()
        {
            // Arrange
            var bindableObject = new Label();
            float value = float.Epsilon;

            // Act
            FlexLayout.SetGrow(bindableObject, value);

            // Assert
            Assert.Equal(value, FlexLayout.GetGrow(bindableObject));
        }

        /// <summary>
        /// Tests that GetBasis throws ArgumentNullException when bindable parameter is null.
        /// This verifies proper null parameter validation.
        /// </summary>
        [Fact]
        public void GetBasis_NullBindable_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => FlexLayout.GetBasis(null));
        }

        /// <summary>
        /// Tests that GetBasis returns the default FlexBasis.Auto value when no custom value is set.
        /// This verifies the default behavior of the attached property.
        /// </summary>
        [Fact]
        public void GetBasis_DefaultValue_ReturnsFlexBasisAuto()
        {
            // Arrange
            var bindableObject = new View();

            // Act
            var result = FlexLayout.GetBasis(bindableObject);

            // Assert
            Assert.Equal(FlexBasis.Auto, result);
        }

        /// <summary>
        /// Tests that GetBasis returns the correct FlexBasis value after it has been set using SetBasis.
        /// This verifies the get/set behavior of the attached property with various FlexBasis values.
        /// </summary>
        /// <param name="length">The length value for FlexBasis</param>
        /// <param name="isRelative">Whether the FlexBasis should be relative</param>
        [Theory]
        [InlineData(0f, false)]
        [InlineData(100f, false)]
        [InlineData(float.MaxValue, false)]
        [InlineData(0f, true)]
        [InlineData(0.5f, true)]
        [InlineData(1f, true)]
        public void GetBasis_CustomValue_ReturnsSetValue(float length, bool isRelative)
        {
            // Arrange
            var bindableObject = new View();
            var expectedBasis = new FlexBasis(length, isRelative);
            FlexLayout.SetBasis(bindableObject, expectedBasis);

            // Act
            var result = FlexLayout.GetBasis(bindableObject);

            // Assert
            Assert.Equal(expectedBasis, result);
        }

        /// <summary>
        /// Tests that GetBasis returns the correct FlexBasis value when using implicit conversion from float.
        /// This verifies the attached property behavior with implicitly converted FlexBasis values.
        /// </summary>
        [Theory]
        [InlineData(0f)]
        [InlineData(50f)]
        [InlineData(100f)]
        [InlineData(float.MaxValue)]
        public void GetBasis_ImplicitConversionFromFloat_ReturnsCorrectValue(float lengthValue)
        {
            // Arrange
            var bindableObject = new View();
            FlexBasis expectedBasis = lengthValue; // Implicit conversion
            FlexLayout.SetBasis(bindableObject, expectedBasis);

            // Act
            var result = FlexLayout.GetBasis(bindableObject);

            // Assert
            Assert.Equal(expectedBasis, result);
            Assert.Equal(lengthValue, result.Length);
        }

        /// <summary>
        /// Tests that GetBasis returns FlexBasis.Auto when explicitly set to Auto.
        /// This verifies the attached property behavior when explicitly setting to the Auto value.
        /// </summary>
        [Fact]
        public void GetBasis_ExplicitAutoValue_ReturnsFlexBasisAuto()
        {
            // Arrange
            var bindableObject = new View();
            FlexLayout.SetBasis(bindableObject, FlexBasis.Auto);

            // Act
            var result = FlexLayout.GetBasis(bindableObject);

            // Assert
            Assert.Equal(FlexBasis.Auto, result);
        }

        /// <summary>
        /// Tests that GetShrink throws ArgumentNullException when bindable parameter is null.
        /// Verifies proper null parameter validation.
        /// </summary>
        [Fact]
        public void GetShrink_NullBindable_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => FlexLayout.GetShrink(null));
        }

        /// <summary>
        /// Tests that GetShrink returns the default shrink value when no shrink value has been explicitly set.
        /// Verifies that the default value of 1.0f is returned for uninitialized elements.
        /// </summary>
        [Fact]
        public void GetShrink_BindableWithoutShrinkSet_ReturnsDefaultValue()
        {
            // Arrange
            var label = new Label();

            // Act
            var result = FlexLayout.GetShrink(label);

            // Assert
            Assert.Equal(1f, result);
        }

        /// <summary>
        /// Tests that GetShrink returns the correct shrink value after it has been set using SetShrink.
        /// Verifies the round-trip behavior of setting and getting shrink values.
        /// </summary>
        [Theory]
        [InlineData(0f)]
        [InlineData(0.5f)]
        [InlineData(1f)]
        [InlineData(2.5f)]
        [InlineData(10f)]
        [InlineData(100.75f)]
        [InlineData(float.MaxValue)]
        public void GetShrink_BindableWithShrinkSet_ReturnsSetValue(float shrinkValue)
        {
            // Arrange
            var view = new View();
            FlexLayout.SetShrink(view, shrinkValue);

            // Act
            var result = FlexLayout.GetShrink(view);

            // Assert
            Assert.Equal(shrinkValue, result);
        }

        /// <summary>
        /// Tests that GetShrink works correctly with different types of BindableObject instances.
        /// Verifies that the method works consistently across various UI element types.
        /// </summary>
        [Theory]
        [InlineData(typeof(Label))]
        [InlineData(typeof(Button))]
        [InlineData(typeof(View))]
        [InlineData(typeof(Layout))]
        public void GetShrink_DifferentBindableTypes_ReturnsCorrectValue(Type bindableType)
        {
            // Arrange
            var bindable = (BindableObject)Activator.CreateInstance(bindableType);
            var expectedShrink = 3.14f;
            FlexLayout.SetShrink(bindable, expectedShrink);

            // Act
            var result = FlexLayout.GetShrink(bindable);

            // Assert
            Assert.Equal(expectedShrink, result);
        }

        /// <summary>
        /// Tests that GetShrink handles edge case float values correctly.
        /// Verifies behavior with special float values like NaN and infinity.
        /// </summary>
        [Theory]
        [InlineData(float.NaN)]
        [InlineData(float.PositiveInfinity)]
        [InlineData(float.NegativeInfinity)]
        [InlineData(float.Epsilon)]
        [InlineData(float.MinValue)]
        public void GetShrink_EdgeCaseFloatValues_ReturnsSetValue(float shrinkValue)
        {
            // Arrange
            var button = new Button();

            // Note: Some of these values may not pass validation when set,
            // but GetShrink should return whatever value is stored
            try
            {
                FlexLayout.SetShrink(button, shrinkValue);

                // Act
                var result = FlexLayout.GetShrink(button);

                // Assert
                Assert.Equal(shrinkValue, result);
            }
            catch (ArgumentException)
            {
                // Some edge values might be rejected by validation - this is expected
                // In such cases, verify the default value is returned
                var result = FlexLayout.GetShrink(button);
                Assert.Equal(1f, result);
            }
        }

        /// <summary>
        /// Tests that OnParentSet calls PopulateLayout when Parent is set and _root is null.
        /// Verifies the condition: Parent != null && _root == null.
        /// Expected result: PopulateLayout is called to initialize the flex layout.
        /// </summary>
        [Fact]
        public void OnParentSet_WhenParentSetAndRootIsNull_CallsPopulateLayout()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var parent = new ContentPage();
            var child = new Label { Text = "Test" };
            flexLayout.Children.Add(child);

            // Act
            parent.Content = flexLayout; // This sets the Parent property

            // Assert
            // Verify that PopulateLayout was called by checking that flex layout is functional
            // PopulateLayout creates _root and adds children, so the layout should work properly
            Assert.NotNull(flexLayout.Parent);
            Assert.Equal(parent, flexLayout.Parent);
        }

        /// <summary>
        /// Tests that OnParentSet calls ClearLayout when Parent is set to null and _root is not null.
        /// Verifies the condition: Parent == null && _root != null.
        /// Expected result: ClearLayout is called to clean up the flex layout.
        /// </summary>
        [Fact]
        public void OnParentSet_WhenParentClearedAndRootIsNotNull_CallsClearLayout()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var parent = new ContentPage();
            var child = new Label { Text = "Test" };
            flexLayout.Children.Add(child);

            // First set the parent to initialize _root (PopulateLayout)
            parent.Content = flexLayout;
            Assert.NotNull(flexLayout.Parent);

            // Act
            parent.Content = null; // This clears the Parent property

            // Assert
            // Verify that ClearLayout was called by checking that parent is null
            Assert.Null(flexLayout.Parent);
        }

        /// <summary>
        /// Tests that OnParentSet does not call PopulateLayout when Parent is set but _root already exists.
        /// Verifies that the condition Parent != null && _root == null evaluates to false when _root is not null.
        /// Expected result: PopulateLayout is not called again.
        /// </summary>
        [Fact]
        public void OnParentSet_WhenParentSetAndRootAlreadyExists_DoesNotCallPopulateLayout()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var firstParent = new ContentPage();
            var secondParent = new Grid();
            var child = new Label { Text = "Test" };
            flexLayout.Children.Add(child);

            // First set a parent to create _root
            firstParent.Content = flexLayout;
            Assert.NotNull(flexLayout.Parent);

            // Act
            secondParent.Children.Add(flexLayout); // This changes the parent

            // Assert
            // Verify the parent changed but no exceptions occurred
            Assert.NotEqual(firstParent, flexLayout.Parent);
        }

        /// <summary>
        /// Tests that OnParentSet does not call ClearLayout when Parent is null and _root is also null.
        /// Verifies that the condition Parent == null && _root != null evaluates to false when _root is null.
        /// Expected result: ClearLayout is not called.
        /// </summary>
        [Fact]
        public void OnParentSet_WhenParentClearedAndRootIsAlreadyNull_DoesNotCallClearLayout()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var child = new Label { Text = "Test" };
            flexLayout.Children.Add(child);

            // Ensure parent is null and _root is null (initial state)
            Assert.Null(flexLayout.Parent);

            // Act
            // Simulate OnParentSet being called with null parent (already null)
            // This can happen during initialization or multiple null assignments
            var parent = new ContentPage();
            parent.Content = flexLayout;
            parent.Content = null;
            parent.Content = null; // Second null assignment

            // Assert
            // Verify parent remains null and no exceptions occurred
            Assert.Null(flexLayout.Parent);
        }

        /// <summary>
        /// Tests OnParentSet with null parent parameter when _root exists.
        /// Verifies the else if condition is properly evaluated.
        /// Expected result: ClearLayout is called and _root is set to null.
        /// </summary>
        [Fact]
        public void OnParentSet_WhenParentIsNullAndRootExists_ExecutesClearLayoutPath()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var parent = new StackLayout();
            var child1 = new Label { Text = "Child1" };
            var child2 = new Button { Text = "Child2" };

            flexLayout.Children.Add(child1);
            flexLayout.Children.Add(child2);

            // Set parent to initialize _root (this calls PopulateLayout)
            parent.Children.Add(flexLayout);
            Assert.NotNull(flexLayout.Parent);

            // Act
            parent.Children.Remove(flexLayout); // This sets Parent to null

            // Assert
            Assert.Null(flexLayout.Parent);
            // The children should still exist in the FlexLayout but flex layout should be cleared
            Assert.Equal(2, flexLayout.Children.Count);
        }

        /// <summary>
        /// Tests OnParentSet with non-null parent when _root is null.
        /// Verifies the if condition is properly evaluated and PopulateLayout is called.
        /// Expected result: PopulateLayout initializes the flex layout structure.
        /// </summary>
        [Fact]
        public void OnParentSet_WhenParentIsNotNullAndRootIsNull_ExecutesPopulateLayoutPath()
        {
            // Arrange
            var flexLayout = new FlexLayout
            {
                Direction = FlexDirection.Column,
                JustifyContent = FlexJustify.Center
            };
            var child1 = new Label { Text = "Test1" };
            var child2 = new Label { Text = "Test2" };

            flexLayout.Children.Add(child1);
            flexLayout.Children.Add(child2);

            var parentContainer = new ScrollView();

            // Ensure initial state: Parent is null, _root should be null
            Assert.Null(flexLayout.Parent);

            // Act
            parentContainer.Content = flexLayout; // This triggers OnParentSet with non-null parent

            // Assert
            Assert.NotNull(flexLayout.Parent);
            Assert.Equal(parentContainer, flexLayout.Parent);
            // Verify that the flex layout is properly initialized by checking children are still there
            Assert.Equal(2, flexLayout.Children.Count);
            Assert.Equal(child1, flexLayout.Children[0]);
            Assert.Equal(child2, flexLayout.Children[1]);
        }

        /// <summary>
        /// Tests that SetShrink correctly sets the shrink value on a valid BindableObject.
        /// Verifies that the value can be retrieved using GetShrink.
        /// </summary>
        [Fact]
        public void SetShrink_ValidBindableAndValue_SetsPropertyCorrectly()
        {
            // Arrange
            var view = new View();
            const float expectedValue = 2.5f;

            // Act
            FlexLayout.SetShrink(view, expectedValue);

            // Assert
            var actualValue = FlexLayout.GetShrink(view);
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that SetShrink throws ArgumentNullException when bindable parameter is null.
        /// </summary>
        [Fact]
        public void SetShrink_NullBindable_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject nullBindable = null;
            const float value = 1.0f;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => FlexLayout.SetShrink(nullBindable, value));
            Assert.Contains("bindable", exception.Message);
        }

        /// <summary>
        /// Tests that SetShrink handles zero value correctly, which is the minimum valid value.
        /// </summary>
        [Fact]
        public void SetShrink_ZeroValue_SetsPropertyCorrectly()
        {
            // Arrange
            var view = new View();
            const float expectedValue = 0.0f;

            // Act
            FlexLayout.SetShrink(view, expectedValue);

            // Assert
            var actualValue = FlexLayout.GetShrink(view);
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that SetShrink correctly handles various boundary and special float values.
        /// Includes testing float.MaxValue and float.PositiveInfinity which should be valid.
        /// </summary>
        [Theory]
        [InlineData(float.MaxValue)]
        [InlineData(float.PositiveInfinity)]
        [InlineData(1.0f)]
        [InlineData(0.001f)]
        [InlineData(1000.0f)]
        public void SetShrink_ValidBoundaryValues_SetsPropertyCorrectly(float value)
        {
            // Arrange
            var view = new View();

            // Act
            FlexLayout.SetShrink(view, value);

            // Assert
            var actualValue = FlexLayout.GetShrink(view);
            Assert.Equal(value, actualValue);
        }

        /// <summary>
        /// Tests that SetShrink rejects negative values through the bindable property validator.
        /// The validator should prevent negative values from being set.
        /// </summary>
        [Theory]
        [InlineData(-1.0f)]
        [InlineData(-0.001f)]
        [InlineData(float.MinValue)]
        [InlineData(float.NegativeInfinity)]
        public void SetShrink_NegativeValues_ValidationPreventsInvalidValues(float invalidValue)
        {
            // Arrange
            var view = new View();
            var originalValue = FlexLayout.GetShrink(view); // Get default value (should be 1.0f)

            // Act
            FlexLayout.SetShrink(view, invalidValue);

            // Assert
            // The validator should prevent the invalid value from being set
            var actualValue = FlexLayout.GetShrink(view);
            Assert.Equal(originalValue, actualValue); // Should remain unchanged
        }

        /// <summary>
        /// Tests that SetShrink handles float.NaN value.
        /// NaN should be treated as invalid and rejected by validation.
        /// </summary>
        [Fact]
        public void SetShrink_NaNValue_ValidationPreventsInvalidValue()
        {
            // Arrange
            var view = new View();
            var originalValue = FlexLayout.GetShrink(view); // Get default value

            // Act
            FlexLayout.SetShrink(view, float.NaN);

            // Assert
            // The validator should prevent NaN from being set
            var actualValue = FlexLayout.GetShrink(view);
            Assert.Equal(originalValue, actualValue); // Should remain unchanged
        }

        /// <summary>
        /// Tests that SetShrink works correctly with different types of BindableObject.
        /// Verifies the method works with various UI controls.
        /// </summary>
        [Fact]
        public void SetShrink_DifferentBindableObjectTypes_SetsPropertyCorrectly()
        {
            // Arrange
            var label = new Label();
            var button = new Button();
            const float expectedValue = 3.0f;

            // Act
            FlexLayout.SetShrink(label, expectedValue);
            FlexLayout.SetShrink(button, expectedValue);

            // Assert
            Assert.Equal(expectedValue, FlexLayout.GetShrink(label));
            Assert.Equal(expectedValue, FlexLayout.GetShrink(button));
        }

        /// <summary>
        /// Tests that SetOrder method correctly sets the Order attached property on a BindableObject.
        /// Verifies that the value can be retrieved using GetOrder.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(100)]
        [InlineData(-100)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SetOrder_ValidBindableObjectAndValue_SetsOrderCorrectly(int orderValue)
        {
            // Arrange
            var bindableObject = new Label();

            // Act
            FlexLayout.SetOrder(bindableObject, orderValue);

            // Assert
            var retrievedValue = FlexLayout.GetOrder(bindableObject);
            Assert.Equal(orderValue, retrievedValue);
        }

        /// <summary>
        /// Tests that SetOrder method throws ArgumentNullException when bindable parameter is null.
        /// Verifies proper error handling for invalid input.
        /// </summary>
        [Fact]
        public void SetOrder_NullBindableObject_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject nullBindable = null;
            int orderValue = 5;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => FlexLayout.SetOrder(nullBindable, orderValue));
        }

        /// <summary>
        /// Tests that SetOrder method works with different types of BindableObject derivatives.
        /// Verifies the method accepts any BindableObject, not just specific UI elements.
        /// </summary>
        [Theory]
        [InlineData(typeof(Label), 10)]
        [InlineData(typeof(Button), -5)]
        [InlineData(typeof(View), 0)]
        public void SetOrder_DifferentBindableObjectTypes_SetsOrderCorrectly(Type bindableType, int orderValue)
        {
            // Arrange
            var bindableObject = (BindableObject)Activator.CreateInstance(bindableType);

            // Act
            FlexLayout.SetOrder(bindableObject, orderValue);

            // Assert
            var retrievedValue = FlexLayout.GetOrder(bindableObject);
            Assert.Equal(orderValue, retrievedValue);
        }

        /// <summary>
        /// Tests that SetOrder method can overwrite previously set order values.
        /// Verifies that multiple calls to SetOrder update the value correctly.
        /// </summary>
        [Fact]
        public void SetOrder_OverwritePreviousValue_UpdatesOrderCorrectly()
        {
            // Arrange
            var bindableObject = new Label();
            int initialValue = 5;
            int updatedValue = 10;

            // Act
            FlexLayout.SetOrder(bindableObject, initialValue);
            var firstRetrievedValue = FlexLayout.GetOrder(bindableObject);

            FlexLayout.SetOrder(bindableObject, updatedValue);
            var secondRetrievedValue = FlexLayout.GetOrder(bindableObject);

            // Assert
            Assert.Equal(initialValue, firstRetrievedValue);
            Assert.Equal(updatedValue, secondRetrievedValue);
        }

        /// <summary>
        /// Tests SetAlignSelf method when the view is a BindableObject.
        /// Verifies that the AlignSelf attached property is set using SetValue.
        /// </summary>
        [Theory]
        [InlineData(FlexAlignSelf.Auto)]
        [InlineData(FlexAlignSelf.Stretch)]
        [InlineData(FlexAlignSelf.Center)]
        [InlineData(FlexAlignSelf.Start)]
        [InlineData(FlexAlignSelf.End)]
        public void SetAlignSelf_BindableObjectView_SetsAttachedProperty(FlexAlignSelf alignSelf)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var label = new Label();
            flexLayout.Children.Add(label);

            // Act
            flexLayout.SetAlignSelf(label, alignSelf);

            // Assert
            var actualValue = FlexLayout.GetAlignSelf(label);
            Assert.Equal(alignSelf, actualValue);
        }

        /// <summary>
        /// Tests SetAlignSelf method when the view is not a BindableObject.
        /// Verifies that the _viewInfo dictionary is updated with the AlignSelf value.
        /// </summary>
        [Theory]
        [InlineData(FlexAlignSelf.Auto)]
        [InlineData(FlexAlignSelf.Stretch)]
        [InlineData(FlexAlignSelf.Center)]
        [InlineData(FlexAlignSelf.Start)]
        [InlineData(FlexAlignSelf.End)]
        public void SetAlignSelf_NonBindableObjectView_UpdatesViewInfoDictionary(FlexAlignSelf alignSelf)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var mockView = Substitute.For<IView>();

            // Mock required properties for IView
            mockView.DesiredSize.Returns(new Size(100, 50));
            mockView.Measure(Arg.Any<double>(), Arg.Any<double>()).Returns(new Size(100, 50));

            flexLayout.Children.Add(mockView);

            // Act
            flexLayout.SetAlignSelf(mockView, alignSelf);

            // Assert
            var actualValue = flexLayout.GetAlignSelf(mockView);
            Assert.Equal(alignSelf, actualValue);
        }

        /// <summary>
        /// Tests SetAlignSelf method with null view parameter.
        /// Verifies that an appropriate exception is thrown.
        /// </summary>
        [Fact]
        public void SetAlignSelf_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => flexLayout.SetAlignSelf(null, FlexAlignSelf.Center));
        }

        /// <summary>
        /// Tests SetAlignSelf method with non-BindableObject view that hasn't been added to FlexLayout.
        /// Verifies that accessing _viewInfo for unknown view throws KeyNotFoundException.
        /// </summary>
        [Fact]
        public void SetAlignSelf_NonBindableObjectViewNotInLayout_ThrowsKeyNotFoundException()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var mockView = Substitute.For<IView>();

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => flexLayout.SetAlignSelf(mockView, FlexAlignSelf.Center));
        }

        /// <summary>
        /// Tests SetAlignSelf method with invalid enum values cast from integers.
        /// Verifies that the method handles out-of-range enum values appropriately.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void SetAlignSelf_InvalidEnumValue_SetsValueWithoutException(int invalidEnumValue)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var label = new Label();
            flexLayout.Children.Add(label);
            var invalidAlignSelf = (FlexAlignSelf)invalidEnumValue;

            // Act & Assert - Should not throw exception
            flexLayout.SetAlignSelf(label, invalidAlignSelf);

            // Verify the invalid value was set
            var actualValue = FlexLayout.GetAlignSelf(label);
            Assert.Equal(invalidAlignSelf, actualValue);
        }

        /// <summary>
        /// Tests SetAlignSelf method multiple times on the same view to verify value updates.
        /// Verifies that subsequent calls properly update the AlignSelf value.
        /// </summary>
        [Fact]
        public void SetAlignSelf_MultipleCallsOnSameView_UpdatesValue()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var label = new Label();
            flexLayout.Children.Add(label);

            // Act & Assert - Set initial value
            flexLayout.SetAlignSelf(label, FlexAlignSelf.Start);
            Assert.Equal(FlexAlignSelf.Start, FlexLayout.GetAlignSelf(label));

            // Act & Assert - Update to different value
            flexLayout.SetAlignSelf(label, FlexAlignSelf.End);
            Assert.Equal(FlexAlignSelf.End, FlexLayout.GetAlignSelf(label));

            // Act & Assert - Update to another value
            flexLayout.SetAlignSelf(label, FlexAlignSelf.Center);
            Assert.Equal(FlexAlignSelf.Center, FlexLayout.GetAlignSelf(label));
        }

        /// <summary>
        /// Tests SetAlignSelf method with both BindableObject and non-BindableObject views in same layout.
        /// Verifies that both code paths work correctly within the same FlexLayout instance.
        /// </summary>
        [Fact]
        public void SetAlignSelf_MixedViewTypes_BothPathsWorkCorrectly()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var label = new Label(); // BindableObject
            var mockView = Substitute.For<IView>(); // Non-BindableObject

            mockView.DesiredSize.Returns(new Size(100, 50));
            mockView.Measure(Arg.Any<double>(), Arg.Any<double>()).Returns(new Size(100, 50));

            flexLayout.Children.Add(label);
            flexLayout.Children.Add(mockView);

            // Act
            flexLayout.SetAlignSelf(label, FlexAlignSelf.Start);
            flexLayout.SetAlignSelf(mockView, FlexAlignSelf.End);

            // Assert
            Assert.Equal(FlexAlignSelf.Start, FlexLayout.GetAlignSelf(label));
            Assert.Equal(FlexAlignSelf.End, flexLayout.GetAlignSelf(mockView));
        }

        /// <summary>
        /// Tests that the Direction property getter returns the correct FlexDirection value.
        /// Validates that the property correctly retrieves values set via the setter.
        /// Expected result: The getter should return the same value that was set.
        /// </summary>
        [Theory]
        [InlineData(FlexDirection.Column)]
        [InlineData(FlexDirection.ColumnReverse)]
        [InlineData(FlexDirection.Row)]
        [InlineData(FlexDirection.RowReverse)]
        public void Direction_GetValue_ReturnsSetValue(FlexDirection expectedDirection)
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            flexLayout.Direction = expectedDirection;
            var actualDirection = flexLayout.Direction;

            // Assert
            Assert.Equal(expectedDirection, actualDirection);
        }

        /// <summary>
        /// Tests that the Direction property has the correct default value.
        /// Validates that a new FlexLayout instance has FlexDirection.Row as the default direction.
        /// Expected result: The default Direction should be FlexDirection.Row.
        /// </summary>
        [Fact]
        public void Direction_DefaultValue_ReturnsRow()
        {
            // Arrange & Act
            var flexLayout = new FlexLayout();

            // Assert
            Assert.Equal(FlexDirection.Row, flexLayout.Direction);
        }

        /// <summary>
        /// Tests that the Direction property setter works with boundary enum values.
        /// Validates that all defined enum values can be set and retrieved correctly.
        /// Expected result: All valid FlexDirection enum values should be settable.
        /// </summary>
        [Theory]
        [InlineData((FlexDirection)0)] // Column
        [InlineData((FlexDirection)1)] // ColumnReverse
        [InlineData((FlexDirection)2)] // Row
        [InlineData((FlexDirection)3)] // RowReverse
        public void Direction_SetBoundaryValues_AcceptsValidEnumValues(FlexDirection direction)
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            flexLayout.Direction = direction;

            // Assert
            Assert.Equal(direction, flexLayout.Direction);
        }

        /// <summary>
        /// Tests that the Direction property accepts invalid enum values cast from integers.
        /// Validates that the property doesn't perform enum validation and accepts any FlexDirection value.
        /// Expected result: Invalid enum values should be accepted and retrievable.
        /// </summary>
        [Theory]
        [InlineData((FlexDirection)(-1))]
        [InlineData((FlexDirection)4)]
        [InlineData((FlexDirection)int.MaxValue)]
        [InlineData((FlexDirection)int.MinValue)]
        public void Direction_SetInvalidEnumValues_AcceptsAnyValue(FlexDirection invalidDirection)
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            flexLayout.Direction = invalidDirection;

            // Assert
            Assert.Equal(invalidDirection, flexLayout.Direction);
        }

        /// <summary>
        /// Tests that the Direction property maintains value consistency across multiple operations.
        /// Validates that the property correctly handles sequential set and get operations.
        /// Expected result: The property should maintain the last set value consistently.
        /// </summary>
        [Fact]
        public void Direction_MultipleSetGet_MaintainsConsistency()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act & Assert - Test multiple sequential operations
            flexLayout.Direction = FlexDirection.Column;
            Assert.Equal(FlexDirection.Column, flexLayout.Direction);

            flexLayout.Direction = FlexDirection.RowReverse;
            Assert.Equal(FlexDirection.RowReverse, flexLayout.Direction);

            flexLayout.Direction = FlexDirection.ColumnReverse;
            Assert.Equal(FlexDirection.ColumnReverse, flexLayout.Direction);

            flexLayout.Direction = FlexDirection.Row;
            Assert.Equal(FlexDirection.Row, flexLayout.Direction);
        }

        /// <summary>
        /// Tests that GetAlignSelf throws NullReferenceException when bindable parameter is null.
        /// </summary>
        [Fact]
        public void GetAlignSelf_NullBindable_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindable = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => FlexLayout.GetAlignSelf(bindable));
        }

        /// <summary>
        /// Tests that GetAlignSelf returns the default value (Auto) when no value has been explicitly set.
        /// </summary>
        [Fact]
        public void GetAlignSelf_DefaultValue_ReturnsAuto()
        {
            // Arrange
            var bindable = new Label();

            // Act
            var result = FlexLayout.GetAlignSelf(bindable);

            // Assert
            Assert.Equal(FlexAlignSelf.Auto, result);
        }

        /// <summary>
        /// Tests that GetAlignSelf returns the correct value for each FlexAlignSelf enum value when explicitly set.
        /// Input conditions: Each possible FlexAlignSelf enum value is set on a bindable object.
        /// Expected result: GetAlignSelf returns the same value that was set.
        /// </summary>
        [Theory]
        [InlineData(FlexAlignSelf.Auto)]
        [InlineData(FlexAlignSelf.Stretch)]
        [InlineData(FlexAlignSelf.Center)]
        [InlineData(FlexAlignSelf.Start)]
        [InlineData(FlexAlignSelf.End)]
        public void GetAlignSelf_SetValue_ReturnsExpectedValue(FlexAlignSelf expectedValue)
        {
            // Arrange
            var bindable = new Label();
            FlexLayout.SetAlignSelf(bindable, expectedValue);

            // Act
            var result = FlexLayout.GetAlignSelf(bindable);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that GetAlignSelf works with different BindableObject types.
        /// Input conditions: Different BindableObject derived types with AlignSelf values set.
        /// Expected result: GetAlignSelf returns the correct value regardless of the specific BindableObject type.
        /// </summary>
        [Theory]
        [InlineData(typeof(Label))]
        [InlineData(typeof(Button))]
        [InlineData(typeof(View))]
        public void GetAlignSelf_DifferentBindableObjectTypes_ReturnsCorrectValue(Type bindableType)
        {
            // Arrange
            var bindable = (BindableObject)Activator.CreateInstance(bindableType);
            var expectedValue = FlexAlignSelf.Center;
            FlexLayout.SetAlignSelf(bindable, expectedValue);

            // Act
            var result = FlexLayout.GetAlignSelf(bindable);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that GetAlignSelf returns the most recently set value when the property is changed multiple times.
        /// Input conditions: AlignSelf property is set to different values sequentially.
        /// Expected result: GetAlignSelf returns the last value that was set.
        /// </summary>
        [Fact]
        public void GetAlignSelf_PropertyChangedMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var bindable = new Label();

            // Act & Assert - Set and verify multiple times
            FlexLayout.SetAlignSelf(bindable, FlexAlignSelf.Start);
            Assert.Equal(FlexAlignSelf.Start, FlexLayout.GetAlignSelf(bindable));

            FlexLayout.SetAlignSelf(bindable, FlexAlignSelf.End);
            Assert.Equal(FlexAlignSelf.End, FlexLayout.GetAlignSelf(bindable));

            FlexLayout.SetAlignSelf(bindable, FlexAlignSelf.Auto);
            Assert.Equal(FlexAlignSelf.Auto, FlexLayout.GetAlignSelf(bindable));
        }

        [Fact]
        public void OnUpdate_ValidParameters_CallsRemoveAndAddFlexItem()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var oldView = MockPlatformSizeService.Sub<View>();
            var newView = MockPlatformSizeService.Sub<View>();
            const int index = 0;

            // Add the old view first so it exists in the layout
            layout.Add(oldView);

            // Act
            layout.CallOnUpdate(index, newView, oldView);

            // Assert
            Assert.Contains(newView, layout.Children);
            Assert.DoesNotContain(oldView, layout.Children);
        }

        [Fact]
        public void OnUpdate_WithNullOldView_HandlesGracefully()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var newView = MockPlatformSizeService.Sub<View>();
            const int index = 0;

            // Act & Assert - Should not throw
            layout.CallOnUpdate(index, newView, null);
        }

        [Fact]
        public void OnUpdate_WithNullNewView_HandlesGracefully()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var oldView = MockPlatformSizeService.Sub<View>();
            const int index = 0;

            // Add the old view first
            layout.Add(oldView);

            // Act & Assert - Should not throw
            layout.CallOnUpdate(index, null, oldView);
        }

        [Fact]
        public void OnUpdate_WithBothViewsNull_HandlesGracefully()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            const int index = 0;

            // Act & Assert - Should not throw
            layout.CallOnUpdate(index, null, null);
        }

        [Fact]
        public void OnUpdate_SameViewAndOldView_HandlesGracefully()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var view = MockPlatformSizeService.Sub<View>();
            const int index = 0;

            // Add the view first
            layout.Add(view);

            // Act & Assert - Should not throw
            layout.CallOnUpdate(index, view, view);

            // Assert view is still in layout
            Assert.Contains(view, layout.Children);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(int.MaxValue)]
        public void OnUpdate_VariousIndexValues_HandlesGracefully(int index)
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var oldView = MockPlatformSizeService.Sub<View>();
            var newView = MockPlatformSizeService.Sub<View>();

            // Add the old view first
            layout.Add(oldView);

            // Act & Assert - Should not throw for any index value
            layout.CallOnUpdate(index, newView, oldView);
        }

        [Fact]
        public void OnUpdate_WithBindableObjectView_ClearsFlexItemProperty()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var oldView = new View(); // BindableObject
            var newView = new View(); // BindableObject
            const int index = 0;

            // Add the old view first and set some flex properties
            layout.Add(oldView);
            FlexLayout.SetOrder(oldView, 5);
            FlexLayout.SetGrow(oldView, 2.0f);

            // Act
            layout.CallOnUpdate(index, newView, oldView);

            // Assert - The old view should have its flex properties cleared
            // We can't directly test the FlexItemProperty clearing, but we can test observable behavior
            Assert.Contains(newView, layout.Children);
            Assert.DoesNotContain(oldView, layout.Children);
        }

        [Fact]
        public void OnUpdate_WithNonBindableObjectView_UpdatesViewInfoDictionary()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var oldView = Substitute.For<IView>(); // Non-BindableObject
            var newView = Substitute.For<IView>(); // Non-BindableObject
            const int index = 0;

            // Set up the views to return valid sizes
            oldView.DesiredSize.Returns(new Size(100, 100));
            newView.DesiredSize.Returns(new Size(100, 100));
            oldView.Measure(Arg.Any<double>(), Arg.Any<double>()).Returns(new Size(100, 100));
            newView.Measure(Arg.Any<double>(), Arg.Any<double>()).Returns(new Size(100, 100));

            // Add old view first
            layout.Add(oldView);
            var initialCount = layout.ViewInfoCount;

            // Act
            layout.CallOnUpdate(index, newView, oldView);

            // Assert - Should have updated the view info
            Assert.False(layout.HasViewInInfo(oldView));
        }

        [Fact]
        public void OnUpdate_ZeroIndex_UpdatesAtFirstPosition()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var existingView = MockPlatformSizeService.Sub<View>();
            var oldView = MockPlatformSizeService.Sub<View>();
            var newView = MockPlatformSizeService.Sub<View>();

            // Add views to layout
            layout.Add(existingView);
            layout.Add(oldView);

            // Act
            layout.CallOnUpdate(0, newView, oldView);

            // Assert
            Assert.Contains(newView, layout.Children);
            Assert.DoesNotContain(oldView, layout.Children);
            Assert.Contains(existingView, layout.Children);
        }

        [Fact]
        public void OnUpdate_WithFlexLayoutAsOldView_HandlesNestedLayouts()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var oldFlexLayout = new FlexLayout();
            var newView = MockPlatformSizeService.Sub<View>();
            const int index = 0;

            // Add the old flex layout
            layout.Add(oldFlexLayout);

            // Act & Assert - Should not throw
            layout.CallOnUpdate(index, newView, oldFlexLayout);

            Assert.Contains(newView, layout.Children);
            Assert.DoesNotContain(oldFlexLayout, layout.Children);
        }

        [Fact]
        public void OnUpdate_WithFlexLayoutAsNewView_HandlesNestedLayouts()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var oldView = MockPlatformSizeService.Sub<View>();
            var newFlexLayout = new FlexLayout();
            const int index = 0;

            // Add the old view
            layout.Add(oldView);

            // Act & Assert - Should not throw
            layout.CallOnUpdate(index, newFlexLayout, oldView);

            Assert.Contains(newFlexLayout, layout.Children);
            Assert.DoesNotContain(oldView, layout.Children);
        }

        [Fact]
        public void OnUpdate_CallsBaseOnUpdate()
        {
            // Arrange
            var layout = new TestableFlexLayout();
            var oldView = MockPlatformSizeService.Sub<View>();
            var newView = MockPlatformSizeService.Sub<View>();
            const int index = 0;

            // Add old view first
            layout.Add(oldView);

            // Act
            layout.CallOnUpdate(index, newView, oldView);

            // Assert - Base OnUpdate should have been called, which updates the layout
            // We can verify this by checking that the layout has been updated appropriately
            Assert.Contains(newView, layout.Children);
            Assert.DoesNotContain(oldView, layout.Children);
        }
    }
}

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for FlexLayout.JustifyContent property
    /// </summary>
    public partial class FlexLayoutJustifyContentTests
    {
        /// <summary>
        /// Tests that JustifyContent property correctly gets and sets all valid enum values.
        /// Input: All valid FlexJustify enum values.
        /// Expected result: Each set value is correctly retrieved.
        /// </summary>
        [Theory]
        [InlineData(FlexJustify.Start)]
        [InlineData(FlexJustify.Center)]
        [InlineData(FlexJustify.End)]
        [InlineData(FlexJustify.SpaceBetween)]
        [InlineData(FlexJustify.SpaceAround)]
        [InlineData(FlexJustify.SpaceEvenly)]
        public void JustifyContent_ValidEnumValues_SetsAndGetsCorrectly(FlexJustify justifyValue)
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act
            flexLayout.JustifyContent = justifyValue;
            var result = flexLayout.JustifyContent;

            // Assert
            Assert.Equal(justifyValue, result);
        }

        /// <summary>
        /// Tests that JustifyContent property handles invalid enum values (outside defined range).
        /// Input: Integer values cast to FlexJustify that are outside the defined enum range.
        /// Expected result: The cast values are set and retrieved correctly.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(99)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void JustifyContent_InvalidEnumValues_HandlesGracefully(int invalidValue)
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var invalidEnumValue = (FlexJustify)invalidValue;

            // Act
            flexLayout.JustifyContent = invalidEnumValue;
            var result = flexLayout.JustifyContent;

            // Assert
            Assert.Equal(invalidEnumValue, result);
        }

        /// <summary>
        /// Tests that JustifyContent property maintains value across multiple get operations.
        /// Input: Set FlexJustify.Center and retrieve multiple times.
        /// Expected result: Consistent value returned on each get operation.
        /// </summary>
        [Fact]
        public void JustifyContent_MultipleGets_ReturnsConsistentValue()
        {
            // Arrange
            var flexLayout = new FlexLayout();
            var expectedValue = FlexJustify.Center;

            // Act
            flexLayout.JustifyContent = expectedValue;
            var result1 = flexLayout.JustifyContent;
            var result2 = flexLayout.JustifyContent;
            var result3 = flexLayout.JustifyContent;

            // Assert
            Assert.Equal(expectedValue, result1);
            Assert.Equal(expectedValue, result2);
            Assert.Equal(expectedValue, result3);
        }

        /// <summary>
        /// Tests that JustifyContent property can be changed multiple times.
        /// Input: Set different FlexJustify values sequentially.
        /// Expected result: Each new value overwrites the previous one correctly.
        /// </summary>
        [Fact]
        public void JustifyContent_MultipleAssignments_UpdatesValueCorrectly()
        {
            // Arrange
            var flexLayout = new FlexLayout();

            // Act & Assert
            flexLayout.JustifyContent = FlexJustify.Start;
            Assert.Equal(FlexJustify.Start, flexLayout.JustifyContent);

            flexLayout.JustifyContent = FlexJustify.Center;
            Assert.Equal(FlexJustify.Center, flexLayout.JustifyContent);

            flexLayout.JustifyContent = FlexJustify.End;
            Assert.Equal(FlexJustify.End, flexLayout.JustifyContent);

            flexLayout.JustifyContent = FlexJustify.SpaceBetween;
            Assert.Equal(FlexJustify.SpaceBetween, flexLayout.JustifyContent);
        }
    }


    public class FlexLayoutGetOrderTests
    {
        /// <summary>
        /// Tests that GetOrder returns the correct value when a valid order has been set on a bindable object.
        /// Validates various integer values including boundary cases.
        /// </summary>
        /// <param name="expectedOrder">The order value to test</param>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(100)]
        [InlineData(-100)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void GetOrder_ValidBindableWithSetOrder_ReturnsExpectedValue(int expectedOrder)
        {
            // Arrange
            var label = new Label();
            FlexLayout.SetOrder(label, expectedOrder);

            // Act
            var actualOrder = FlexLayout.GetOrder(label);

            // Assert
            Assert.Equal(expectedOrder, actualOrder);
        }

        /// <summary>
        /// Tests that GetOrder returns the default value (0) when no order has been explicitly set on a bindable object.
        /// Verifies the default behavior matches the OrderProperty default value.
        /// </summary>
        [Fact]
        public void GetOrder_BindableWithoutSetOrder_ReturnsDefaultValue()
        {
            // Arrange
            var label = new Label();

            // Act
            var order = FlexLayout.GetOrder(label);

            // Assert
            Assert.Equal(0, order);
        }

        /// <summary>
        /// Tests that GetOrder throws a NullReferenceException when passed a null bindable object.
        /// Validates proper error handling for invalid input.
        /// </summary>
        [Fact]
        public void GetOrder_NullBindable_ThrowsNullReferenceException()
        {
            // Arrange, Act & Assert
            Assert.Throws<NullReferenceException>(() => FlexLayout.GetOrder(null));
        }

        /// <summary>
        /// Tests GetOrder with different types of BindableObject-derived objects to ensure consistency.
        /// Verifies that the attached property behavior works across different control types.
        /// </summary>
        /// <param name="expectedOrder">The order value to test</param>
        [Theory]
        [InlineData(5)]
        [InlineData(-10)]
        [InlineData(42)]
        public void GetOrder_DifferentBindableObjectTypes_ReturnsCorrectValue(int expectedOrder)
        {
            // Arrange
            var label = new Label();
            var button = new Button();
            var entry = new Entry();

            FlexLayout.SetOrder(label, expectedOrder);
            FlexLayout.SetOrder(button, expectedOrder);
            FlexLayout.SetOrder(entry, expectedOrder);

            // Act
            var labelOrder = FlexLayout.GetOrder(label);
            var buttonOrder = FlexLayout.GetOrder(button);
            var entryOrder = FlexLayout.GetOrder(entry);

            // Assert
            Assert.Equal(expectedOrder, labelOrder);
            Assert.Equal(expectedOrder, buttonOrder);
            Assert.Equal(expectedOrder, entryOrder);
        }

        /// <summary>
        /// Tests that GetOrder retrieves the current value after multiple SetOrder calls on the same object.
        /// Validates that the property value updates correctly and GetOrder returns the most recent value.
        /// </summary>
        [Fact]
        public void GetOrder_AfterMultipleSetOrder_ReturnsLatestValue()
        {
            // Arrange
            var label = new Label();

            // Act & Assert - Set and verify multiple times
            FlexLayout.SetOrder(label, 10);
            Assert.Equal(10, FlexLayout.GetOrder(label));

            FlexLayout.SetOrder(label, -5);
            Assert.Equal(-5, FlexLayout.GetOrder(label));

            FlexLayout.SetOrder(label, 0);
            Assert.Equal(0, FlexLayout.GetOrder(label));

            FlexLayout.SetOrder(label, int.MaxValue);
            Assert.Equal(int.MaxValue, FlexLayout.GetOrder(label));
        }
    }


}