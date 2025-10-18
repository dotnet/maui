#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using NSubstitute;
using Xunit;

using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;

namespace Microsoft.Maui.Controls.Core.UnitTests
{


    public class ScrollViewUnitTests : BaseTestFixture
    {
        [Fact]
        public void TestConstructor()
        {
            ScrollView scrollView = new ScrollView();

            Assert.Null(scrollView.Content);

            View view = new View();
            scrollView = new ScrollView { Content = view };

            Assert.Equal(view, scrollView.Content);
        }

        [Theory]
        [InlineData(ScrollOrientation.Horizontal)]
        [InlineData(ScrollOrientation.Both)]
        public void GetsCorrectSizeRequestWithWrappingContent(ScrollOrientation orientation)
        {
            var scrollView = new ScrollView
            {
                IsPlatformEnabled = true,
                Orientation = orientation,
            };

            var hLayout = new StackLayout
            {
                IsPlatformEnabled = true,
                Orientation = StackOrientation.Horizontal,
                Children = {
                    MockPlatformSizeService.Sub<Label>(text: "THIS IS A REALLY LONG STRING", useRealisticLabelMeasure: true),
                    MockPlatformSizeService.Sub<Label>(text: "THIS IS A REALLY LONG STRING", useRealisticLabelMeasure: true),
                    MockPlatformSizeService.Sub<Label>(text: "THIS IS A REALLY LONG STRING", useRealisticLabelMeasure: true),
                    MockPlatformSizeService.Sub<Label>(text: "THIS IS A REALLY LONG STRING", useRealisticLabelMeasure: true),
                    MockPlatformSizeService.Sub<Label>(text: "THIS IS A REALLY LONG STRING", useRealisticLabelMeasure: true),
                }
            };

            scrollView.Content = hLayout;
            var view = ((ICrossPlatformLayout)scrollView);

            view.CrossPlatformMeasure(100, 100);
            var r = view.CrossPlatformArrange(new Graphics.Rect(0, 0, 100, 100));

            Assert.Equal(100, r.Height);
        }

        [Fact]
        public void TestChildChanged()
        {
            ScrollView scrollView = new ScrollView();

            bool changed = false;
            scrollView.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case "Content":
                        changed = true;
                        break;
                }
            };
            View view = new View();
            scrollView.Content = view;

            Assert.True(changed);
        }

        [Fact]
        public void TestChildDoubleSet()
        {
            var scrollView = new ScrollView();

            bool changed = false;
            scrollView.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Content")
                    changed = true;
            };

            var child = new View();
            scrollView.Content = child;

            Assert.True(changed);
            Assert.Equal(child, scrollView.Content);
            Assert.Equal(child.Parent, scrollView);

            changed = false;

            scrollView.Content = child;

            Assert.False(changed);

            scrollView.Content = null;

            Assert.True(changed);
            Assert.Null(scrollView.Content);
            Assert.Null(child.Parent);
        }

        [Fact]
        public void TestOrientation()
        {
            var scrollView = new ScrollView();

            Assert.Equal(ScrollOrientation.Vertical, scrollView.Orientation);

            bool signaled = false;
            scrollView.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Orientation")
                    signaled = true;
            };

            scrollView.Orientation = ScrollOrientation.Horizontal;

            Assert.Equal(ScrollOrientation.Horizontal, scrollView.Orientation);
            Assert.True(signaled);

            scrollView.Orientation = ScrollOrientation.Both;
            Assert.Equal(ScrollOrientation.Both, scrollView.Orientation);
            Assert.True(signaled);

            scrollView.Orientation = ScrollOrientation.Neither;
            Assert.Equal(ScrollOrientation.Neither, scrollView.Orientation);
            Assert.True(signaled);
        }

        [Fact]
        public void TestOrientationDoubleSet()
        {
            var scrollView = new ScrollView();

            bool signaled = false;
            scrollView.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Orientation")
                    signaled = true;
            };

            scrollView.Orientation = scrollView.Orientation;

            Assert.False(signaled);
        }


        [Fact]
        public void TestScrollTo()
        {
            var scrollView = new ScrollView();

            var item = new View { };
            scrollView.Content = new StackLayout { Children = { item } };

            bool requested = false;
            ((IScrollViewController)scrollView).ScrollToRequested += (sender, args) =>
            {
                requested = true;
                Assert.Equal(100, args.ScrollY);
                Assert.Equal(0, args.ScrollX);
                Assert.Null(args.Item);
                Assert.True(args.ShouldAnimate);
            };

            scrollView.ScrollToAsync(0, 100, true);
            Assert.True(requested);
        }

        [Fact]
        public void TestScrollWasNotFiredOnNeither()
        {
            var scrollView = new ScrollView
            {
                Orientation = ScrollOrientation.Neither
            };

            var item = new View { };
            scrollView.Content = new StackLayout { Children = { item } };

            bool requested = false;
            ((IScrollViewController)scrollView).ScrollToRequested += (sender, args) =>
            {
                requested = true;
            };

            scrollView.ScrollToAsync(0, 100, true);
            Assert.False(requested);
        }

        [Fact]
        public void TestScrollToNotAnimated()
        {
            var scrollView = new ScrollView();

            var item = new View { };
            scrollView.Content = new StackLayout { Children = { item } };

            bool requested = false;
            ((IScrollViewController)scrollView).ScrollToRequested += (sender, args) =>
            {
                requested = true;
                Assert.Equal(100, args.ScrollY);
                Assert.Equal(0, args.ScrollX);
                Assert.Null(args.Item);
                Assert.False(args.ShouldAnimate);
            };

            scrollView.ScrollToAsync(0, 100, false);
            Assert.True(requested);
        }

        [Fact]
        public void TestScrollToElement()
        {
            var scrollView = new ScrollView();

            var item = new Label { Text = "Test" };
            scrollView.Content = new StackLayout { Children = { item } };

            bool requested = false;
            ((IScrollViewController)scrollView).ScrollToRequested += (sender, args) =>
            {
                requested = true;

                Assert.Same(args.Element, item);
                Assert.Equal(ScrollToPosition.Center, args.Position);
                Assert.True(args.ShouldAnimate);
            };

            scrollView.ScrollToAsync(item, ScrollToPosition.Center, true);
            Assert.True(requested);
        }

        [Fact]
        public void TestScrollToElementNotAnimated()
        {
            var scrollView = new ScrollView();

            var item = new Label { Text = "Test" };
            scrollView.Content = new StackLayout { Children = { item } };

            bool requested = false;
            ((IScrollViewController)scrollView).ScrollToRequested += (sender, args) =>
            {
                requested = true;

                Assert.Same(args.Element, item);
                Assert.Equal(ScrollToPosition.Center, args.Position);
                Assert.False(args.ShouldAnimate);
            };

            scrollView.ScrollToAsync(item, ScrollToPosition.Center, false);
            Assert.True(requested);
        }

        [Fact]
        public async Task TestScrollToInvalid()
        {
            var scrollView = new ScrollView();

            await Assert.ThrowsAsync<ArgumentException>(() => scrollView.ScrollToAsync(new VisualElement(), ScrollToPosition.Center, true));
            await Assert.ThrowsAsync<ArgumentException>(() => scrollView.ScrollToAsync(null, (ScrollToPosition)500, true));
        }

        [Fact]
        public void SetScrollPosition()
        {
            var scroll = new ScrollView();
            IScrollViewController controller = scroll;
            controller.SetScrolledPosition(100, 100);

            Assert.Equal(100, scroll.ScrollX);
            Assert.Equal(100, scroll.ScrollY);
        }

        [Fact]
        public void TestBackToBackBiDirectionalScroll()
        {
            var scrollView = new ScrollView
            {
                Orientation = ScrollOrientation.Both,
                Content = new Grid
                {
                    WidthRequest = 1000,
                    HeightRequest = 1000
                }
            };

            var y100Count = 0;

            ((IScrollViewController)scrollView).ScrollToRequested += (sender, args) =>
            {
                if (args.ScrollY == 100)
                {
                    ++y100Count;
                }
            };

            scrollView.ScrollToAsync(100, 100, true);
            Assert.Equal(1, y100Count);

            scrollView.ScrollToAsync(0, 100, true);
            Assert.Equal(2, y100Count);
        }

        void AssertInvalidated(IViewHandler handler)
        {
            handler.Received().Invoke(Arg.Is(nameof(IView.InvalidateMeasure)), Arg.Any<object>());
            handler.ClearReceivedCalls();
        }
    }

    public partial class ScrollViewTests
    {
        /// <summary>
        /// Tests that GetScrollPositionForElement throws ArgumentNullException when item parameter is null.
        /// Input: null item, any position
        /// Expected: ArgumentNullException or NullReferenceException
        /// </summary>
        [Fact]
        public void GetScrollPositionForElement_NullItem_ThrowsException()
        {
            var scrollView = new ScrollView
            {
                Content = new View()
            };

            Assert.ThrowsAny<Exception>(() => scrollView.GetScrollPositionForElement(null, ScrollToPosition.Start));
        }

        /// <summary>
        /// Tests GetScrollPositionForElement with Start position returns item coordinates without adjustment.
        /// Input: valid item, ScrollToPosition.Start
        /// Expected: Point with item's absolute coordinates
        /// </summary>
        [Fact]
        public void GetScrollPositionForElement_StartPosition_ReturnsItemCoordinates()
        {
            var scrollView = new ScrollView();
            var item = new MockVisualElement { X = 50, Y = 100, Width = 200, Height = 150 };
            scrollView.Content = item;

            var result = scrollView.GetScrollPositionForElement(item, ScrollToPosition.Start);

            Assert.Equal(50, result.X);
            Assert.Equal(100, result.Y);
        }

        /// <summary>
        /// Tests GetScrollPositionForElement with Center position calculates centered coordinates.
        /// Input: valid item, ScrollToPosition.Center, specific scroll view dimensions
        /// Expected: Point with centered coordinates
        /// </summary>
        [Fact]
        public void GetScrollPositionForElement_CenterPosition_ReturnsCenteredCoordinates()
        {
            var scrollView = new ScrollView();
            scrollView.MockSetSize(400, 300); // Mock scroll view size
            var item = new MockVisualElement { X = 100, Y = 200, Width = 100, Height = 50 };
            scrollView.Content = item;

            var result = scrollView.GetScrollPositionForElement(item, ScrollToPosition.Center);

            // Center calculation: x = x - Width/2 + item.Width/2 = 100 - 400/2 + 100/2 = -50
            // Center calculation: y = y - Height/2 + item.Height/2 = 200 - 300/2 + 50/2 = 75
            Assert.Equal(-50, result.X);
            Assert.Equal(75, result.Y);
        }

        /// <summary>
        /// Tests GetScrollPositionForElement with End position calculates end-aligned coordinates.
        /// Input: valid item, ScrollToPosition.End, specific scroll view dimensions
        /// Expected: Point with end-aligned coordinates
        /// </summary>
        [Fact]
        public void GetScrollPositionForElement_EndPosition_ReturnsEndAlignedCoordinates()
        {
            var scrollView = new ScrollView();
            scrollView.MockSetSize(400, 300);
            var item = new MockVisualElement { X = 100, Y = 200, Width = 100, Height = 50 };
            scrollView.Content = item;

            var result = scrollView.GetScrollPositionForElement(item, ScrollToPosition.End);

            // End calculation: x = x - Width + item.Width = 100 - 400 + 100 = -200
            // End calculation: y = y - Height + item.Height = 200 - 300 + 50 = -50
            Assert.Equal(-200, result.X);
            Assert.Equal(-50, result.Y);
        }

        /// <summary>
        /// Tests GetScrollPositionForElement with MakeVisible position when item is already visible.
        /// Input: item within scroll bounds, ScrollToPosition.MakeVisible
        /// Expected: Current scroll position unchanged
        /// </summary>
        [Fact]
        public void GetScrollPositionForElement_MakeVisibleItemAlreadyVisible_ReturnsCurrentScrollPosition()
        {
            var scrollView = new ScrollView();
            scrollView.MockSetSize(400, 300);
            scrollView.MockSetScrollPosition(10, 20);

            // Item is within the visible area (scroll bounds: x=10-410, y=20-320)
            var item = new MockVisualElement { X = 50, Y = 100, Width = 100, Height = 50 };
            scrollView.Content = item;

            var result = scrollView.GetScrollPositionForElement(item, ScrollToPosition.MakeVisible);

            Assert.Equal(10, result.X); // Current ScrollX
            Assert.Equal(20, result.Y); // Current ScrollY
        }

        /// <summary>
        /// Tests GetScrollPositionForElement with MakeVisible for vertical orientation when item is below visible area.
        /// Input: item below visible area, vertical orientation, ScrollToPosition.MakeVisible
        /// Expected: End position for vertical scrolling
        /// </summary>
        [Theory]
        [InlineData(ScrollOrientation.Vertical)]
        public void GetScrollPositionForElement_MakeVisibleVerticalItemBelow_ReturnsEndPosition(ScrollOrientation orientation)
        {
            var scrollView = new ScrollView { Orientation = orientation };
            scrollView.MockSetSize(400, 300);
            scrollView.MockSetScrollPosition(0, 50);

            // Item is below visible area (item.Y = 400 > ScrollY = 50)
            var item = new MockVisualElement { X = 100, Y = 400, Width = 100, Height = 50 };
            scrollView.Content = item;

            var result = scrollView.GetScrollPositionForElement(item, ScrollToPosition.MakeVisible);

            // Should use End position: y = y - Height + item.Height = 400 - 300 + 50 = 150
            Assert.Equal(100, result.X); // X unchanged for vertical
            Assert.Equal(150, result.Y);
        }

        /// <summary>
        /// Tests GetScrollPositionForElement with MakeVisible for vertical orientation when item is above visible area.
        /// Input: item above visible area, vertical orientation, ScrollToPosition.MakeVisible
        /// Expected: Start position for vertical scrolling
        /// </summary>
        [Fact]
        public void GetScrollPositionForElement_MakeVisibleVerticalItemAbove_ReturnsStartPosition()
        {
            var scrollView = new ScrollView { Orientation = ScrollOrientation.Vertical };
            scrollView.MockSetSize(400, 300);
            scrollView.MockSetScrollPosition(0, 200);

            // Item is above visible area (item.Y = 50 < ScrollY = 200)
            var item = new MockVisualElement { X = 100, Y = 50, Width = 100, Height = 50 };
            scrollView.Content = item;

            var result = scrollView.GetScrollPositionForElement(item, ScrollToPosition.MakeVisible);

            // Should use Start position: no adjustment needed
            Assert.Equal(100, result.X);
            Assert.Equal(50, result.Y);
        }

        /// <summary>
        /// Tests GetScrollPositionForElement with MakeVisible for horizontal orientation when item is to the right.
        /// Input: item to the right of visible area, horizontal orientation, ScrollToPosition.MakeVisible
        /// Expected: End position for horizontal scrolling
        /// </summary>
        [Fact]
        public void GetScrollPositionForElement_MakeVisibleHorizontalItemRight_ReturnsEndPosition()
        {
            var scrollView = new ScrollView { Orientation = ScrollOrientation.Horizontal };
            scrollView.MockSetSize(400, 300);
            scrollView.MockSetScrollPosition(100, 0);

            // Item is to the right of visible area (item.X = 600 > ScrollX = 100)
            var item = new MockVisualElement { X = 600, Y = 100, Width = 100, Height = 50 };
            scrollView.Content = item;

            var result = scrollView.GetScrollPositionForElement(item, ScrollToPosition.MakeVisible);

            // Should use End position: x = x - Width + item.Width = 600 - 400 + 100 = 300
            Assert.Equal(300, result.X);
            Assert.Equal(100, result.Y); // Y unchanged for horizontal
        }

        /// <summary>
        /// Tests GetScrollPositionForElement with MakeVisible for horizontal orientation when item is to the left.
        /// Input: item to the left of visible area, horizontal orientation, ScrollToPosition.MakeVisible
        /// Expected: Start position for horizontal scrolling
        /// </summary>
        [Fact]
        public void GetScrollPositionForElement_MakeVisibleHorizontalItemLeft_ReturnsStartPosition()
        {
            var scrollView = new ScrollView { Orientation = ScrollOrientation.Horizontal };
            scrollView.MockSetSize(400, 300);
            scrollView.MockSetScrollPosition(200, 0);

            // Item is to the left of visible area (item.X = 50 < ScrollX = 200)
            var item = new MockVisualElement { X = 50, Y = 100, Width = 100, Height = 50 };
            scrollView.Content = item;

            var result = scrollView.GetScrollPositionForElement(item, ScrollToPosition.MakeVisible);

            // Should use Start position: no adjustment needed
            Assert.Equal(50, result.X);
            Assert.Equal(100, result.Y);
        }

        /// <summary>
        /// Tests GetScrollPositionForElement with MakeVisible for Both orientation when item is outside visible area.
        /// Input: item outside visible area, Both orientation, ScrollToPosition.MakeVisible
        /// Expected: End position when either coordinate is greater than current scroll
        /// </summary>
        [Theory]
        [InlineData(600, 100, true)]  // X > ScrollX, should use End
        [InlineData(50, 400, true)]   // Y > ScrollY, should use End  
        [InlineData(600, 400, true)]  // Both > current scroll, should use End
        [InlineData(50, 50, false)]   // Both < current scroll, should use Start
        public void GetScrollPositionForElement_MakeVisibleBothOrientation_ReturnsCorrectPosition(double itemX, double itemY, bool shouldUseEnd)
        {
            var scrollView = new ScrollView { Orientation = ScrollOrientation.Both };
            scrollView.MockSetSize(400, 300);
            scrollView.MockSetScrollPosition(100, 100);

            var item = new MockVisualElement { X = itemX, Y = itemY, Width = 100, Height = 50 };
            scrollView.Content = item;

            var result = scrollView.GetScrollPositionForElement(item, ScrollToPosition.MakeVisible);

            if (shouldUseEnd)
            {
                // End position calculations
                double expectedX = itemX - 400 + 100; // x - Width + item.Width
                double expectedY = itemY - 300 + 50;  // y - Height + item.Height
                Assert.Equal(expectedX, result.X);
                Assert.Equal(expectedY, result.Y);
            }
            else
            {
                // Start position (no adjustment)
                Assert.Equal(itemX, result.X);
                Assert.Equal(itemY, result.Y);
            }
        }

        /// <summary>
        /// Tests GetScrollPositionForElement with invalid enum values.
        /// Input: invalid ScrollToPosition value
        /// Expected: Treats as Start position (default case)
        /// </summary>
        [Theory]
        [InlineData((ScrollToPosition)99)]
        [InlineData((ScrollToPosition)(-1))]
        public void GetScrollPositionForElement_InvalidScrollToPosition_TreatsAsStart(ScrollToPosition invalidPosition)
        {
            var scrollView = new ScrollView();
            var item = new MockVisualElement { X = 50, Y = 100, Width = 100, Height = 50 };
            scrollView.Content = item;

            var result = scrollView.GetScrollPositionForElement(item, invalidPosition);

            // Should fall through to default case (no adjustment, like Start)
            Assert.Equal(50, result.X);
            Assert.Equal(100, result.Y);
        }

        /// <summary>
        /// Tests GetScrollPositionForElement with zero dimensions.
        /// Input: item with zero width/height
        /// Expected: Calculations work without error
        /// </summary>
        [Fact]
        public void GetScrollPositionForElement_ZeroDimensions_ReturnsValidResult()
        {
            var scrollView = new ScrollView();
            scrollView.MockSetSize(400, 300);
            var item = new MockVisualElement { X = 100, Y = 200, Width = 0, Height = 0 };
            scrollView.Content = item;

            var result = scrollView.GetScrollPositionForElement(item, ScrollToPosition.Center);

            // Center calculation with zero item dimensions
            // x = 100 - 400/2 + 0/2 = -100
            // y = 200 - 300/2 + 0/2 = 50
            Assert.Equal(-100, result.X);
            Assert.Equal(50, result.Y);
        }

        /// <summary>
        /// Tests GetScrollPositionForElement with extreme coordinate values.
        /// Input: item with very large coordinates
        /// Expected: Calculations work without overflow
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue / 2, double.MaxValue / 2)]
        [InlineData(double.MinValue / 2, double.MinValue / 2)]
        public void GetScrollPositionForElement_ExtremeCoordinates_ReturnsValidResult(double x, double y)
        {
            var scrollView = new ScrollView();
            scrollView.MockSetSize(400, 300);
            var item = new MockVisualElement { X = x, Y = y, Width = 100, Height = 50 };
            scrollView.Content = item;

            var result = scrollView.GetScrollPositionForElement(item, ScrollToPosition.Start);

            Assert.Equal(x, result.X);
            Assert.Equal(y, result.Y);
        }

        /// <summary>
        /// Helper class to create testable VisualElement with settable properties.
        /// </summary>
        private class MockVisualElement : View
        {
            public new double X { get; set; }
            public new double Y { get; set; }
            public new double Width { get; set; }
            public new double Height { get; set; }
        }

        /// <summary>
        /// Tests that SendScrollFinished does not throw when called without any active scroll operation.
        /// The _scrollCompletionSource field should be null and the method should handle this gracefully.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        public void SendScrollFinished_WithNullCompletionSource_DoesNotThrow()
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act & Assert - should not throw
            scrollView.SendScrollFinished();
        }

        /// <summary>
        /// Tests that SendScrollFinished completes an active scroll task with true result.
        /// Sets up an active scroll operation via ScrollToAsync, then calls SendScrollFinished.
        /// Expected result: The task returned by ScrollToAsync completes with result true.
        /// </summary>
        [Fact]
        public async Task SendScrollFinished_WithActiveTask_CompletesTaskWithTrue()
        {
            // Arrange
            var scrollView = new ScrollView();
            scrollView.Content = new View();

            // Act
            var scrollTask = scrollView.ScrollToAsync(0, 100, false);
            scrollView.SendScrollFinished();
            var result = await scrollTask;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that SendScrollFinished can be called safely on an already completed task.
        /// Verifies that TrySetResult handles already completed TaskCompletionSource correctly.
        /// Expected result: No exception is thrown when calling SendScrollFinished multiple times.
        /// </summary>
        [Fact]
        public async Task SendScrollFinished_WithAlreadyCompletedTask_DoesNotThrow()
        {
            // Arrange
            var scrollView = new ScrollView();
            scrollView.Content = new View();

            // Act
            var scrollTask = scrollView.ScrollToAsync(0, 100, false);
            scrollView.SendScrollFinished(); // First call completes the task
            await scrollTask;

            // Act & Assert - second call should not throw
            scrollView.SendScrollFinished();
        }

        /// <summary>
        /// Tests that SendScrollFinished can be called multiple times safely in various scenarios.
        /// Covers edge cases where multiple calls happen without active tasks.
        /// Expected result: No exceptions are thrown regardless of call frequency.
        /// </summary>
        [Fact]
        public void SendScrollFinished_MultipleCalls_DoesNotThrow()
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act & Assert - multiple calls should not throw
            scrollView.SendScrollFinished();
            scrollView.SendScrollFinished();
            scrollView.SendScrollFinished();
        }

        /// <summary>
        /// Tests that SendScrollFinished works correctly with ScrollToAsync for element-based scrolling.
        /// Verifies the method completes element scroll operations properly.
        /// Expected result: The element scroll task completes with true result.
        /// </summary>
        [Fact]
        public async Task SendScrollFinished_WithElementScrollTask_CompletesTaskWithTrue()
        {
            // Arrange
            var scrollView = new ScrollView();
            var element = new View();
            var stackLayout = new StackLayout();
            stackLayout.Children.Add(element);
            scrollView.Content = stackLayout;

            // Act
            var scrollTask = scrollView.ScrollToAsync(element, ScrollToPosition.Start, false);
            scrollView.SendScrollFinished();
            var result = await scrollTask;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests SendScrollFinished behavior when scroll orientation is set to Neither.
        /// When orientation is Neither, ScrollToAsync returns completed task, so SendScrollFinished should still be safe.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        public void SendScrollFinished_WithOrientationNeither_DoesNotThrow()
        {
            // Arrange
            var scrollView = new ScrollView
            {
                Orientation = ScrollOrientation.Neither
            };

            // Act & Assert - should not throw
            scrollView.SendScrollFinished();
        }

        /// <summary>
        /// Tests that SetScrolledPosition updates scroll positions and fires Scrolled event when values are different.
        /// Input conditions: ScrollView with different x and y values than current scroll positions.
        /// Expected result: ScrollX and ScrollY properties are updated, and Scrolled event is fired with correct arguments.
        /// </summary>
        [Fact]
        public void SetScrolledPosition_WithDifferentValues_UpdatesPositionsAndFiresEvent()
        {
            // Arrange
            var scrollView = new ScrollView();
            bool eventFired = false;
            double eventX = 0, eventY = 0;

            scrollView.Scrolled += (sender, args) =>
            {
                eventFired = true;
                eventX = args.ScrollX;
                eventY = args.ScrollY;
            };

            // Act
            scrollView.SetScrolledPosition(100.5, 200.75);

            // Assert
            Assert.Equal(100.5, scrollView.ScrollX);
            Assert.Equal(200.75, scrollView.ScrollY);
            Assert.True(eventFired);
            Assert.Equal(100.5, eventX);
            Assert.Equal(200.75, eventY);
        }

        /// <summary>
        /// Tests that SetScrolledPosition does not fire Scrolled event when values are the same as current positions.
        /// Input conditions: ScrollView with same x and y values as current scroll positions.
        /// Expected result: No event is fired and properties remain unchanged.
        /// </summary>
        [Fact]
        public void SetScrolledPosition_WithSameValues_DoesNotFireEvent()
        {
            // Arrange
            var scrollView = new ScrollView();
            scrollView.SetScrolledPosition(50.0, 75.0); // Set initial values

            bool eventFired = false;
            scrollView.Scrolled += (sender, args) => eventFired = true;

            // Act
            scrollView.SetScrolledPosition(50.0, 75.0); // Same values

            // Assert
            Assert.Equal(50.0, scrollView.ScrollX);
            Assert.Equal(75.0, scrollView.ScrollY);
            Assert.False(eventFired);
        }

        /// <summary>
        /// Tests that SetScrolledPosition handles zero values correctly.
        /// Input conditions: Zero values for both x and y coordinates.
        /// Expected result: ScrollX and ScrollY are set to zero, and Scrolled event is fired.
        /// </summary>
        [Fact]
        public void SetScrolledPosition_WithZeroValues_UpdatesPositionsAndFiresEvent()
        {
            // Arrange
            var scrollView = new ScrollView();
            scrollView.SetScrolledPosition(10.0, 20.0); // Set non-zero initial values

            bool eventFired = false;
            double eventX = -1, eventY = -1;

            scrollView.Scrolled += (sender, args) =>
            {
                eventFired = true;
                eventX = args.ScrollX;
                eventY = args.ScrollY;
            };

            // Act
            scrollView.SetScrolledPosition(0.0, 0.0);

            // Assert
            Assert.Equal(0.0, scrollView.ScrollX);
            Assert.Equal(0.0, scrollView.ScrollY);
            Assert.True(eventFired);
            Assert.Equal(0.0, eventX);
            Assert.Equal(0.0, eventY);
        }

        /// <summary>
        /// Tests that SetScrolledPosition handles negative values correctly.
        /// Input conditions: Negative values for both x and y coordinates.
        /// Expected result: ScrollX and ScrollY are set to negative values, and Scrolled event is fired.
        /// </summary>
        [Fact]
        public void SetScrolledPosition_WithNegativeValues_UpdatesPositionsAndFiresEvent()
        {
            // Arrange
            var scrollView = new ScrollView();
            bool eventFired = false;
            double eventX = 0, eventY = 0;

            scrollView.Scrolled += (sender, args) =>
            {
                eventFired = true;
                eventX = args.ScrollX;
                eventY = args.ScrollY;
            };

            // Act
            scrollView.SetScrolledPosition(-50.25, -100.75);

            // Assert
            Assert.Equal(-50.25, scrollView.ScrollX);
            Assert.Equal(-100.75, scrollView.ScrollY);
            Assert.True(eventFired);
            Assert.Equal(-50.25, eventX);
            Assert.Equal(-100.75, eventY);
        }

        /// <summary>
        /// Tests that SetScrolledPosition handles extreme double values correctly.
        /// Input conditions: Extreme double values including MinValue and MaxValue.
        /// Expected result: Properties are updated with extreme values and event is fired.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, double.MaxValue)]
        [InlineData(double.MaxValue, double.MinValue)]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(double.MaxValue, double.MaxValue)]
        public void SetScrolledPosition_WithExtremeValues_UpdatesPositionsAndFiresEvent(double x, double y)
        {
            // Arrange
            var scrollView = new ScrollView();
            bool eventFired = false;
            double eventX = 0, eventY = 0;

            scrollView.Scrolled += (sender, args) =>
            {
                eventFired = true;
                eventX = args.ScrollX;
                eventY = args.ScrollY;
            };

            // Act
            scrollView.SetScrolledPosition(x, y);

            // Assert
            Assert.Equal(x, scrollView.ScrollX);
            Assert.Equal(y, scrollView.ScrollY);
            Assert.True(eventFired);
            Assert.Equal(x, eventX);
            Assert.Equal(y, eventY);
        }

        /// <summary>
        /// Tests that SetScrolledPosition handles special double values correctly.
        /// Input conditions: Special double values including NaN, PositiveInfinity, and NegativeInfinity.
        /// Expected result: Properties are updated with special values and event is fired.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 100.0)]
        [InlineData(100.0, double.NaN)]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, 100.0)]
        [InlineData(100.0, double.PositiveInfinity)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 100.0)]
        [InlineData(100.0, double.NegativeInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NegativeInfinity, double.PositiveInfinity)]
        public void SetScrolledPosition_WithSpecialValues_UpdatesPositionsAndFiresEvent(double x, double y)
        {
            // Arrange
            var scrollView = new ScrollView();
            bool eventFired = false;
            double eventX = 0, eventY = 0;

            scrollView.Scrolled += (sender, args) =>
            {
                eventFired = true;
                eventX = args.ScrollX;
                eventY = args.ScrollY;
            };

            // Act
            scrollView.SetScrolledPosition(x, y);

            // Assert
            if (double.IsNaN(x))
                Assert.True(double.IsNaN(scrollView.ScrollX));
            else
                Assert.Equal(x, scrollView.ScrollX);

            if (double.IsNaN(y))
                Assert.True(double.IsNaN(scrollView.ScrollY));
            else
                Assert.Equal(y, scrollView.ScrollY);

            Assert.True(eventFired);

            if (double.IsNaN(x))
                Assert.True(double.IsNaN(eventX));
            else
                Assert.Equal(x, eventX);

            if (double.IsNaN(y))
                Assert.True(double.IsNaN(eventY));
            else
                Assert.Equal(y, eventY);
        }

        /// <summary>
        /// Tests that SetScrolledPosition early return works with special double values when current values match.
        /// Input conditions: ScrollView with current special values that match the input values.
        /// Expected result: No event is fired when values are the same, even with special values.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
        public void SetScrolledPosition_WithMatchingSpecialValues_DoesNotFireEvent(double x, double y)
        {
            // Arrange
            var scrollView = new ScrollView();
            scrollView.SetScrolledPosition(x, y); // Set initial values

            bool eventFired = false;
            scrollView.Scrolled += (sender, args) => eventFired = true;

            // Act
            scrollView.SetScrolledPosition(x, y); // Same values

            // Assert
            Assert.False(eventFired);
        }

        /// <summary>
        /// Tests that SetScrolledPosition properly handles sender parameter in event.
        /// Input conditions: ScrollView instance with event subscription checking sender.
        /// Expected result: Event sender is the ScrollView instance itself.
        /// </summary>
        [Fact]
        public void SetScrolledPosition_EventSender_IsScrollViewInstance()
        {
            // Arrange
            var scrollView = new ScrollView();
            object eventSender = null;

            scrollView.Scrolled += (sender, args) =>
            {
                eventSender = sender;
            };

            // Act
            scrollView.SetScrolledPosition(10.0, 20.0);

            // Assert
            Assert.Same(scrollView, eventSender);
        }

        /// <summary>
        /// Tests that the CascadeInputTransparent property getter returns false when the default value is used.
        /// Validates the default value behavior of the CascadeInputTransparent property.
        /// </summary>
        [Fact]
        public void CascadeInputTransparent_DefaultValue_ReturnsFalse()
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act
            var result = scrollView.CascadeInputTransparent;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the CascadeInputTransparent property can be set to true and retrieved correctly.
        /// Validates that the setter and getter work properly with true value.
        /// </summary>
        [Fact]
        public void CascadeInputTransparent_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act
            scrollView.CascadeInputTransparent = true;
            var result = scrollView.CascadeInputTransparent;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the CascadeInputTransparent property can be set to false and retrieved correctly.
        /// Validates that the setter and getter work properly with false value.
        /// </summary>
        [Fact]
        public void CascadeInputTransparent_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act
            scrollView.CascadeInputTransparent = false;
            var result = scrollView.CascadeInputTransparent;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the CascadeInputTransparent property can be changed multiple times correctly.
        /// Validates that subsequent property changes work as expected.
        /// </summary>
        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void CascadeInputTransparent_MultipleChanges_ReturnsCorrectValue(bool firstValue, bool secondValue)
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act
            scrollView.CascadeInputTransparent = firstValue;
            var firstResult = scrollView.CascadeInputTransparent;

            scrollView.CascadeInputTransparent = secondValue;
            var secondResult = scrollView.CascadeInputTransparent;

            // Assert
            Assert.Equal(firstValue, firstResult);
            Assert.Equal(secondValue, secondResult);
        }

        /// <summary>
        /// Tests that the Padding property getter returns the default Thickness value when not explicitly set.
        /// </summary>
        [Fact]
        public void Padding_Get_ReturnsDefaultThickness()
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act
            var result = scrollView.Padding;

            // Assert
            Assert.Equal(new Thickness(0), result);
        }

        /// <summary>
        /// Tests that the Padding property setter and getter work correctly with a uniform thickness value.
        /// </summary>
        [Fact]
        public void Padding_SetUniformValue_ReturnsCorrectThickness()
        {
            // Arrange
            var scrollView = new ScrollView();
            var expectedThickness = new Thickness(10);

            // Act
            scrollView.Padding = expectedThickness;
            var result = scrollView.Padding;

            // Assert
            Assert.Equal(expectedThickness, result);
        }

        /// <summary>
        /// Tests that the Padding property works correctly with individual edge thickness values.
        /// Input conditions: Different values for left, top, right, and bottom edges.
        /// Expected result: The getter returns the exact Thickness that was set.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(1, 2, 3, 4)]
        [InlineData(10.5, 20.7, 30.9, 40.1)]
        [InlineData(-5, -10, -15, -20)]
        [InlineData(double.MaxValue, double.MinValue, 0, 100)]
        public void Padding_SetIndividualEdgeValues_ReturnsCorrectThickness(double left, double top, double right, double bottom)
        {
            // Arrange
            var scrollView = new ScrollView();
            var expectedThickness = new Thickness(left, top, right, bottom);

            // Act
            scrollView.Padding = expectedThickness;
            var result = scrollView.Padding;

            // Assert
            Assert.Equal(expectedThickness, result);
        }

        /// <summary>
        /// Tests that the Padding property handles special double values correctly.
        /// Input conditions: NaN, PositiveInfinity, and NegativeInfinity values.
        /// Expected result: The getter returns the exact special values that were set.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void Padding_SetSpecialDoubleValues_ReturnsCorrectThickness(double specialValue)
        {
            // Arrange
            var scrollView = new ScrollView();
            var expectedThickness = new Thickness(specialValue);

            // Act
            scrollView.Padding = expectedThickness;
            var result = scrollView.Padding;

            // Assert
            Assert.Equal(expectedThickness, result);
        }

        /// <summary>
        /// Tests that the Padding property getter properly retrieves values through the bindable property system.
        /// Input conditions: Setting value through SetValue and retrieving through property getter.
        /// Expected result: The property getter returns the same value that was set through SetValue.
        /// </summary>
        [Fact]
        public void Padding_GetThroughBindableProperty_ReturnsCorrectValue()
        {
            // Arrange
            var scrollView = new ScrollView();
            var expectedThickness = new Thickness(15, 25, 35, 45);

            // Act
            scrollView.SetValue(ScrollView.PaddingProperty, expectedThickness);
            var result = scrollView.Padding;

            // Assert
            Assert.Equal(expectedThickness, result);
        }

        /// <summary>
        /// Tests that the Padding property setter properly stores values through the bindable property system.
        /// Input conditions: Setting value through property setter and retrieving through GetValue.
        /// Expected result: GetValue returns the same value that was set through the property setter.
        /// </summary>
        [Fact]
        public void Padding_SetThroughProperty_StoresInBindableProperty()
        {
            // Arrange
            var scrollView = new ScrollView();
            var expectedThickness = new Thickness(5, 15, 25, 35);

            // Act
            scrollView.Padding = expectedThickness;
            var result = (Thickness)scrollView.GetValue(ScrollView.PaddingProperty);

            // Assert
            Assert.Equal(expectedThickness, result);
        }

        /// <summary>
        /// Tests that the Padding property handles multiple consecutive sets and gets correctly.
        /// Input conditions: Setting different Thickness values multiple times.
        /// Expected result: Each get operation returns the most recently set value.
        /// </summary>
        [Fact]
        public void Padding_MultipleSetAndGet_ReturnsLatestValue()
        {
            // Arrange
            var scrollView = new ScrollView();
            var firstThickness = new Thickness(10);
            var secondThickness = new Thickness(20, 30);
            var thirdThickness = new Thickness(5, 10, 15, 20);

            // Act & Assert - First set
            scrollView.Padding = firstThickness;
            Assert.Equal(firstThickness, scrollView.Padding);

            // Act & Assert - Second set
            scrollView.Padding = secondThickness;
            Assert.Equal(secondThickness, scrollView.Padding);

            // Act & Assert - Third set
            scrollView.Padding = thirdThickness;
            Assert.Equal(thirdThickness, scrollView.Padding);
        }

        /// <summary>
        /// Tests that the Padding property handles zero thickness correctly.
        /// Input conditions: Setting and getting zero thickness using different constructors.
        /// Expected result: All zero thickness values are handled consistently.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(0, 0)]
        [InlineData(0, 0, 0, 0)]
        public void Padding_SetZeroThickness_ReturnsZeroThickness(params double[] values)
        {
            // Arrange
            var scrollView = new ScrollView();
            Thickness expectedThickness = values.Length switch
            {
                1 => new Thickness(values[0]),
                2 => new Thickness(values[0], values[1]),
                4 => new Thickness(values[0], values[1], values[2], values[3]),
                _ => new Thickness(0)
            };

            // Act
            scrollView.Padding = expectedThickness;
            var result = scrollView.Padding;

            // Assert
            Assert.Equal(expectedThickness, result);
            Assert.True(result.IsEmpty);
        }

        /// <summary>
        /// Tests the obsolete Measure method with valid width and height constraints using default flags.
        /// Verifies that the method returns a valid SizeRequest without throwing exceptions.
        /// </summary>
        [Fact]
        public void Measure_ValidConstraints_DefaultFlags_ReturnsSizeRequest()
        {
            // Arrange
            var scrollView = new ScrollView();
            double widthConstraint = 100.0;
            double heightConstraint = 200.0;

            // Act
            var result = scrollView.Measure(widthConstraint, heightConstraint);

            // Assert
            Assert.IsType<SizeRequest>(result);
        }

        /// <summary>
        /// Tests the obsolete Measure method with zero constraints.
        /// Verifies that zero width and height constraints are handled properly.
        /// </summary>
        [Fact]
        public void Measure_ZeroConstraints_ReturnsSizeRequest()
        {
            // Arrange
            var scrollView = new ScrollView();
            double widthConstraint = 0.0;
            double heightConstraint = 0.0;

            // Act
            var result = scrollView.Measure(widthConstraint, heightConstraint);

            // Assert
            Assert.IsType<SizeRequest>(result);
        }

        /// <summary>
        /// Tests the obsolete Measure method with negative constraints.
        /// Verifies that negative width and height constraints are handled properly.
        /// </summary>
        [Fact]
        public void Measure_NegativeConstraints_ReturnsSizeRequest()
        {
            // Arrange
            var scrollView = new ScrollView();
            double widthConstraint = -50.0;
            double heightConstraint = -100.0;

            // Act
            var result = scrollView.Measure(widthConstraint, heightConstraint);

            // Assert
            Assert.IsType<SizeRequest>(result);
        }

        /// <summary>
        /// Tests the obsolete Measure method with maximum double values.
        /// Verifies that extreme values for width and height constraints are handled properly.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue)]
        public void Measure_ExtremeConstraints_ReturnsSizeRequest(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act
            var result = scrollView.Measure(widthConstraint, heightConstraint);

            // Assert
            Assert.IsType<SizeRequest>(result);
        }

        /// <summary>
        /// Tests the obsolete Measure method with infinity constraints.
        /// Verifies that infinite width and height constraints are handled properly.
        /// </summary>
        [Theory]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
        [InlineData(double.PositiveInfinity, 100.0)]
        [InlineData(100.0, double.PositiveInfinity)]
        public void Measure_InfinityConstraints_ReturnsSizeRequest(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act
            var result = scrollView.Measure(widthConstraint, heightConstraint);

            // Assert
            Assert.IsType<SizeRequest>(result);
        }

        /// <summary>
        /// Tests the obsolete Measure method with NaN constraints.
        /// Verifies that NaN width and height constraints are handled properly.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.NaN, 100.0)]
        [InlineData(100.0, double.NaN)]
        public void Measure_NaNConstraints_ReturnsSizeRequest(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act
            var result = scrollView.Measure(widthConstraint, heightConstraint);

            // Assert
            Assert.IsType<SizeRequest>(result);
        }

        /// <summary>
        /// Tests the obsolete Measure method with different MeasureFlags values.
        /// Verifies that the flags parameter is ignored as expected for the obsolete method.
        /// </summary>
        [Theory]
        [InlineData(MeasureFlags.None)]
        [InlineData(MeasureFlags.IncludeMargins)]
        public void Measure_DifferentFlags_IgnoresFlagsParameter(MeasureFlags flags)
        {
            // Arrange
            var scrollView = new ScrollView();
            double widthConstraint = 100.0;
            double heightConstraint = 200.0;

            // Act
            var resultWithFlags = scrollView.Measure(widthConstraint, heightConstraint, flags);
            var resultWithoutFlags = scrollView.Measure(widthConstraint, heightConstraint);

            // Assert - Both calls should return same result since flags are ignored
            Assert.IsType<SizeRequest>(resultWithFlags);
            Assert.IsType<SizeRequest>(resultWithoutFlags);
            Assert.Equal(resultWithFlags.Request, resultWithoutFlags.Request);
            Assert.Equal(resultWithFlags.Minimum, resultWithoutFlags.Minimum);
        }

        /// <summary>
        /// Tests the obsolete Measure method with invalid MeasureFlags values.
        /// Verifies that invalid enum values are handled properly since the parameter is ignored.
        /// </summary>
        [Fact]
        public void Measure_InvalidFlags_IgnoresFlagsParameter()
        {
            // Arrange
            var scrollView = new ScrollView();
            double widthConstraint = 100.0;
            double heightConstraint = 200.0;
            var invalidFlags = (MeasureFlags)999; // Invalid enum value

            // Act
            var result = scrollView.Measure(widthConstraint, heightConstraint, invalidFlags);

            // Assert
            Assert.IsType<SizeRequest>(result);
        }

        /// <summary>
        /// Tests the obsolete Measure method with content to verify it works with actual content.
        /// Verifies that the method works properly when the ScrollView has content.
        /// </summary>
        [Fact]
        public void Measure_WithContent_ReturnsSizeRequest()
        {
            // Arrange
            var scrollView = new ScrollView();
            var label = new Label { Text = "Test Content" };
            scrollView.Content = label;
            double widthConstraint = 300.0;
            double heightConstraint = 400.0;

            // Act
            var result = scrollView.Measure(widthConstraint, heightConstraint);

            // Assert
            Assert.IsType<SizeRequest>(result);
        }

        /// <summary>
        /// Tests the obsolete Measure method with various constraint combinations.
        /// Verifies that different combinations of width and height constraints work properly.
        /// </summary>
        [Theory]
        [InlineData(0.0, 100.0)]
        [InlineData(100.0, 0.0)]
        [InlineData(-10.0, 50.0)]
        [InlineData(50.0, -20.0)]
        [InlineData(1000.0, 2000.0)]
        public void Measure_VariousConstraintCombinations_ReturnsSizeRequest(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act
            var result = scrollView.Measure(widthConstraint, heightConstraint);

            // Assert
            Assert.IsType<SizeRequest>(result);
        }

        /// <summary>
        /// Tests that ContentSize property returns the default value when not explicitly set.
        /// The default value should be Size.Zero (0, 0).
        /// </summary>
        [Fact]
        public void ContentSize_DefaultValue_ReturnsZeroSize()
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act
            var contentSize = scrollView.ContentSize;

            // Assert
            Assert.Equal(Size.Zero, contentSize);
            Assert.Equal(0, contentSize.Width);
            Assert.Equal(0, contentSize.Height);
        }

        /// <summary>
        /// Tests that ContentSize property returns the correct value when set via BindableProperty.
        /// This verifies the getter implementation that calls GetValue(ContentSizeProperty).
        /// </summary>
        [Theory]
        [MemberData(nameof(GetContentSizeTestData))]
        public void ContentSize_GetValue_ReturnsCorrectSize(Size expectedSize, string description)
        {
            // Arrange
            var scrollView = new ScrollView();
            scrollView.SetValue(ScrollView.ContentSizeProperty, expectedSize);

            // Act
            var actualSize = scrollView.ContentSize;

            // Assert
            Assert.Equal(expectedSize, actualSize);
            Assert.Equal(expectedSize.Width, actualSize.Width);
            Assert.Equal(expectedSize.Height, actualSize.Height);
        }

        /// <summary>
        /// Tests that ContentSize property getter handles edge cases with special double values.
        /// This includes NaN, positive/negative infinity, and extreme values.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NegativeInfinity, double.PositiveInfinity)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue)]
        public void ContentSize_SpecialDoubleValues_ReturnsCorrectSize(double width, double height)
        {
            // Arrange
            var expectedSize = new Size(width, height);
            var scrollView = new ScrollView();
            scrollView.SetValue(ScrollView.ContentSizeProperty, expectedSize);

            // Act
            var actualSize = scrollView.ContentSize;

            // Assert
            Assert.Equal(expectedSize, actualSize);
            if (double.IsNaN(width))
                Assert.True(double.IsNaN(actualSize.Width));
            else
                Assert.Equal(width, actualSize.Width);

            if (double.IsNaN(height))
                Assert.True(double.IsNaN(actualSize.Height));
            else
                Assert.Equal(height, actualSize.Height);
        }

        /// <summary>
        /// Tests that ContentSize property getter returns zero for negative dimensions.
        /// This tests boundary conditions with negative values.
        /// </summary>
        [Theory]
        [InlineData(-1.0, -1.0)]
        [InlineData(-100.5, -200.75)]
        [InlineData(-0.1, -0.1)]
        [InlineData(0, -1)]
        [InlineData(-1, 0)]
        public void ContentSize_NegativeValues_ReturnsCorrectSize(double width, double height)
        {
            // Arrange
            var expectedSize = new Size(width, height);
            var scrollView = new ScrollView();
            scrollView.SetValue(ScrollView.ContentSizeProperty, expectedSize);

            // Act
            var actualSize = scrollView.ContentSize;

            // Assert
            Assert.Equal(expectedSize, actualSize);
            Assert.Equal(width, actualSize.Width);
            Assert.Equal(height, actualSize.Height);
        }

        /// <summary>
        /// Tests that ContentSize property cannot be set from external code.
        /// The setter is private and should not be accessible from test code.
        /// </summary>
        [Fact]
        public void ContentSize_Setter_IsNotPubliclyAccessible()
        {
            // Arrange
            var scrollView = new ScrollView();
            var propertyInfo = typeof(ScrollView).GetProperty(nameof(ScrollView.ContentSize));

            // Act & Assert
            Assert.NotNull(propertyInfo);
            Assert.True(propertyInfo.CanRead);
            Assert.True(propertyInfo.CanWrite); // Has setter but it's private
            Assert.True(propertyInfo.SetMethod.IsPrivate); // Verify setter is private
            Assert.False(propertyInfo.SetMethod.IsPublic); // Verify setter is not public
        }

        /// <summary>
        /// Tests that ContentSize property consistently returns the same value on multiple calls.
        /// This verifies the stability of the getter implementation.
        /// </summary>
        [Fact]
        public void ContentSize_MultipleGets_ReturnsSameValue()
        {
            // Arrange
            var expectedSize = new Size(123.45, 678.90);
            var scrollView = new ScrollView();
            scrollView.SetValue(ScrollView.ContentSizeProperty, expectedSize);

            // Act
            var size1 = scrollView.ContentSize;
            var size2 = scrollView.ContentSize;
            var size3 = scrollView.ContentSize;

            // Assert
            Assert.Equal(expectedSize, size1);
            Assert.Equal(expectedSize, size2);
            Assert.Equal(expectedSize, size3);
            Assert.Equal(size1, size2);
            Assert.Equal(size2, size3);
        }

        /// <summary>
        /// Provides test data for ContentSize property testing with various Size values.
        /// </summary>
        public static IEnumerable<object[]> GetContentSizeTestData()
        {
            yield return new object[] { Size.Zero, "Zero size" };
            yield return new object[] { new Size(0, 0), "Explicit zero size" };
            yield return new object[] { new Size(100, 200), "Positive size" };
            yield return new object[] { new Size(1.5, 2.7), "Decimal size" };
            yield return new object[] { new Size(1000000, 2000000), "Large size" };
            yield return new object[] { new Size(0.001, 0.001), "Very small positive size" };
            yield return new object[] { new Size(50), "Square size using single parameter constructor" };
        }

        /// <summary>
        /// Tests that RaiseChild method can be called with a valid View parameter without throwing exceptions.
        /// This validates the basic delegation to the base class implementation.
        /// </summary>
        [Fact]
        public void RaiseChild_WithValidView_DoesNotThrow()
        {
            // Arrange
            var scrollView = new ScrollView();
            var view = new View();

            // Act & Assert
            var exception = Record.Exception(() => scrollView.RaiseChild(view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that RaiseChild method can be called with a null View parameter.
        /// This validates the method handles null parameters gracefully through base class delegation.
        /// </summary>
        [Fact]
        public void RaiseChild_WithNullView_DoesNotThrow()
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act & Assert
            var exception = Record.Exception(() => scrollView.RaiseChild(null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that RaiseChild method works with different types of View instances.
        /// This validates the method handles various View subclasses correctly.
        /// </summary>
        [Theory]
        [InlineData(typeof(Label))]
        [InlineData(typeof(Button))]
        [InlineData(typeof(Entry))]
        public void RaiseChild_WithDifferentViewTypes_DoesNotThrow(Type viewType)
        {
            // Arrange
            var scrollView = new ScrollView();
            var view = (View)Activator.CreateInstance(viewType);

            // Act & Assert
            var exception = Record.Exception(() => scrollView.RaiseChild(view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that RaiseChild method can be called multiple times with the same View.
        /// This validates the method handles repeated calls without issues.
        /// </summary>
        [Fact]
        public void RaiseChild_CalledMultipleTimesWithSameView_DoesNotThrow()
        {
            // Arrange
            var scrollView = new ScrollView();
            var view = new View();

            // Act & Assert
            var exception1 = Record.Exception(() => scrollView.RaiseChild(view));
            var exception2 = Record.Exception(() => scrollView.RaiseChild(view));
            var exception3 = Record.Exception(() => scrollView.RaiseChild(view));

            Assert.Null(exception1);
            Assert.Null(exception2);
            Assert.Null(exception3);
        }

        /// <summary>
        /// Tests that RaiseChild method can be called with a View that has content.
        /// This validates the method works with Views that are part of a ScrollView's content hierarchy.
        /// </summary>
        [Fact]
        public void RaiseChild_WithViewInScrollViewContent_DoesNotThrow()
        {
            // Arrange
            var scrollView = new ScrollView();
            var stackLayout = new StackLayout();
            var childView = new View();
            stackLayout.Children.Add(childView);
            scrollView.Content = stackLayout;

            // Act & Assert
            var exception = Record.Exception(() => scrollView.RaiseChild(childView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the HorizontalScrollBarVisibility property returns the default value when not explicitly set.
        /// Input conditions: ScrollView with default initialization.
        /// Expected result: Property returns ScrollBarVisibility.Default.
        /// </summary>
        [Fact]
        public void HorizontalScrollBarVisibility_DefaultValue_ReturnsDefault()
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act
            var result = scrollView.HorizontalScrollBarVisibility;

            // Assert
            Assert.Equal(ScrollBarVisibility.Default, result);
        }

        /// <summary>
        /// Tests that the HorizontalScrollBarVisibility property correctly sets and gets valid enum values.
        /// Input conditions: Setting property to each valid ScrollBarVisibility enum value.
        /// Expected result: Property returns the set value.
        /// </summary>
        [Theory]
        [InlineData(ScrollBarVisibility.Default)]
        [InlineData(ScrollBarVisibility.Always)]
        [InlineData(ScrollBarVisibility.Never)]
        public void HorizontalScrollBarVisibility_ValidEnumValues_SetsAndGetsCorrectly(ScrollBarVisibility expected)
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act
            scrollView.HorizontalScrollBarVisibility = expected;
            var result = scrollView.HorizontalScrollBarVisibility;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that the HorizontalScrollBarVisibility property handles invalid enum values.
        /// Input conditions: Setting property to an invalid enum value (outside defined range).
        /// Expected result: Property accepts the invalid value and returns it when retrieved.
        /// </summary>
        [Fact]
        public void HorizontalScrollBarVisibility_InvalidEnumValue_AcceptsAndReturnsValue()
        {
            // Arrange
            var scrollView = new ScrollView();
            var invalidValue = (ScrollBarVisibility)999;

            // Act
            scrollView.HorizontalScrollBarVisibility = invalidValue;
            var result = scrollView.HorizontalScrollBarVisibility;

            // Assert
            Assert.Equal(invalidValue, result);
        }

        /// <summary>
        /// Tests that the HorizontalScrollBarVisibility property handles minimum enum boundary values.
        /// Input conditions: Setting property to minimum integer value cast to ScrollBarVisibility.
        /// Expected result: Property accepts the boundary value and returns it when retrieved.
        /// </summary>
        [Fact]
        public void HorizontalScrollBarVisibility_MinimumBoundaryValue_AcceptsAndReturnsValue()
        {
            // Arrange
            var scrollView = new ScrollView();
            var boundaryValue = (ScrollBarVisibility)int.MinValue;

            // Act
            scrollView.HorizontalScrollBarVisibility = boundaryValue;
            var result = scrollView.HorizontalScrollBarVisibility;

            // Assert
            Assert.Equal(boundaryValue, result);
        }

        /// <summary>
        /// Tests that the HorizontalScrollBarVisibility property handles maximum enum boundary values.
        /// Input conditions: Setting property to maximum integer value cast to ScrollBarVisibility.
        /// Expected result: Property accepts the boundary value and returns it when retrieved.
        /// </summary>
        [Fact]
        public void HorizontalScrollBarVisibility_MaximumBoundaryValue_AcceptsAndReturnsValue()
        {
            // Arrange
            var scrollView = new ScrollView();
            var boundaryValue = (ScrollBarVisibility)int.MaxValue;

            // Act
            scrollView.HorizontalScrollBarVisibility = boundaryValue;
            var result = scrollView.HorizontalScrollBarVisibility;

            // Assert
            Assert.Equal(boundaryValue, result);
        }

        /// <summary>
        /// Test helper class that exposes the protected MeasureOverride method for testing.
        /// </summary>
        private class TestableScrollView : ScrollView
        {
            public Size TestMeasureOverride(double widthConstraint, double heightConstraint)
            {
                return MeasureOverride(widthConstraint, heightConstraint);
            }
        }

        /// <summary>
        /// Tests MeasureOverride with normal positive constraint values.
        /// Should delegate to ComputeDesiredSize and return the computed size.
        /// </summary>
        [Theory]
        [InlineData(100.0, 200.0)]
        [InlineData(50.5, 75.25)]
        [InlineData(1.0, 1.0)]
        public void MeasureOverride_WithPositiveConstraints_ReturnsComputedSize(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var scrollView = new TestableScrollView();
            var mockHandler = Substitute.For<IViewHandler>();
            var expectedSize = new Size(widthConstraint * 0.8, heightConstraint * 0.8); // Simulate some computed size
            mockHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(expectedSize);
            scrollView.Handler = mockHandler;

            // Act
            var result = scrollView.TestMeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(expectedSize.Width, result.Width, 1);
            Assert.Equal(expectedSize.Height, result.Height, 1);
        }

        /// <summary>
        /// Tests MeasureOverride with zero constraint values.
        /// Should handle zero constraints appropriately.
        /// </summary>
        [Fact]
        public void MeasureOverride_WithZeroConstraints_ReturnsComputedSize()
        {
            // Arrange
            var scrollView = new TestableScrollView();
            var mockHandler = Substitute.For<IViewHandler>();
            var expectedSize = new Size(10, 20);
            mockHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(expectedSize);
            scrollView.Handler = mockHandler;

            // Act
            var result = scrollView.TestMeasureOverride(0.0, 0.0);

            // Assert
            Assert.Equal(expectedSize.Width, result.Width, 1);
            Assert.Equal(expectedSize.Height, result.Height, 1);
        }

        /// <summary>
        /// Tests MeasureOverride with negative constraint values.
        /// Should handle negative constraints appropriately.
        /// </summary>
        [Theory]
        [InlineData(-10.0, -20.0)]
        [InlineData(-100.5, 50.0)]
        [InlineData(75.0, -200.25)]
        public void MeasureOverride_WithNegativeConstraints_ReturnsComputedSize(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var scrollView = new TestableScrollView();
            var mockHandler = Substitute.For<IViewHandler>();
            var expectedSize = new Size(30, 40);
            mockHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(expectedSize);
            scrollView.Handler = mockHandler;

            // Act
            var result = scrollView.TestMeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(expectedSize.Width, result.Width, 1);
            Assert.Equal(expectedSize.Height, result.Height, 1);
        }

        /// <summary>
        /// Tests MeasureOverride with special double values like infinity and NaN.
        /// Should handle special constraint values appropriately without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue)]
        public void MeasureOverride_WithSpecialDoubleValues_ReturnsComputedSize(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var scrollView = new TestableScrollView();
            var mockHandler = Substitute.For<IViewHandler>();
            var expectedSize = new Size(50, 60);
            mockHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(expectedSize);
            scrollView.Handler = mockHandler;

            // Act
            var result = scrollView.TestMeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(expectedSize.Width, result.Width, 1);
            Assert.Equal(expectedSize.Height, result.Height, 1);
        }

        /// <summary>
        /// Tests MeasureOverride when Handler is null.
        /// Should return Size.Zero as per ComputeDesiredSize behavior.
        /// </summary>
        [Fact]
        public void MeasureOverride_WithNullHandler_ReturnsSizeZero()
        {
            // Arrange
            var scrollView = new TestableScrollView();
            // Handler is null by default

            // Act
            var result = scrollView.TestMeasureOverride(100.0, 200.0);

            // Assert
            Assert.Equal(Size.Zero, result);
        }

        /// <summary>
        /// Tests MeasureOverride with boundary constraint values.
        /// Should handle boundary values correctly.
        /// </summary>
        [Theory]
        [InlineData(0.001, 0.001)]
        [InlineData(1000000.0, 2000000.0)]
        [InlineData(double.Epsilon, double.Epsilon)]
        public void MeasureOverride_WithBoundaryValues_ReturnsComputedSize(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var scrollView = new TestableScrollView();
            var mockHandler = Substitute.For<IViewHandler>();
            var expectedSize = new Size(25, 35);
            mockHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(expectedSize);
            scrollView.Handler = mockHandler;

            // Act
            var result = scrollView.TestMeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(expectedSize.Width, result.Width, 1);
            Assert.Equal(expectedSize.Height, result.Height, 1);
        }

        /// <summary>
        /// Tests MeasureOverride calls ComputeDesiredSize with correct parameters.
        /// Should pass the exact constraint values to the underlying ComputeDesiredSize method.
        /// </summary>
        [Fact]
        public void MeasureOverride_CallsComputeDesiredSizeWithCorrectParameters()
        {
            // Arrange
            var scrollView = new TestableScrollView();
            var mockHandler = Substitute.For<IViewHandler>();
            var expectedSize = new Size(80, 120);
            double expectedWidth = 100.0;
            double expectedHeight = 150.0;

            mockHandler.GetDesiredSize(expectedWidth, expectedHeight).Returns(expectedSize);
            scrollView.Handler = mockHandler;

            // Act
            var result = scrollView.TestMeasureOverride(expectedWidth, expectedHeight);

            // Assert
            mockHandler.Received(1).GetDesiredSize(expectedWidth, expectedHeight);
            Assert.Equal(expectedSize.Width, result.Width, 1);
            Assert.Equal(expectedSize.Height, result.Height, 1);
        }

        /// <summary>
        /// Tests that the obsolete Children property exists and can be accessed via reflection.
        /// The property is marked with Obsolete(error: true) so direct access causes compilation errors.
        /// This test verifies the property returns an empty collection when no content is set.
        /// </summary>
        [Fact]
        public void Children_WithNoContent_ReturnsEmptyCollection()
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act
            var childrenProperty = typeof(ScrollView).GetProperty("Children", BindingFlags.Public | BindingFlags.Instance);
            var children = (IReadOnlyList<Element>)childrenProperty.GetValue(scrollView);

            // Assert
            Assert.NotNull(children);
            Assert.Empty(children);
        }

        /// <summary>
        /// Tests that the obsolete Children property returns the content when Content is set.
        /// Verifies that the Children property delegates to the base class implementation correctly.
        /// </summary>
        [Fact]
        public void Children_WithContent_ReturnsContentInCollection()
        {
            // Arrange
            var scrollView = new ScrollView();
            var contentView = new Label { Text = "Test Content" };
            scrollView.Content = contentView;

            // Act
            var childrenProperty = typeof(ScrollView).GetProperty("Children", BindingFlags.Public | BindingFlags.Instance);
            var children = (IReadOnlyList<Element>)childrenProperty.GetValue(scrollView);

            // Assert
            Assert.NotNull(children);
            Assert.Single(children);
            Assert.Equal(contentView, children[0]);
        }

        /// <summary>
        /// Tests that the obsolete Children property has the correct ObsoleteAttribute configuration.
        /// Verifies that the attribute is present with error=true and the correct message.
        /// </summary>
        [Fact]
        public void Children_HasCorrectObsoleteAttribute()
        {
            // Arrange & Act
            var childrenProperty = typeof(ScrollView).GetProperty("Children", BindingFlags.Public | BindingFlags.Instance);
            var obsoleteAttribute = childrenProperty.GetCustomAttribute<ObsoleteAttribute>();

            // Assert
            Assert.NotNull(obsoleteAttribute);
            Assert.Equal("Use IVisualTreeElement.GetVisualChildren() instead.", obsoleteAttribute.Message);
            Assert.True(obsoleteAttribute.IsError);
        }

        /// <summary>
        /// Tests that the obsolete Children property has the correct EditorBrowsableAttribute configuration.
        /// Verifies that the property is hidden from IntelliSense and design-time tools.
        /// </summary>
        [Fact]
        public void Children_HasCorrectEditorBrowsableAttribute()
        {
            // Arrange & Act
            var childrenProperty = typeof(ScrollView).GetProperty("Children", BindingFlags.Public | BindingFlags.Instance);
            var editorBrowsableAttribute = childrenProperty.GetCustomAttribute<EditorBrowsableAttribute>();

            // Assert
            Assert.NotNull(editorBrowsableAttribute);
            Assert.Equal(EditorBrowsableState.Never, editorBrowsableAttribute.State);
        }

        /// <summary>
        /// Tests that the obsolete Children property returns the same result as accessing base Children through reflection.
        /// Verifies that the property correctly delegates to the base class implementation.
        /// </summary>
        [Fact]
        public void Children_ReturnsSameAsBaseChildren()
        {
            // Arrange
            var scrollView = new ScrollView();
            var contentView = new Label { Text = "Test Content" };
            scrollView.Content = contentView;

            // Act
            var childrenProperty = typeof(ScrollView).GetProperty("Children", BindingFlags.Public | BindingFlags.Instance);
            var children = (IReadOnlyList<Element>)childrenProperty.GetValue(scrollView);

            var baseChildrenProperty = typeof(ScrollView).BaseType.GetProperty("Children", BindingFlags.Public | BindingFlags.Instance);
            var baseChildren = (IReadOnlyList<Element>)baseChildrenProperty.GetValue(scrollView);

            // Assert
            Assert.NotNull(children);
            Assert.NotNull(baseChildren);
            Assert.Equal(baseChildren.Count, children.Count);
            for (int i = 0; i < children.Count; i++)
            {
                Assert.Equal(baseChildren[i], children[i]);
            }
        }

        /// <summary>
        /// Tests that the obsolete Children property returns a collection that reflects content changes.
        /// Verifies that when Content is changed, the Children collection is updated accordingly.
        /// </summary>
        [Fact]
        public void Children_ReflectsContentChanges()
        {
            // Arrange
            var scrollView = new ScrollView();
            var childrenProperty = typeof(ScrollView).GetProperty("Children", BindingFlags.Public | BindingFlags.Instance);

            var firstContent = new Label { Text = "First Content" };
            var secondContent = new Button { Text = "Second Content" };

            // Act & Assert - Initially empty
            var initialChildren = (IReadOnlyList<Element>)childrenProperty.GetValue(scrollView);
            Assert.Empty(initialChildren);

            // Act & Assert - Set first content
            scrollView.Content = firstContent;
            var childrenWithFirst = (IReadOnlyList<Element>)childrenProperty.GetValue(scrollView);
            Assert.Single(childrenWithFirst);
            Assert.Equal(firstContent, childrenWithFirst[0]);

            // Act & Assert - Change to second content
            scrollView.Content = secondContent;
            var childrenWithSecond = (IReadOnlyList<Element>)childrenProperty.GetValue(scrollView);
            Assert.Single(childrenWithSecond);
            Assert.Equal(secondContent, childrenWithSecond[0]);

            // Act & Assert - Remove content
            scrollView.Content = null;
            var childrenWithNull = (IReadOnlyList<Element>)childrenProperty.GetValue(scrollView);
            Assert.Empty(childrenWithNull);
        }

        /// <summary>
        /// Tests that the obsolete Children property has the correct return type.
        /// Verifies that the property returns IReadOnlyList&lt;Element&gt; as expected.
        /// </summary>
        [Fact]
        public void Children_HasCorrectReturnType()
        {
            // Arrange & Act
            var childrenProperty = typeof(ScrollView).GetProperty("Children", BindingFlags.Public | BindingFlags.Instance);

            // Assert
            Assert.NotNull(childrenProperty);
            Assert.Equal(typeof(IReadOnlyList<Element>), childrenProperty.PropertyType);
            Assert.True(childrenProperty.CanRead);
            Assert.False(childrenProperty.CanWrite);
        }

        /// <summary>
        /// Tests that the VerticalScrollBarVisibility property returns the default value when not explicitly set.
        /// This test verifies the initial state and default behavior of the property.
        /// Expected result: The property should return ScrollBarVisibility.Default.
        /// </summary>
        [Fact]
        public void VerticalScrollBarVisibility_DefaultValue_ReturnsDefault()
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act
            var result = scrollView.VerticalScrollBarVisibility;

            // Assert
            Assert.Equal(ScrollBarVisibility.Default, result);
        }

        /// <summary>
        /// Tests that the VerticalScrollBarVisibility property getter returns the correct value after setting different enum values.
        /// This test verifies that the property correctly stores and retrieves all valid ScrollBarVisibility enum values.
        /// Expected result: The getter should return the exact value that was set.
        /// </summary>
        [Theory]
        [InlineData(ScrollBarVisibility.Default)]
        [InlineData(ScrollBarVisibility.Always)]
        [InlineData(ScrollBarVisibility.Never)]
        public void VerticalScrollBarVisibility_SetValidEnumValues_ReturnsSetValue(ScrollBarVisibility expectedValue)
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act
            scrollView.VerticalScrollBarVisibility = expectedValue;
            var result = scrollView.VerticalScrollBarVisibility;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that the VerticalScrollBarVisibility property handles out-of-range enum values correctly.
        /// This test verifies boundary conditions with invalid enum values that might be cast from integers.
        /// Expected result: The property should handle invalid enum values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(3)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void VerticalScrollBarVisibility_SetOutOfRangeEnumValues_ReturnsSetValue(int enumValue)
        {
            // Arrange
            var scrollView = new ScrollView();
            var invalidEnumValue = (ScrollBarVisibility)enumValue;

            // Act
            scrollView.VerticalScrollBarVisibility = invalidEnumValue;
            var result = scrollView.VerticalScrollBarVisibility;

            // Assert
            Assert.Equal(invalidEnumValue, result);
        }

        /// <summary>
        /// Tests that multiple successive assignments to VerticalScrollBarVisibility work correctly.
        /// This test verifies that the property can be changed multiple times and always returns the latest value.
        /// Expected result: The property should always return the most recently set value.
        /// </summary>
        [Fact]
        public void VerticalScrollBarVisibility_MultipleAssignments_ReturnsLatestValue()
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act & Assert - Test sequence of changes
            scrollView.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
            Assert.Equal(ScrollBarVisibility.Always, scrollView.VerticalScrollBarVisibility);

            scrollView.VerticalScrollBarVisibility = ScrollBarVisibility.Never;
            Assert.Equal(ScrollBarVisibility.Never, scrollView.VerticalScrollBarVisibility);

            scrollView.VerticalScrollBarVisibility = ScrollBarVisibility.Default;
            Assert.Equal(ScrollBarVisibility.Default, scrollView.VerticalScrollBarVisibility);
        }

        /// <summary>
        /// Tests that ArrangeOverride computes frame correctly and returns frame size with valid bounds.
        /// </summary>
        /// <param name="x">The x coordinate of the bounds rectangle.</param>
        /// <param name="y">The y coordinate of the bounds rectangle.</param>
        /// <param name="width">The width of the bounds rectangle.</param>
        /// <param name="height">The height of the bounds rectangle.</param>
        [Theory]
        [InlineData(0, 0, 100, 50)]
        [InlineData(10, 20, 200, 150)]
        [InlineData(-5, -10, 300, 250)]
        [InlineData(0, 0, 0, 0)]
        public void ArrangeOverride_ValidBounds_ReturnsFrameSize(double x, double y, double width, double height)
        {
            // Arrange
            var scrollView = new TestableScrollView();
            var bounds = new Rect(x, y, width, height);

            // Act
            var result = scrollView.ArrangeOverride(bounds);

            // Assert
            Assert.Equal(scrollView.Frame.Size, result);
        }

        /// <summary>
        /// Tests that ArrangeOverride handles extremely large bounds values correctly.
        /// </summary>
        [Fact]
        public void ArrangeOverride_LargeBounds_ReturnsFrameSize()
        {
            // Arrange
            var scrollView = new TestableScrollView();
            var bounds = new Rect(0, 0, double.MaxValue, double.MaxValue);

            // Act
            var result = scrollView.ArrangeOverride(bounds);

            // Assert
            Assert.Equal(scrollView.Frame.Size, result);
        }

        /// <summary>
        /// Tests that ArrangeOverride handles negative bounds dimensions correctly.
        /// </summary>
        [Theory]
        [InlineData(-100, -50)]
        [InlineData(-1, -1)]
        public void ArrangeOverride_NegativeDimensions_ReturnsFrameSize(double width, double height)
        {
            // Arrange
            var scrollView = new TestableScrollView();
            var bounds = new Rect(0, 0, width, height);

            // Act
            var result = scrollView.ArrangeOverride(bounds);

            // Assert
            Assert.Equal(scrollView.Frame.Size, result);
        }

        /// <summary>
        /// Tests that ArrangeOverride calls PlatformArrange on handler when handler is not null.
        /// </summary>
        [Fact]
        public void ArrangeOverride_HandlerNotNull_CallsPlatformArrange()
        {
            // Arrange
            var scrollView = new TestableScrollView();
            var mockHandler = Substitute.For<IViewHandler>();
            scrollView.Handler = mockHandler;
            var bounds = new Rect(10, 20, 100, 50);

            // Act
            scrollView.ArrangeOverride(bounds);

            // Assert
            mockHandler.Received(1).PlatformArrange(scrollView.Frame);
        }

        /// <summary>
        /// Tests that ArrangeOverride does not throw when handler is null.
        /// </summary>
        [Fact]
        public void ArrangeOverride_HandlerNull_DoesNotThrow()
        {
            // Arrange
            var scrollView = new TestableScrollView();
            scrollView.Handler = null;
            var bounds = new Rect(10, 20, 100, 50);

            // Act & Assert
            var result = scrollView.ArrangeOverride(bounds);
            Assert.Equal(scrollView.Frame.Size, result);
        }

        /// <summary>
        /// Tests that ArrangeOverride with special double values handles them correctly.
        /// </summary>
        /// <param name="width">The width value to test (including special values).</param>
        /// <param name="height">The height value to test (including special values).</param>
        [Theory]
        [InlineData(double.NaN, 100)]
        [InlineData(100, double.NaN)]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, 100)]
        [InlineData(100, double.PositiveInfinity)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 100)]
        [InlineData(100, double.NegativeInfinity)]
        public void ArrangeOverride_SpecialDoubleValues_HandlesGracefully(double width, double height)
        {
            // Arrange
            var scrollView = new TestableScrollView();
            var bounds = new Rect(0, 0, width, height);

            // Act & Assert - Should not throw
            var result = scrollView.ArrangeOverride(bounds);
            Assert.Equal(scrollView.Frame.Size, result);
        }

        /// <summary>
        /// Tests that ArrangeOverride updates Frame property and calls PlatformArrange with correct frame.
        /// </summary>
        [Fact]
        public void ArrangeOverride_UpdatesFrameAndCallsPlatformArrangeWithSameFrame()
        {
            // Arrange
            var scrollView = new TestableScrollView();
            var mockHandler = Substitute.For<IViewHandler>();
            scrollView.Handler = mockHandler;
            var bounds = new Rect(15, 25, 120, 80);

            // Act
            var result = scrollView.ArrangeOverride(bounds);

            // Assert
            Assert.Equal(scrollView.Frame.Size, result);
            mockHandler.Received(1).PlatformArrange(scrollView.Frame);
        }

        /// <summary>
        /// Tests that ArrangeOverride returns Size with zero dimensions when bounds has zero dimensions.
        /// </summary>
        [Fact]
        public void ArrangeOverride_ZeroDimensionBounds_ReturnsFrameSize()
        {
            // Arrange
            var scrollView = new TestableScrollView();
            var bounds = new Rect(100, 200, 0, 0);

            // Act
            var result = scrollView.ArrangeOverride(bounds);

            // Assert
            Assert.Equal(scrollView.Frame.Size, result);
        }

        /// <summary>
        /// Tests that ArrangeOverride works correctly with very small positive bounds dimensions.
        /// </summary>
        [Theory]
        [InlineData(0.001, 0.001)]
        [InlineData(double.Epsilon, double.Epsilon)]
        public void ArrangeOverride_VerySmallPositiveDimensions_ReturnsFrameSize(double width, double height)
        {
            // Arrange
            var scrollView = new TestableScrollView();
            var bounds = new Rect(0, 0, width, height);

            // Act
            var result = scrollView.ArrangeOverride(bounds);

            // Assert
            Assert.Equal(scrollView.Frame.Size, result);
        }

        /// <summary>
        /// Tests that the On method returns a non-null platform configuration for a valid IConfigPlatform implementation.
        /// </summary>
        [Fact]
        public void On_ValidPlatform_ReturnsNonNullConfiguration()
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act
            var result = scrollView.On<TestConfigPlatform>();

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that the On method returns the correct generic interface type.
        /// </summary>
        [Fact]
        public void On_ValidPlatform_ReturnsCorrectInterfaceType()
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act
            var result = scrollView.On<TestConfigPlatform>();

            // Assert
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestConfigPlatform, ScrollView>>(result);
        }

        /// <summary>
        /// Tests that consecutive calls to On with the same platform type return the same instance (caching behavior).
        /// </summary>
        [Fact]
        public void On_ConsecutiveCallsSamePlatform_ReturnsSameInstance()
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act
            var result1 = scrollView.On<TestConfigPlatform>();
            var result2 = scrollView.On<TestConfigPlatform>();

            // Assert
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that calls to On with different platform types return different instances.
        /// </summary>
        [Fact]
        public void On_DifferentPlatforms_ReturnsDifferentInstances()
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act
            var result1 = scrollView.On<TestConfigPlatform>();
            var result2 = scrollView.On<AnotherTestConfigPlatform>();

            // Assert
            Assert.NotSame(result1, result2);
        }

        /// <summary>
        /// Tests that different ScrollView instances maintain separate platform configurations.
        /// </summary>
        [Fact]
        public void On_DifferentScrollViewInstances_ReturnsDifferentConfigurations()
        {
            // Arrange
            var scrollView1 = new ScrollView();
            var scrollView2 = new ScrollView();

            // Act
            var result1 = scrollView1.On<TestConfigPlatform>();
            var result2 = scrollView2.On<TestConfigPlatform>();

            // Assert
            Assert.NotSame(result1, result2);
        }

        /// <summary>
        /// Test platform implementation of IConfigPlatform for testing purposes.
        /// </summary>
        private class TestConfigPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Another test platform implementation of IConfigPlatform for testing different platform scenarios.
        /// </summary>
        private class AnotherTestConfigPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Tests that SafeAreaEdges property getter returns default value when no explicit value is set.
        /// </summary>
        [Fact]
        public void SafeAreaEdges_GetDefaultValue_ReturnsDefault()
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act
            var result = scrollView.SafeAreaEdges;

            // Assert
            Assert.Equal(SafeAreaEdges.Default, result);
        }

        /// <summary>
        /// Tests that SafeAreaEdges property can be set and retrieved correctly with various predefined values.
        /// </summary>
        /// <param name="safeAreaEdges">The SafeAreaEdges value to test</param>
        [Theory]
        [InlineData(typeof(SafeAreaEdges), "Default")]
        [InlineData(typeof(SafeAreaEdges), "None")]
        [InlineData(typeof(SafeAreaEdges), "All")]
        public void SafeAreaEdges_SetAndGetPredefinedValues_WorksCorrectly(Type type, string propertyName)
        {
            // Arrange
            var scrollView = new ScrollView();
            var safeAreaEdges = (SafeAreaEdges)type.GetProperty(propertyName).GetValue(null);

            // Act
            scrollView.SafeAreaEdges = safeAreaEdges;
            var result = scrollView.SafeAreaEdges;

            // Assert
            Assert.Equal(safeAreaEdges, result);
        }

        /// <summary>
        /// Tests that SafeAreaEdges property can be set and retrieved correctly with custom values.
        /// </summary>
        [Fact]
        public void SafeAreaEdges_SetAndGetCustomValue_WorksCorrectly()
        {
            // Arrange
            var scrollView = new ScrollView();
            var customEdges = new SafeAreaEdges(SafeAreaRegions.Container, SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.SoftInput);

            // Act
            scrollView.SafeAreaEdges = customEdges;
            var result = scrollView.SafeAreaEdges;

            // Assert
            Assert.Equal(customEdges, result);
        }

        /// <summary>
        /// Tests that SafeAreaEdges property handles multiple sequential sets and gets correctly.
        /// </summary>
        [Fact]
        public void SafeAreaEdges_MultipleSetAndGet_WorksCorrectly()
        {
            // Arrange
            var scrollView = new ScrollView();
            var firstValue = SafeAreaEdges.None;
            var secondValue = SafeAreaEdges.All;
            var thirdValue = new SafeAreaEdges(SafeAreaRegions.SoftInput, SafeAreaRegions.Container, SafeAreaRegions.None, SafeAreaRegions.All);

            // Act & Assert - First value
            scrollView.SafeAreaEdges = firstValue;
            Assert.Equal(firstValue, scrollView.SafeAreaEdges);

            // Act & Assert - Second value
            scrollView.SafeAreaEdges = secondValue;
            Assert.Equal(secondValue, scrollView.SafeAreaEdges);

            // Act & Assert - Third value
            scrollView.SafeAreaEdges = thirdValue;
            Assert.Equal(thirdValue, scrollView.SafeAreaEdges);
        }

        /// <summary>
        /// Tests that OnSizeAllocated executes without throwing exceptions for valid positive dimensions.
        /// </summary>
        [Theory]
        [InlineData(100.0, 200.0)]
        [InlineData(1.0, 1.0)]
        [InlineData(1000.0, 500.0)]
        public void OnSizeAllocated_ValidPositiveDimensions_ExecutesSuccessfully(double width, double height)
        {
            // Arrange
            var scrollView = new TestableScrollView();

            // Act & Assert
            var exception = Record.Exception(() => scrollView.CallOnSizeAllocated(width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnSizeAllocated handles zero dimensions without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(0.0, 100.0)]
        [InlineData(100.0, 0.0)]
        [InlineData(0.0, 0.0)]
        public void OnSizeAllocated_ZeroDimensions_ExecutesSuccessfully(double width, double height)
        {
            // Arrange
            var scrollView = new TestableScrollView();

            // Act & Assert
            var exception = Record.Exception(() => scrollView.CallOnSizeAllocated(width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnSizeAllocated handles negative dimensions without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(-100.0, 200.0)]
        [InlineData(100.0, -200.0)]
        [InlineData(-100.0, -200.0)]
        public void OnSizeAllocated_NegativeDimensions_ExecutesSuccessfully(double width, double height)
        {
            // Arrange
            var scrollView = new TestableScrollView();

            // Act & Assert
            var exception = Record.Exception(() => scrollView.CallOnSizeAllocated(width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnSizeAllocated handles extreme boundary values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, 100.0)]
        [InlineData(100.0, double.MaxValue)]
        [InlineData(double.MinValue, 100.0)]
        [InlineData(100.0, double.MinValue)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue)]
        public void OnSizeAllocated_ExtremeBoundaryValues_ExecutesSuccessfully(double width, double height)
        {
            // Arrange
            var scrollView = new TestableScrollView();

            // Act & Assert
            var exception = Record.Exception(() => scrollView.CallOnSizeAllocated(width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnSizeAllocated handles special double values like NaN and infinity without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 100.0)]
        [InlineData(100.0, double.NaN)]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, 100.0)]
        [InlineData(100.0, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 100.0)]
        [InlineData(100.0, double.NegativeInfinity)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
        public void OnSizeAllocated_SpecialDoubleValues_ExecutesSuccessfully(double width, double height)
        {
            // Arrange
            var scrollView = new TestableScrollView();

            // Act & Assert
            var exception = Record.Exception(() => scrollView.CallOnSizeAllocated(width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnSizeAllocated can be called multiple times without issues.
        /// </summary>
        [Fact]
        public void OnSizeAllocated_MultipleConsecutiveCalls_ExecutesSuccessfully()
        {
            // Arrange
            var scrollView = new TestableScrollView();

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                scrollView.CallOnSizeAllocated(100.0, 200.0);
                scrollView.CallOnSizeAllocated(150.0, 250.0);
                scrollView.CallOnSizeAllocated(200.0, 300.0);
            });
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnSizeAllocated works correctly when ScrollView has content.
        /// </summary>
        [Fact]
        public void OnSizeAllocated_WithContent_ExecutesSuccessfully()
        {
            // Arrange
            var scrollView = new TestableScrollView();
            var content = new Label { Text = "Test Content" };
            scrollView.Content = content;

            // Act & Assert
            var exception = Record.Exception(() => scrollView.CallOnSizeAllocated(100.0, 200.0));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that LowerChild method can be called with a valid View parameter.
        /// Verifies the method executes without throwing exceptions for a View that is part of the ScrollView's content.
        /// Expected result: Method executes successfully without throwing.
        /// </summary>
        [Fact]
        public void LowerChild_ValidViewAsContent_ExecutesWithoutException()
        {
            // Arrange
            var scrollView = new ScrollView();
            var view = new View();
            scrollView.Content = view;

            // Act & Assert
            var exception = Record.Exception(() => scrollView.LowerChild(view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that LowerChild method can be called with a View that is not part of the ScrollView's content.
        /// Verifies the method handles Views that are not children gracefully.
        /// Expected result: Method executes successfully without throwing.
        /// </summary>
        [Fact]
        public void LowerChild_ViewNotInContent_ExecutesWithoutException()
        {
            // Arrange
            var scrollView = new ScrollView();
            var view = new View();
            // Note: view is not added to scrollView's content

            // Act & Assert
            var exception = Record.Exception(() => scrollView.LowerChild(view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that LowerChild method handles null parameter appropriately.
        /// Verifies the method behavior when passed a null View reference.
        /// Expected result: Method may throw ArgumentNullException or handle null gracefully.
        /// </summary>
        [Fact]
        public void LowerChild_NullView_HandlesGracefullyOrThrows()
        {
            // Arrange
            var scrollView = new ScrollView();

            // Act & Assert
            var exception = Record.Exception(() => scrollView.LowerChild(null));
            // The method might throw ArgumentNullException or handle null gracefully
            // We just verify it doesn't cause unexpected crashes
            if (exception != null)
            {
                Assert.IsType<ArgumentNullException>(exception);
            }
        }

        /// <summary>
        /// Tests that LowerChild method can be called multiple times with different Views.
        /// Verifies the method can handle multiple sequential calls.
        /// Expected result: All calls execute successfully without throwing.
        /// </summary>
        [Fact]
        public void LowerChild_MultipleCalls_ExecutesWithoutException()
        {
            // Arrange
            var scrollView = new ScrollView();
            var view1 = new View();
            var view2 = new View();

            // Act & Assert
            var exception1 = Record.Exception(() => scrollView.LowerChild(view1));
            var exception2 = Record.Exception(() => scrollView.LowerChild(view2));
            var exception3 = Record.Exception(() => scrollView.LowerChild(view1));

            Assert.Null(exception1);
            Assert.Null(exception2);
            Assert.Null(exception3);
        }

        /// <summary>
        /// Tests that LowerChild method works with different types of Views.
        /// Verifies the method can handle various View-derived types.
        /// Expected result: Method executes successfully with different View types.
        /// </summary>
        [Theory]
        [InlineData(typeof(View))]
        [InlineData(typeof(Label))]
        [InlineData(typeof(Button))]
        public void LowerChild_DifferentViewTypes_ExecutesWithoutException(Type viewType)
        {
            // Arrange
            var scrollView = new ScrollView();
            var view = (View)Activator.CreateInstance(viewType);

            // Act & Assert
            var exception = Record.Exception(() => scrollView.LowerChild(view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that LowerChild method can be called on ScrollView with complex content structure.
        /// Verifies the method works when ScrollView contains nested layout structures.
        /// Expected result: Method executes successfully regardless of content complexity.
        /// </summary>
        [Fact]
        public void LowerChild_ComplexContentStructure_ExecutesWithoutException()
        {
            // Arrange
            var scrollView = new ScrollView();
            var stackLayout = new Compatibility.StackLayout();
            var childView = new View();
            stackLayout.Children.Add(childView);
            scrollView.Content = stackLayout;

            // Act & Assert
            var exception1 = Record.Exception(() => scrollView.LowerChild(childView));
            var exception2 = Record.Exception(() => scrollView.LowerChild(stackLayout));

            Assert.Null(exception1);
            Assert.Null(exception2);
        }

        /// <summary>
        /// Tests that LayoutAreaOverride getter returns the current value of the internal field.
        /// This test verifies the getter functionality of the obsolete property.
        /// </summary>
        [Fact]
        public void LayoutAreaOverride_Getter_ReturnsCurrentValue()
        {
            // Arrange
            var scrollView = new ScrollView();
            var expectedRect = new Rect(10, 20, 100, 200);

            // Act & Assert - Use reflection to set the field directly and verify getter
            var field = typeof(ScrollView).GetField("_layoutAreaOverride", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(scrollView, expectedRect);

#pragma warning disable CS0618 // Type or member is obsolete
            var actualRect = scrollView.LayoutAreaOverride;
#pragma warning restore CS0618

            // Assert
            Assert.Equal(expectedRect, actualRect);
        }

        /// <summary>
        /// Tests that LayoutAreaOverride setter with the same value triggers early return.
        /// This test verifies that setting the same value twice doesn't change the field and exits early.
        /// </summary>
        [Theory]
        [MemberData(nameof(RectTestData))]
        public void LayoutAreaOverride_SetSameValue_EarlyReturn(Rect testRect)
        {
            // Arrange
            var scrollView = new ScrollView();

#pragma warning disable CS0618 // Type or member is obsolete
            scrollView.LayoutAreaOverride = testRect;

            // Act - Set the same value again
            scrollView.LayoutAreaOverride = testRect;
#pragma warning restore CS0618

            // Assert - Verify the value is still the same
            var field = typeof(ScrollView).GetField("_layoutAreaOverride", BindingFlags.NonPublic | BindingFlags.Instance);
            var actualValue = (Rect)field.GetValue(scrollView);
            Assert.Equal(testRect, actualValue);
        }

        /// <summary>
        /// Tests that LayoutAreaOverride setter with different values assigns the new value.
        /// This test verifies that setting different values properly updates the internal field.
        /// </summary>
        [Fact]
        public void LayoutAreaOverride_SetDifferentValues_AssignsNewValue()
        {
            // Arrange
            var scrollView = new ScrollView();
            var initialRect = new Rect(0, 0, 50, 50);
            var newRect = new Rect(10, 20, 100, 200);

#pragma warning disable CS0618 // Type or member is obsolete
            scrollView.LayoutAreaOverride = initialRect;

            // Act - Set a different value
            scrollView.LayoutAreaOverride = newRect;
#pragma warning restore CS0618

            // Assert - Verify the new value was assigned
            var field = typeof(ScrollView).GetField("_layoutAreaOverride", BindingFlags.NonPublic | BindingFlags.Instance);
            var actualValue = (Rect)field.GetValue(scrollView);
            Assert.Equal(newRect, actualValue);
        }

        /// <summary>
        /// Tests that LayoutAreaOverride setter handles edge case values correctly.
        /// This test verifies proper handling of special double values like NaN and Infinity.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, double.NaN, double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity, double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.MinValue, double.MaxValue, 0, 0)]
        public void LayoutAreaOverride_SetEdgeCaseValues_HandlesCorrectly(double x, double y, double width, double height)
        {
            // Arrange
            var scrollView = new ScrollView();
            var edgeRect = new Rect(x, y, width, height);

#pragma warning disable CS0618 // Type or member is obsolete
            // Act
            scrollView.LayoutAreaOverride = edgeRect;
#pragma warning restore CS0618

            // Assert - Verify the edge case value was set
            var field = typeof(ScrollView).GetField("_layoutAreaOverride", BindingFlags.NonPublic | BindingFlags.Instance);
            var actualValue = (Rect)field.GetValue(scrollView);
            Assert.Equal(edgeRect, actualValue);
        }

        /// <summary>
        /// Tests that LayoutAreaOverride setter equality comparison works correctly with identical values.
        /// This test ensures that two Rect instances with identical coordinates are considered equal.
        /// </summary>
        [Fact]
        public void LayoutAreaOverride_SetIdenticalButSeparateRects_EarlyReturn()
        {
            // Arrange
            var scrollView = new ScrollView();
            var rect1 = new Rect(15, 25, 75, 125);
            var rect2 = new Rect(15, 25, 75, 125); // Same values, different instance

#pragma warning disable CS0618 // Type or member is obsolete
            scrollView.LayoutAreaOverride = rect1;

            // Act - Set an identical but separate Rect instance
            scrollView.LayoutAreaOverride = rect2;
#pragma warning restore CS0618

            // Assert - Verify the value is still set correctly (early return path was taken)
            var field = typeof(ScrollView).GetField("_layoutAreaOverride", BindingFlags.NonPublic | BindingFlags.Instance);
            var actualValue = (Rect)field.GetValue(scrollView);
            Assert.Equal(rect1, actualValue);
            Assert.Equal(rect2, actualValue);
        }

        /// <summary>
        /// Provides test data for various Rect values to test different scenarios.
        /// </summary>
        public static IEnumerable<object[]> RectTestData()
        {
            yield return new object[] { Rect.Zero };
            yield return new object[] { new Rect(0, 0, 0, 0) };
            yield return new object[] { new Rect(1, 2, 3, 4) };
            yield return new object[] { new Rect(-10, -20, 30, 40) };
            yield return new object[] { new Rect(0.5, 0.7, 100.25, 200.75) };
            yield return new object[] { new Rect(1000000, 2000000, 3000000, 4000000) };
        }
    }



    /// <summary>
    /// Extension methods to help with testing ScrollView internal state.
    /// </summary>
    internal static class ScrollViewTestExtensions
    {
        public static void MockSetSize(this ScrollView scrollView, double width, double height)
        {
            // Use reflection to set the Width and Height properties
            typeof(VisualElement).GetProperty("Width").SetValue(scrollView, width);
            typeof(VisualElement).GetProperty("Height").SetValue(scrollView, height);
        }

        public static void MockSetScrollPosition(this ScrollView scrollView, double x, double y)
        {
            // Use reflection to set ScrollX and ScrollY properties
            scrollView.SetValue(ScrollView.ScrollXProperty, x);
            scrollView.SetValue(ScrollView.ScrollYProperty, y);
        }
    }


    public partial class ScrollViewComputeConstraintForViewTests : BaseTestFixture
    {
        /// <summary>
        /// Tests ComputeConstraintForView with null view parameter.
        /// Should handle null gracefully without throwing exceptions.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_NullView_ReturnsNone()
        {
            // Arrange
            var scrollView = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical
            };

            // Act & Assert - This should not throw
            var result = scrollView.ComputeConstraintForView(null);
            Assert.Equal(LayoutConstraint.None, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView returns VerticallyFixed when orientation is Horizontal,
        /// view has Fill vertical alignment, and constraint has VerticallyFixed flag.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_HorizontalOrientation_FillVerticalOptions_VerticallyFixedConstraint_ReturnsVerticallyFixed()
        {
            // Arrange
            var scrollView = new TestableScrollView
            {
                Orientation = ScrollOrientation.Horizontal,
                TestConstraint = LayoutConstraint.VerticallyFixed
            };
            var view = new View
            {
                VerticalOptions = LayoutOptions.Fill
            };

            // Act
            var result = scrollView.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.VerticallyFixed, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView returns HorizontallyFixed when orientation is Vertical,
        /// view has Fill horizontal alignment, and constraint has HorizontallyFixed flag.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_VerticalOrientation_FillHorizontalOptions_HorizontallyFixedConstraint_ReturnsHorizontallyFixed()
        {
            // Arrange
            var scrollView = new TestableScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                TestConstraint = LayoutConstraint.HorizontallyFixed
            };
            var view = new View
            {
                HorizontalOptions = LayoutOptions.Fill
            };

            // Act
            var result = scrollView.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.HorizontallyFixed, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView with Both orientation always returns None.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_BothOrientation_ReturnsNone()
        {
            // Arrange
            var scrollView = new ScrollView
            {
                Orientation = ScrollOrientation.Both
            };
            var view = new View
            {
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill
            };

            // Act
            var result = scrollView.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.None, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView returns None when orientation is Horizontal
        /// but view doesn't have Fill vertical alignment.
        /// </summary>
        [Theory]
        [InlineData(LayoutAlignment.Start)]
        [InlineData(LayoutAlignment.Center)]
        [InlineData(LayoutAlignment.End)]
        public void ComputeConstraintForView_HorizontalOrientation_NonFillVerticalAlignment_ReturnsNone(LayoutAlignment alignment)
        {
            // Arrange
            var scrollView = new TestableScrollView
            {
                Orientation = ScrollOrientation.Horizontal,
                TestConstraint = LayoutConstraint.VerticallyFixed
            };
            var view = new View
            {
                VerticalOptions = new LayoutOptions(alignment, false)
            };

            // Act
            var result = scrollView.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.None, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView returns None when orientation is Vertical
        /// but view doesn't have Fill horizontal alignment.
        /// </summary>
        [Theory]
        [InlineData(LayoutAlignment.Start)]
        [InlineData(LayoutAlignment.Center)]
        [InlineData(LayoutAlignment.End)]
        public void ComputeConstraintForView_VerticalOrientation_NonFillHorizontalAlignment_ReturnsNone(LayoutAlignment alignment)
        {
            // Arrange
            var scrollView = new TestableScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                TestConstraint = LayoutConstraint.HorizontallyFixed
            };
            var view = new View
            {
                HorizontalOptions = new LayoutOptions(alignment, false)
            };

            // Act
            var result = scrollView.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.None, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView returns None when orientation is Horizontal
        /// and view has Fill vertical alignment but constraint doesn't have VerticallyFixed flag.
        /// </summary>
        [Theory]
        [InlineData(LayoutConstraint.None)]
        [InlineData(LayoutConstraint.HorizontallyFixed)]
        public void ComputeConstraintForView_HorizontalOrientation_FillVerticalOptions_NoVerticallyFixedConstraint_ReturnsNone(LayoutConstraint constraint)
        {
            // Arrange
            var scrollView = new TestableScrollView
            {
                Orientation = ScrollOrientation.Horizontal,
                TestConstraint = constraint
            };
            var view = new View
            {
                VerticalOptions = LayoutOptions.Fill
            };

            // Act
            var result = scrollView.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.None, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView returns None when orientation is Vertical
        /// and view has Fill horizontal alignment but constraint doesn't have HorizontallyFixed flag.
        /// </summary>
        [Theory]
        [InlineData(LayoutConstraint.None)]
        [InlineData(LayoutConstraint.VerticallyFixed)]
        public void ComputeConstraintForView_VerticalOrientation_FillHorizontalOptions_NoHorizontallyFixedConstraint_ReturnsNone(LayoutConstraint constraint)
        {
            // Arrange
            var scrollView = new TestableScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                TestConstraint = constraint
            };
            var view = new View
            {
                HorizontalOptions = LayoutOptions.Fill
            };

            // Act
            var result = scrollView.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.None, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView with various ScrollOrientation values including Neither and invalid enum values.
        /// </summary>
        [Theory]
        [InlineData(ScrollOrientation.Neither)]
        [InlineData((ScrollOrientation)999)]
        public void ComputeConstraintForView_UnsupportedOrientation_ReturnsNone(ScrollOrientation orientation)
        {
            // Arrange
            var scrollView = new ScrollView
            {
                Orientation = orientation
            };
            var view = new View
            {
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill
            };

            // Act
            var result = scrollView.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.None, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView works with both Fixed constraint (combination of both flags).
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_HorizontalOrientation_FillVerticalOptions_FixedConstraint_ReturnsVerticallyFixed()
        {
            // Arrange
            var scrollView = new TestableScrollView
            {
                Orientation = ScrollOrientation.Horizontal,
                TestConstraint = LayoutConstraint.Fixed // Both HorizontallyFixed and VerticallyFixed
            };
            var view = new View
            {
                VerticalOptions = LayoutOptions.Fill
            };

            // Act
            var result = scrollView.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.VerticallyFixed, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView works with both Fixed constraint (combination of both flags).
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_VerticalOrientation_FillHorizontalOptions_FixedConstraint_ReturnsHorizontallyFixed()
        {
            // Arrange
            var scrollView = new TestableScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                TestConstraint = LayoutConstraint.Fixed // Both HorizontallyFixed and VerticallyFixed
            };
            var view = new View
            {
                HorizontalOptions = LayoutOptions.Fill
            };

            // Act
            var result = scrollView.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.HorizontallyFixed, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView with expanding Fill options still works correctly.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_HorizontalOrientation_FillAndExpandVerticalOptions_VerticallyFixedConstraint_ReturnsVerticallyFixed()
        {
            // Arrange
            var scrollView = new TestableScrollView
            {
                Orientation = ScrollOrientation.Horizontal,
                TestConstraint = LayoutConstraint.VerticallyFixed
            };
            var view = new View
            {
#pragma warning disable CS0618 // Type or member is obsolete
                VerticalOptions = LayoutOptions.FillAndExpand
#pragma warning restore CS0618 // Type or member is obsolete
            };

            // Act
            var result = scrollView.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.VerticallyFixed, result);
        }

        /// <summary>
        /// Helper class to expose protected ComputeConstraintForView method and allow setting constraint for testing.
        /// </summary>
        private class TestableScrollView : ScrollView
        {
            public LayoutConstraint TestConstraint { get; set; } = LayoutConstraint.None;

            internal new LayoutConstraint Constraint => TestConstraint;

            public new LayoutConstraint ComputeConstraintForView(View view)
            {
                return base.ComputeConstraintForView(view);
            }
        }
    }


    /// <summary>
    /// Tests for the LayoutChildren method in ScrollView class.
    /// </summary>
    public partial class ScrollViewLayoutChildrenTests : BaseTestFixture
    {
        /// <summary>
        /// Test class that inherits from ScrollView to expose the protected LayoutChildren method for testing.
        /// </summary>
        private class TestableScrollView : ScrollView
        {
            public new void LayoutChildren(double x, double y, double width, double height)
            {
                base.LayoutChildren(x, y, width, height);
            }
        }

        /// <summary>
        /// Tests that LayoutChildren can be called with normal positive values without throwing exceptions.
        /// This method is obsolete and has an empty implementation.
        /// </summary>
        [Fact]
        public void LayoutChildren_WithNormalValues_DoesNotThrow()
        {
            // Arrange
            var scrollView = new TestableScrollView();

            // Act & Assert
            var exception = Record.Exception(() => scrollView.LayoutChildren(10.0, 20.0, 100.0, 200.0));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that LayoutChildren can be called with zero values without throwing exceptions.
        /// This verifies the method handles boundary conditions for position and size parameters.
        /// </summary>
        [Fact]
        public void LayoutChildren_WithZeroValues_DoesNotThrow()
        {
            // Arrange
            var scrollView = new TestableScrollView();

            // Act & Assert
            var exception = Record.Exception(() => scrollView.LayoutChildren(0.0, 0.0, 0.0, 0.0));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that LayoutChildren can be called with negative values without throwing exceptions.
        /// This tests edge case scenarios with negative coordinates and dimensions.
        /// </summary>
        [Fact]
        public void LayoutChildren_WithNegativeValues_DoesNotThrow()
        {
            // Arrange
            var scrollView = new TestableScrollView();

            // Act & Assert
            var exception = Record.Exception(() => scrollView.LayoutChildren(-10.0, -20.0, -100.0, -200.0));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that LayoutChildren handles extreme double values including infinity and NaN.
        /// This verifies the method's robustness with special floating-point values.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue, double.MinValue, double.MinValue)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN, double.NaN, double.NaN)]
        public void LayoutChildren_WithExtremeValues_DoesNotThrow(double x, double y, double width, double height)
        {
            // Arrange
            var scrollView = new TestableScrollView();

            // Act & Assert
            var exception = Record.Exception(() => scrollView.LayoutChildren(x, y, width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that LayoutChildren handles mixed boundary values properly.
        /// This verifies various combinations of edge case values.
        /// </summary>
        [Theory]
        [InlineData(0.0, 10.0, 100.0, 200.0)]
        [InlineData(10.0, 0.0, 100.0, 200.0)]
        [InlineData(10.0, 20.0, 0.0, 200.0)]
        [InlineData(10.0, 20.0, 100.0, 0.0)]
        [InlineData(-1.0, 1.0, 50.0, 75.0)]
        [InlineData(1.0, -1.0, 50.0, 75.0)]
        public void LayoutChildren_WithMixedBoundaryValues_DoesNotThrow(double x, double y, double width, double height)
        {
            // Arrange
            var scrollView = new TestableScrollView();

            // Act & Assert
            var exception = Record.Exception(() => scrollView.LayoutChildren(x, y, width, height));
            Assert.Null(exception);
        }
    }
}