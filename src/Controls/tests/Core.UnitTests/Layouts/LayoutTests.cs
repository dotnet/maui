#nullable disable

#nullable enable
using System;
using System.Collections.Generic;


using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Layouts;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
    [Category("Layout")]
    public class LayoutTests : BaseTestFixture
    {
        [Fact]
        public void UsingIndexUpdatesParent()
        {
            var layout = new VerticalStackLayout();

            var child0 = new Button();
            var child1 = new Button();

            Assert.Null(child0.Parent);
            Assert.Null(child1.Parent);

            layout.Add(child0);

            Assert.Equal(layout, child0.Parent);
            Assert.Null(child1.Parent);

            layout[0] = child1;

            Assert.Null(child0.Parent);
            Assert.Equal(layout, child1.Parent);
        }

        [Fact]
        public void ClearUpdatesParent()
        {
            var layout = new VerticalStackLayout();

            var child0 = new Button();
            var child1 = new Button();

            layout.Add(child0);
            layout.Add(child1);

            Assert.Equal(layout, child0.Parent);
            Assert.Equal(layout, child1.Parent);

            layout.Clear();

            Assert.Null(child0.Parent);
            Assert.Null(child1.Parent);
        }

        [Theory]
        [InlineData(typeof(VerticalStackLayout))]
        [InlineData(typeof(HorizontalStackLayout))]
        [InlineData(typeof(Grid))]
        [InlineData(typeof(StackLayout))]
        public void AddCallsCorrectHandlerMethod(Type TLayout)
        {
            var layout = CreateLayout(TLayout);
            var handler = Substitute.For<ILayoutHandler>();
            layout.Handler = handler;

            var child0 = new Button();
            layout.Add(child0);

            var command = Arg.Is(nameof(ILayoutHandler.Add));
            var args = Arg.Is<LayoutHandlerUpdate>(lhu => lhu.Index == 0 && lhu.View == child0);

            handler.Received().Invoke(command, args);
        }

        [Theory]
        [InlineData(typeof(VerticalStackLayout))]
        [InlineData(typeof(HorizontalStackLayout))]
        [InlineData(typeof(Grid))]
        [InlineData(typeof(StackLayout))]
        public void RemoveCallsCorrectHandlerMethod(Type TLayout)
        {
            var layout = CreateLayout(TLayout);
            var handler = Substitute.For<ILayoutHandler>();
            layout.Handler = handler;

            var child0 = new Button();
            layout.Add(child0);
            layout.Remove(child0);

            var command = Arg.Is(nameof(ILayoutHandler.Remove));
            var args = Arg.Is<LayoutHandlerUpdate>(lhu => lhu.Index == 0 && lhu.View == child0);

            handler.Received().Invoke(command, args);
        }

        [Theory]
        [InlineData(typeof(VerticalStackLayout))]
        [InlineData(typeof(HorizontalStackLayout))]
        [InlineData(typeof(Grid))]
        [InlineData(typeof(StackLayout))]
        public void InsertCallsCorrectHandlerMethod(Type TLayout)
        {
            var events = new List<(string Name, LayoutHandlerUpdate? Args)>();

            var layout = CreateLayout(TLayout);
            var handler = Substitute.For<ILayoutHandler>();
            layout.Handler = handler;

            var child0 = new Button();
            var child1 = new Button();
            var child2 = new Button();

            layout.Add(child0);
            layout.Add(child2);
            layout.Insert(1, child1);

            var command = Arg.Is(nameof(ILayoutHandler.Insert));
            var args = Arg.Is<LayoutHandlerUpdate>(lhu => lhu.Index == 1 && lhu.View == child1);

            handler.Received().Invoke(command, args);
        }

        static Layout CreateLayout(Type TLayout)
        {
            var layout = (Layout)Activator.CreateInstance(TLayout)!;
            return layout;
        }

        IMauiContext SetupContext(ILayoutManagerFactory layoutManagerFactory)
        {
            var services = Substitute.For<IServiceProvider>();
            services.GetService(Arg.Any<Type>()).Returns(layoutManagerFactory);
            var context = Substitute.For<IMauiContext>();
            context.Services.Returns(services);

            return context;
        }

        class AlternateLayoutManager : ILayoutManager
        {
            readonly double _width;
            readonly double _height;

            public AlternateLayoutManager(double width, double height)
            {
                _width = width;
                _height = height;
            }

            public Size ArrangeChildren(Rect bounds)
            {
                throw new NotImplementedException();
            }

            public Size Measure(double widthConstraint, double heightConstraint)
            {
                return new Size(_width, _height);
            }
        }

        [Fact]
        public void CanUseFactoryForAlternateManager()
        {
            var layoutManagerFactory = Substitute.For<Controls.ILayoutManagerFactory>();
            layoutManagerFactory.CreateLayoutManager(Arg.Any<Layout>()).Returns(new AlternateLayoutManager(8765, 4321));

            var context = SetupContext(layoutManagerFactory);

            var handler = Substitute.For<IViewHandler>();
            handler.MauiContext.Returns(context);

            var layout = new Grid
            {
                Handler = handler
            };

            var result = layout.CrossPlatformMeasure(100, 100);

            Assert.Equal(8765, result.Width);
            Assert.Equal(4321, result.Height);
        }

        class NullLayoutManagerFactory : Controls.ILayoutManagerFactory
        {
            public ILayoutManager? CreateLayoutManager(Layout layout)
            {
                return null;
            }
        }

        [Fact]
        public void FactoryCanPuntAndUseOriginalType()
        {
            var layoutManagerFactory = new NullLayoutManagerFactory();
            var context = SetupContext(layoutManagerFactory);

            var handler = Substitute.For<IViewHandler>();
            handler.MauiContext.Returns(context);

            var layout = new VerticalStackLayout();
            layout.Handler = handler;

            var view = new Label { Text = "a", WidthRequest = 100, HeightRequest = 100 };
            layout.Add(view);

            var result = layout.CrossPlatformMeasure(100, 100);

            Assert.Equal(0, result.Width);
            Assert.Equal(0, result.Height);
        }

        class ChoosyLayoutManagerFactory : Controls.ILayoutManagerFactory
        {
            public ILayoutManager? CreateLayoutManager(Layout layout)
            {
                if (layout is AbsoluteLayout)
                {
                    return new AlternateLayoutManager(1234, 1234);
                }

                return new AlternateLayoutManager(4567, 4567);
            }
        }

        [Fact]
        public void FactoryCanCustomizeBasedOnLayoutType()
        {
            var layoutManagerFactory = new ChoosyLayoutManagerFactory();
            var context = SetupContext(layoutManagerFactory);

            var handler = Substitute.For<IViewHandler>();
            handler.MauiContext.Returns(context);

            var absLayout = new AbsoluteLayout
            {
                Handler = handler
            };

            var view = new Label { Text = "a", WidthRequest = 100, HeightRequest = 100 };
            absLayout.Add(view);

            var absResult = absLayout.CrossPlatformMeasure(100, 100);

            Assert.Equal(1234, absResult.Width);
            Assert.Equal(1234, absResult.Height);

            var vsl = new VerticalStackLayout
            {
                Handler = handler // Obviously this wouldn't be okay in a real app, but it works well enough for this test
            };

            var vslResult = vsl.CrossPlatformMeasure(100, 100);

            Assert.Equal(4567, vslResult.Width);
            Assert.Equal(4567, vslResult.Height);
        }

        /// <summary>
        /// Tests that IsReadOnly property returns false for empty layout.
        /// Validates that the underlying List collection's IsReadOnly behavior is correctly exposed.
        /// </summary>
        [Theory]
        [InlineData(typeof(VerticalStackLayout))]
        [InlineData(typeof(HorizontalStackLayout))]
        [InlineData(typeof(Grid))]
        [InlineData(typeof(StackLayout))]
        public void IsReadOnly_EmptyLayout_ReturnsFalse(Type layoutType)
        {
            // Arrange
            var layout = CreateLayoutInstance(layoutType);

            // Act
            bool isReadOnly = layout.IsReadOnly;

            // Assert
            Assert.False(isReadOnly);
        }

        /// <summary>
        /// Tests that IsReadOnly property returns false for layout with children.
        /// Validates that the IsReadOnly property remains false regardless of collection contents.
        /// </summary>
        [Theory]
        [InlineData(typeof(VerticalStackLayout))]
        [InlineData(typeof(HorizontalStackLayout))]
        [InlineData(typeof(Grid))]
        [InlineData(typeof(StackLayout))]
        public void IsReadOnly_LayoutWithChildren_ReturnsFalse(Type layoutType)
        {
            // Arrange
            var layout = CreateLayoutInstance(layoutType);
            var child1 = Substitute.For<IView>();
            var child2 = Substitute.For<IView>();
            layout.Add(child1);
            layout.Add(child2);

            // Act
            bool isReadOnly = layout.IsReadOnly;

            // Assert
            Assert.False(isReadOnly);
            Assert.Equal(2, layout.Count);
        }

        /// <summary>
        /// Tests that IsReadOnly property remains consistent after modifications.
        /// Validates that adding and removing children does not affect the IsReadOnly state.
        /// </summary>
        [Fact]
        public void IsReadOnly_AfterAddAndRemoveOperations_RemainsFalse()
        {
            // Arrange
            var layout = new VerticalStackLayout();
            var child = Substitute.For<IView>();

            // Act & Assert - Initially false
            Assert.False(layout.IsReadOnly);

            // Add child
            layout.Add(child);
            Assert.False(layout.IsReadOnly);

            // Remove child
            layout.Remove(child);
            Assert.False(layout.IsReadOnly);

            // Clear all
            layout.Clear();
            Assert.False(layout.IsReadOnly);
        }

        /// <summary>
        /// Tests that IsReadOnly property is consistent across multiple calls.
        /// Validates that the property always returns the same value when called multiple times.
        /// </summary>
        [Fact]
        public void IsReadOnly_MultipleCalls_ReturnsConsistentValue()
        {
            // Arrange
            var layout = new Grid();

            // Act
            bool result1 = layout.IsReadOnly;
            bool result2 = layout.IsReadOnly;
            bool result3 = layout.IsReadOnly;

            // Assert
            Assert.False(result1);
            Assert.False(result2);
            Assert.False(result3);
            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        private static Layout CreateLayoutInstance(Type layoutType)
        {
            var layout = (Layout)Activator.CreateInstance(layoutType);
            return layout;
        }

        /// <summary>
        /// Tests that the IsClippedToBounds property returns the default value of false when not explicitly set.
        /// This validates the default behavior and ensures the getter works correctly.
        /// </summary>
        [Theory]
        [InlineData(typeof(VerticalStackLayout))]
        [InlineData(typeof(HorizontalStackLayout))]
        [InlineData(typeof(Grid))]
        [InlineData(typeof(StackLayout))]
        public void IsClippedToBounds_DefaultValue_ReturnsFalse(Type layoutType)
        {
            // Arrange
            var layout = CreateLayoutInstance(layoutType);

            // Act
            var result = layout.IsClippedToBounds;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that setting IsClippedToBounds to true correctly updates the property value.
        /// This validates the setter and getter functionality for the true state.
        /// </summary>
        [Theory]
        [InlineData(typeof(VerticalStackLayout))]
        [InlineData(typeof(HorizontalStackLayout))]
        [InlineData(typeof(Grid))]
        [InlineData(typeof(StackLayout))]
        public void IsClippedToBounds_SetToTrue_ReturnsTrue(Type layoutType)
        {
            // Arrange
            var layout = CreateLayoutInstance(layoutType);

            // Act
            layout.IsClippedToBounds = true;
            var result = layout.IsClippedToBounds;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that setting IsClippedToBounds to false correctly updates the property value.
        /// This validates the setter and getter functionality for the false state.
        /// </summary>
        [Theory]
        [InlineData(typeof(VerticalStackLayout))]
        [InlineData(typeof(HorizontalStackLayout))]
        [InlineData(typeof(Grid))]
        [InlineData(typeof(StackLayout))]
        public void IsClippedToBounds_SetToFalse_ReturnsFalse(Type layoutType)
        {
            // Arrange
            var layout = CreateLayoutInstance(layoutType);
            layout.IsClippedToBounds = true; // Set to true first

            // Act
            layout.IsClippedToBounds = false;
            var result = layout.IsClippedToBounds;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the IsClippedToBounds property correctly toggles between true and false values.
        /// This validates multiple state changes and ensures the property maintains its value correctly.
        /// </summary>
        [Theory]
        [InlineData(typeof(VerticalStackLayout))]
        [InlineData(typeof(HorizontalStackLayout))]
        [InlineData(typeof(Grid))]
        [InlineData(typeof(StackLayout))]
        public void IsClippedToBounds_ToggleValues_MaintainsCorrectState(Type layoutType)
        {
            // Arrange
            var layout = CreateLayoutInstance(layoutType);

            // Act & Assert - Initial state
            Assert.False(layout.IsClippedToBounds);

            // Act & Assert - Set to true
            layout.IsClippedToBounds = true;
            Assert.True(layout.IsClippedToBounds);

            // Act & Assert - Set back to false
            layout.IsClippedToBounds = false;
            Assert.False(layout.IsClippedToBounds);

            // Act & Assert - Set to true again
            layout.IsClippedToBounds = true;
            Assert.True(layout.IsClippedToBounds);
        }

        /// <summary>
        /// Tests that IndexOf returns the correct index for an existing item at the first position.
        /// </summary>
        [Fact]
        public void IndexOf_ItemAtFirstPosition_ReturnsZero()
        {
            // Arrange
            var layout = new VerticalStackLayout();
            var child = Substitute.For<IView>();
            layout.Add(child);

            // Act
            var result = layout.IndexOf(child);

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that IndexOf returns the correct index for an existing item at the last position.
        /// </summary>
        [Fact]
        public void IndexOf_ItemAtLastPosition_ReturnsCorrectIndex()
        {
            // Arrange
            var layout = new VerticalStackLayout();
            var child1 = Substitute.For<IView>();
            var child2 = Substitute.For<IView>();
            var child3 = Substitute.For<IView>();
            layout.Add(child1);
            layout.Add(child2);
            layout.Add(child3);

            // Act
            var result = layout.IndexOf(child3);

            // Assert
            Assert.Equal(2, result);
        }

        /// <summary>
        /// Tests that IndexOf returns the correct index for an existing item at a middle position.
        /// </summary>
        [Fact]
        public void IndexOf_ItemAtMiddlePosition_ReturnsCorrectIndex()
        {
            // Arrange
            var layout = new VerticalStackLayout();
            var child1 = Substitute.For<IView>();
            var child2 = Substitute.For<IView>();
            var child3 = Substitute.For<IView>();
            layout.Add(child1);
            layout.Add(child2);
            layout.Add(child3);

            // Act
            var result = layout.IndexOf(child2);

            // Assert
            Assert.Equal(1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns -1 for an item that does not exist in the layout.
        /// </summary>
        [Fact]
        public void IndexOf_ItemNotInLayout_ReturnsMinusOne()
        {
            // Arrange
            var layout = new VerticalStackLayout();
            var existingChild = Substitute.For<IView>();
            var nonExistentChild = Substitute.For<IView>();
            layout.Add(existingChild);

            // Act
            var result = layout.IndexOf(nonExistentChild);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when called on an empty layout.
        /// </summary>
        [Fact]
        public void IndexOf_EmptyLayout_ReturnsMinusOne()
        {
            // Arrange
            var layout = new VerticalStackLayout();
            var child = Substitute.For<IView>();

            // Act
            var result = layout.IndexOf(child);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when called with null parameter.
        /// </summary>
        [Fact]
        public void IndexOf_NullItem_ReturnsMinusOne()
        {
            // Arrange
            var layout = new VerticalStackLayout();
            var child = Substitute.For<IView>();
            layout.Add(child);

            // Act
            var result = layout.IndexOf(null);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf works correctly with different IView implementations.
        /// </summary>
        [Fact]
        public void IndexOf_WithDifferentIViewImplementations_ReturnsCorrectIndex()
        {
            // Arrange
            var layout = new VerticalStackLayout();
            var mockView = Substitute.For<IView>();
            var button = new Button();
            var label = new Label();

            layout.Add(mockView);
            layout.Add(button);
            layout.Add(label);

            // Act
            var mockViewIndex = layout.IndexOf(mockView);
            var buttonIndex = layout.IndexOf(button);
            var labelIndex = layout.IndexOf(label);

            // Assert
            Assert.Equal(0, mockViewIndex);
            Assert.Equal(1, buttonIndex);
            Assert.Equal(2, labelIndex);
        }

        /// <summary>
        /// Tests that IndexOf returns the index of the first occurrence when the same item is added multiple times.
        /// </summary>
        [Fact]
        public void IndexOf_ItemAddedMultipleTimes_ReturnsFirstOccurrenceIndex()
        {
            // Arrange
            var layout = new VerticalStackLayout();
            var child1 = Substitute.For<IView>();
            var child2 = Substitute.For<IView>();

            layout.Add(child1);
            layout.Add(child2);
            layout.Add(child1); // Adding the same child again

            // Act
            var result = layout.IndexOf(child1);

            // Assert
            Assert.Equal(0, result); // Should return first occurrence
        }

        /// <summary>
        /// Tests IndexOf behavior with various layout types to ensure consistent behavior across implementations.
        /// </summary>
        /// <param name="layoutType">The type of layout to test.</param>
        [Theory]
        [InlineData(typeof(VerticalStackLayout))]
        [InlineData(typeof(HorizontalStackLayout))]
        [InlineData(typeof(Grid))]
        [InlineData(typeof(StackLayout))]
        public void IndexOf_WithDifferentLayoutTypes_ReturnsCorrectIndex(Type layoutType)
        {
            // Arrange
            var layout = (Layout)Activator.CreateInstance(layoutType);
            var child1 = Substitute.For<IView>();
            var child2 = Substitute.For<IView>();
            var nonExistentChild = Substitute.For<IView>();

            layout.Add(child1);
            layout.Add(child2);

            // Act & Assert
            Assert.Equal(0, layout.IndexOf(child1));
            Assert.Equal(1, layout.IndexOf(child2));
            Assert.Equal(-1, layout.IndexOf(nonExistentChild));
        }

        /// <summary>
        /// Tests IndexOf with a single item layout scenario.
        /// </summary>
        [Fact]
        public void IndexOf_SingleItemLayout_ReturnsZeroForExistingItem()
        {
            // Arrange
            var layout = new VerticalStackLayout();
            var child = new Button();
            layout.Add(child);

            // Act
            var existingItemIndex = layout.IndexOf(child);
            var nonExistentItemIndex = layout.IndexOf(new Label());

            // Assert
            Assert.Equal(0, existingItemIndex);
            Assert.Equal(-1, nonExistentItemIndex);
        }

        /// <summary>
        /// Tests IndexOf after removing and re-adding items to ensure correct index tracking.
        /// </summary>
        [Fact]
        public void IndexOf_AfterRemoveAndReAdd_ReturnsCorrectIndex()
        {
            // Arrange
            var layout = new VerticalStackLayout();
            var child1 = Substitute.For<IView>();
            var child2 = Substitute.For<IView>();
            var child3 = Substitute.For<IView>();

            layout.Add(child1);
            layout.Add(child2);
            layout.Add(child3);

            // Act - Remove middle item and re-add at end
            layout.Remove(child2);
            layout.Add(child2);

            var child1Index = layout.IndexOf(child1);
            var child2Index = layout.IndexOf(child2);
            var child3Index = layout.IndexOf(child3);

            // Assert
            Assert.Equal(0, child1Index);
            Assert.Equal(2, child2Index); // Now at end
            Assert.Equal(1, child3Index); // Moved up
        }

        /// <summary>
        /// Tests that RemoveAt returns early without throwing when index is greater than or equal to Count.
        /// This tests the boundary condition where index >= Count should result in early return.
        /// </summary>
        [Theory]
        [InlineData(typeof(VerticalStackLayout))]
        [InlineData(typeof(HorizontalStackLayout))]
        [InlineData(typeof(Grid))]
        [InlineData(typeof(StackLayout))]
        public void RemoveAt_IndexGreaterThanOrEqualToCount_ReturnsWithoutException(Type layoutType)
        {
            // Arrange
            var layout = CreateLayout(layoutType);
            var handler = Substitute.For<ILayoutHandler>();
            layout.Handler = handler;

            var child = new Button();
            layout.Add(child);

            // Act & Assert - should not throw for index == Count
            layout.RemoveAt(layout.Count);

            // Assert - child should still be present
            Assert.Equal(1, layout.Count);
            Assert.Contains(child, layout);

            // Handler should not be called for out-of-bounds removal
            handler.DidNotReceive().Invoke(Arg.Any<string>(), Arg.Any<LayoutHandlerUpdate>());
        }

        /// <summary>
        /// Tests that RemoveAt returns early without throwing when index is much greater than Count.
        /// This tests the boundary condition where index >> Count should result in early return.
        /// </summary>
        [Theory]
        [InlineData(typeof(VerticalStackLayout))]
        [InlineData(typeof(HorizontalStackLayout))]
        [InlineData(typeof(Grid))]
        [InlineData(typeof(StackLayout))]
        public void RemoveAt_IndexMuchGreaterThanCount_ReturnsWithoutException(Type layoutType)
        {
            // Arrange
            var layout = CreateLayout(layoutType);
            var handler = Substitute.For<ILayoutHandler>();
            layout.Handler = handler;

            // Act & Assert - should not throw for index much greater than Count
            layout.RemoveAt(100);

            // Assert - layout should remain empty
            Assert.Equal(0, layout.Count);

            // Handler should not be called for out-of-bounds removal
            handler.DidNotReceive().Invoke(Arg.Any<string>(), Arg.Any<LayoutHandlerUpdate>());
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException when index is negative.
        /// This tests the boundary condition where negative indices should throw an exception.
        /// </summary>
        [Theory]
        [InlineData(typeof(VerticalStackLayout))]
        [InlineData(typeof(HorizontalStackLayout))]
        [InlineData(typeof(Grid))]
        [InlineData(typeof(StackLayout))]
        public void RemoveAt_NegativeIndex_ThrowsArgumentOutOfRangeException(Type layoutType)
        {
            // Arrange
            var layout = CreateLayout(layoutType);
            var child = new Button();
            layout.Add(child);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => layout.RemoveAt(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => layout.RemoveAt(int.MinValue));

            // Assert - child should still be present
            Assert.Equal(1, layout.Count);
            Assert.Contains(child, layout);
        }

        /// <summary>
        /// Tests that RemoveAt successfully removes child at valid index and calls handler.
        /// This tests the normal case where a valid index should remove the child and notify handlers.
        /// </summary>
        [Theory]
        [InlineData(typeof(VerticalStackLayout))]
        [InlineData(typeof(HorizontalStackLayout))]
        [InlineData(typeof(Grid))]
        [InlineData(typeof(StackLayout))]
        public void RemoveAt_ValidIndex_RemovesChildAndCallsHandler(Type layoutType)
        {
            // Arrange
            var layout = CreateLayout(layoutType);
            var handler = Substitute.For<ILayoutHandler>();
            layout.Handler = handler;

            var child0 = new Button();
            var child1 = new Button();
            layout.Add(child0);
            layout.Add(child1);

            // Act
            layout.RemoveAt(0);

            // Assert
            Assert.Equal(1, layout.Count);
            Assert.DoesNotContain(child0, layout);
            Assert.Contains(child1, layout);
            Assert.Equal(child1, layout[0]);

            // Handler should be called with correct parameters
            var command = Arg.Is(nameof(ILayoutHandler.Remove));
            var args = Arg.Is<LayoutHandlerUpdate>(lhu => lhu.Index == 0 && lhu.View == child0);
            handler.Received().Invoke(command, args);
        }

        /// <summary>
        /// Tests that RemoveAt removes logical child relationship when child is an Element.
        /// This tests that Element children have their parent relationship properly cleared.
        /// </summary>
        [Theory]
        [InlineData(typeof(VerticalStackLayout))]
        [InlineData(typeof(HorizontalStackLayout))]
        [InlineData(typeof(Grid))]
        [InlineData(typeof(StackLayout))]
        public void RemoveAt_ElementChild_RemovesLogicalChildRelationship(Type layoutType)
        {
            // Arrange
            var layout = CreateLayout(layoutType);
            var elementChild = new Button();
            layout.Add(elementChild);

            // Verify parent is set
            Assert.Equal(layout, elementChild.Parent);

            // Act
            layout.RemoveAt(0);

            // Assert
            Assert.Null(elementChild.Parent);
            Assert.Equal(0, layout.Count);
        }

        /// <summary>
        /// Tests that RemoveAt works correctly with non-Element IView children.
        /// This tests that non-Element IView children are removed without trying to manage logical relationships.
        /// </summary>
        [Theory]
        [InlineData(typeof(VerticalStackLayout))]
        [InlineData(typeof(HorizontalStackLayout))]
        [InlineData(typeof(Grid))]
        [InlineData(typeof(StackLayout))]
        public void RemoveAt_NonElementChild_RemovesSuccessfully(Type layoutType)
        {
            // Arrange
            var layout = CreateLayout(layoutType);
            var handler = Substitute.For<ILayoutHandler>();
            layout.Handler = handler;

            var nonElementChild = Substitute.For<IView>();
            layout.Add(nonElementChild);

            // Act
            layout.RemoveAt(0);

            // Assert
            Assert.Equal(0, layout.Count);
            Assert.DoesNotContain(nonElementChild, layout);

            // Handler should be called
            var command = Arg.Is(nameof(ILayoutHandler.Remove));
            var args = Arg.Is<LayoutHandlerUpdate>(lhu => lhu.Index == 0 && lhu.View == nonElementChild);
            handler.Received().Invoke(command, args);
        }

        /// <summary>
        /// Tests RemoveAt boundary cases with first and last indices.
        /// This tests boundary conditions at the edges of valid index ranges.
        /// </summary>
        [Theory]
        [InlineData(typeof(VerticalStackLayout))]
        [InlineData(typeof(HorizontalStackLayout))]
        [InlineData(typeof(Grid))]
        [InlineData(typeof(StackLayout))]
        public void RemoveAt_BoundaryIndices_WorksCorrectly(Type layoutType)
        {
            // Arrange
            var layout = CreateLayout(layoutType);
            var child0 = new Button();
            var child1 = new Button();
            var child2 = new Button();

            layout.Add(child0);
            layout.Add(child1);
            layout.Add(child2);

            // Test removing last index (Count - 1)
            layout.RemoveAt(layout.Count - 1);
            Assert.Equal(2, layout.Count);
            Assert.DoesNotContain(child2, layout);

            // Test removing first index (0)
            layout.RemoveAt(0);
            Assert.Equal(1, layout.Count);
            Assert.DoesNotContain(child0, layout);
            Assert.Contains(child1, layout);
            Assert.Equal(child1, layout[0]);
        }

        /// <summary>
        /// Tests RemoveAt on empty layout returns without exception.
        /// This tests the edge case where RemoveAt is called on an empty collection.
        /// </summary>
        [Theory]
        [InlineData(typeof(VerticalStackLayout))]
        [InlineData(typeof(HorizontalStackLayout))]
        [InlineData(typeof(Grid))]
        [InlineData(typeof(StackLayout))]
        public void RemoveAt_EmptyLayout_ReturnsWithoutException(Type layoutType)
        {
            // Arrange
            var layout = CreateLayout(layoutType);
            var handler = Substitute.For<ILayoutHandler>();
            layout.Handler = handler;

            // Act & Assert - should not throw for empty layout
            layout.RemoveAt(0);
            layout.RemoveAt(10);

            // Assert - layout should remain empty
            Assert.Equal(0, layout.Count);

            // Handler should not be called
            handler.DidNotReceive().Invoke(Arg.Any<string>(), Arg.Any<LayoutHandlerUpdate>());
        }

    }
}

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public partial class LayoutContainsTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that Contains returns false when called with a null item.
        /// </summary>
        [Fact]
        public void Contains_NullItem_ReturnsFalse()
        {
            // Arrange
            var layout = new VerticalStackLayout();

            // Act
            var result = layout.Contains(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns false when the layout is empty.
        /// </summary>
        [Fact]
        public void Contains_EmptyLayout_ReturnsFalse()
        {
            // Arrange
            var layout = new VerticalStackLayout();
            var item = Substitute.For<IView>();

            // Act
            var result = layout.Contains(item);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns true when the item exists in the layout.
        /// </summary>
        [Fact]
        public void Contains_ExistingItem_ReturnsTrue()
        {
            // Arrange
            var layout = new VerticalStackLayout();
            var item = Substitute.For<IView>();
            layout.Add(item);

            // Act
            var result = layout.Contains(item);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when the item does not exist in the layout.
        /// </summary>
        [Fact]
        public void Contains_NonExistingItem_ReturnsFalse()
        {
            // Arrange
            var layout = new VerticalStackLayout();
            var existingItem = Substitute.For<IView>();
            var nonExistingItem = Substitute.For<IView>();
            layout.Add(existingItem);

            // Act
            var result = layout.Contains(nonExistingItem);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests Contains behavior with multiple items in various scenarios.
        /// </summary>
        [Theory]
        [InlineData(0, true)]  // First item
        [InlineData(1, true)]  // Middle item
        [InlineData(2, true)]  // Last item
        public void Contains_MultipleItems_ReturnsCorrectResult(int itemIndex, bool expectedResult)
        {
            // Arrange
            var layout = new VerticalStackLayout();
            var items = new[]
            {
                Substitute.For<IView>(),
                Substitute.For<IView>(),
                Substitute.For<IView>()
            };

            foreach (var item in items)
            {
                layout.Add(item);
            }

            var testItem = items[itemIndex];

            // Act
            var result = layout.Contains(testItem);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that Contains returns false for an item that was previously in the layout but has been removed.
        /// </summary>
        [Fact]
        public void Contains_RemovedItem_ReturnsFalse()
        {
            // Arrange
            var layout = new VerticalStackLayout();
            var item = Substitute.For<IView>();
            layout.Add(item);
            layout.Remove(item);

            // Act
            var result = layout.Contains(item);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests Contains with different types of IView implementations.
        /// </summary>
        [Theory]
        [InlineData(typeof(Label))]
        [InlineData(typeof(Button))]
        public void Contains_DifferentViewTypes_ReturnsTrue(Type viewType)
        {
            // Arrange
            var layout = new VerticalStackLayout();
            var item = (IView)Activator.CreateInstance(viewType);
            layout.Add(item);

            // Act
            var result = layout.Contains(item);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains works correctly after clearing and re-adding items.
        /// </summary>
        [Fact]
        public void Contains_AfterClearAndReAdd_ReturnsCorrectResult()
        {
            // Arrange
            var layout = new VerticalStackLayout();
            var item = Substitute.For<IView>();
            layout.Add(item);
            layout.Clear();

            // Act - Check that item is not contained after clear
            var resultAfterClear = layout.Contains(item);

            // Re-add and check again
            layout.Add(item);
            var resultAfterReAdd = layout.Contains(item);

            // Assert
            Assert.False(resultAfterClear);
            Assert.True(resultAfterReAdd);
        }
    }
}