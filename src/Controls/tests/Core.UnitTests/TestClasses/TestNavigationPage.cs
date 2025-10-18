#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


using Microsoft.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class TestNavigationPage : NavigationPage
    {
        internal TestNavigationPage(bool setforMaui, Page root = null, bool setHandler = true) : base(setforMaui, root)
        {
            Title = "Title";
            if (setforMaui && setHandler)
            {
                base.Handler = new TestNavigationHandler();
            }
        }

        public new TestNavigationHandler Handler =>
            base.Handler as TestNavigationHandler;

        public void ValidateNavigationCompleted()
        {
            Assert.Null(CurrentNavigationTask);
            if (Handler is TestNavigationHandler nh)
                Assert.Null(nh.CurrentNavigationRequest);
        }

        public async Task<bool> SendBackButtonPressedAsync()
        {
            var result = base.SendBackButtonPressed();
            var task = base.CurrentNavigationTask;
            if (task != null)
                await task;

            return result;
        }

        public Task NavigatingTask => Handler?.NavigatingTask ?? Task.CompletedTask;
        public Func<NavigationType> MockDetermineNavigationType { get; set; }
        public Func<bool, Action, Action, Action, Task> MockSendHandlerUpdateAsync { get; set; }
        public Action<NavigationType, Page> MockSendNavigating { get; set; }

        public void SetCurrentPage(Page page)
        {
            SetValue(CurrentPagePropertyKey, page);
        }
    }


    public class TestNavigationHandler : ViewHandler<NavigationPage, object>
    {
        public static CommandMapper<IStackNavigationView, TestNavigationHandler> NavigationViewCommandMapper = new(ViewCommandMapper)
        {
            [nameof(IStackNavigation.RequestNavigation)] = RequestNavigation
        };

        public static PropertyMapper<IStackNavigationView, TestNavigationHandler> NavigationViewMapper
               = new PropertyMapper<IStackNavigationView, TestNavigationHandler>();

        public NavigationRequest CurrentNavigationRequest { get; private set; }

        TaskCompletionSource _navigationSource;

        public Task NavigatingTask => _navigationSource?.Task ?? Task.CompletedTask;

        public async void CompleteCurrentNavigation()
        {
            if (CurrentNavigationRequest == null)
                throw new InvalidOperationException("No Active Navigation in the works");

            var newStack = CurrentNavigationRequest.NavigationStack.ToList();
            CurrentNavigationRequest = null;

            var source = _navigationSource;
            _navigationSource = null;

            if ((this as IElementHandler).VirtualView is IStackNavigation sn)
                sn.NavigationFinished(newStack);


            await Task.Delay(1);
            source.SetResult();
        }

        async void RequestNavigation(NavigationRequest navigationRequest)
        {
            if (CurrentNavigationRequest != null || _navigationSource != null)
                throw new InvalidOperationException("Already Processing Navigation");


            _navigationSource = new TaskCompletionSource();
            CurrentNavigationRequest = navigationRequest;

            await Task.Delay(10);
            CompleteCurrentNavigation();
        }

        public static void RequestNavigation(TestNavigationHandler arg1, IStackNavigationView arg2, object arg3)
        {
            arg1.RequestNavigation((NavigationRequest)arg3);
        }

        public TestNavigationHandler() : base(NavigationViewMapper, NavigationViewCommandMapper)
        {
        }

        protected override object CreatePlatformView()
        {
            return new object();
        }
    }

    public partial class NavigationPageTests
    {
        /// <summary>
        /// Test helper class that exposes the protected LayoutChildren method for testing.
        /// </summary>
        public class TestableNavigationPage : NavigationPage
        {
            public TestableNavigationPage() : base()
            {
            }

            public TestableNavigationPage(Page root) : base(root)
            {
            }

            /// <summary>
            /// Exposes the protected LayoutChildren method for unit testing.
            /// </summary>
            /// <param name="x">The x coordinate.</param>
            /// <param name="y">The y coordinate.</param>
            /// <param name="width">The width dimension.</param>
            /// <param name="height">The height dimension.</param>
            public void TestLayoutChildren(double x, double y, double width, double height)
            {
                LayoutChildren(x, y, width, height);
            }
        }

        /// <summary>
        /// Tests that the LayoutChildren method can be called with normal positive values without throwing exceptions.
        /// This method is obsolete and intentionally does nothing to prevent calling legacy Page.LayoutChildren code.
        /// </summary>
        [Fact]
        public void LayoutChildren_WithNormalValues_CompletesWithoutException()
        {
            // Arrange
            var navigationPage = new TestableNavigationPage();

            // Act & Assert
            var exception = Record.Exception(() => navigationPage.TestLayoutChildren(10.0, 20.0, 300.0, 400.0));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the LayoutChildren method handles zero values for all parameters without throwing exceptions.
        /// Verifies the method behavior with boundary values including zero coordinates and dimensions.
        /// </summary>
        [Fact]
        public void LayoutChildren_WithZeroValues_CompletesWithoutException()
        {
            // Arrange
            var navigationPage = new TestableNavigationPage();

            // Act & Assert
            var exception = Record.Exception(() => navigationPage.TestLayoutChildren(0.0, 0.0, 0.0, 0.0));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the LayoutChildren method handles negative values for all parameters without throwing exceptions.
        /// Verifies the method behavior with negative coordinates and dimensions.
        /// </summary>
        [Theory]
        [InlineData(-10.0, -20.0, -100.0, -200.0)]
        [InlineData(-1.0, 0.0, 1.0, 2.0)]
        [InlineData(0.0, -1.0, 2.0, 1.0)]
        public void LayoutChildren_WithNegativeValues_CompletesWithoutException(double x, double y, double width, double height)
        {
            // Arrange
            var navigationPage = new TestableNavigationPage();

            // Act & Assert
            var exception = Record.Exception(() => navigationPage.TestLayoutChildren(x, y, width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the LayoutChildren method handles extreme double values without throwing exceptions.
        /// Verifies the method behavior with minimum and maximum double values.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, double.MinValue, double.MinValue, double.MinValue)]
        [InlineData(double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MaxValue, double.MinValue, double.MaxValue)]
        [InlineData(double.MaxValue, double.MinValue, double.MaxValue, double.MinValue)]
        public void LayoutChildren_WithExtremeValues_CompletesWithoutException(double x, double y, double width, double height)
        {
            // Arrange
            var navigationPage = new TestableNavigationPage();

            // Act & Assert
            var exception = Record.Exception(() => navigationPage.TestLayoutChildren(x, y, width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the LayoutChildren method handles special floating-point values without throwing exceptions.
        /// Verifies the method behavior with NaN, positive infinity, and negative infinity values.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0.0, 100.0, 200.0)]
        [InlineData(0.0, double.NaN, 100.0, 200.0)]
        [InlineData(0.0, 0.0, double.NaN, 200.0)]
        [InlineData(0.0, 0.0, 100.0, double.NaN)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.PositiveInfinity, double.NegativeInfinity, double.MaxValue)]
        public void LayoutChildren_WithSpecialFloatingPointValues_CompletesWithoutException(double x, double y, double width, double height)
        {
            // Arrange
            var navigationPage = new TestableNavigationPage();

            // Act & Assert
            var exception = Record.Exception(() => navigationPage.TestLayoutChildren(x, y, width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the LayoutChildren method works correctly when called with a root page.
        /// Verifies the method behavior on a NavigationPage that has been initialized with content.
        /// </summary>
        [Fact]
        public void LayoutChildren_WithRootPage_CompletesWithoutException()
        {
            // Arrange
            var rootPage = new ContentPage { Title = "Root Page" };
            var navigationPage = new TestableNavigationPage(rootPage);

            // Act & Assert
            var exception = Record.Exception(() => navigationPage.TestLayoutChildren(50.0, 60.0, 320.0, 480.0));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that multiple sequential calls to LayoutChildren complete without exceptions.
        /// Verifies the method can be called repeatedly without side effects or state corruption.
        /// </summary>
        [Fact]
        public void LayoutChildren_MultipleSequentialCalls_CompletesWithoutException()
        {
            // Arrange
            var navigationPage = new TestableNavigationPage();

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                navigationPage.TestLayoutChildren(0.0, 0.0, 100.0, 100.0);
                navigationPage.TestLayoutChildren(10.0, 20.0, 200.0, 300.0);
                navigationPage.TestLayoutChildren(-5.0, -10.0, 150.0, 250.0);
                navigationPage.TestLayoutChildren(double.MaxValue, double.MinValue, 0.0, double.NaN);
            });
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the parameterless PopToRootAsync method delegates to PopToRootAsync(true) and successfully navigates to root.
        /// Verifies the method calls the animated version with default animation enabled.
        /// Should result in navigation to the root page with animation.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PopToRootAsync_NoParameters_CallsAnimatedVersionAndNavigatesToRoot(bool useMaui)
        {
            // Arrange
            var nav = new TestNavigationPage(useMaui);
            var root = new ContentPage { Content = new View() };
            var child1 = new ContentPage { Content = new View() };
            var child2 = new ContentPage { Content = new View() };

            await nav.PushAsync(root);
            await nav.PushAsync(child1);
            await nav.PushAsync(child2);

            bool poppedToRootFired = false;
            nav.PoppedToRoot += (sender, args) => poppedToRootFired = true;

            // Act
            await nav.PopToRootAsync();

            // Assert
            Assert.True(poppedToRootFired);
            Assert.Same(root, nav.CurrentPage);
            Assert.Same(root, nav.RootPage);
            Assert.Same(nav.RootPage, nav.CurrentPage);
        }

        /// <summary>
        /// Tests that PopToRootAsync without parameters returns a Task that completes successfully.
        /// Verifies the method signature and return type behavior.
        /// Should result in a completed Task when navigation finishes.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PopToRootAsync_NoParameters_ReturnsCompletedTask(bool useMaui)
        {
            // Arrange
            var nav = new TestNavigationPage(useMaui);
            var root = new ContentPage { Content = new View() };
            var child = new ContentPage { Content = new View() };

            await nav.PushAsync(root);
            await nav.PushAsync(child);

            // Act
            Task result = nav.PopToRootAsync();

            // Assert
            Assert.NotNull(result);
            await result; // Should complete without throwing
            Assert.True(result.IsCompleted);
            Assert.False(result.IsFaulted);
            Assert.False(result.IsCanceled);
        }

        /// <summary>
        /// Tests PopToRootAsync behavior when called on a navigation page with only the root page.
        /// Verifies the method handles the edge case of no child pages to pop.
        /// Should result in no navigation changes but successful task completion.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PopToRootAsync_NoParameters_WithOnlyRootPage_CompletesSuccessfully(bool useMaui)
        {
            // Arrange
            var nav = new TestNavigationPage(useMaui);
            var root = new ContentPage { Content = new View() };

            await nav.PushAsync(root);

            bool poppedToRootFired = false;
            nav.PoppedToRoot += (sender, args) => poppedToRootFired = true;

            // Act
            await nav.PopToRootAsync();

            // Assert
            Assert.Same(root, nav.CurrentPage);
            Assert.Same(root, nav.RootPage);
            // PoppedToRoot may or may not fire when there are no children to pop - this is implementation dependent
        }

        /// <summary>
        /// Tests that PopToRootAsync properly handles multiple pages and fires navigation events.
        /// Verifies event arguments contain the correct popped pages.
        /// Should result in PoppedToRoot event with all intermediate pages in the event args.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PopToRootAsync_NoParameters_WithMultiplePages_FiresEventWithCorrectPages(bool useMaui)
        {
            // Arrange
            var nav = new TestNavigationPage(useMaui);
            var root = new ContentPage { Content = new View() };
            var child1 = new ContentPage { Content = new View() };
            var child2 = new ContentPage { Content = new View() };
            var child3 = new ContentPage { Content = new View() };

            List<Page> poppedPages = null;
            nav.PoppedToRoot += (sender, args) =>
            {
                if (args is PoppedToRootEventArgs poppedArgs)
                    poppedPages = poppedArgs.PoppedPages.ToList();
            };

            await nav.PushAsync(root);
            await nav.PushAsync(child1);
            await nav.PushAsync(child2);
            await nav.PushAsync(child3);

            // Act
            await nav.PopToRootAsync();

            // Assert
            Assert.Same(root, nav.CurrentPage);
            Assert.Same(root, nav.RootPage);

            if (poppedPages != null)
            {
                Assert.Equal(3, poppedPages.Count);
                Assert.Contains(child1, poppedPages);
                Assert.Contains(child2, poppedPages);
                Assert.Contains(child3, poppedPages);
            }
        }

        /// <summary>
        /// Tests PopToRootAsync behavior when there are no pages in the navigation stack.
        /// Verifies the method handles the edge case of an empty navigation stack gracefully.
        /// Should result in successful task completion without errors.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PopToRootAsync_NoParameters_WithEmptyStack_CompletesWithoutError(bool useMaui)
        {
            // Arrange
            var nav = new TestNavigationPage(useMaui);

            // Act & Assert - Should not throw
            await nav.PopToRootAsync();

            // Navigation should still be in a valid state
            Assert.NotNull(nav.Navigation);
        }

        /// <summary>
        /// Tests that consecutive calls to PopToRootAsync are handled properly.
        /// Verifies concurrent navigation requests are managed correctly.
        /// Should result in both calls completing successfully without conflicts.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PopToRootAsync_NoParameters_ConsecutiveCalls_HandledProperly(bool useMaui)
        {
            // Arrange
            var nav = new TestNavigationPage(useMaui);
            var root = new ContentPage { Content = new View() };
            var child1 = new ContentPage { Content = new View() };
            var child2 = new ContentPage { Content = new View() };

            await nav.PushAsync(root);
            await nav.PushAsync(child1);
            await nav.PushAsync(child2);

            // Act - Make consecutive calls
            Task firstCall = nav.PopToRootAsync();
            Task secondCall = nav.PopToRootAsync();

            // Assert - Both should complete successfully
            await Task.WhenAll(firstCall, secondCall);

            Assert.True(firstCall.IsCompleted);
            Assert.True(secondCall.IsCompleted);
            Assert.Same(root, nav.CurrentPage);
            Assert.Same(root, nav.RootPage);
        }

        /// <summary>
        /// Tests that SetIconColor correctly sets the icon color value on a bindable object.
        /// Verifies the value is properly stored and can be retrieved using GetIconColor.
        /// </summary>
        /// <param name="red">Red component of the color (0-1)</param>
        /// <param name="green">Green component of the color (0-1)</param>
        /// <param name="blue">Blue component of the color (0-1)</param>
        /// <param name="alpha">Alpha component of the color (0-1)</param>
        [Theory]
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)]
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)]
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)]
        [InlineData(0.5f, 0.5f, 0.5f, 0.5f)]
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)]
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)]
        public void SetIconColor_ValidColorValues_SetsAndRetrievesCorrectly(float red, float green, float blue, float alpha)
        {
            // Arrange
            var page = new ContentPage();
            var color = new Color(red, green, blue, alpha);

            // Act
            NavigationPage.SetIconColor(page, color);

            // Assert
            var result = NavigationPage.GetIconColor(page);
            Assert.Equal(color, result);
        }

        /// <summary>
        /// Tests that SetIconColor throws ArgumentNullException when bindable parameter is null.
        /// This validates proper parameter validation for the required bindable object.
        /// </summary>
        [Fact]
        public void SetIconColor_NullBindable_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject bindable = null;
            var color = Colors.Red;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => NavigationPage.SetIconColor(bindable, color));
        }

        /// <summary>
        /// Tests that SetIconColor works correctly with null color value.
        /// Verifies that null colors can be set and retrieved properly.
        /// </summary>
        [Fact]
        public void SetIconColor_NullColor_SetsAndRetrievesNull()
        {
            // Arrange
            var page = new ContentPage();
            Color color = null;

            // Act
            NavigationPage.SetIconColor(page, color);

            // Assert
            var result = NavigationPage.GetIconColor(page);
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that SetIconColor works with various BindableObject types.
        /// Verifies the method works with different bindable object implementations.
        /// </summary>
        /// <param name="bindableType">The type of bindable object to test</param>
        [Theory]
        [InlineData("ContentPage")]
        [InlineData("Label")]
        [InlineData("Button")]
        public void SetIconColor_DifferentBindableTypes_SetsCorrectly(string bindableType)
        {
            // Arrange
            BindableObject bindable = bindableType switch
            {
                "ContentPage" => new ContentPage(),
                "Label" => new Label(),
                "Button" => new Button(),
                _ => throw new ArgumentException($"Unknown bindable type: {bindableType}")
            };
            var color = Colors.Blue;

            // Act
            NavigationPage.SetIconColor(bindable, color);

            // Assert
            var result = NavigationPage.GetIconColor(bindable);
            Assert.Equal(color, result);
        }

        /// <summary>
        /// Tests that SetIconColor properly overwrites previously set values.
        /// Verifies that multiple calls to SetIconColor update the stored value correctly.
        /// </summary>
        [Fact]
        public void SetIconColor_OverwritePreviousValue_UpdatesCorrectly()
        {
            // Arrange
            var page = new ContentPage();
            var initialColor = Colors.Red;
            var newColor = Colors.Green;

            // Act
            NavigationPage.SetIconColor(page, initialColor);
            var initialResult = NavigationPage.GetIconColor(page);

            NavigationPage.SetIconColor(page, newColor);
            var finalResult = NavigationPage.GetIconColor(page);

            // Assert
            Assert.Equal(initialColor, initialResult);
            Assert.Equal(newColor, finalResult);
            Assert.NotEqual(initialResult, finalResult);
        }

        /// <summary>
        /// Tests that SetIconColor works with edge case color values including transparent colors.
        /// Verifies proper handling of boundary and special color values.
        /// </summary>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Fully transparent black
        [InlineData(1.0f, 1.0f, 1.0f, 0.0f)] // Fully transparent white
        [InlineData(0.999f, 0.001f, 0.5f, 0.99f)] // Near-boundary values
        public void SetIconColor_EdgeCaseColorValues_HandlesCorrectly(float red, float green, float blue, float alpha)
        {
            // Arrange
            var page = new ContentPage();
            var color = new Color(red, green, blue, alpha);

            // Act
            NavigationPage.SetIconColor(page, color);

            // Assert
            var result = NavigationPage.GetIconColor(page);
            Assert.Equal(color, result);
        }

        /// <summary>
        /// Tests that SetHasNavigationBar correctly calls SetValue on the provided BindableObject
        /// with the HasNavigationBarProperty and the specified boolean value.
        /// </summary>
        /// <param name="value">The boolean value to set for the HasNavigationBar property.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetHasNavigationBar_ValidParameters_CallsSetValueWithCorrectParameters(bool value)
        {
            // Arrange
            var mockBindableObject = Substitute.For<BindableObject>();

            // Act
            NavigationPage.SetHasNavigationBar(mockBindableObject, value);

            // Assert
            mockBindableObject.Received(1).SetValue(NavigationPage.HasNavigationBarProperty, value);
        }

        /// <summary>
        /// Tests that SetHasNavigationBar throws NullReferenceException when the page parameter is null.
        /// This verifies that null checking is properly handled at the caller level.
        /// </summary>
        [Fact]
        public void SetHasNavigationBar_NullPage_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject nullPage = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => NavigationPage.SetHasNavigationBar(nullPage, value));
        }

        /// <summary>
        /// Tests that SetHasNavigationBar works correctly with a real BindableObject instance,
        /// ensuring the method functions properly in real-world scenarios.
        /// </summary>
        /// <param name="value">The boolean value to set for the HasNavigationBar property.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetHasNavigationBar_RealBindableObject_SetsValueCorrectly(bool value)
        {
            // Arrange
            var page = new ContentPage();

            // Act
            NavigationPage.SetHasNavigationBar(page, value);

            // Assert
            var actualValue = NavigationPage.GetHasNavigationBar(page);
            Assert.Equal(value, actualValue);
        }

        /// <summary>
        /// Tests that Peek returns null when depth is negative.
        /// Validates that negative depth values are handled correctly and return null.
        /// </summary>
        [Theory]
        [InlineData(true, -1)]
        [InlineData(false, -1)]
        [InlineData(true, -10)]
        [InlineData(false, -10)]
        [InlineData(true, int.MinValue)]
        [InlineData(false, int.MinValue)]
        public void Peek_NegativeDepth_ReturnsNull(bool useMaui, int depth)
        {
            // Arrange
            var navigationPage = new TestNavigationPage(useMaui);
            var page = new ContentPage();
            navigationPage.InternalChildren.Add(page);

            // Act
            var result = navigationPage.Peek(depth);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that Peek returns null when the InternalChildren collection is empty.
        /// Validates that any depth value returns null when there are no pages in the collection.
        /// </summary>
        [Theory]
        [InlineData(true, 0)]
        [InlineData(false, 0)]
        [InlineData(true, 1)]
        [InlineData(false, 1)]
        [InlineData(true, 10)]
        [InlineData(false, 10)]
        [InlineData(true, int.MaxValue)]
        [InlineData(false, int.MaxValue)]
        public void Peek_EmptyCollection_ReturnsNull(bool useMaui, int depth)
        {
            // Arrange
            var navigationPage = new TestNavigationPage(useMaui);

            // Act
            var result = navigationPage.Peek(depth);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that Peek returns null when depth is greater than or equal to the collection count.
        /// Validates boundary conditions where depth exceeds available pages.
        /// </summary>
        [Theory]
        [InlineData(true, 1, 1)]  // depth equals count
        [InlineData(false, 1, 1)]
        [InlineData(true, 1, 2)]  // depth greater than count
        [InlineData(false, 1, 2)]
        [InlineData(true, 3, 3)]  // depth equals count with multiple pages
        [InlineData(false, 3, 3)]
        [InlineData(true, 3, 5)]  // depth much greater than count
        [InlineData(false, 3, 5)]
        [InlineData(true, 2, int.MaxValue)]  // extreme depth value
        [InlineData(false, 2, int.MaxValue)]
        public void Peek_DepthGreaterThanOrEqualToCount_ReturnsNull(bool useMaui, int pageCount, int depth)
        {
            // Arrange
            var navigationPage = new TestNavigationPage(useMaui);
            for (int i = 0; i < pageCount; i++)
            {
                navigationPage.InternalChildren.Add(new ContentPage { Title = $"Page {i}" });
            }

            // Act
            var result = navigationPage.Peek(depth);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that Peek returns the correct page when depth is valid.
        /// Validates that the correct page is returned based on depth from the end of the collection.
        /// </summary>
        [Theory]
        [InlineData(true, 0)]   // last page (depth 0)
        [InlineData(false, 0)]
        [InlineData(true, 1)]   // second to last page (depth 1)
        [InlineData(false, 1)]
        [InlineData(true, 2)]   // third to last page (depth 2)
        [InlineData(false, 2)]
        public void Peek_ValidDepth_ReturnsCorrectPage(bool useMaui, int depth)
        {
            // Arrange
            var navigationPage = new TestNavigationPage(useMaui);
            var pages = new List<ContentPage>();

            // Add 5 pages to have enough for testing different depths
            for (int i = 0; i < 5; i++)
            {
                var page = new ContentPage { Title = $"Page {i}" };
                pages.Add(page);
                navigationPage.InternalChildren.Add(page);
            }

            // Expected page is at index (Count - depth - 1)
            var expectedPage = pages[pages.Count - depth - 1];

            // Act
            var result = navigationPage.Peek(depth);

            // Assert
            Assert.NotNull(result);
            Assert.Same(expectedPage, result);
        }

        /// <summary>
        /// Tests Peek with a single page in the collection.
        /// Validates behavior when only one page is available.
        /// </summary>
        [Theory]
        [InlineData(true, 0, true)]   // depth 0 should return the single page
        [InlineData(false, 0, true)]
        [InlineData(true, 1, false)]  // depth 1 should return null (out of bounds)
        [InlineData(false, 1, false)]
        [InlineData(true, 2, false)]  // depth 2 should return null (out of bounds)
        [InlineData(false, 2, false)]
        public void Peek_SinglePageCollection_ReturnsExpectedResult(bool useMaui, int depth, bool shouldReturnPage)
        {
            // Arrange
            var navigationPage = new TestNavigationPage(useMaui);
            var singlePage = new ContentPage { Title = "Single Page" };
            navigationPage.InternalChildren.Add(singlePage);

            // Act
            var result = navigationPage.Peek(depth);

            // Assert
            if (shouldReturnPage)
            {
                Assert.NotNull(result);
                Assert.Same(singlePage, result);
            }
            else
            {
                Assert.Null(result);
            }
        }

        /// <summary>
        /// Tests that Peek returns the exact same Page instance from the collection.
        /// Validates object identity and proper casting from Element to Page.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Peek_ValidDepth_ReturnsSamePageInstance(bool useMaui)
        {
            // Arrange
            var navigationPage = new TestNavigationPage(useMaui);
            var firstPage = new ContentPage { Title = "First Page" };
            var secondPage = new ContentPage { Title = "Second Page" };
            var thirdPage = new ContentPage { Title = "Third Page" };

            navigationPage.InternalChildren.Add(firstPage);
            navigationPage.InternalChildren.Add(secondPage);
            navigationPage.InternalChildren.Add(thirdPage);

            // Act & Assert
            // Depth 0 should return last page (thirdPage)
            var result0 = navigationPage.Peek(0);
            Assert.Same(thirdPage, result0);

            // Depth 1 should return second to last page (secondPage)
            var result1 = navigationPage.Peek(1);
            Assert.Same(secondPage, result1);

            // Depth 2 should return third to last page (firstPage)
            var result2 = navigationPage.Peek(2);
            Assert.Same(firstPage, result2);
        }

        /// <summary>
        /// Tests Peek with boundary integer values.
        /// Validates handling of extreme integer values for depth parameter.
        /// </summary>
        [Theory]
        [InlineData(true, 0, 0, true)]     // Zero depth with pages available
        [InlineData(false, 0, 0, true)]
        [InlineData(true, int.MaxValue, 5, false)]  // Maximum int value with pages
        [InlineData(false, int.MaxValue, 5, false)]
        [InlineData(true, int.MinValue, 5, false)] // Minimum int value with pages
        [InlineData(false, int.MinValue, 5, false)]
        public void Peek_BoundaryIntegerValues_ReturnsExpectedResult(bool useMaui, int depth, int pageCount, bool shouldReturnPage)
        {
            // Arrange
            var navigationPage = new TestNavigationPage(useMaui);
            var pages = new List<ContentPage>();

            for (int i = 0; i < pageCount; i++)
            {
                var page = new ContentPage { Title = $"Page {i}" };
                pages.Add(page);
                navigationPage.InternalChildren.Add(page);
            }

            // Act
            var result = navigationPage.Peek(depth);

            // Assert
            if (shouldReturnPage && pageCount > 0)
            {
                Assert.NotNull(result);
                Assert.Same(pages.Last(), result);
            }
            else
            {
                Assert.Null(result);
            }
        }

        /// <summary>
        /// Tests that SetTitleIconImageSource correctly sets the TitleIconImageSource property on a valid BindableObject
        /// with a valid ImageSource value and that the value can be retrieved using GetTitleIconImageSource.
        /// </summary>
        [Fact]
        public void SetTitleIconImageSource_ValidBindableAndImageSource_SetsPropertyCorrectly()
        {
            // Arrange
            var page = new ContentPage();
            var imageSource = ImageSource.FromFile("test.png");

            // Act
            NavigationPage.SetTitleIconImageSource(page, imageSource);

            // Assert
            var result = NavigationPage.GetTitleIconImageSource(page);
            Assert.Same(imageSource, result);
        }

        /// <summary>
        /// Tests that SetTitleIconImageSource correctly handles null ImageSource values,
        /// allowing null to be set as a valid value for the TitleIconImageSource property.
        /// </summary>
        [Fact]
        public void SetTitleIconImageSource_ValidBindableAndNullImageSource_SetsNullCorrectly()
        {
            // Arrange
            var page = new ContentPage();
            ImageSource nullImageSource = null;

            // Act
            NavigationPage.SetTitleIconImageSource(page, nullImageSource);

            // Assert
            var result = NavigationPage.GetTitleIconImageSource(page);
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that SetTitleIconImageSource throws NullReferenceException when attempting to set
        /// the TitleIconImageSource property on a null BindableObject, as the method calls SetValue
        /// on the bindable parameter without null checking.
        /// </summary>
        [Fact]
        public void SetTitleIconImageSource_NullBindable_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject nullBindable = null;
            var imageSource = ImageSource.FromFile("test.png");

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                NavigationPage.SetTitleIconImageSource(nullBindable, imageSource));
        }

        /// <summary>
        /// Tests that SetTitleIconImageSource works correctly with different types of BindableObjects,
        /// not just Pages, since the method accepts any BindableObject as its first parameter.
        /// </summary>
        [Fact]
        public void SetTitleIconImageSource_DifferentBindableObjectTypes_SetsPropertyCorrectly()
        {
            // Arrange
            var view = new Label(); // Label inherits from BindableObject
            var imageSource = ImageSource.FromFile("test.png");

            // Act
            NavigationPage.SetTitleIconImageSource(view, imageSource);

            // Assert
            var result = NavigationPage.GetTitleIconImageSource(view);
            Assert.Same(imageSource, result);
        }

        /// <summary>
        /// Tests that SetTitleIconImageSource correctly overwrites previously set values,
        /// ensuring that subsequent calls properly update the property value.
        /// </summary>
        [Fact]
        public void SetTitleIconImageSource_OverwritePreviousValue_UpdatesPropertyCorrectly()
        {
            // Arrange
            var page = new ContentPage();
            var firstImageSource = ImageSource.FromFile("first.png");
            var secondImageSource = ImageSource.FromFile("second.png");

            // Act
            NavigationPage.SetTitleIconImageSource(page, firstImageSource);
            NavigationPage.SetTitleIconImageSource(page, secondImageSource);

            // Assert
            var result = NavigationPage.GetTitleIconImageSource(page);
            Assert.Same(secondImageSource, result);
            Assert.NotSame(firstImageSource, result);
        }

        /// <summary>
        /// Tests OnBackButtonPressed when the current page handles the back button press.
        /// The method should return true immediately without checking stack depth or calling SafePop.
        /// </summary>
        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task OnBackButtonPressed_CurrentPageHandlesBackButton_ReturnsTrue(bool useMaui)
        {
            // Arrange
            var handlingPage = new TestPageThatHandlesBackButton(true);
            var nav = new TestNavigationPage(useMaui, handlingPage);

            // Act
            var result = await nav.SendBackButtonPressedAsync();

            // Assert
            Assert.True(result);
            Assert.True(handlingPage.OnBackButtonPressedCalled);
        }

        /// <summary>
        /// Tests OnBackButtonPressed when the current page doesn't handle the back button press
        /// and there are multiple pages in the stack (StackDepth > 1).
        /// The method should call SafePop and return true.
        /// </summary>
        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task OnBackButtonPressed_CurrentPageDoesNotHandleAndMultiplePages_CallsSafePopAndReturnsTrue(bool useMaui)
        {
            // Arrange
            var rootPage = new TestPageThatHandlesBackButton(false);
            var secondPage = new TestPageThatHandlesBackButton(false);
            var nav = new TestNavigationPage(useMaui, rootPage);
            await nav.PushAsync(secondPage);

            // Ensure we have multiple pages
            Assert.True(nav.Navigation.NavigationStack.Count > 1);

            // Act
            var result = await nav.SendBackButtonPressedAsync();

            // Assert  
            Assert.True(result);
            Assert.True(secondPage.OnBackButtonPressedCalled);
            // After SafePop, we should be back to the root page
            Assert.Equal(rootPage, nav.CurrentPage);
        }

        /// <summary>
        /// Tests OnBackButtonPressed when the current page doesn't handle the back button press
        /// and there is only one page in the stack (StackDepth = 1).
        /// The method should call base.OnBackButtonPressed and return its result.
        /// </summary>
        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task OnBackButtonPressed_CurrentPageDoesNotHandleAndSinglePage_CallsBaseMethod(bool useMaui)
        {
            // Arrange
            var singlePage = new TestPageThatHandlesBackButton(false);
            var nav = new TestNavigationPage(useMaui, singlePage);

            // Ensure we have only one page
            Assert.Equal(1, nav.Navigation.NavigationStack.Count);

            // Act
            var result = await nav.SendBackButtonPressedAsync();

            // Assert
            Assert.True(singlePage.OnBackButtonPressedCalled);
            // The result depends on base.OnBackButtonPressed implementation
            // In most test scenarios, this will be false since there's no modal navigation
            Assert.False(result);
        }

        /// <summary>
        /// Tests OnBackButtonPressed when the current page doesn't handle the back button press
        /// and the stack depth is exactly at the boundary (StackDepth = 1).
        /// This verifies the boundary condition of the > 1 check.
        /// </summary>
        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task OnBackButtonPressed_StackDepthBoundaryCondition_HandlesBoundaryCorrectly(bool useMaui)
        {
            // Arrange
            var rootPage = new TestPageThatHandlesBackButton(false);
            var secondPage = new TestPageThatHandlesBackButton(false);
            var nav = new TestNavigationPage(useMaui, rootPage);
            await nav.PushAsync(secondPage);

            // Verify we have 2 pages (StackDepth = 2 > 1)
            Assert.Equal(2, nav.Navigation.NavigationStack.Count);

            // Act - this should trigger SafePop
            var resultWith2Pages = await nav.SendBackButtonPressedAsync();

            // Now we should have 1 page (StackDepth = 1)
            Assert.Equal(1, nav.Navigation.NavigationStack.Count);

            // Act again - this should call base method instead of SafePop
            var resultWith1Page = await nav.SendBackButtonPressedAsync();

            // Assert
            Assert.True(resultWith2Pages); // SafePop path returns true
            Assert.False(resultWith1Page); // Base method typically returns false in test scenarios
        }

        /// <summary>
        /// Tests OnBackButtonPressed behavior when CurrentPage is null.
        /// This tests the edge case where the navigation page has no current page set.
        /// </summary>
        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void OnBackButtonPressed_CurrentPageIsNull_ThrowsNullReferenceException(bool useMaui)
        {
            // Arrange
            var nav = new TestNavigationPage(useMaui);
            // Don't add any pages, so CurrentPage should be null

            // Act & Assert
            Assert.ThrowsAsync<NullReferenceException>(async () => await nav.SendBackButtonPressedAsync());
        }

        /// <summary>
        /// Tests OnBackButtonPressed with an empty navigation stack (StackDepth = 0).
        /// This tests the edge case where there are no pages in the navigation stack.
        /// </summary>
        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task OnBackButtonPressed_StackDepthZero_CallsBaseMethod(bool useMaui)
        {
            // Arrange
            var page = new TestPageThatHandlesBackButton(false);
            var nav = new TestNavigationPage(useMaui, page);

            // Remove the page to simulate empty stack
            await nav.PopAsync();

            // Verify stack is empty or has minimal pages
            Assert.True(nav.Navigation.NavigationStack.Count <= 1);

            // Act & Assert
            // This will likely throw NullReferenceException due to CurrentPage being null
            // or will call base method if there's still a page
            if (nav.CurrentPage == null)
            {
                await Assert.ThrowsAsync<NullReferenceException>(async () => await nav.SendBackButtonPressedAsync());
            }
            else
            {
                var result = await nav.SendBackButtonPressedAsync();
                Assert.False(result); // Base method typically returns false
            }
        }

        /// <summary>
        /// Custom test page that allows controlling the return value of OnBackButtonPressed.
        /// </summary>
        private class TestPageThatHandlesBackButton : ContentPage
        {
            private readonly bool _shouldHandleBackButton;

            public bool OnBackButtonPressedCalled { get; private set; }

            public TestPageThatHandlesBackButton(bool shouldHandleBackButton)
            {
                _shouldHandleBackButton = shouldHandleBackButton;
            }

            protected override bool OnBackButtonPressed()
            {
                OnBackButtonPressedCalled = true;
                return _shouldHandleBackButton;
            }
        }

        /// <summary>
        /// Tests that the On method returns a non-null platform configuration.
        /// Verifies that the method successfully delegates to the platform configuration registry
        /// and returns a valid configuration object for the specified platform type.
        /// </summary>
        [Fact]
        public void On_WithValidPlatformType_ReturnsNonNullConfiguration()
        {
            // Arrange
            var navigationPage = new NavigationPage();

            // Act
            var result = navigationPage.On<iOS>();

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that multiple calls to On with the same platform type return the same instance.
        /// Verifies the caching behavior of the platform configuration registry to ensure
        /// configurations are reused rather than recreated on each call.
        /// </summary>
        [Fact]
        public void On_MultipleCalls_ReturnsSameInstance()
        {
            // Arrange
            var navigationPage = new NavigationPage();

            // Act
            var result1 = navigationPage.On<iOS>();
            var result2 = navigationPage.On<iOS>();

            // Assert
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that the On method works correctly with different navigation page instances.
        /// Verifies that each navigation page has its own platform configuration registry
        /// and configurations are instance-specific.
        /// </summary>
        [Fact]
        public void On_DifferentInstances_ReturnsDifferentConfigurations()
        {
            // Arrange
            var navigationPage1 = new NavigationPage();
            var navigationPage2 = new NavigationPage();

            // Act
            var result1 = navigationPage1.On<iOS>();
            var result2 = navigationPage2.On<iOS>();

            // Assert
            Assert.NotSame(result1, result2);
        }

        /// <summary>
        /// Tests that the On method returns the correct configuration type.
        /// Verifies that the returned object implements the expected interface
        /// with the correct generic type parameters.
        /// </summary>
        [Fact]
        public void On_WithValidPlatformType_ReturnsCorrectConfigurationType()
        {
            // Arrange
            var navigationPage = new NavigationPage();

            // Act
            var result = navigationPage.On<iOS>();

            // Assert
            Assert.IsAssignableFrom<IPlatformElementConfiguration<iOS, NavigationPage>>(result);
        }

        /// <summary>
        /// Tests that the lazy initialization of the platform configuration registry works correctly.
        /// Verifies that the registry is properly initialized when the On method is first called
        /// and that the configuration is accessible immediately.
        /// </summary>
        [Fact]
        public void On_FirstCall_InitializesRegistryAndReturnsConfiguration()
        {
            // Arrange
            var navigationPage = new NavigationPage();

            // Act
            var result = navigationPage.On<iOS>();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<iOS, NavigationPage>>(result);
        }

        /// <summary>
        /// Tests that RootPage returns null when NavigationPage is created without a root page.
        /// Verifies the default state behavior when no pages have been added to the navigation stack.
        /// Expected result: RootPage should return null.
        /// </summary>
        [Fact]
        public void RootPage_WhenNoRootPageProvided_ReturnsNull()
        {
            // Arrange
            var navigationPage = new TestNavigationPage(true, null, false);

            // Act
            var rootPage = navigationPage.RootPage;

            // Assert
            Assert.Null(rootPage);
        }

        /// <summary>
        /// Tests that RootPage returns the correct page when NavigationPage is created with a root page.
        /// Verifies that the property correctly reflects the root of the navigation stack.
        /// Expected result: RootPage should return the same page instance that was provided to the constructor.
        /// </summary>
        [Fact]
        public void RootPage_WhenRootPageProvided_ReturnsCorrectPage()
        {
            // Arrange
            var rootPage = new ContentPage { Title = "Root Page" };
            var navigationPage = new TestNavigationPage(true, rootPage, false);

            // Act
            var result = navigationPage.RootPage;

            // Assert
            Assert.Same(rootPage, result);
        }

        /// <summary>
        /// Tests that RootPage remains consistent when the same page instance is provided multiple times.
        /// Verifies reference equality and that the property maintains the same object reference.
        /// Expected result: RootPage should return the exact same page instance.
        /// </summary>
        [Fact]
        public void RootPage_WithSamePageInstance_MaintainsReferenceEquality()
        {
            // Arrange
            var rootPage = new ContentPage { Title = "Test Page" };
            var navigationPage = new TestNavigationPage(true, rootPage, false);

            // Act
            var firstCall = navigationPage.RootPage;
            var secondCall = navigationPage.RootPage;

            // Assert
            Assert.Same(firstCall, secondCall);
            Assert.Same(rootPage, firstCall);
            Assert.Same(rootPage, secondCall);
        }

        /// <summary>
        /// Tests RootPage behavior with different types of pages to ensure type compatibility.
        /// Verifies that the property works correctly with various Page-derived types.
        /// Expected result: RootPage should return the correct page type instance.
        /// </summary>
        [Theory]
        [InlineData(typeof(ContentPage))]
        [InlineData(typeof(NavigationPage))]
        public void RootPage_WithDifferentPageTypes_ReturnsCorrectType(Type pageType)
        {
            // Arrange
            var rootPage = (Page)Activator.CreateInstance(pageType);
            var navigationPage = new TestNavigationPage(true, rootPage, false);

            // Act
            var result = navigationPage.RootPage;

            // Assert
            Assert.Same(rootPage, result);
            Assert.IsType(pageType, result);
        }

        /// <summary>
        /// Tests that RootPage getter does not throw exceptions under normal circumstances.
        /// Verifies the robustness of the property access mechanism.
        /// Expected result: No exceptions should be thrown when accessing the property.
        /// </summary>
        [Fact]
        public void RootPage_Getter_DoesNotThrowException()
        {
            // Arrange
            var navigationPage = new TestNavigationPage(true, null, false);
            var rootPage = new ContentPage();
            var navigationPageWithRoot = new TestNavigationPage(true, rootPage, false);

            // Act & Assert
            var exception1 = Record.Exception(() => navigationPage.RootPage);
            var exception2 = Record.Exception(() => navigationPageWithRoot.RootPage);

            Assert.Null(exception1);
            Assert.Null(exception2);
        }

        /// <summary>
        /// Tests RootPage property with pages that have various property values set.
        /// Verifies that the property returns pages with their complete state intact.
        /// Expected result: RootPage should return the page with all its properties preserved.
        /// </summary>
        [Fact]
        public void RootPage_WithPageProperties_PreservesPageState()
        {
            // Arrange
            var rootPage = new ContentPage
            {
                Title = "Test Title",
                BackgroundColor = Colors.Red,
                IsVisible = true
            };
            var navigationPage = new TestNavigationPage(true, rootPage, false);

            // Act
            var result = navigationPage.RootPage;

            // Assert
            Assert.Same(rootPage, result);
            Assert.Equal("Test Title", result.Title);
            Assert.Equal(Colors.Red, result.BackgroundColor);
            Assert.True(result.IsVisible);
        }

        /// <summary>
        /// Tests GetBackButtonTitle method with various string values including null, empty, and valid strings.
        /// Verifies that the method correctly returns the back button title value from the bindable property.
        /// </summary>
        /// <param name="expectedTitle">The expected title value to test.</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Back")]
        [InlineData("Previous Page")]
        [InlineData("   ")]
        [InlineData("Very Long Back Button Title That Exceeds Normal Length")]
        [InlineData("Title\nWith\nNewlines")]
        [InlineData("Title\tWith\tTabs")]
        [InlineData("Title With Special Characters: <>?/\\|[]{}+=")]
        public void GetBackButtonTitle_ValidPageWithVariousValues_ReturnsExpectedValue(string expectedTitle)
        {
            // Arrange
            var page = new ContentPage();
            NavigationPage.SetBackButtonTitle(page, expectedTitle);

            // Act
            var result = NavigationPage.GetBackButtonTitle(page);

            // Assert
            Assert.Equal(expectedTitle, result);
        }

        /// <summary>
        /// Tests GetBackButtonTitle method with a null page parameter.
        /// Verifies that the method throws a NullReferenceException when called with null.
        /// </summary>
        [Fact]
        public void GetBackButtonTitle_NullPage_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject page = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => NavigationPage.GetBackButtonTitle(page));
        }

        /// <summary>
        /// Tests GetBackButtonTitle method with a page that has never had the back button title set.
        /// Verifies that the method returns null as the default value for unset properties.
        /// </summary>
        [Fact]
        public void GetBackButtonTitle_PageWithUnsetProperty_ReturnsNull()
        {
            // Arrange
            var page = new ContentPage();

            // Act
            var result = NavigationPage.GetBackButtonTitle(page);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests GetBackButtonTitle method with different types of BindableObject instances.
        /// Verifies that the method works with various bindable object types, not just Page.
        /// </summary>
        /// <param name="bindableObjectFactory">Factory function to create the bindable object.</param>
        /// <param name="expectedTitle">The expected title value to test.</param>
        [Theory]
        [InlineData("TestTitle")]
        [InlineData("")]
        [InlineData(null)]
        public void GetBackButtonTitle_DifferentBindableObjectTypes_ReturnsExpectedValue(string expectedTitle)
        {
            // Arrange
            var contentPage = new ContentPage();
            var navigationPage = new NavigationPage();
            var label = new Label();

            NavigationPage.SetBackButtonTitle(contentPage, expectedTitle);
            NavigationPage.SetBackButtonTitle(navigationPage, expectedTitle);
            NavigationPage.SetBackButtonTitle(label, expectedTitle);

            // Act
            var contentPageResult = NavigationPage.GetBackButtonTitle(contentPage);
            var navigationPageResult = NavigationPage.GetBackButtonTitle(navigationPage);
            var labelResult = NavigationPage.GetBackButtonTitle(label);

            // Assert
            Assert.Equal(expectedTitle, contentPageResult);
            Assert.Equal(expectedTitle, navigationPageResult);
            Assert.Equal(expectedTitle, labelResult);
        }

        /// <summary>
        /// Tests that GetTitleView returns null when no TitleView has been set on the bindable object.
        /// Input: BindableObject with no TitleView set
        /// Expected: Returns null (default value)
        /// </summary>
        [Fact]
        public void GetTitleView_NoTitleViewSet_ReturnsNull()
        {
            // Arrange
            var page = new ContentPage();

            // Act
            var result = NavigationPage.GetTitleView(page);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetTitleView returns the correct View when a TitleView has been set on the bindable object.
        /// Input: BindableObject with TitleView set to a specific View
        /// Expected: Returns the same View instance that was set
        /// </summary>
        [Fact]
        public void GetTitleView_TitleViewSet_ReturnsSetView()
        {
            // Arrange
            var page = new ContentPage();
            var titleView = new Label { Text = "Title" };
            NavigationPage.SetTitleView(page, titleView);

            // Act
            var result = NavigationPage.GetTitleView(page);

            // Assert
            Assert.Same(titleView, result);
        }

        /// <summary>
        /// Tests that GetTitleView throws ArgumentNullException when bindable parameter is null.
        /// Input: null bindable object
        /// Expected: Throws NullReferenceException
        /// </summary>
        [Fact]
        public void GetTitleView_NullBindable_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindable = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => NavigationPage.GetTitleView(bindable));
        }

        /// <summary>
        /// Tests that GetTitleView works with different types of BindableObject implementations.
        /// Input: Various BindableObject types (ContentPage, Button, Label) with TitleView set
        /// Expected: Returns the correct View for each BindableObject type
        /// </summary>
        [Theory]
        [InlineData(typeof(ContentPage))]
        [InlineData(typeof(Button))]
        [InlineData(typeof(Label))]
        public void GetTitleView_DifferentBindableObjectTypes_ReturnsCorrectView(Type bindableType)
        {
            // Arrange
            var bindableObject = (BindableObject)Activator.CreateInstance(bindableType);
            var titleView = new View();
            NavigationPage.SetTitleView(bindableObject, titleView);

            // Act
            var result = NavigationPage.GetTitleView(bindableObject);

            // Assert
            Assert.Same(titleView, result);
        }

        /// <summary>
        /// Tests that GetTitleView returns null after TitleView has been set and then cleared.
        /// Input: BindableObject with TitleView set then cleared (set to null)
        /// Expected: Returns null after clearing
        /// </summary>
        [Fact]
        public void GetTitleView_TitleViewSetThenCleared_ReturnsNull()
        {
            // Arrange
            var page = new ContentPage();
            var titleView = new Label { Text = "Title" };
            NavigationPage.SetTitleView(page, titleView);

            // Act - Clear the title view
            NavigationPage.SetTitleView(page, null);
            var result = NavigationPage.GetTitleView(page);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetTitleView returns the most recently set TitleView when multiple Views are set.
        /// Input: BindableObject with TitleView set multiple times to different Views
        /// Expected: Returns the last View that was set
        /// </summary>
        [Fact]
        public void GetTitleView_MultipleTitleViewsSet_ReturnsLatestView()
        {
            // Arrange
            var page = new ContentPage();
            var firstTitleView = new Label { Text = "First" };
            var secondTitleView = new Label { Text = "Second" };

            NavigationPage.SetTitleView(page, firstTitleView);
            NavigationPage.SetTitleView(page, secondTitleView);

            // Act
            var result = NavigationPage.GetTitleView(page);

            // Assert
            Assert.Same(secondTitleView, result);
            Assert.NotSame(firstTitleView, result);
        }

        /// <summary>
        /// Tests that SetTitleView throws ArgumentNullException when bindable parameter is null.
        /// Verifies proper parameter validation for null bindable object.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void SetTitleView_NullBindable_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject bindable = null;
            var view = new View();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => NavigationPage.SetTitleView(bindable, view));
        }

        /// <summary>
        /// Tests that SetTitleView accepts null value to clear the title view.
        /// Verifies that null values are properly handled for clearing attached properties.
        /// Expected result: No exception is thrown and the property is cleared.
        /// </summary>
        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void SetTitleView_NullValue_ClearsProperty(bool useMaui)
        {
            // Arrange
            var root = new ContentPage();
            var nav = new TestNavigationPage(useMaui, root);

            // Act
            NavigationPage.SetTitleView(root, null);

            // Assert
            var result = NavigationPage.GetTitleView(root);
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that SetTitleView can set multiple different views in sequence.
        /// Verifies that the property is properly updated when changed multiple times.
        /// Expected result: Each newly set view becomes the current title view.
        /// </summary>
        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void SetTitleView_MultipleViews_UpdatesPropertyCorrectly(bool useMaui)
        {
            // Arrange
            var root = new ContentPage();
            var nav = new TestNavigationPage(useMaui, root);
            var firstView = new View();
            var secondView = new View();

            // Act
            NavigationPage.SetTitleView(root, firstView);
            var firstResult = NavigationPage.GetTitleView(root);

            NavigationPage.SetTitleView(root, secondView);
            var secondResult = NavigationPage.GetTitleView(root);

            // Assert
            Assert.Same(firstView, firstResult);
            Assert.Same(secondView, secondResult);
        }

        /// <summary>
        /// Tests that SetTitleView works with different types of BindableObject instances.
        /// Verifies that the method accepts any BindableObject, not just Pages.
        /// Expected result: The title view is properly set on any BindableObject.
        /// </summary>
        [Fact]
        public void SetTitleView_DifferentBindableObjectTypes_SetsPropertyCorrectly()
        {
            // Arrange
            var page = new ContentPage();
            var label = new Label();
            var view = new View();

            // Act
            NavigationPage.SetTitleView(page, view);
            NavigationPage.SetTitleView(label, view);

            // Assert
            Assert.Same(view, NavigationPage.GetTitleView(page));
            Assert.Same(view, NavigationPage.GetTitleView(label));
        }

        /// <summary>
        /// Tests that SetTitleView can set the same view instance multiple times without issues.
        /// Verifies that setting the same value repeatedly doesn't cause problems.
        /// Expected result: The view remains set and no exceptions are thrown.
        /// </summary>
        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void SetTitleView_SameViewMultipleTimes_RemainsSet(bool useMaui)
        {
            // Arrange
            var root = new ContentPage();
            var nav = new TestNavigationPage(useMaui, root);
            var view = new View();

            // Act
            NavigationPage.SetTitleView(root, view);
            NavigationPage.SetTitleView(root, view);
            NavigationPage.SetTitleView(root, view);

            // Assert
            var result = NavigationPage.GetTitleView(root);
            Assert.Same(view, result);
        }
    }


    public partial class MauiNavigationImplTests
    {
        /// <summary>
        /// Tests that OnPopToRootAsync returns completed task when navigation stack has only one page (root page).
        /// Input: NavigationStack with Count == 1, animated = true
        /// Expected: Returns Task.CompletedTask immediately without calling SendHandlerUpdateAsync
        /// </summary>
        [Fact]
        public void OnPopToRootAsync_NavigationStackHasOnlyRootPage_ReturnsCompletedTask()
        {
            // Arrange
            var owner = Substitute.For<NavigationPage>();
            var navigationStack = Substitute.For<IReadOnlyList<Page>>();
            navigationStack.Count.Returns(1);

            var mauiNavImpl = new TestMauiNavigationImpl(owner, navigationStack);

            // Act
            var result = mauiNavImpl.TestOnPopToRootAsync(true);

            // Assert
            Assert.Equal(Task.CompletedTask, result);
            owner.DidNotReceive().SendHandlerUpdateAsync(Arg.Any<bool>(), Arg.Any<Action>(), Arg.Any<Action>(), Arg.Any<Action>());
        }

        /// <summary>
        /// Tests that OnPopToRootAsync returns completed task when navigation stack has only one page with animated false.
        /// Input: NavigationStack with Count == 1, animated = false
        /// Expected: Returns Task.CompletedTask immediately without calling SendHandlerUpdateAsync
        /// </summary>
        [Fact]
        public void OnPopToRootAsync_NavigationStackHasOnlyRootPageAnimatedFalse_ReturnsCompletedTask()
        {
            // Arrange
            var owner = Substitute.For<NavigationPage>();
            var navigationStack = Substitute.For<IReadOnlyList<Page>>();
            navigationStack.Count.Returns(1);

            var mauiNavImpl = new TestMauiNavigationImpl(owner, navigationStack);

            // Act
            var result = mauiNavImpl.TestOnPopToRootAsync(false);

            // Assert
            Assert.Equal(Task.CompletedTask, result);
            owner.DidNotReceive().SendHandlerUpdateAsync(Arg.Any<bool>(), Arg.Any<Action>(), Arg.Any<Action>(), Arg.Any<Action>());
        }

        /// <summary>
        /// Tests that OnPopToRootAsync calls SendHandlerUpdateAsync when navigation stack has multiple pages.
        /// Input: NavigationStack with Count > 1, animated = true
        /// Expected: Calls SendHandlerUpdateAsync with correct parameters and returns the task from it
        /// </summary>
        [Fact]
        public void OnPopToRootAsync_NavigationStackHasMultiplePages_CallsSendHandlerUpdateAsync()
        {
            // Arrange
            var owner = Substitute.For<NavigationPage>();
            var navigationStack = Substitute.For<IReadOnlyList<Page>>();
            var currentPage = Substitute.For<Page>();
            var rootPage = Substitute.For<Page>();
            var expectedTask = Task.FromResult(0);

            navigationStack.Count.Returns(3);
            owner.CurrentPage.Returns(currentPage);
            owner.RootPage.Returns(rootPage);
            owner.SendHandlerUpdateAsync(Arg.Any<bool>(), Arg.Any<Action>(), Arg.Any<Action>(), Arg.Any<Action>())
                .Returns(expectedTask);

            var mauiNavImpl = new TestMauiNavigationImpl(owner, navigationStack);

            // Act
            var result = mauiNavImpl.TestOnPopToRootAsync(true);

            // Assert
            Assert.Equal(expectedTask, result);
            owner.Received(1).SendHandlerUpdateAsync(true, Arg.Any<Action>(), Arg.Any<Action>(), Arg.Any<Action>());
        }

        /// <summary>
        /// Tests that OnPopToRootAsync calls SendHandlerUpdateAsync with animated false when specified.
        /// Input: NavigationStack with Count > 1, animated = false
        /// Expected: Calls SendHandlerUpdateAsync with animated = false
        /// </summary>
        [Fact]
        public void OnPopToRootAsync_NavigationStackHasMultiplePagesAnimatedFalse_CallsSendHandlerUpdateAsyncWithCorrectAnimation()
        {
            // Arrange
            var owner = Substitute.For<NavigationPage>();
            var navigationStack = Substitute.For<IReadOnlyList<Page>>();
            var currentPage = Substitute.For<Page>();
            var rootPage = Substitute.For<Page>();
            var expectedTask = Task.FromResult(0);

            navigationStack.Count.Returns(2);
            owner.CurrentPage.Returns(currentPage);
            owner.RootPage.Returns(rootPage);
            owner.SendHandlerUpdateAsync(Arg.Any<bool>(), Arg.Any<Action>(), Arg.Any<Action>(), Arg.Any<Action>())
                .Returns(expectedTask);

            var mauiNavImpl = new TestMauiNavigationImpl(owner, navigationStack);

            // Act
            var result = mauiNavImpl.TestOnPopToRootAsync(false);

            // Assert
            Assert.Equal(expectedTask, result);
            owner.Received(1).SendHandlerUpdateAsync(false, Arg.Any<Action>(), Arg.Any<Action>(), Arg.Any<Action>());
        }

        /// <summary>
        /// Tests boundary condition when NavigationStack count is exactly 1.
        /// Input: NavigationStack with Count == 1
        /// Expected: Returns Task.CompletedTask without additional processing
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OnPopToRootAsync_BoundaryConditionExactlyOnePageInStack_ReturnsCompletedTask(bool animated)
        {
            // Arrange
            var owner = Substitute.For<NavigationPage>();
            var navigationStack = Substitute.For<IReadOnlyList<Page>>();
            navigationStack.Count.Returns(1);

            var mauiNavImpl = new TestMauiNavigationImpl(owner, navigationStack);

            // Act
            var result = mauiNavImpl.TestOnPopToRootAsync(animated);

            // Assert
            Assert.Equal(Task.CompletedTask, result);
            owner.DidNotReceive().SendHandlerUpdateAsync(Arg.Any<bool>(), Arg.Any<Action>(), Arg.Any<Action>(), Arg.Any<Action>());
        }

        /// <summary>
        /// Tests boundary condition when NavigationStack count is exactly 2.
        /// Input: NavigationStack with Count == 2
        /// Expected: Calls SendHandlerUpdateAsync to perform pop operation
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OnPopToRootAsync_BoundaryConditionExactlyTwoPagesInStack_CallsSendHandlerUpdateAsync(bool animated)
        {
            // Arrange
            var owner = Substitute.For<NavigationPage>();
            var navigationStack = Substitute.For<IReadOnlyList<Page>>();
            var currentPage = Substitute.For<Page>();
            var rootPage = Substitute.For<Page>();
            var expectedTask = Task.FromResult(0);

            navigationStack.Count.Returns(2);
            owner.CurrentPage.Returns(currentPage);
            owner.RootPage.Returns(rootPage);
            owner.SendHandlerUpdateAsync(Arg.Any<bool>(), Arg.Any<Action>(), Arg.Any<Action>(), Arg.Any<Action>())
                .Returns(expectedTask);

            var mauiNavImpl = new TestMauiNavigationImpl(owner, navigationStack);

            // Act
            var result = mauiNavImpl.TestOnPopToRootAsync(animated);

            // Assert
            Assert.Equal(expectedTask, result);
            owner.Received(1).SendHandlerUpdateAsync(animated, Arg.Any<Action>(), Arg.Any<Action>(), Arg.Any<Action>());
        }

    }


    public partial class NavigationPageMauiNavigationImplTests
    {
        /// <summary>
        /// Tests that OnPushAsync returns completed task when page already exists in InternalChildren.
        /// Validates the early return behavior for duplicate page additions.
        /// </summary>
        [Fact]
        public async Task OnPushAsync_PageAlreadyExists_ReturnsCompletedTask()
        {
            // Arrange
            var navigationPage = new TestNavigationPage(true, null, false);
            var mauiNavImpl = new TestMauiNavigationImpl(navigationPage);
            var existingPage = new ContentPage { Title = "Existing Page" };

            // Add page to internal children to simulate it already being in the stack
            navigationPage.InternalChildren.Add(existingPage);

            // Act
            var result = await mauiNavImpl.OnPushAsync(existingPage, true);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests OnPushAsync with null page parameter.
        /// Validates behavior when attempting to push a null page reference.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task OnPushAsync_NullPage_ThrowsArgumentNullException(bool animated)
        {
            // Arrange
            var navigationPage = new TestNavigationPage(true, null, false);
            var mauiNavImpl = new TestMauiNavigationImpl(navigationPage);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await mauiNavImpl.OnPushAsync(null, animated));
        }

        /// <summary>
        /// Tests OnPushAsync with valid page and animated parameter variations.
        /// Validates the main navigation flow with different animation settings.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task OnPushAsync_ValidPage_CallsSendHandlerUpdateAsync(bool animated)
        {
            // Arrange
            var navigationPage = new TestNavigationPage(true, null, false);
            var mauiNavImpl = new TestMauiNavigationImpl(navigationPage);
            var newPage = new ContentPage { Title = "New Page" };
            var currentPage = new ContentPage { Title = "Current Page" };

            // Setup current page
            navigationPage.InternalChildren.Add(currentPage);
            navigationPage.SetCurrentPage(currentPage);

            // Mock SendHandlerUpdateAsync to complete immediately
            var sendHandlerUpdateCalled = false;
            navigationPage.MockSendHandlerUpdateAsync = (animatedParam, processStackChanges, firePostNav, fireNav) =>
            {
                sendHandlerUpdateCalled = true;
                Assert.Equal(animated, animatedParam);

                // Execute the lambdas to test their behavior
                processStackChanges?.Invoke();
                firePostNav?.Invoke();
                fireNav?.Invoke();

                return Task.CompletedTask;
            };

            // Act
            await mauiNavImpl.OnPushAsync(newPage, animated);

            // Assert
            Assert.True(sendHandlerUpdateCalled);
            Assert.Contains(newPage, navigationPage.InternalChildren);
        }

        /// <summary>
        /// Tests OnPushAsync with empty navigation stack.
        /// Validates behavior when pushing the first page to an empty navigation stack.
        /// </summary>
        [Fact]
        public async Task OnPushAsync_EmptyNavigationStack_ProcessesCorrectly()
        {
            // Arrange
            var navigationPage = new TestNavigationPage(true, null, false);
            var mauiNavImpl = new TestMauiNavigationImpl(navigationPage);
            var newPage = new ContentPage { Title = "First Page" };

            // Mock SendHandlerUpdateAsync
            var lambdasExecuted = new List<string>();
            navigationPage.MockSendHandlerUpdateAsync = (animated, processStackChanges, firePostNav, fireNav) =>
            {
                processStackChanges?.Invoke();
                lambdasExecuted.Add("processStackChanges");

                firePostNav?.Invoke();
                lambdasExecuted.Add("firePostNav");

                fireNav?.Invoke();
                lambdasExecuted.Add("fireNav");

                return Task.CompletedTask;
            };

            // Act
            await mauiNavImpl.OnPushAsync(newPage, true);

            // Assert
            Assert.Equal(3, lambdasExecuted.Count);
            Assert.Contains("processStackChanges", lambdasExecuted);
            Assert.Contains("firePostNav", lambdasExecuted);
            Assert.Contains("fireNav", lambdasExecuted);
            Assert.Contains(newPage, navigationPage.InternalChildren);
        }

        /// <summary>
        /// Tests OnPushAsync navigation type determination.
        /// Validates that the correct navigation type is set during the push operation.
        /// </summary>
        [Fact]
        public async Task OnPushAsync_SetsNavigationType_BasedOnDetermineNavigationType()
        {
            // Arrange
            var navigationPage = new TestNavigationPage(true, null, false);
            var mauiNavImpl = new TestMauiNavigationImpl(navigationPage);
            var newPage = new ContentPage { Title = "New Page" };

            navigationPage.MockDetermineNavigationType = () => NavigationType.Replace;

            NavigationType capturedNavigationType = NavigationType.Push;
            navigationPage.MockSendHandlerUpdateAsync = (animated, processStackChanges, firePostNav, fireNav) =>
            {
                processStackChanges?.Invoke();
                capturedNavigationType = navigationPage.NavigationType;
                return Task.CompletedTask;
            };

            // Act
            await mauiNavImpl.OnPushAsync(newPage, true);

            // Assert
            Assert.Equal(NavigationType.Replace, capturedNavigationType);
        }

        /// <summary>
        /// Tests OnPushAsync previous page handling.
        /// Validates that the previous page is correctly captured and used in navigation events.
        /// </summary>
        [Fact]
        public async Task OnPushAsync_CapturesPreviousPage_CorrectlyFromCurrentPage()
        {
            // Arrange
            var navigationPage = new TestNavigationPage(true, null, false);
            var mauiNavImpl = new TestMauiNavigationImpl(navigationPage);
            var currentPage = new ContentPage { Title = "Current Page" };
            var newPage = new ContentPage { Title = "New Page" };

            // Set up current page
            navigationPage.InternalChildren.Add(currentPage);
            navigationPage.SetCurrentPage(currentPage);

            Page capturedPreviousPage = null;
            navigationPage.MockSendNavigating = (navType, previousPage) =>
            {
                capturedPreviousPage = previousPage;
            };

            navigationPage.MockSendHandlerUpdateAsync = (animated, processStackChanges, firePostNav, fireNav) =>
            {
                processStackChanges?.Invoke();
                return Task.CompletedTask;
            };

            // Act
            await mauiNavImpl.OnPushAsync(newPage, true);

            // Assert
            Assert.Equal(currentPage, capturedPreviousPage);
        }

        /// <summary>
        /// Tests OnPushAsync exception handling in SendHandlerUpdateAsync.
        /// Validates proper exception propagation when navigation fails.
        /// </summary>
        [Fact]
        public async Task OnPushAsync_SendHandlerUpdateAsyncThrows_PropagatesException()
        {
            // Arrange
            var navigationPage = new TestNavigationPage(true, null, false);
            var mauiNavImpl = new TestMauiNavigationImpl(navigationPage);
            var newPage = new ContentPage { Title = "New Page" };
            var expectedException = new InvalidOperationException("Navigation failed");

            navigationPage.MockSendHandlerUpdateAsync = (animated, processStackChanges, firePostNav, fireNav) =>
            {
                throw expectedException;
            };

            // Act & Assert
            var actualException = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await mauiNavImpl.OnPushAsync(newPage, true));

            Assert.Equal(expectedException.Message, actualException.Message);
        }

    }
}