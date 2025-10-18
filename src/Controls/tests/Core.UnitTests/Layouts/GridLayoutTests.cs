#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
    public class GridLayoutTests
    {
        [Fact]
        public void RemovedMauiViewsHaveNoRowColumnInfo()
        {
            var gl = new Grid();
            var view = NSubstitute.Substitute.For<IView>();

            gl.Add(view);
            gl.SetRow(view, 2);

            // Check our assumptions
            Assert.Equal(2, gl.GetRow(view));

            // Okay, removing the View from the Grid should mean that any attempt to get row/column info
            // for that View should fail
            gl.Remove(view);

            Assert.Throws<KeyNotFoundException>(() => gl.GetRow(view));
            Assert.Throws<KeyNotFoundException>(() => gl.GetRowSpan(view));
            Assert.Throws<KeyNotFoundException>(() => gl.GetColumn(view));
            Assert.Throws<KeyNotFoundException>(() => gl.GetColumnSpan(view));
        }

        [Fact]
        public void AddedViewGetsDefaultRowAndColumn()
        {
            var gl = new Grid();
            var view = new Label();

            gl.Add(view);
            Assert.Equal(0, gl.GetRow(view));
            Assert.Equal(0, gl.GetColumn(view));
            Assert.Equal(1, gl.GetRowSpan(view));
            Assert.Equal(1, gl.GetColumnSpan(view));
        }

        [Fact]
        public void AddedMauiViewGetsDefaultRowAndColumn()
        {
            var gl = new Grid();
            var view = NSubstitute.Substitute.For<IView>();

            gl.Add(view);
            Assert.Equal(0, gl.GetRow(view));
            Assert.Equal(0, gl.GetColumn(view));
            Assert.Equal(1, gl.GetRowSpan(view));
            Assert.Equal(1, gl.GetColumnSpan(view));
        }

        [Fact]
        public void ChangingRowSpacingInvalidatesGrid()
        {
            var grid = new Grid();

            var handler = ListenForInvalidation(grid);
            grid.RowSpacing = 100;
            AssertInvalidated(handler);
        }

        [Fact]
        public void ChangingColumnSpacingInvalidatesGrid()
        {
            var grid = new Grid();

            var handler = ListenForInvalidation(grid);
            grid.ColumnSpacing = 100;
            AssertInvalidated(handler);
        }

        [Fact]
        public void ChangingChildRowInvalidatesGrid()
        {
            var grid = new Grid()
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition(), new RowDefinition()
                }
            };

            var view = Substitute.For<IView>();
            grid.Add(view);

            var handler = ListenForInvalidation(grid);

            grid.SetRow(view, 1);

            AssertInvalidated(handler);
        }

        [Fact]
        public void ChangingChildColumnInvalidatesGrid()
        {
            var grid = new Grid()
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(), new ColumnDefinition()
                }
            };

            var view = Substitute.For<IView>();
            grid.Add(view);

            var handler = ListenForInvalidation(grid);

            grid.SetColumn(view, 1);

            AssertInvalidated(handler);
        }

        static IViewHandler ListenForInvalidation(IView view)
        {
            var handler = Substitute.For<IViewHandler>();
            view.Handler = handler;
            handler.ClearReceivedCalls();
            return handler;
        }

        static void AssertInvalidated(IViewHandler handler)
        {
            handler.Received().Invoke(Arg.Is(nameof(IView.InvalidateMeasure)), Arg.Any<object>());
        }

        [Fact]
        public void RowDefinitionsGetBindingContext()
        {
            var def = new RowDefinition();
            var def2 = new RowDefinition();

            var grid = new Grid()
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    def
                }
            };

            var context = new object();

            Assert.Null(def.BindingContext);
            Assert.Null(def2.BindingContext);

            grid.BindingContext = context;

            Assert.Equal(def.BindingContext, context);

            grid.RowDefinitions.Add(def2);

            Assert.Equal(def2.BindingContext, context);
        }

        [Fact]
        public void ColumnDefinitionsGetBindingContext()
        {
            var def = new ColumnDefinition();
            var def2 = new ColumnDefinition();

            var grid = new Grid()
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    def
                }
            };

            var context = new object();

            Assert.Null(def.BindingContext);
            Assert.Null(def2.BindingContext);

            grid.BindingContext = context;

            Assert.Equal(def.BindingContext, context);

            grid.ColumnDefinitions.Add(def2);

            Assert.Equal(def2.BindingContext, context);
        }

        [Fact, Category(TestCategory.Memory)]
        public async Task ColumnDefinitionDoesNotLeak()
        {
            // Long-lived column, like from a Style in App.Resources
            var columnDefinition = new ColumnDefinition();

            WeakReference CreateReference()
            {
                var grid = new Grid();
                grid.ColumnDefinitions.Add(columnDefinition);
                return new(grid);
            }

            WeakReference reference = CreateReference();

            await TestHelpers.Collect();

            Assert.False(await reference.WaitForCollect(), "Grid should not be alive!");

            // Ensure that the ColumnDefinition isn't collected during the test
            GC.KeepAlive(columnDefinition);
        }

        [Fact]
        public async Task RowDefinitionDoesNotLeak()
        {
            // Long-lived row, like from a Style in App.Resources
            var row = new RowDefinition();
            WeakReference reference;

            {
                var grid = new Grid();
                grid.RowDefinitions.Add(row);
                reference = new(grid);
            }

            await TestHelpers.Collect();

            Assert.False(await reference.WaitForCollect(), "Grid should not be alive!");
        }
    }
}

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class LayoutTests
    {
        /// <summary>
        /// Tests that LayoutHandler returns null when Handler is null.
        /// </summary>
        [Fact]
        public void LayoutHandler_WhenHandlerIsNull_ReturnsNull()
        {
            // Arrange
            var layout = new TestLayout();
            layout.Handler = null;

            // Act
            var result = layout.LayoutHandler;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that LayoutHandler returns null when Handler is not an ILayoutHandler.
        /// </summary>
        [Fact]
        public void LayoutHandler_WhenHandlerIsNotILayoutHandler_ReturnsNull()
        {
            // Arrange
            var layout = new TestLayout();
            var mockViewHandler = Substitute.For<IViewHandler>();
            layout.Handler = mockViewHandler;

            // Act
            var result = layout.LayoutHandler;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that LayoutHandler returns the handler when Handler is an ILayoutHandler.
        /// </summary>
        [Fact]
        public void LayoutHandler_WhenHandlerIsILayoutHandler_ReturnsHandler()
        {
            // Arrange
            var layout = new TestLayout();
            var mockLayoutHandler = Substitute.For<ILayoutHandler>();
            layout.Handler = mockLayoutHandler;

            // Act
            var result = layout.LayoutHandler;

            // Assert
            Assert.Same(mockLayoutHandler, result);
        }

        /// <summary>
        /// Tests that IsClippedToBounds property getter returns true when the property is set to true.
        /// Verifies the property correctly calls GetValue and casts the result to bool.
        /// </summary>
        [Fact]
        public void IsClippedToBounds_WhenSetToTrue_ReturnsTrue()
        {
            // Arrange
            var layout = Substitute.ForPartsOf<TestableLayout>();

            // Act
            layout.IsClippedToBounds = true;
            var result = layout.IsClippedToBounds;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsClippedToBounds property getter returns false when the property is set to false.
        /// Verifies the property correctly calls GetValue and casts the result to bool.
        /// </summary>
        [Fact]
        public void IsClippedToBounds_WhenSetToFalse_ReturnsFalse()
        {
            // Arrange
            var layout = Substitute.ForPartsOf<TestableLayout>();

            // Act
            layout.IsClippedToBounds = false;
            var result = layout.IsClippedToBounds;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsClippedToBounds property getter returns the default value (false) when not explicitly set.
        /// Verifies the default behavior of the property.
        /// </summary>
        [Fact]
        public void IsClippedToBounds_WhenNotSet_ReturnsDefaultValue()
        {
            // Arrange
            var layout = Substitute.ForPartsOf<TestableLayout>();

            // Act
            var result = layout.IsClippedToBounds;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests IsClippedToBounds property with parameterized boolean values.
        /// Verifies that the property correctly handles both true and false values.
        /// </summary>
        /// <param name="value">The boolean value to test</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsClippedToBounds_SetAndGet_ReturnsExpectedValue(bool value)
        {
            // Arrange
            var layout = Substitute.ForPartsOf<TestableLayout>();

            // Act
            layout.IsClippedToBounds = value;
            var result = layout.IsClippedToBounds;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Concrete test implementation of the abstract Layout class for testing purposes.
        /// Provides minimal implementation of abstract methods to enable testing of the Layout class.
        /// </summary>
        private class TestableLayout : Microsoft.Maui.Controls.Compatibility.Layout
        {
            protected override void LayoutChildren(double x, double y, double width, double height)
            {
                // Empty implementation for testing
            }
        }

        /// <summary>
        /// Tests that the default Padding property returns Thickness.Zero when no value has been set.
        /// </summary>
        [Fact]
        public void Padding_DefaultValue_ReturnsThicknessZero()
        {
            // Arrange
            var layout = new TestLayout();

            // Act
            var result = layout.Padding;

            // Assert
            Assert.Equal(Thickness.Zero, result);
            Assert.Equal(0, result.Left);
            Assert.Equal(0, result.Top);
            Assert.Equal(0, result.Right);
            Assert.Equal(0, result.Bottom);
        }

        /// <summary>
        /// Tests setting and getting the Padding property with various valid Thickness values.
        /// Verifies round-trip behavior for different thickness configurations.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(10, 10, 10, 10)]
        [InlineData(5, 15, 5, 15)]
        [InlineData(1, 2, 3, 4)]
        [InlineData(-5, -10, -3, -7)]
        [InlineData(1.5, 2.7, 3.9, 4.1)]
        public void Padding_SetValidThickness_GetReturnsExpectedValue(double left, double top, double right, double bottom)
        {
            // Arrange
            var layout = new TestLayout();
            var expectedThickness = new Thickness(left, top, right, bottom);

            // Act
            layout.Padding = expectedThickness;
            var result = layout.Padding;

            // Assert
            Assert.Equal(expectedThickness, result);
            Assert.Equal(left, result.Left);
            Assert.Equal(top, result.Top);
            Assert.Equal(right, result.Right);
            Assert.Equal(bottom, result.Bottom);
        }

        /// <summary>
        /// Tests setting and getting the Padding property with uniform thickness values.
        /// Verifies that uniform thickness constructor works correctly.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(10.5)]
        [InlineData(-2.3)]
        public void Padding_SetUniformThickness_GetReturnsExpectedValue(double uniformSize)
        {
            // Arrange
            var layout = new TestLayout();
            var expectedThickness = new Thickness(uniformSize);

            // Act
            layout.Padding = expectedThickness;
            var result = layout.Padding;

            // Assert
            Assert.Equal(expectedThickness, result);
            Assert.Equal(uniformSize, result.Left);
            Assert.Equal(uniformSize, result.Top);
            Assert.Equal(uniformSize, result.Right);
            Assert.Equal(uniformSize, result.Bottom);
        }

        /// <summary>
        /// Tests setting and getting the Padding property with horizontal and vertical thickness values.
        /// Verifies that horizontal/vertical thickness constructor works correctly.
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(5, 10)]
        [InlineData(2.5, 7.8)]
        [InlineData(-1, -3)]
        public void Padding_SetHorizontalVerticalThickness_GetReturnsExpectedValue(double horizontal, double vertical)
        {
            // Arrange
            var layout = new TestLayout();
            var expectedThickness = new Thickness(horizontal, vertical);

            // Act
            layout.Padding = expectedThickness;
            var result = layout.Padding;

            // Assert
            Assert.Equal(expectedThickness, result);
            Assert.Equal(horizontal, result.Left);
            Assert.Equal(vertical, result.Top);
            Assert.Equal(horizontal, result.Right);
            Assert.Equal(vertical, result.Bottom);
        }

        /// <summary>
        /// Tests setting the Padding property with extreme double values including boundaries.
        /// Verifies that extreme values are handled correctly and round-trip properly.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        public void Padding_SetExtremeValues_GetReturnsExpectedValue(double extremeValue)
        {
            // Arrange
            var layout = new TestLayout();
            var expectedThickness = new Thickness(extremeValue);

            // Act
            layout.Padding = expectedThickness;
            var result = layout.Padding;

            // Assert
            Assert.Equal(expectedThickness, result);
            Assert.Equal(extremeValue, result.Left);
            Assert.Equal(extremeValue, result.Top);
            Assert.Equal(extremeValue, result.Right);
            Assert.Equal(extremeValue, result.Bottom);
        }

        /// <summary>
        /// Tests setting the Padding property with special double values (NaN, Infinity).
        /// Verifies that special floating-point values are preserved correctly.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void Padding_SetSpecialDoubleValues_GetReturnsExpectedValue(double specialValue)
        {
            // Arrange
            var layout = new TestLayout();
            var expectedThickness = new Thickness(specialValue);

            // Act
            layout.Padding = expectedThickness;
            var result = layout.Padding;

            // Assert
            Assert.Equal(expectedThickness, result);

            if (double.IsNaN(specialValue))
            {
                Assert.True(double.IsNaN(result.Left));
                Assert.True(double.IsNaN(result.Top));
                Assert.True(double.IsNaN(result.Right));
                Assert.True(double.IsNaN(result.Bottom));
            }
            else
            {
                Assert.Equal(specialValue, result.Left);
                Assert.Equal(specialValue, result.Top);
                Assert.Equal(specialValue, result.Right);
                Assert.Equal(specialValue, result.Bottom);
            }
        }

        /// <summary>
        /// Tests that multiple set operations on the Padding property work correctly.
        /// Verifies that the property can be updated multiple times with different values.
        /// </summary>
        [Fact]
        public void Padding_MultipleSetOperations_GetReturnsLatestValue()
        {
            // Arrange
            var layout = new TestLayout();
            var firstThickness = new Thickness(5);
            var secondThickness = new Thickness(1, 2, 3, 4);
            var thirdThickness = new Thickness(10, 20);

            // Act & Assert - First set
            layout.Padding = firstThickness;
            Assert.Equal(firstThickness, layout.Padding);

            // Act & Assert - Second set
            layout.Padding = secondThickness;
            Assert.Equal(secondThickness, layout.Padding);

            // Act & Assert - Third set
            layout.Padding = thirdThickness;
            Assert.Equal(thirdThickness, layout.Padding);
        }

        /// <summary>
        /// Tests setting the Padding property back to zero after setting other values.
        /// Verifies that the property can be reset to default state.
        /// </summary>
        [Fact]
        public void Padding_ResetToZero_GetReturnsThicknessZero()
        {
            // Arrange
            var layout = new TestLayout();
            layout.Padding = new Thickness(10, 20, 30, 40);

            // Act
            layout.Padding = Thickness.Zero;
            var result = layout.Padding;

            // Assert
            Assert.Equal(Thickness.Zero, result);
            Assert.Equal(0, result.Left);
            Assert.Equal(0, result.Top);
            Assert.Equal(0, result.Right);
            Assert.Equal(0, result.Bottom);
        }

        /// <summary>
        /// Tests that the CascadeInputTransparent property getter returns the expected boolean value.
        /// Verifies that the getter correctly retrieves the value from the underlying BindableProperty.
        /// </summary>
        /// <param name="expectedValue">The boolean value to test (true or false).</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CascadeInputTransparent_Get_ReturnsExpectedValue(bool expectedValue)
        {
            // Arrange
            var layout = new TestLayout();
            layout.SetValue(Microsoft.Maui.Controls.InputTransparentContainerElement.CascadeInputTransparentProperty, expectedValue);

            // Act
            var actualValue = layout.CascadeInputTransparent;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the CascadeInputTransparent property setter correctly sets the value.
        /// Verifies that the setter properly stores the value in the underlying BindableProperty.
        /// </summary>
        /// <param name="valueToSet">The boolean value to set (true or false).</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CascadeInputTransparent_Set_StoresValueCorrectly(bool valueToSet)
        {
            // Arrange
            var layout = new TestLayout();

            // Act
            layout.CascadeInputTransparent = valueToSet;

            // Assert
            var actualValue = (bool)layout.GetValue(Microsoft.Maui.Controls.InputTransparentContainerElement.CascadeInputTransparentProperty);
            Assert.Equal(valueToSet, actualValue);
        }

        /// <summary>
        /// Tests that the CascadeInputTransparent property returns the default value when not explicitly set.
        /// Verifies the default behavior of the property before any explicit assignment.
        /// </summary>
        [Fact]
        public void CascadeInputTransparent_DefaultValue_ReturnsExpectedDefault()
        {
            // Arrange
            var layout = new TestLayout();

            // Act
            var defaultValue = layout.CascadeInputTransparent;

            // Assert
            // The default value should be false based on the property's default behavior
            Assert.False(defaultValue);
        }

        /// <summary>
        /// Tests that the CascadeInputTransparent property getter and setter work together correctly.
        /// Verifies round-trip functionality by setting a value and ensuring the getter returns the same value.
        /// </summary>
        /// <param name="testValue">The boolean value to test in the round-trip scenario.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CascadeInputTransparent_SetThenGet_RoundTripWorksCorrectly(bool testValue)
        {
            // Arrange
            var layout = new TestLayout();

            // Act
            layout.CascadeInputTransparent = testValue;
            var retrievedValue = layout.CascadeInputTransparent;

            // Assert
            Assert.Equal(testValue, retrievedValue);
        }

        /// <summary>
        /// Tests that the Children property returns the InternalChildren collection as an IReadOnlyList.
        /// This test verifies that the property correctly exposes the internal collection.
        /// </summary>
        [Fact]
        public void Children_ReturnsInternalChildren_AsReadOnlyList()
        {
            // Arrange
            var layout = new TestLayout();

            // Act
            var children = layout.Children;

            // Assert
            Assert.NotNull(children);
            Assert.IsAssignableFrom<IReadOnlyList<Element>>(children);
            Assert.Same(layout.GetInternalChildren(), children);
        }

        /// <summary>
        /// Tests that the Children property reflects changes made to the InternalChildren collection.
        /// This test verifies that the property provides a live view of the internal collection.
        /// </summary>
        [Fact]
        public void Children_ReflectsInternalChildrenChanges_WhenItemsAdded()
        {
            // Arrange
            var layout = new TestLayout();
            var element = new TestElement();

            // Act
            layout.GetInternalChildren().Add(element);
            var children = layout.Children;

            // Assert
            Assert.Single(children);
            Assert.Same(element, children[0]);
        }

        /// <summary>
        /// Tests that the Children property returns an empty collection when InternalChildren is empty.
        /// This test verifies the property behavior with an empty internal collection.
        /// </summary>
        [Fact]
        public void Children_ReturnsEmptyCollection_WhenInternalChildrenEmpty()
        {
            // Arrange
            var layout = new TestLayout();

            // Act
            var children = layout.Children;

            // Assert
            Assert.NotNull(children);
            Assert.Empty(children);
        }

        /// <summary>
        /// Tests that the Children property reflects multiple items in the InternalChildren collection.
        /// This test verifies the property behavior with multiple elements.
        /// </summary>
        [Fact]
        public void Children_ReflectsMultipleItems_WhenInternalChildrenHasMultipleElements()
        {
            // Arrange
            var layout = new TestLayout();
            var element1 = new TestElement();
            var element2 = new TestElement();
            var element3 = new TestElement();

            // Act
            layout.GetInternalChildren().Add(element1);
            layout.GetInternalChildren().Add(element2);
            layout.GetInternalChildren().Add(element3);
            var children = layout.Children;

            // Assert
            Assert.Equal(3, children.Count);
            Assert.Same(element1, children[0]);
            Assert.Same(element2, children[1]);
            Assert.Same(element3, children[2]);
        }

        /// <summary>
        /// Tests that the Children property reflects removals from the InternalChildren collection.
        /// This test verifies that the property provides a live view when items are removed.
        /// </summary>
        [Fact]
        public void Children_ReflectsInternalChildrenChanges_WhenItemsRemoved()
        {
            // Arrange
            var layout = new TestLayout();
            var element1 = new TestElement();
            var element2 = new TestElement();
            layout.GetInternalChildren().Add(element1);
            layout.GetInternalChildren().Add(element2);

            // Act
            layout.GetInternalChildren().Remove(element1);
            var children = layout.Children;

            // Assert
            Assert.Single(children);
            Assert.Same(element2, children[0]);
        }

        /// <summary>
        /// Tests that the Children property reflects clearing of the InternalChildren collection.
        /// This test verifies the property behavior when the internal collection is cleared.
        /// </summary>
        [Fact]
        public void Children_ReflectsInternalChildrenChanges_WhenCollectionCleared()
        {
            // Arrange
            var layout = new TestLayout();
            var element1 = new TestElement();
            var element2 = new TestElement();
            layout.GetInternalChildren().Add(element1);
            layout.GetInternalChildren().Add(element2);

            // Act
            layout.GetInternalChildren().Clear();
            var children = layout.Children;

            // Assert
            Assert.Empty(children);
        }

        /// <summary>
        /// Simple test element for use in layout tests.
        /// </summary>
        private class TestElement : Element
        {
        }

        /// <summary>
        /// Tests that Measure calls MeasureOverride when UseCompatibilityMode is false.
        /// This test verifies the non-compatibility mode path that was not covered by existing tests.
        /// Expected result: MeasureOverride is called and its result is returned directly.
        /// </summary>
        [Fact]
        public void Measure_UseCompatibilityModeFalse_CallsMeasureOverride()
        {
            // Arrange
            var layout = new TestNonCompatibilityLayout();
            var expectedSize = new Size(100, 50);
            layout.SetMeasureOverrideResult(expectedSize);
            double widthConstraint = 200;
            double heightConstraint = 150;

            // Act
            var result = layout.Measure(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(expectedSize.Width, result.Request.Width);
            Assert.Equal(expectedSize.Height, result.Request.Height);
            Assert.True(layout.MeasureOverrideCalled);
            Assert.Equal(widthConstraint, layout.MeasureOverrideWidth);
            Assert.Equal(heightConstraint, layout.MeasureOverrideHeight);
        }

        /// <summary>
        /// Tests that Measure handles various double constraint values correctly in compatibility mode.
        /// This test verifies edge cases with extreme double values including infinity and NaN.
        /// Expected result: Method completes without throwing exceptions for all valid inputs.
        /// </summary>
        [Theory]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(0, 0)]
        [InlineData(-100, -50)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(1000, 500)]
        public void Measure_VariousConstraintValues_HandlesEdgeCases(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var layout = new TestCompatibilityLayout();
            layout.SetBaseMeasureResult(new SizeRequest(new Size(50, 25)));

            // Act & Assert - Should not throw
            var result = layout.Measure(widthConstraint, heightConstraint);

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that Measure handles different MeasureFlags values correctly.
        /// This test verifies that both None and IncludeMargins flags are processed properly.
        /// Expected result: Method executes with different flag values without errors.
        /// </summary>
        [Theory]
        [InlineData(MeasureFlags.None)]
        [InlineData(MeasureFlags.IncludeMargins)]
        public void Measure_DifferentMeasureFlags_ProcessesCorrectly(MeasureFlags flags)
        {
            // Arrange
            var layout = new TestCompatibilityLayout();
            layout.SetBaseMeasureResult(new SizeRequest(new Size(100, 50)));

            // Act
            var result = layout.Measure(200, 150, flags);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(flags, layout.LastMeasureFlags);
        }

        /// <summary>
        /// Tests that Measure correctly adds padding to the base measurement result in compatibility mode.
        /// This test verifies the padding calculation logic that adds horizontal and vertical thickness.
        /// Expected result: Returned sizes include the padding values added to the base measurement.
        /// </summary>
        [Fact]
        public void Measure_CompatibilityMode_AddsPaddingCorrectly()
        {
            // Arrange
            var layout = new TestCompatibilityLayout();
            var baseMeasureRequest = new Size(100, 50);
            var baseMeasureMinimum = new Size(80, 40);
            var padding = new Thickness(10, 15, 20, 25); // left, top, right, bottom

            layout.SetBaseMeasureResult(new SizeRequest(baseMeasureRequest, baseMeasureMinimum));
            layout.TestPadding = padding;

            double widthConstraint = 300;
            double heightConstraint = 200;

            // Act
            var result = layout.Measure(widthConstraint, heightConstraint);

            // Assert
            var expectedHorizontalThickness = padding.Left + padding.Right; // 30
            var expectedVerticalThickness = padding.Top + padding.Bottom; // 40

            Assert.Equal(baseMeasureRequest.Width + expectedHorizontalThickness, result.Request.Width);
            Assert.Equal(baseMeasureRequest.Height + expectedVerticalThickness, result.Request.Height);
            Assert.Equal(baseMeasureMinimum.Width + expectedHorizontalThickness, result.Minimum.Width);
            Assert.Equal(baseMeasureMinimum.Height + expectedVerticalThickness, result.Minimum.Height);

            // Verify base.Measure was called with constraints adjusted for padding
            Assert.Equal(widthConstraint - expectedHorizontalThickness, layout.LastBaseMeasureWidth);
            Assert.Equal(heightConstraint - expectedVerticalThickness, layout.LastBaseMeasureHeight);
        }

        /// <summary>
        /// Tests that Measure sets the DesiredSize property correctly in compatibility mode.
        /// This test verifies that the DesiredSize property is updated with the calculated request size.
        /// Expected result: DesiredSize property matches the request size from the returned SizeRequest.
        /// </summary>
        [Fact]
        public void Measure_CompatibilityMode_SetsDesiredSize()
        {
            // Arrange
            var layout = new TestCompatibilityLayout();
            var baseMeasureSize = new Size(120, 80);
            var padding = new Thickness(5, 10, 15, 20);

            layout.SetBaseMeasureResult(new SizeRequest(baseMeasureSize));
            layout.TestPadding = padding;

            // Act
            var result = layout.Measure(400, 300);

            // Assert
            var expectedWidth = baseMeasureSize.Width + padding.HorizontalThickness;
            var expectedHeight = baseMeasureSize.Height + padding.VerticalThickness;

            Assert.Equal(expectedWidth, layout.DesiredSize.Width);
            Assert.Equal(expectedHeight, layout.DesiredSize.Height);
            Assert.Equal(result.Request.Width, layout.DesiredSize.Width);
            Assert.Equal(result.Request.Height, layout.DesiredSize.Height);
        }

        /// <summary>
        /// Tests that Measure handles zero padding values correctly.
        /// This test verifies that when padding is zero, the result equals the base measurement.
        /// Expected result: Result sizes match base measurement when padding is zero.
        /// </summary>
        [Fact]
        public void Measure_ZeroPadding_ReturnsBaseMeasurement()
        {
            // Arrange
            var layout = new TestCompatibilityLayout();
            var baseMeasureRequest = new Size(150, 75);
            var baseMeasureMinimum = new Size(100, 50);

            layout.SetBaseMeasureResult(new SizeRequest(baseMeasureRequest, baseMeasureMinimum));
            layout.TestPadding = new Thickness(0);

            // Act
            var result = layout.Measure(500, 400);

            // Assert
            Assert.Equal(baseMeasureRequest.Width, result.Request.Width);
            Assert.Equal(baseMeasureRequest.Height, result.Request.Height);
            Assert.Equal(baseMeasureMinimum.Width, result.Minimum.Width);
            Assert.Equal(baseMeasureMinimum.Height, result.Minimum.Height);
        }

    }


    /// <summary>
    /// Tests for the Layout.LayoutChildIntoBoundingRegion static method.
    /// </summary>
    public partial class LayoutChildIntoBoundingRegionTests
    {
        /// <summary>
        /// Tests that LayoutChildIntoBoundingRegion handles null child parameter gracefully.
        /// Expected behavior: Method should handle null child without throwing exception.
        /// </summary>
        [Fact]
        public void LayoutChildIntoBoundingRegion_NullChild_DoesNotThrow()
        {
            // Arrange
            VisualElement child = null;
            var region = new Rect(0, 0, 100, 100);

            // Act & Assert
            var exception = Record.Exception(() => Layout.LayoutChildIntoBoundingRegion(child, region));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests early return when child's parent is a Layout with UseCompatibilityMode set to false.
        /// This tests the not covered lines 200-202.
        /// Expected behavior: Method should return early without further processing.
        /// </summary>
        [Fact]
        public void LayoutChildIntoBoundingRegion_LayoutParentWithCompatibilityModeFalse_ReturnsEarly()
        {
            // Arrange
            var child = Substitute.For<VisualElement>();
            var layout = Substitute.For<Layout>();
            layout.UseCompatibilityMode.Returns(false);
            child.Parent.Returns(layout);
            var region = new Rect(10, 20, 100, 200);

            // Act
            Layout.LayoutChildIntoBoundingRegion(child, region);

            // Assert - if method returns early, Layout should not be called
            child.DidNotReceive().Layout(Arg.Any<Rect>());
        }

        /// <summary>
        /// Tests that when child is IView with Handler, Arrange is called and method returns early.
        /// Expected behavior: fe.Arrange should be called with the region.
        /// </summary>
        [Fact]
        public void LayoutChildIntoBoundingRegion_IViewWithHandler_CallsArrangeAndReturns()
        {
            // Arrange
            var child = Substitute.For<VisualElement, IView>();
            var handler = Substitute.For<IViewHandler>();
            ((IView)child).Handler.Returns(handler);
            var region = new Rect(10, 20, 100, 200);

            // Act
            Layout.LayoutChildIntoBoundingRegion(child, region);

            // Assert
            ((IView)child).Received(1).Arrange(region);
            child.DidNotReceive().Layout(Arg.Any<Rect>());
        }

        /// <summary>
        /// Tests that when child is IView without Handler, continues to legacy layout logic.
        /// Expected behavior: Should not call Arrange, should continue processing.
        /// </summary>
        [Fact]
        public void LayoutChildIntoBoundingRegion_IViewWithoutHandler_ContinuesToLegacyLogic()
        {
            // Arrange
            var child = Substitute.For<VisualElement, IView>();
            ((IView)child).Handler.Returns((IViewHandler)null);
            var region = new Rect(10, 20, 100, 200);

            // Act
            Layout.LayoutChildIntoBoundingRegion(child, region);

            // Assert
            ((IView)child).DidNotReceive().Arrange(Arg.Any<Rect>());
            child.Received(1).Layout(Arg.Any<Rect>());
        }

        /// <summary>
        /// Tests early Layout call when child is not a View.
        /// This tests the not covered lines 221-224.
        /// Expected behavior: Should call child.Layout directly with region and return.
        /// </summary>
        [Fact]
        public void LayoutChildIntoBoundingRegion_ChildNotView_CallsLayoutDirectly()
        {
            // Arrange
            var child = Substitute.For<VisualElement>();
            var region = new Rect(10, 20, 100, 200);

            // Act
            Layout.LayoutChildIntoBoundingRegion(child, region);

            // Assert
            child.Received(1).Layout(region);
        }

        /// <summary>
        /// Tests RTL layout adjustment when parent supports flow direction and is RTL.
        /// Expected behavior: Region should be adjusted for right-to-left layout.
        /// </summary>
        [Theory]
        [InlineData(300, 10, 100, 50, 190, 10, 100, 50)] // parent.Width - region.Right = 300 - 110 = 190
        [InlineData(400, 20, 80, 60, 300, 20, 80, 60)]   // parent.Width - region.Right = 400 - 100 = 300
        public void LayoutChildIntoBoundingRegion_RTLLayout_AdjutsRegion(
            double parentWidth, double regionX, double regionWidth, double regionHeight,
            double expectedX, double expectedY, double expectedWidth, double expectedHeight)
        {
            // Arrange
            var view = CreateMockView(LayoutOptions.Fill, LayoutOptions.Fill, new Thickness(0));
            var parent = Substitute.For<IFlowDirectionController>();
            parent.Width.Returns(parentWidth);
            parent.ApplyEffectiveFlowDirectionToChildContainer.Returns(true);
            parent.EffectiveFlowDirection.Returns(EffectiveFlowDirection.RightToLeft);
            view.Parent.Returns(parent);

            var region = new Rect(regionX, 10, regionWidth, regionHeight);
            var capturedRegion = default(Rect);
            view.When(x => x.Layout(Arg.Any<Rect>())).Do(x => capturedRegion = x.Arg<Rect>());

            // Act
            Layout.LayoutChildIntoBoundingRegion(view, region);

            // Assert
            Assert.Equal(expectedX, capturedRegion.X, 1);
            Assert.Equal(expectedY, capturedRegion.Y, 1);
            Assert.Equal(expectedWidth, capturedRegion.Width, 1);
            Assert.Equal(expectedHeight, capturedRegion.Height, 1);
        }

        /// <summary>
        /// Tests that RTL adjustment is not applied when region.X equals parent.Width - region.Right.
        /// Expected behavior: Region should not be modified.
        /// </summary>
        [Fact]
        public void LayoutChildIntoBoundingRegion_RTLLayoutEqualCondition_NoAdjustment()
        {
            // Arrange
            var view = CreateMockView(LayoutOptions.Fill, LayoutOptions.Fill, new Thickness(0));
            var parent = Substitute.For<IFlowDirectionController>();
            parent.Width.Returns(200);
            parent.ApplyEffectiveFlowDirectionToChildContainer.Returns(true);
            parent.EffectiveFlowDirection.Returns(EffectiveFlowDirection.RightToLeft);
            view.Parent.Returns(parent);

            // Set region where parent.Width - region.Right == region.X
            // parent.Width = 200, region.Right = 150, so parent.Width - region.Right = 50
            // region.X should be 50 for no adjustment
            var region = new Rect(50, 10, 100, 80);
            var capturedRegion = default(Rect);
            view.When(x => x.Layout(Arg.Any<Rect>())).Do(x => capturedRegion = x.Arg<Rect>());

            // Act
            Layout.LayoutChildIntoBoundingRegion(view, region);

            // Assert - region should be unchanged (except for margins which are 0)
            Assert.Equal(50, capturedRegion.X, 1);
            Assert.Equal(10, capturedRegion.Y, 1);
            Assert.Equal(100, capturedRegion.Width, 1);
            Assert.Equal(80, capturedRegion.Height, 1);
        }

        /// <summary>
        /// Tests horizontal alignment options and their effect on positioning.
        /// Expected behavior: Different alignments should adjust X position and width differently.
        /// </summary>
        [Theory]
        [InlineData(LayoutAlignment.Fill, 10, 100)]     // Fill: no adjustment
        [InlineData(LayoutAlignment.Start, 10, 50)]     // Start: X unchanged, width reduced
        [InlineData(LayoutAlignment.Center, 35, 50)]    // Center: X moved right, width reduced  
        [InlineData(LayoutAlignment.End, 60, 50)]       // End: X moved to right, width reduced
        public void LayoutChildIntoBoundingRegion_HorizontalAlignment_PositionsCorrectly(
            LayoutAlignment alignment, double expectedX, double expectedWidth)
        {
            // Arrange
            var horizontalOptions = new LayoutOptions(alignment, false);
            var view = CreateMockView(horizontalOptions, LayoutOptions.Fill, new Thickness(0));
            var mockRequest = new SizeRequest(new Size(50, 30), new Size(25, 15));
            view.Measure(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<MeasureFlags>()).Returns(mockRequest);

            var region = new Rect(10, 20, 100, 80);
            var capturedRegion = default(Rect);
            view.When(x => x.Layout(Arg.Any<Rect>())).Do(x => capturedRegion = x.Arg<Rect>());

            // Act
            Layout.LayoutChildIntoBoundingRegion(view, region);

            // Assert
            Assert.Equal(expectedX, capturedRegion.X, 1);
            Assert.Equal(expectedWidth, capturedRegion.Width, 1);
        }

        /// <summary>
        /// Tests vertical alignment options and their effect on positioning.
        /// Expected behavior: Different alignments should adjust Y position and height differently.
        /// </summary>
        [Theory]
        [InlineData(LayoutAlignment.Fill, 20, 80)]      // Fill: no adjustment
        [InlineData(LayoutAlignment.Start, 20, 30)]     // Start: Y unchanged, height reduced
        [InlineData(LayoutAlignment.Center, 45, 30)]    // Center: Y moved down, height reduced
        [InlineData(LayoutAlignment.End, 70, 30)]       // End: Y moved to bottom, height reduced
        public void LayoutChildIntoBoundingRegion_VerticalAlignment_PositionsCorrectly(
            LayoutAlignment alignment, double expectedY, double expectedHeight)
        {
            // Arrange
            var verticalOptions = new LayoutOptions(alignment, false);
            var view = CreateMockView(LayoutOptions.Fill, verticalOptions, new Thickness(0));
            var mockRequest = new SizeRequest(new Size(50, 30), new Size(25, 15));
            view.Measure(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<MeasureFlags>()).Returns(mockRequest);

            var region = new Rect(10, 20, 100, 80);
            var capturedRegion = default(Rect);
            view.When(x => x.Layout(Arg.Any<Rect>())).Do(x => capturedRegion = x.Arg<Rect>());

            // Act
            Layout.LayoutChildIntoBoundingRegion(view, region);

            // Assert
            Assert.Equal(expectedY, capturedRegion.Y, 1);
            Assert.Equal(expectedHeight, capturedRegion.Height, 1);
        }

        /// <summary>
        /// Tests margin application to the final layout region.
        /// Expected behavior: Margins should reduce the available space and offset position.
        /// </summary>
        [Theory]
        [InlineData(5, 10, 15, 20, 15, 30, 80, 50)]     // Left=5, Top=10, Right=15, Bottom=20
        [InlineData(0, 0, 0, 0, 10, 20, 100, 80)]       // No margins
        [InlineData(10, 10, 10, 10, 20, 30, 80, 60)]    // Equal margins
        public void LayoutChildIntoBoundingRegion_Margins_AppliedCorrectly(
            double left, double top, double right, double bottom,
            double expectedX, double expectedY, double expectedWidth, double expectedHeight)
        {
            // Arrange
            var margin = new Thickness(left, top, right, bottom);
            var view = CreateMockView(LayoutOptions.Fill, LayoutOptions.Fill, margin);

            var region = new Rect(10, 20, 100, 80);
            var capturedRegion = default(Rect);
            view.When(x => x.Layout(Arg.Any<Rect>())).Do(x => capturedRegion = x.Arg<Rect>());

            // Act
            Layout.LayoutChildIntoBoundingRegion(view, region);

            // Assert
            Assert.Equal(expectedX, capturedRegion.X, 1);
            Assert.Equal(expectedY, capturedRegion.Y, 1);
            Assert.Equal(expectedWidth, capturedRegion.Width, 1);
            Assert.Equal(expectedHeight, capturedRegion.Height, 1);
        }

        /// <summary>
        /// Tests RTL adjustment combined with horizontal alignment.
        /// Expected behavior: RTL adjustment should be applied with reversed horizontal alignment.
        /// </summary>
        [Theory]
        [InlineData(LayoutAlignment.Start, 1.0)]    // Start becomes End (1.0) in RTL
        [InlineData(LayoutAlignment.Center, 0.5)]   // Center remains Center (0.5) in RTL  
        [InlineData(LayoutAlignment.End, 0.0)]      // End becomes Start (0.0) in RTL
        public void LayoutChildIntoBoundingRegion_RTLWithHorizontalAlignment_ReversesAlignment(
            LayoutAlignment alignment, double expectedReversedAlign)
        {
            // Arrange
            var horizontalOptions = new LayoutOptions(alignment, false);
            var view = CreateMockView(horizontalOptions, LayoutOptions.Fill, new Thickness(0));
            var parent = Substitute.For<IFlowDirectionController>();
            parent.Width.Returns(300);
            parent.ApplyEffectiveFlowDirectionToChildContainer.Returns(true);
            parent.EffectiveFlowDirection.Returns(EffectiveFlowDirection.RightToLeft);
            view.Parent.Returns(parent);

            var mockRequest = new SizeRequest(new Size(50, 30), new Size(25, 15));
            view.Measure(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<MeasureFlags>()).Returns(mockRequest);

            var region = new Rect(10, 20, 100, 80);
            var capturedRegion = default(Rect);
            view.When(x => x.Layout(Arg.Any<Rect>())).Do(x => capturedRegion = x.Arg<Rect>());

            // Act
            Layout.LayoutChildIntoBoundingRegion(view, region);

            // Assert
            // The region should be adjusted for RTL first, then alignment applied
            // RTL adjustment: new Rect(300 - (10 + 100), 20, 100, 80) = new Rect(190, 20, 100, 80)
            // Then horizontal alignment with reversed multiplier
            var diff = Math.Max(0, 100 - 50); // 50
            var expectedX = 190 + (int)(diff * expectedReversedAlign);
            Assert.Equal(expectedX, capturedRegion.X, 1);
        }

        /// <summary>
        /// Tests boundary values for region dimensions.
        /// Expected behavior: Method should handle zero, negative, and very large values gracefully.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0)]                           // Zero dimensions
        [InlineData(-10, -20, 50, 60)]                     // Negative position
        [InlineData(10, 20, double.MaxValue, double.MaxValue)] // Very large dimensions
        public void LayoutChildIntoBoundingRegion_BoundaryValues_HandledGracefully(
            double x, double y, double width, double height)
        {
            // Arrange
            var view = CreateMockView(LayoutOptions.Fill, LayoutOptions.Fill, new Thickness(0));
            var region = new Rect(x, y, width, height);

            // Act & Assert - Should not throw
            var exception = Record.Exception(() => Layout.LayoutChildIntoBoundingRegion(view, region));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that parent not implementing IFlowDirectionController skips RTL logic.
        /// Expected behavior: No RTL adjustment should be applied.
        /// </summary>
        [Fact]
        public void LayoutChildIntoBoundingRegion_ParentNotIFlowDirectionController_SkipsRTL()
        {
            // Arrange
            var view = CreateMockView(LayoutOptions.Fill, LayoutOptions.Fill, new Thickness(0));
            var parent = Substitute.For<Element>(); // Not IFlowDirectionController
            view.Parent.Returns(parent);

            var region = new Rect(10, 20, 100, 80);
            var capturedRegion = default(Rect);
            view.When(x => x.Layout(Arg.Any<Rect>())).Do(x => capturedRegion = x.Arg<Rect>());

            // Act
            Layout.LayoutChildIntoBoundingRegion(view, region);

            // Assert - region should be unchanged (no RTL adjustment)
            Assert.Equal(10, capturedRegion.X, 1);
            Assert.Equal(20, capturedRegion.Y, 1);
            Assert.Equal(100, capturedRegion.Width, 1);
            Assert.Equal(80, capturedRegion.Height, 1);
        }

        private static View CreateMockView(LayoutOptions horizontalOptions, LayoutOptions verticalOptions, Thickness margin)
        {
            var view = Substitute.For<View>();
            view.HorizontalOptions.Returns(horizontalOptions);
            view.VerticalOptions.Returns(verticalOptions);
            view.Margin.Returns(margin);
            return view;
        }
    }


    public partial class LayoutGenericTests
    {
        /// <summary>
        /// Tests that the Layout&lt;T&gt; constructor properly initializes the _children field with a new ElementCollection&lt;T&gt;
        /// using the InternalChildren property as the backing collection.
        /// Expected result: The Children property should return a non-null collection that is properly linked to InternalChildren.
        /// </summary>
        [Fact]
        public void Constructor_InitializesChildrenCollection_Successfully()
        {
            // Arrange & Act
            var layout = new TestLayout<View>();

            // Assert
            Assert.NotNull(layout.Children);
            Assert.IsType<ElementCollection<View>>(layout.GetChildrenField());
        }

        /// <summary>
        /// Tests that the Layout&lt;T&gt; constructor creates a Children collection that is properly linked to the InternalChildren property.
        /// Expected result: Adding items to Children should reflect in InternalChildren and vice versa.
        /// </summary>
        [Fact]
        public void Constructor_ChildrenCollectionLinkedToInternalChildren_Successfully()
        {
            // Arrange
            var layout = new TestLayout<View>();
            var view = new TestView();

            // Act
            layout.Children.Add(view);

            // Assert
            Assert.Single(layout.InternalChildren);
            Assert.Contains(view, layout.InternalChildren);
        }

        /// <summary>
        /// Tests that the Layout&lt;T&gt; constructor works correctly with different generic type parameters that inherit from View.
        /// Expected result: The constructor should successfully initialize regardless of the specific View subtype used.
        /// </summary>
        [Theory]
        [InlineData(typeof(TestView))]
        [InlineData(typeof(TestLabel))]
        [InlineData(typeof(TestButton))]
        public void Constructor_WithDifferentGenericTypes_InitializesSuccessfully(Type viewType)
        {
            // Arrange & Act
            var layoutType = typeof(TestLayout<>).MakeGenericType(viewType);
            var layout = (IViewContainer<View>)Activator.CreateInstance(layoutType);

            // Assert
            Assert.NotNull(layout.Children);
            Assert.Empty(layout.Children);
        }

        /// <summary>
        /// Tests that the Layout&lt;T&gt; constructor initializes an empty collection initially.
        /// Expected result: The Children collection should be empty immediately after construction.
        /// </summary>
        [Fact]
        public void Constructor_InitializesEmptyCollection_Successfully()
        {
            // Arrange & Act
            var layout = new TestLayout<View>();

            // Assert
            Assert.Empty(layout.Children);
            Assert.Empty(layout.InternalChildren);
        }

        /// <summary>
        /// Tests that multiple instances of Layout&lt;T&gt; have independent Children collections.
        /// Expected result: Each layout instance should have its own separate Children collection.
        /// </summary>
        [Fact]
        public void Constructor_MultipleInstances_HaveIndependentCollections()
        {
            // Arrange & Act
            var layout1 = new TestLayout<View>();
            var layout2 = new TestLayout<View>();
            var view = new TestView();

            layout1.Children.Add(view);

            // Assert
            Assert.Single(layout1.Children);
            Assert.Empty(layout2.Children);
            Assert.NotSame(layout1.Children, layout2.Children);
        }

        private class TestLayout<T> : Layout<T> where T : View
        {
            public ElementCollection<T> GetChildrenField() => _children;

            protected override void LayoutChildren(double x, double y, double width, double height)
            {
                // Test implementation - no layout logic needed
            }

            protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
            {
                return new SizeRequest(new Size(100, 100));
            }
        }

        private class TestView : View
        {
        }

        private class TestLabel : View
        {
        }

        private class TestButton : View
        {
        }
    }
}