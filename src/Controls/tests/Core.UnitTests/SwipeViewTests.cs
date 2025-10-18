#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class SwipeViewTests : BaseTestFixture
    {
        [Fact]
        public void TestConstructor()
        {
            var swipeView = new SwipeView();

            Assert.Empty(swipeView.LeftItems);
            Assert.Empty(swipeView.TopItems);
            Assert.Empty(swipeView.RightItems);
            Assert.Empty(swipeView.BottomItems);
        }

        [Fact]
        public void TestSwipeViewBindingContextChangedEvent()
        {
            var swipeView = new SwipeView();
            bool passed = false;
            swipeView.BindingContextChanged += (sender, args) => passed = true;

            swipeView.BindingContext = new object();

            if (!passed)
            {
                throw new XunitException("The BindingContextChanged event was not fired.");
            }
        }

        [Fact]
        public void TestContentBindingContextChangedEvent()
        {
            var content = new Label();
            var swipeView = new SwipeView
            {
                Content = content
            };

            bool passed = false;
            content.BindingContextChanged += (sender, args) => passed = true;

            swipeView.BindingContext = new object();

            if (!passed)
            {
                throw new XunitException("The BindingContextChanged event was not fired.");
            }
        }

        [Fact]
        public void TestTemplatedContentBindingContextChangedEvent()
        {
            var content = new Label();
            var swipeView = new SwipeView();

            swipeView.ControlTemplate = new ControlTemplate(() => content);

            bool passed = false;
            content.BindingContextChanged += (sender, args) => passed = true;

            swipeView.BindingContext = new object();

            if (!passed)
            {
                throw new XunitException("The BindingContextChanged event was not fired.");
            }
        }

        [Fact]
        public void ClearRemovesLogicalChildren()
        {
            var swipeView = new SwipeView();

            swipeView.LeftItems = new SwipeItems
            {
                new SwipeItem(),
                new SwipeItem(),
                new SwipeItem(),
                new SwipeItem(),
                new SwipeItem()
            };

            swipeView.LeftItems.Clear();
            Assert.Empty((swipeView.LeftItems as IVisualTreeElement).GetVisualChildren());
        }

        [Fact]
        public void TestContentBindingContextPropagatesToNewSwipeItems()
        {
            var swipeView = new SwipeView();

            swipeView.LeftItems = new SwipeItems
            {
                new SwipeItem()
            };
            swipeView.RightItems = new SwipeItems
            {
                new SwipeItem()
            };
            swipeView.TopItems = new SwipeItems
            {
                new SwipeItem()
            };
            swipeView.BottomItems = new SwipeItems
            {
                new SwipeItem()
            };

            swipeView.BindingContext = new object();
            Assert.Equal(swipeView.BindingContext, ((BindableObject)swipeView.LeftItems[0]).BindingContext);
            Assert.Equal(swipeView.BindingContext, ((BindableObject)swipeView.RightItems[0]).BindingContext);
            Assert.Equal(swipeView.BindingContext, ((BindableObject)swipeView.TopItems[0]).BindingContext);
            Assert.Equal(swipeView.BindingContext, ((BindableObject)swipeView.BottomItems[0]).BindingContext);
        }

        [Fact]
        public void TestContentBindingContextPropagatesToPassedInSwipeItem()
        {
            var swipeView = new SwipeView();

            swipeView.LeftItems = new SwipeItems(new[] { new SwipeItem() });

            swipeView.BindingContext = new object();
            Assert.Equal(swipeView.BindingContext, ((BindableObject)swipeView.LeftItems[0]).BindingContext);
        }

        [Fact]
        public void TestContentBindingContextPropagatesToAddedSwipeItems()
        {
            var swipeView = new SwipeView();

            swipeView.LeftItems.Add(new SwipeItem());
            swipeView.RightItems.Add(new SwipeItem());
            swipeView.TopItems.Add(new SwipeItem());
            swipeView.BottomItems.Add(new SwipeItem());

            swipeView.BindingContext = new object();
            Assert.Equal(swipeView.BindingContext, ((BindableObject)swipeView.LeftItems[0]).BindingContext);
            Assert.Equal(swipeView.BindingContext, ((BindableObject)swipeView.RightItems[0]).BindingContext);
            Assert.Equal(swipeView.BindingContext, ((BindableObject)swipeView.TopItems[0]).BindingContext);
            Assert.Equal(swipeView.BindingContext, ((BindableObject)swipeView.BottomItems[0]).BindingContext);
        }

        [Fact]
        public void BindingContextTransfersToNewSetOfSwipeItems()
        {
            var bc1 = new object();
            var bc2 = new object();

            var swipeView = new SwipeView();
            var leftItems = swipeView.LeftItems;
            var swipeItem = new SwipeItem();
            leftItems.Add(swipeItem);
            swipeView.BindingContext = bc1;

            Assert.Equal(leftItems.BindingContext, bc1);
            Assert.Equal(swipeItem.BindingContext, bc1);

            Assert.Equal(leftItems, swipeView.LeftItems);
            Assert.Contains(leftItems, (swipeView as IVisualTreeElement).GetVisualChildren());
            Assert.Equal(swipeItem, (leftItems as IVisualTreeElement).GetVisualChildren()[0]);

            var leftItems2 = new SwipeItems();
            leftItems2.Add(swipeItem);
            swipeView.LeftItems = leftItems2;

            // now that this isn't the logical child of SwipeItems the parent should be null
            Assert.Null(leftItems.Parent);

            // The parent on swipeItem should now be leftItems2 because that's been set on SwipeView
            Assert.NotSame(swipeItem.Parent, leftItems);
            Assert.NotSame(leftItems.Parent, swipeView);

            Assert.Equal(swipeItem.Parent, leftItems2);
            Assert.Equal(leftItems2.Parent, swipeView);

            swipeView.BindingContext = bc2;
            Assert.Equal(leftItems2.BindingContext, bc2);
            Assert.Equal(swipeItem.BindingContext, bc2);
        }

        [Fact]
        public void TestDefaultSwipeItems()
        {
            var swipeView = new SwipeView();

            var swipeItem = new SwipeItem
            {
                BackgroundColor = Colors.Red,
                Text = "Text"
            };

            swipeView.LeftItems = new SwipeItems
            {
                swipeItem
            };

            Assert.Equal(SwipeMode.Reveal, swipeView.LeftItems.Mode);
            Assert.Equal(SwipeBehaviorOnInvoked.Auto, swipeView.LeftItems.SwipeBehaviorOnInvoked);
        }

        [Fact]
        public void TestSwipeItemsExecuteMode()
        {
            var swipeView = new SwipeView();

            var swipeItem = new SwipeItem
            {
                BackgroundColor = Colors.Red,
                Text = "Text"
            };

            var swipeItems = new SwipeItems
            {
                Mode = SwipeMode.Execute
            };

            swipeItems.Add(swipeItem);

            swipeView.LeftItems = swipeItems;

            Assert.Equal(SwipeMode.Execute, swipeView.LeftItems.Mode);
        }

        [Fact]
        public void TestSwipeItemsSwipeBehaviorOnInvoked()
        {
            var swipeView = new SwipeView();

            var swipeItem = new SwipeItem
            {
                BackgroundColor = Colors.Red,
                Text = "Text"
            };

            var swipeItems = new SwipeItems
            {
                SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close
            };

            swipeItems.Add(swipeItem);

            swipeView.LeftItems = swipeItems;

            Assert.Equal(SwipeBehaviorOnInvoked.Close, swipeView.LeftItems.SwipeBehaviorOnInvoked);
        }

        [Fact]
        public void TestLeftItems()
        {
            var swipeView = new SwipeView();

            var swipeItem = new SwipeItem
            {
                BackgroundColor = Colors.Red,
                Text = "Text"
            };

            swipeView.LeftItems = new SwipeItems
            {
                swipeItem
            };

            Assert.NotEmpty(swipeView.LeftItems);
        }

        [Fact]
        public void TestRightItems()
        {
            var swipeView = new SwipeView();

            var swipeItem = new SwipeItem
            {
                BackgroundColor = Colors.Red,
                Text = "Text"
            };

            swipeView.RightItems = new SwipeItems
            {
                swipeItem
            };

            Assert.NotEmpty(swipeView.RightItems);
        }

        [Fact]
        public void TestTopItems()
        {
            var swipeView = new SwipeView();

            var swipeItem = new SwipeItem
            {
                BackgroundColor = Colors.Red,
                Text = "Text"
            };

            swipeView.TopItems = new SwipeItems
            {
                swipeItem
            };

            Assert.NotEmpty(swipeView.TopItems);
        }

        [Fact]
        public void TestBottomItems()
        {
            var swipeView = new SwipeView();

            var swipeItem = new SwipeItem
            {
                BackgroundColor = Colors.Red,
                Text = "Text"
            };

            swipeView.BottomItems = new SwipeItems
            {
                swipeItem
            };

            Assert.NotEmpty(swipeView.BottomItems);
        }

        [Fact]
        public void TestProgrammaticallyOpen()
        {
            bool isOpen = false;

            var swipeView = new SwipeView();

            swipeView.OpenRequested += (sender, args) =>
            {
                isOpen = true;
            };

            var swipeItem = new SwipeItem
            {
                BackgroundColor = Colors.Red,
                Text = "Text"
            };

            swipeView.LeftItems = new SwipeItems
            {
                swipeItem
            };

            swipeView.Open(OpenSwipeItem.LeftItems);

            Assert.True(isOpen);
        }

        [Fact]
        public void TestProgrammaticallyClose()
        {
            bool isOpen = false;

            var swipeView = new SwipeView();

            swipeView.OpenRequested += (sender, args) => isOpen = true;
            swipeView.CloseRequested += (sender, args) => isOpen = false;

            var swipeItem = new SwipeItem
            {
                BackgroundColor = Colors.Red,
                Text = "Text"
            };

            swipeView.LeftItems = new SwipeItems
            {
                swipeItem
            };

            swipeView.Open(OpenSwipeItem.LeftItems);

            swipeView.Close();

            Assert.False(isOpen);
        }

        [Fact]
        public void TestSwipeItemView()
        {
            var swipeView = new SwipeView();

            var swipeItemViewContent = new Grid();
            swipeItemViewContent.BackgroundColor = Colors.Red;

            swipeItemViewContent.Children.Add(new Label
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Text = "SwipeItemView"
            });

            var swipeItemView = new SwipeItemView
            {
                Content = swipeItemViewContent
            };

            swipeView.LeftItems = new SwipeItems
            {
                swipeItemView
            };

            Assert.NotNull(swipeItemView);
            Assert.NotNull(swipeItemView.Content);
            Assert.NotEmpty(swipeView.LeftItems);
        }

        /// <summary>
        /// Tests that the Threshold property returns the default value of 0.0 when not explicitly set.
        /// This verifies the default value specified in the BindableProperty.Create call.
        /// </summary>
        [Fact]
        public void TestThreshold_DefaultValue_ReturnsZero()
        {
            // Arrange
            var swipeView = new SwipeView();

            // Act
            double actualThreshold = swipeView.Threshold;

            // Assert
            Assert.Equal(0.0, actualThreshold);
        }

        /// <summary>
        /// Tests that the Threshold property correctly stores and retrieves various double values,
        /// including edge cases like infinity, NaN, and boundary values.
        /// This verifies both the setter and getter functionality.
        /// </summary>
        /// <param name="threshold">The threshold value to test</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(100.5)]
        [InlineData(-100.5)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        [InlineData(1e-10)]
        [InlineData(-1e-10)]
        [InlineData(1e10)]
        [InlineData(-1e10)]
        public void TestThreshold_SetAndGet_ReturnsExpectedValue(double threshold)
        {
            // Arrange
            var swipeView = new SwipeView();

            // Act
            swipeView.Threshold = threshold;
            double actualThreshold = swipeView.Threshold;

            // Assert
            if (double.IsNaN(threshold))
            {
                Assert.True(double.IsNaN(actualThreshold));
            }
            else
            {
                Assert.Equal(threshold, actualThreshold);
            }
        }
    }


    public partial class SwipeViewHandlerSwipeItemsTests
    {
        /// <summary>
        /// Tests that the HandlerSwipeItems constructor properly initializes with valid SwipeItems.
        /// Input: Valid SwipeItems instance.
        /// Expected: Constructor completes successfully and fields are properly assigned.
        /// </summary>
        [Fact]
        public void HandlerSwipeItemsConstructor_ValidSwipeItems_InitializesSuccessfully()
        {
            // Arrange
            var swipeItems = new SwipeItems();

            // Act
            var handlerSwipeItems = new SwipeView.HandlerSwipeItems(swipeItems);

            // Assert
            Assert.NotNull(handlerSwipeItems);
            Assert.Equal(swipeItems.Mode, handlerSwipeItems.Mode);
            Assert.Equal(swipeItems.SwipeBehaviorOnInvoked, handlerSwipeItems.SwipeBehaviorOnInvoked);
        }

        /// <summary>
        /// Tests that the HandlerSwipeItems constructor throws ArgumentNullException when passed null.
        /// Input: null SwipeItems parameter.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void HandlerSwipeItemsConstructor_NullSwipeItems_ThrowsArgumentNullException()
        {
            // Arrange
            SwipeItems nullSwipeItems = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SwipeView.HandlerSwipeItems(nullSwipeItems));
        }

        /// <summary>
        /// Tests that the HandlerSwipeItems constructor properly inherits from List with SwipeItems containing items.
        /// Input: SwipeItems with ISwipeItem elements.
        /// Expected: HandlerSwipeItems contains the same items as the source SwipeItems.
        /// </summary>
        [Fact]
        public void HandlerSwipeItemsConstructor_SwipeItemsWithItems_InheritsItemsFromBase()
        {
            // Arrange
            var swipeItem = new SwipeItem { Text = "Test" };
            var swipeItems = new SwipeItems { swipeItem };

            // Act
            var handlerSwipeItems = new SwipeView.HandlerSwipeItems(swipeItems);

            // Assert
            Assert.Single(handlerSwipeItems);
            Assert.Contains(swipeItem, handlerSwipeItems);
        }

        /// <summary>
        /// Tests that the HandlerSwipeItems constructor properly handles empty SwipeItems.
        /// Input: Empty SwipeItems instance.
        /// Expected: HandlerSwipeItems is empty but properly initialized.
        /// </summary>
        [Fact]
        public void HandlerSwipeItemsConstructor_EmptySwipeItems_InitializesEmptyList()
        {
            // Arrange
            var swipeItems = new SwipeItems();

            // Act
            var handlerSwipeItems = new SwipeView.HandlerSwipeItems(swipeItems);

            // Assert
            Assert.Empty(handlerSwipeItems);
            Assert.Equal(swipeItems.Mode, handlerSwipeItems.Mode);
            Assert.Equal(swipeItems.SwipeBehaviorOnInvoked, handlerSwipeItems.SwipeBehaviorOnInvoked);
        }

        /// <summary>
        /// Tests that the HandlerSwipeItems constructor preserves SwipeItems properties.
        /// Input: SwipeItems with specific Mode and SwipeBehaviorOnInvoked values.
        /// Expected: HandlerSwipeItems exposes the same property values.
        /// </summary>
        [Theory]
        [InlineData(SwipeMode.Execute, SwipeBehaviorOnInvoked.Auto)]
        [InlineData(SwipeMode.Reveal, SwipeBehaviorOnInvoked.Close)]
        [InlineData(SwipeMode.Execute, SwipeBehaviorOnInvoked.RemainOpen)]
        public void HandlerSwipeItemsConstructor_SwipeItemsWithDifferentProperties_PreservesProperties(SwipeMode mode, SwipeBehaviorOnInvoked behavior)
        {
            // Arrange
            var swipeItems = new SwipeItems
            {
                Mode = mode,
                SwipeBehaviorOnInvoked = behavior
            };

            // Act
            var handlerSwipeItems = new SwipeView.HandlerSwipeItems(swipeItems);

            // Assert
            Assert.Equal(mode, handlerSwipeItems.Mode);
            Assert.Equal(behavior, handlerSwipeItems.SwipeBehaviorOnInvoked);
        }

        /// <summary>
        /// Tests that HandlerSwipeItems.Mode property correctly returns the Mode from the underlying SwipeItems.
        /// Tests both Reveal and Execute enum values to ensure proper delegation.
        /// </summary>
        /// <param name="expectedMode">The SwipeMode value to test</param>
        [Theory]
        [InlineData(SwipeMode.Reveal)]
        [InlineData(SwipeMode.Execute)]
        public void Mode_WithDifferentSwipeModes_ReturnsCorrectMode(SwipeMode expectedMode)
        {
            // Arrange
            var swipeView = new SwipeView();
            var swipeItems = new SwipeItems
            {
                Mode = expectedMode
            };
            swipeView.LeftItems = swipeItems;

            // Act
            var handlerSwipeItems = ((ISwipeView)swipeView).LeftItems;
            var actualMode = handlerSwipeItems.Mode;

            // Assert
            Assert.Equal(expectedMode, actualMode);
        }

        /// <summary>
        /// Tests that HandlerSwipeItems.Mode property reflects changes when the underlying SwipeItems Mode is modified.
        /// This ensures the property delegation is dynamic and not cached.
        /// </summary>
        [Fact]
        public void Mode_WhenUnderlyingSwipeItemsModeChanges_ReflectsNewValue()
        {
            // Arrange
            var swipeView = new SwipeView();
            var swipeItems = new SwipeItems
            {
                Mode = SwipeMode.Reveal
            };
            swipeView.RightItems = swipeItems;
            var handlerSwipeItems = ((ISwipeView)swipeView).RightItems;

            // Act
            swipeItems.Mode = SwipeMode.Execute;
            var actualMode = handlerSwipeItems.Mode;

            // Assert
            Assert.Equal(SwipeMode.Execute, actualMode);
        }

        /// <summary>
        /// Tests that HandlerSwipeItems.Mode property works correctly for all SwipeItems directions (Left, Right, Top, Bottom).
        /// This ensures the Mode property delegation works consistently across all SwipeItems collections.
        /// </summary>
        /// <param name="expectedMode">The SwipeMode value to test</param>
        [Theory]
        [InlineData(SwipeMode.Reveal)]
        [InlineData(SwipeMode.Execute)]
        public void Mode_WithAllSwipeItemsDirections_ReturnsCorrectMode(SwipeMode expectedMode)
        {
            // Arrange
            var swipeView = new SwipeView();
            var leftItems = new SwipeItems { Mode = expectedMode };
            var rightItems = new SwipeItems { Mode = expectedMode };
            var topItems = new SwipeItems { Mode = expectedMode };
            var bottomItems = new SwipeItems { Mode = expectedMode };

            swipeView.LeftItems = leftItems;
            swipeView.RightItems = rightItems;
            swipeView.TopItems = topItems;
            swipeView.BottomItems = bottomItems;

            // Act & Assert
            Assert.Equal(expectedMode, ((ISwipeView)swipeView).LeftItems.Mode);
            Assert.Equal(expectedMode, ((ISwipeView)swipeView).RightItems.Mode);
            Assert.Equal(expectedMode, ((ISwipeView)swipeView).TopItems.Mode);
            Assert.Equal(expectedMode, ((ISwipeView)swipeView).BottomItems.Mode);
        }

        /// <summary>
        /// Tests that SwipeBehaviorOnInvoked property returns the correct value from the underlying SwipeItems for all enum values.
        /// This test verifies that the HandlerSwipeItems wrapper correctly delegates to the wrapped SwipeItems instance.
        /// </summary>
        /// <param name="expectedBehavior">The SwipeBehaviorOnInvoked value to test</param>
        [Theory]
        [InlineData(SwipeBehaviorOnInvoked.Auto)]
        [InlineData(SwipeBehaviorOnInvoked.Close)]
        [InlineData(SwipeBehaviorOnInvoked.RemainOpen)]
        public void SwipeBehaviorOnInvoked_WithDifferentValues_ReturnsExpectedValue(SwipeBehaviorOnInvoked expectedBehavior)
        {
            // Arrange
            var swipeItems = new SwipeItems
            {
                SwipeBehaviorOnInvoked = expectedBehavior
            };
            var handlerSwipeItems = new SwipeView.HandlerSwipeItems(swipeItems);

            // Act
            var actualBehavior = handlerSwipeItems.SwipeBehaviorOnInvoked;

            // Assert
            Assert.Equal(expectedBehavior, actualBehavior);
        }

        /// <summary>
        /// Tests that SwipeBehaviorOnInvoked property returns the default value when SwipeItems uses default value.
        /// This test ensures the property delegation works correctly with default enum values.
        /// </summary>
        [Fact]
        public void SwipeBehaviorOnInvoked_WithDefaultValue_ReturnsAuto()
        {
            // Arrange
            var swipeItems = new SwipeItems(); // Uses default SwipeBehaviorOnInvoked value
            var handlerSwipeItems = new SwipeView.HandlerSwipeItems(swipeItems);

            // Act
            var actualBehavior = handlerSwipeItems.SwipeBehaviorOnInvoked;

            // Assert
            Assert.Equal(SwipeBehaviorOnInvoked.Auto, actualBehavior);
        }

        /// <summary>
        /// Tests that SwipeBehaviorOnInvoked property reflects changes when the underlying SwipeItems property is modified.
        /// This test verifies that the HandlerSwipeItems wrapper returns live values from the wrapped instance.
        /// </summary>
        [Fact]
        public void SwipeBehaviorOnInvoked_AfterModifyingSwipeItems_ReturnsUpdatedValue()
        {
            // Arrange
            var swipeItems = new SwipeItems
            {
                SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Auto
            };
            var handlerSwipeItems = new SwipeView.HandlerSwipeItems(swipeItems);

            // Act & Assert - Initial value
            Assert.Equal(SwipeBehaviorOnInvoked.Auto, handlerSwipeItems.SwipeBehaviorOnInvoked);

            // Act - Modify the underlying SwipeItems
            swipeItems.SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close;

            // Assert - Updated value
            Assert.Equal(SwipeBehaviorOnInvoked.Close, handlerSwipeItems.SwipeBehaviorOnInvoked);

            // Act - Modify again
            swipeItems.SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.RemainOpen;

            // Assert - Final updated value
            Assert.Equal(SwipeBehaviorOnInvoked.RemainOpen, handlerSwipeItems.SwipeBehaviorOnInvoked);
        }
    }
}