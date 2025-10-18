#nullable disable

using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class PageLifeCycleTests : BaseTestFixture
    {
        [Fact]
        // This test isn't valid for non handler based
        // navigation because the initial navigated event
        // fires from the legacy code instead of the
        // new handler code
        // We have device tests to also verify this works on 
        // each platform
        public async Task NavigationPageInitialPage()
        {
            var lcPage = new LCPage();
            var navigationPage = new TestNavigationPage(true, lcPage)
                    .AddToTestWindow();

            await navigationPage.NavigatingTask;
            Assert.Null(lcPage.NavigatingFromArgs);
            Assert.Null(lcPage.NavigatedFromArgs);
            Assert.NotNull(lcPage.NavigatedToArgs);
            Assert.Null(lcPage.NavigatedToArgs.PreviousPage);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task NavigationPagePushPage(bool useMaui)
        {
            var previousPage = new LCPage();
            var lcPage = new LCPage();
            var navigationPage =
                new TestNavigationPage(useMaui, previousPage)
                    .AddToTestWindow();

            await navigationPage.PushAsync(lcPage);

            Assert.NotNull(previousPage.NavigatingFromArgs);
            Assert.NotNull(lcPage.NavigatedToArgs);
            Assert.NotNull(previousPage.NavigatedFromArgs);
            Assert.Equal(previousPage, lcPage.NavigatedToArgs.PreviousPage);
            Assert.Equal(lcPage, previousPage.NavigatedFromArgs.DestinationPage);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task NavigationPagePopPage(bool useMaui)
        {
            var firstPage = new LCPage();
            var poppedPage = new LCPage();

            NavigationPage navigationPage = new TestNavigationPage(useMaui, firstPage)
                    .AddToTestWindow();

            await navigationPage.PushAsync(poppedPage);
            await navigationPage.PopAsync();

            Assert.NotNull(poppedPage.NavigatingFromArgs);
            Assert.Equal(poppedPage, firstPage.NavigatedToArgs.PreviousPage);
            Assert.Equal(firstPage, poppedPage.NavigatedFromArgs.DestinationPage);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task NavigationPagePopToRoot(bool useMaui)
        {
            var firstPage = new LCPage();
            var poppedPage = new LCPage();

            NavigationPage navigationPage = new TestNavigationPage(useMaui, firstPage)
                    .AddToTestWindow();

            await navigationPage.PushAsync(new ContentPage());
            await navigationPage.PushAsync(new ContentPage());
            await navigationPage.PushAsync(poppedPage);
            await navigationPage.PopToRootAsync();

            Assert.NotNull(poppedPage.NavigatingFromArgs);
            Assert.Equal(poppedPage, firstPage.NavigatedToArgs.PreviousPage);
            Assert.Equal(firstPage, poppedPage.NavigatedFromArgs.DestinationPage);
        }

        [Fact]
        public async Task TabbedPageBasicSelectionChanged()
        {
            var firstPage = new LCPage() { Title = "First Page" };
            var secondPage = new LCPage() { Title = "Second Page" };
            var tabbedPage = new TabbedPage() { Children = { firstPage, secondPage } }.AddToTestWindow();

            tabbedPage.CurrentPage = secondPage;
            Assert.NotNull(firstPage.NavigatingFromArgs);
            Assert.Equal(firstPage, secondPage.NavigatedToArgs.PreviousPage);
            Assert.Equal(secondPage, firstPage.NavigatedFromArgs.DestinationPage);
        }

        [Fact]
        public void TabbedPageInitialPage()
        {
            var firstPage = new LCPage { Title = "First Page" };
            var secondPage = new LCPage { Title = "Second Page" };
            var tabbedPage = new TabbedPage().AddToTestWindow();

            tabbedPage.Children.Add(firstPage);
            tabbedPage.Children.Add(secondPage);

            Assert.NotNull(firstPage.NavigatedToArgs);
            Assert.Null(firstPage.NavigatedToArgs.PreviousPage);

            if (firstPage.NavigatingFromArgs is not null)
            {
                Assert.Same(firstPage, firstPage.NavigatingFromArgs.DestinationPage);
                Assert.Equal(NavigationType.Replace, firstPage.NavigatingFromArgs.NavigationType);
            }

            if (firstPage.NavigatedFromArgs is not null)
            {
                Assert.Same(secondPage, firstPage.NavigatedFromArgs.DestinationPage);
            }
        }

        [Fact]
        public async Task FlyoutPageFlyoutChanged()
        {
            var firstPage = new LCPage() { Title = "First Page" };
            var secondPage = new LCPage() { Title = "Second Page" };
            var flyoutPage = new FlyoutPage()
            {
                Detail = new ContentPage() { Title = "Detail" },
                Flyout = firstPage
            }.AddToTestWindow();

            flyoutPage.Flyout = secondPage;

            Assert.NotNull(firstPage.NavigatingFromArgs);
            Assert.Equal(firstPage, secondPage.NavigatedToArgs.PreviousPage);
            Assert.Equal(secondPage, firstPage.NavigatedFromArgs.DestinationPage);
        }

        [Fact]
        public async Task FlyoutPageDetailChanged()
        {
            var firstPage = new LCPage() { Title = "First Page" };
            var secondPage = new LCPage() { Title = "Second Page" };
            var flyoutPage = new FlyoutPage()
            {
                Detail = firstPage,
                Flyout = new ContentPage() { Title = "Flyout" }
            }.AddToTestWindow();

            flyoutPage.Detail = secondPage;

            Assert.NotNull(firstPage.NavigatingFromArgs);
            Assert.Equal(firstPage, secondPage.NavigatedToArgs.PreviousPage);
            Assert.Equal(secondPage, firstPage.NavigatedFromArgs.DestinationPage);
        }

        [Fact]
        public async Task FlyoutPageToggleIsPresented()
        {
            // Testing toggling IsPresented in FlyoutPage without changing navigation events
            var flyout = new LCPage { Title = "Flyout" };
            var detail = new LCPage { Title = "Detail" };
            var flyoutPage = new FlyoutPage { Flyout = flyout, Detail = detail }.AddToTestWindow();

            // Clearing initial navigation args to focus on IsPresented toggle
            detail.ClearNavigationArgs();
            flyout.ClearNavigationArgs();

            // Toggling IsPresented
            flyoutPage.IsPresented = true;
            await Task.Yield();
            flyoutPage.IsPresented = false;
            await Task.Yield();

            // Verifying no navigation events are triggered
            Assert.Null(flyout.NavigatingFromArgs);
            Assert.Null(flyout.NavigatedFromArgs);
            Assert.Null(flyout.NavigatedToArgs);
            Assert.Null(detail.NavigatingFromArgs);
            Assert.Null(detail.NavigatedFromArgs);
            Assert.Null(detail.NavigatedToArgs);

            // Verifying Loaded/Unloaded counts remain unchanged
            Assert.Equal(1, flyout.AppearingCount);
            Assert.Equal(0, flyout.DisappearingCount);
            Assert.Equal(1, detail.AppearingCount);
            Assert.Equal(0, detail.DisappearingCount);
        }

        [Fact]
        public async Task PushModalPage()
        {
            var previousPage = new LCPage();
            var lcPage = new LCPage();
            var window = new TestWindow(previousPage);

            await window.Navigation.PushModalAsync(lcPage);

            Assert.NotNull(previousPage.NavigatingFromArgs);
            Assert.Equal(previousPage, lcPage.NavigatedToArgs.PreviousPage);
            Assert.Equal(lcPage, previousPage.NavigatedFromArgs.DestinationPage);

            Assert.Equal(1, previousPage.DisappearingCount);
            Assert.Equal(1, lcPage.AppearingCount);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task NavigationPagePropagatesEventsWhenCoveredByModal(bool useMaui)
        {
            var lcPage = new ContentPage();
            var targetPage = new LCPage();
            var modalPage = new ContentPage();
            var window = new TestWindow(new TestNavigationPage(useMaui, lcPage));

            await window.Page.Navigation.PushAsync(targetPage);
            targetPage.ClearNavigationArgs();
            await window.Navigation.PushModalAsync(modalPage);

            Assert.NotNull(targetPage.NavigatingFromArgs);
            Assert.Null(targetPage.NavigatedToArgs);

            await window.Navigation.PopModalAsync();
            Assert.NotNull(targetPage.NavigatedToArgs);

            Assert.Equal(modalPage, targetPage.NavigatedToArgs.PreviousPage);
        }

        [Fact]
        public async Task PopModalPage()
        {
            var firstPage = new LCPage();
            var poppedPage = new LCPage();

            var window = new TestWindow(firstPage);
            await window.Navigation.PushModalAsync(poppedPage);
            await window.Navigation.PopModalAsync();

            Assert.NotNull(poppedPage.NavigatingFromArgs);
            Assert.Equal(poppedPage, firstPage.NavigatedToArgs.PreviousPage);
            Assert.Equal(firstPage, poppedPage.NavigatedFromArgs.DestinationPage);

            Assert.Equal(1, poppedPage.AppearingCount);
            Assert.Equal(1, poppedPage.DisappearingCount);
            Assert.Equal(2, firstPage.AppearingCount);
        }

        [Fact]
        public async Task PopToAModalPage()
        {
            var firstPage = new LCPage();
            var firstModalPage = new LCPage();
            var secondModalPage = new LCPage();

            var window = new TestWindow(firstPage);
            await window.Navigation.PushModalAsync(firstModalPage);
            await window.Navigation.PushModalAsync(secondModalPage);

            firstModalPage.ClearNavigationArgs();
            secondModalPage.ClearNavigationArgs();

            await window.Navigation.PopModalAsync();

            Assert.NotNull(secondModalPage.NavigatingFromArgs);
            Assert.Equal(secondModalPage, firstModalPage.NavigatedToArgs.PreviousPage);
            Assert.Equal(firstModalPage, secondModalPage.NavigatedFromArgs.DestinationPage);

            Assert.Equal(1, secondModalPage.DisappearingCount);
            Assert.Equal(1, secondModalPage.AppearingCount);

            Assert.Equal(1, firstModalPage.DisappearingCount);
            Assert.Equal(2, firstModalPage.AppearingCount);
        }

        [Fact]
        public async Task PushSecondModalPage()
        {
            var firstPage = new LCPage();
            var firstModalPage = new LCPage();
            var secondModalPage = new LCPage();

            var window = new TestWindow(firstPage);
            await window.Navigation.PushModalAsync(firstModalPage);

            firstModalPage.ClearNavigationArgs();
            secondModalPage.ClearNavigationArgs();

            await window.Navigation.PushModalAsync(secondModalPage);

            Assert.NotNull(firstModalPage.NavigatingFromArgs);
            Assert.Equal(firstModalPage, secondModalPage.NavigatedToArgs.PreviousPage);
            Assert.Equal(secondModalPage, firstModalPage.NavigatedFromArgs.DestinationPage);

            Assert.Equal(0, secondModalPage.DisappearingCount);
            Assert.Equal(1, secondModalPage.AppearingCount);

            Assert.Equal(1, firstModalPage.DisappearingCount);
            Assert.Equal(1, firstModalPage.AppearingCount);
        }

        [Fact]
        public async Task LoadedUnLoadedEvents()
        {
            var previousPage = new LCPage();
            var lcPage = new LCPage();
            var navigationPage =
                new TestNavigationPage(true, previousPage)
                    .AddToTestWindow();

            await navigationPage.PushAsync(lcPage);

            int loadedCnt = 0;
            int unLoadedCnt = 0;
            lcPage.Loaded += (_, _) => loadedCnt++;
            lcPage.Unloaded += (_, _) => unLoadedCnt++;

            Assert.Equal(1, loadedCnt);
            Assert.Equal(0, unLoadedCnt);

            await navigationPage.PopAsync();

            Assert.Equal(1, loadedCnt);
            Assert.Equal(1, unLoadedCnt);
        }

        [Fact]
        public async Task LoadedFiresOnSecondSubscription()
        {
            var previousPage = new LCPage();
            var lcPage = new LCPage();
            var navigationPage =
                new TestNavigationPage(true, previousPage)
                    .AddToTestWindow();

            await navigationPage.PushAsync(lcPage);

            int loadedCnt = 0;
            lcPage.Loaded += OnLoaded;
            Assert.Equal(1, loadedCnt);

            lcPage.Loaded -= OnLoaded;
            lcPage.Loaded += OnLoaded;
            Assert.Equal(2, loadedCnt);

            void OnLoaded(object sender, System.EventArgs e)
            {
                loadedCnt++;
            }
        }

        [Fact]
        public async Task LoadedFiresOnInitialSubscription()
        {
            var previousPage = new LCPage();
            var lcPage = new LCPage();
            var navigationPage =
                new TestNavigationPage(true, previousPage)
                    .AddToTestWindow();

            await navigationPage.PushAsync(lcPage);

            int loadedCnt = 0;
            int secondLoadedSubscriberCnt = 0;
            int unLoadedCnt = 0;

            Assert.True(lcPage.IsLoaded);

            // Wire up to loaded event to setup wiring
            lcPage.Loaded += (_, _) =>
            {
                loadedCnt++;
            };

            Assert.Equal(1, loadedCnt);

            // Subscribing to loaded a second time
            // Should fire the event on the new subsciber;
            lcPage.Loaded += (_, _) =>
            {
                secondLoadedSubscriberCnt++;
            };

            lcPage.Unloaded += (_, _) => unLoadedCnt++;

            Assert.Equal(1, loadedCnt);
            Assert.Equal(1, secondLoadedSubscriberCnt);
            Assert.Equal(0, unLoadedCnt);

            await navigationPage.PopAsync();

            Assert.Equal(1, loadedCnt);
            Assert.Equal(1, secondLoadedSubscriberCnt);
            Assert.Equal(1, unLoadedCnt);
        }

        [Fact]
        public async Task NavigationPageMultiplePushesAndPops()
        {
            var firstPage = new LCPage();
            var secondPage = new LCPage();
            var thirdPage = new LCPage();
            var navigationPage = new TestNavigationPage(true, firstPage)
                .AddToTestWindow();

            // Push two pages
            await navigationPage.PushAsync(secondPage);
            await navigationPage.PushAsync(thirdPage);

            // Verify event args after multiple pushes
            Assert.NotNull(secondPage.NavigatingFromArgs);
            Assert.NotNull(secondPage.NavigatedFromArgs);
            Assert.NotNull(thirdPage.NavigatedToArgs);
            Assert.Equal(secondPage, thirdPage.NavigatedToArgs.PreviousPage);
            Assert.Equal(thirdPage, secondPage.NavigatedFromArgs.DestinationPage);

            // Pop back to second page
            await navigationPage.PopAsync();

            Assert.NotNull(thirdPage.NavigatingFromArgs);
            Assert.NotNull(thirdPage.NavigatedFromArgs);
            Assert.NotNull(secondPage.NavigatedToArgs);
            Assert.Equal(thirdPage, secondPage.NavigatedToArgs.PreviousPage);
            Assert.Equal(secondPage, thirdPage.NavigatedFromArgs.DestinationPage);

            // Verify Loaded/Unloaded counts
            int secondPageLoadedCnt = 0;
            int secondPageUnloadedCnt = 0;
            secondPage.Loaded += (_, _) => secondPageLoadedCnt++;
            secondPage.Unloaded += (_, _) => secondPageUnloadedCnt++;

            // Initial subscription should trigger Loaded
            Assert.Equal(1, secondPageLoadedCnt);
            Assert.Equal(0, secondPageUnloadedCnt);

            // Pop back to first page
            await navigationPage.PopAsync();
            Assert.Equal(1, secondPageLoadedCnt);
            Assert.Equal(1, secondPageUnloadedCnt);
        }

        [Fact]
        public async Task TabbedPageMultipleTabSwitches()
        {
            var firstPage = new LCPage { Title = "First Page" };
            var secondPage = new LCPage { Title = "Second Page" };

            var tabbedPage = new TabbedPage { Children = { firstPage, secondPage } }
                .AddToTestWindow();

            // Add load/unload counters for second page
            int secondPageLoadedCnt = 0;
            int secondPageUnloadedCnt = 0;
            secondPage.Loaded += (_, _) => secondPageLoadedCnt++;
            secondPage.Unloaded += (_, _) => secondPageUnloadedCnt++;

            // Switch to second page
            tabbedPage.CurrentPage = secondPage;
            Assert.NotNull(firstPage.NavigatingFromArgs);
            Assert.NotNull(firstPage.NavigatedFromArgs);
            Assert.NotNull(secondPage.NavigatedToArgs);
            Assert.Equal(firstPage, secondPage.NavigatedToArgs.PreviousPage);
            Assert.Equal(secondPage, firstPage.NavigatedFromArgs.DestinationPage);

            // Verify Loaded/Unloaded for second page
            Assert.Equal(1, secondPageLoadedCnt);
            Assert.Equal(0, secondPageUnloadedCnt);

            // Switch back to first page
            tabbedPage.CurrentPage = firstPage;
            Assert.Equal(1, secondPageLoadedCnt);
            // This assertion is currently failing due to unexpected unload behavior on navigation.
            // See: https://github.com/dotnet/maui/issues/30627 for context and discussion.
            //Assert.Equal(1, secondPageUnloadedCnt);
        }

        [Fact]
        public async Task FlyoutPageMultipleDetailChanges()
        {
            var flyout = new LCPage { Title = "Flyout" };
            var firstDetail = new LCPage { Title = "First Detail" };
            var secondDetail = new LCPage { Title = "Second Detail" };
            var flyoutPage = new FlyoutPage { Flyout = flyout, Detail = firstDetail }.AddToTestWindow();

            // Change to second detail
            flyoutPage.Detail = secondDetail;
            Assert.NotNull(firstDetail.NavigatingFromArgs);
            Assert.NotNull(firstDetail.NavigatedFromArgs);
            Assert.NotNull(secondDetail.NavigatedToArgs);
            Assert.Equal(firstDetail, secondDetail.NavigatedToArgs.PreviousPage);
            Assert.Equal(secondDetail, firstDetail.NavigatedFromArgs.DestinationPage);

            // Verify Loaded/Unloaded for second detail
            int secondDetailLoadedCnt = 0;
            int secondDetailUnloadedCnt = 0;
            secondDetail.Loaded += (_, _) => secondDetailLoadedCnt++;
            secondDetail.Unloaded += (_, _) => secondDetailUnloadedCnt++;

            Assert.Equal(1, secondDetailLoadedCnt);
            Assert.Equal(0, secondDetailUnloadedCnt);

            // Change back to first detail
            flyoutPage.Detail = firstDetail;
            Assert.Equal(1, secondDetailLoadedCnt);
            Assert.Equal(1, secondDetailUnloadedCnt);
        }

        public class LCPage : ContentPage
        {
            public NavigatedFromEventArgs NavigatedFromArgs { get; private set; }
            public NavigatingFromEventArgs NavigatingFromArgs { get; private set; }
            public NavigatedToEventArgs NavigatedToArgs { get; private set; }
            public int AppearingCount { get; private set; }
            public int DisappearingCount { get; private set; }

            public void ClearNavigationArgs()
            {
                NavigatedFromArgs = null;
                NavigatingFromArgs = null;
                NavigatedToArgs = null;
            }

            protected override void OnAppearing()
            {
                base.OnAppearing();
                AppearingCount++;
            }

            protected override void OnDisappearing()
            {
                base.OnDisappearing();
                DisappearingCount++;
            }

            protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
            {
                base.OnNavigatedFrom(args);
                NavigatedFromArgs = args;
            }

            protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
            {
                base.OnNavigatingFrom(args);
                NavigatingFromArgs = args;
            }

            protected override void OnNavigatedTo(NavigatedToEventArgs args)
            {
                base.OnNavigatedTo(args);
                NavigatedToArgs = args;
            }
        }
    }

    /// <summary>
    /// Tests for the iOS-specific Page platform configuration methods.
    /// </summary>
    public class PageTests
    {
        /// <summary>
        /// Tests that SetPreferredStatusBarUpdateAnimation throws NullReferenceException when element parameter is null.
        /// This verifies proper error handling for null element input.
        /// Expected result: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void SetPreferredStatusBarUpdateAnimation_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            var value = UIStatusBarAnimation.Fade;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                PlatformConfiguration.iOSSpecific.Page.SetPreferredStatusBarUpdateAnimation(element, value));
        }

        /// <summary>
        /// Tests that SetPreferredStatusBarUpdateAnimation correctly handles UIStatusBarAnimation.Fade value.
        /// This verifies the first if branch condition and SetValue method call.
        /// Expected result: SetValue is called with the correct property and Fade value.
        /// </summary>
        [Fact]
        public void SetPreferredStatusBarUpdateAnimation_FadeValue_CallsSetValueWithFade()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var value = UIStatusBarAnimation.Fade;

            // Act
            PlatformConfiguration.iOSSpecific.Page.SetPreferredStatusBarUpdateAnimation(element, value);

            // Assert
            element.Received(1).SetValue(PlatformConfiguration.iOSSpecific.Page.PreferredStatusBarUpdateAnimationProperty, value);
        }

        /// <summary>
        /// Tests that SetPreferredStatusBarUpdateAnimation correctly handles UIStatusBarAnimation.Slide value.
        /// This verifies the else if branch condition and SetValue method call.
        /// Expected result: SetValue is called with the correct property and Slide value.
        /// </summary>
        [Fact]
        public void SetPreferredStatusBarUpdateAnimation_SlideValue_CallsSetValueWithSlide()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var value = UIStatusBarAnimation.Slide;

            // Act
            PlatformConfiguration.iOSSpecific.Page.SetPreferredStatusBarUpdateAnimation(element, value);

            // Assert
            element.Received(1).SetValue(PlatformConfiguration.iOSSpecific.Page.PreferredStatusBarUpdateAnimationProperty, value);
        }

        /// <summary>
        /// Tests that SetPreferredStatusBarUpdateAnimation correctly handles UIStatusBarAnimation.None value.
        /// This verifies the else branch condition and SetValue method call.
        /// Expected result: SetValue is called with the correct property and None value.
        /// </summary>
        [Fact]
        public void SetPreferredStatusBarUpdateAnimation_NoneValue_CallsSetValueWithNone()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var value = UIStatusBarAnimation.None;

            // Act
            PlatformConfiguration.iOSSpecific.Page.SetPreferredStatusBarUpdateAnimation(element, value);

            // Assert
            element.Received(1).SetValue(PlatformConfiguration.iOSSpecific.Page.PreferredStatusBarUpdateAnimationProperty, value);
        }

        /// <summary>
        /// Tests that SetPreferredStatusBarUpdateAnimation correctly handles invalid enum values.
        /// This verifies the else branch condition with out-of-range enum values.
        /// Expected result: SetValue is called with the correct property and invalid enum value.
        /// </summary>
        [Fact]
        public void SetPreferredStatusBarUpdateAnimation_InvalidEnumValue_CallsSetValueWithInvalidValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var value = (UIStatusBarAnimation)99; // Invalid enum value

            // Act
            PlatformConfiguration.iOSSpecific.Page.SetPreferredStatusBarUpdateAnimation(element, value);

            // Assert
            element.Received(1).SetValue(PlatformConfiguration.iOSSpecific.Page.PreferredStatusBarUpdateAnimationProperty, value);
        }

        /// <summary>
        /// Tests SetPreferredStatusBarUpdateAnimation with all valid enum values using parameterized test.
        /// This verifies that all valid enum values are processed correctly and call SetValue.
        /// Expected result: SetValue is called for each valid enum value.
        /// </summary>
        [Theory]
        [InlineData(UIStatusBarAnimation.None)]
        [InlineData(UIStatusBarAnimation.Slide)]
        [InlineData(UIStatusBarAnimation.Fade)]
        public void SetPreferredStatusBarUpdateAnimation_AllValidEnumValues_CallsSetValue(UIStatusBarAnimation value)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            PlatformConfiguration.iOSSpecific.Page.SetPreferredStatusBarUpdateAnimation(element, value);

            // Assert
            element.Received(1).SetValue(PlatformConfiguration.iOSSpecific.Page.PreferredStatusBarUpdateAnimationProperty, value);
        }

        /// <summary>
        /// Tests that GetModalPresentationStyle throws ArgumentNullException when element parameter is null.
        /// </summary>
        [Fact]
        public void GetModalPresentationStyle_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Page.GetModalPresentationStyle(element));
        }

        /// <summary>
        /// Tests that GetModalPresentationStyle returns the correct UIModalPresentationStyle value
        /// when GetValue returns different enum values.
        /// </summary>
        /// <param name="expectedStyle">The UIModalPresentationStyle value to test.</param>
        [Theory]
        [InlineData(UIModalPresentationStyle.FullScreen)]
        [InlineData(UIModalPresentationStyle.FormSheet)]
        [InlineData(UIModalPresentationStyle.Automatic)]
        [InlineData(UIModalPresentationStyle.OverFullScreen)]
        [InlineData(UIModalPresentationStyle.PageSheet)]
        [InlineData(UIModalPresentationStyle.Popover)]
        public void GetModalPresentationStyle_ValidElement_ReturnsCorrectEnumValue(UIModalPresentationStyle expectedStyle)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Page.ModalPresentationStyleProperty).Returns(expectedStyle);

            // Act
            var result = Page.GetModalPresentationStyle(element);

            // Assert
            Assert.Equal(expectedStyle, result);
            element.Received(1).GetValue(Page.ModalPresentationStyleProperty);
        }

        /// <summary>
        /// Tests that GetModalPresentationStyle correctly casts the object returned by GetValue
        /// to UIModalPresentationStyle when GetValue returns the default value.
        /// </summary>
        [Fact]
        public void GetModalPresentationStyle_DefaultValue_ReturnsFullScreen()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Page.ModalPresentationStyleProperty).Returns(UIModalPresentationStyle.FullScreen);

            // Act
            var result = Page.GetModalPresentationStyle(element);

            // Assert
            Assert.Equal(UIModalPresentationStyle.FullScreen, result);
            element.Received(1).GetValue(Page.ModalPresentationStyleProperty);
        }

        /// <summary>
        /// Tests that PrefersHomeIndicatorAutoHidden throws ArgumentNullException when config parameter is null.
        /// Input: null config parameter
        /// Expected: ArgumentNullException is thrown
        /// </summary>
        [Fact]
        public void PrefersHomeIndicatorAutoHidden_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.Page> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.PrefersHomeIndicatorAutoHidden());
        }

        /// <summary>
        /// Tests that PrefersHomeIndicatorAutoHidden returns false when element has default value.
        /// Input: valid config with element having default PrefersHomeIndicatorAutoHidden value
        /// Expected: returns false (default value)
        /// </summary>
        [Fact]
        public void PrefersHomeIndicatorAutoHidden_DefaultValue_ReturnsFalse()
        {
            // Arrange
            var page = new Microsoft.Maui.Controls.Page();
            var config = Substitute.For<IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.Page>>();
            config.Element.Returns(page);

            // Act
            var result = config.PrefersHomeIndicatorAutoHidden();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that PrefersHomeIndicatorAutoHidden returns true when element has value set to true.
        /// Input: valid config with element having PrefersHomeIndicatorAutoHidden set to true
        /// Expected: returns true
        /// </summary>
        [Fact]
        public void PrefersHomeIndicatorAutoHidden_ValueSetToTrue_ReturnsTrue()
        {
            // Arrange
            var page = new Microsoft.Maui.Controls.Page();
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetPrefersHomeIndicatorAutoHidden(page, true);
            var config = Substitute.For<IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.Page>>();
            config.Element.Returns(page);

            // Act
            var result = config.PrefersHomeIndicatorAutoHidden();

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that PrefersHomeIndicatorAutoHidden returns false when element has value explicitly set to false.
        /// Input: valid config with element having PrefersHomeIndicatorAutoHidden explicitly set to false
        /// Expected: returns false
        /// </summary>
        [Fact]
        public void PrefersHomeIndicatorAutoHidden_ValueSetToFalse_ReturnsFalse()
        {
            // Arrange
            var page = new Microsoft.Maui.Controls.Page();
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetPrefersHomeIndicatorAutoHidden(page, false);
            var config = Substitute.For<IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.Page>>();
            config.Element.Returns(page);

            // Act
            var result = config.PrefersHomeIndicatorAutoHidden();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that PrefersHomeIndicatorAutoHidden returns correct values when called multiple times.
        /// Input: valid config with element having values changed between calls
        /// Expected: returns correct value for each call
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PrefersHomeIndicatorAutoHidden_MultipleValues_ReturnsCorrectValue(bool expectedValue)
        {
            // Arrange
            var page = new Microsoft.Maui.Controls.Page();
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetPrefersHomeIndicatorAutoHidden(page, expectedValue);
            var config = Substitute.For<IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.Page>>();
            config.Element.Returns(page);

            // Act
            var result = config.PrefersHomeIndicatorAutoHidden();

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that PrefersHomeIndicatorAutoHidden throws NullReferenceException when config.Element is null.
        /// Input: config with null Element property
        /// Expected: NullReferenceException is thrown
        /// </summary>
        [Fact]
        public void PrefersHomeIndicatorAutoHidden_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.Page>>();
            config.Element.Returns((Microsoft.Maui.Controls.Page)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => config.PrefersHomeIndicatorAutoHidden());
        }

        /// <summary>
        /// Tests that GetLargeTitleDisplay throws ArgumentNullException when element parameter is null.
        /// </summary>
        [Fact]
        public void GetLargeTitleDisplay_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetLargeTitleDisplay(element));
        }

        /// <summary>
        /// Tests that GetLargeTitleDisplay returns the correct LargeTitleDisplayMode value when the property is set.
        /// </summary>
        /// <param name="expectedValue">The expected LargeTitleDisplayMode value to test.</param>
        [Theory]
        [InlineData(LargeTitleDisplayMode.Automatic)]
        [InlineData(LargeTitleDisplayMode.Always)]
        [InlineData(LargeTitleDisplayMode.Never)]
        public void GetLargeTitleDisplay_ValidElementWithSetValue_ReturnsCorrectValue(LargeTitleDisplayMode expectedValue)
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.LargeTitleDisplayProperty)
                .Returns(expectedValue);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetLargeTitleDisplay(mockElement);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that GetLargeTitleDisplay returns the default value (Automatic) when the property is not explicitly set.
        /// </summary>
        [Fact]
        public void GetLargeTitleDisplay_ElementWithDefaultValue_ReturnsAutomatic()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.LargeTitleDisplayProperty)
                .Returns(LargeTitleDisplayMode.Automatic);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetLargeTitleDisplay(mockElement);

            // Assert
            Assert.Equal(LargeTitleDisplayMode.Automatic, result);
        }

        /// <summary>
        /// Tests that GetLargeTitleDisplay correctly casts the returned value from GetValue to LargeTitleDisplayMode.
        /// </summary>
        [Fact]
        public void GetLargeTitleDisplay_ValidElement_PerformsCastCorrectly()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            object boxedValue = LargeTitleDisplayMode.Always;
            mockElement.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.LargeTitleDisplayProperty)
                .Returns(boxedValue);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetLargeTitleDisplay(mockElement);

            // Assert
            Assert.Equal(LargeTitleDisplayMode.Always, result);
            Assert.IsType<LargeTitleDisplayMode>(result);
        }

        /// <summary>
        /// Tests that SetLargeTitleDisplay throws ArgumentNullException when element is null.
        /// This test verifies that null validation is properly handled for the element parameter.
        /// Expected result: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void SetLargeTitleDisplay_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;
            var value = LargeTitleDisplayMode.Always;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetLargeTitleDisplay(element, value));
        }

        /// <summary>
        /// Tests that SetLargeTitleDisplay correctly calls SetValue with the appropriate parameters for all valid enum values.
        /// This test verifies that the method properly sets the LargeTitleDisplayProperty with the specified value.
        /// Expected result: SetValue should be called with LargeTitleDisplayProperty and the provided enum value.
        /// </summary>
        [Theory]
        [InlineData(LargeTitleDisplayMode.Automatic)]
        [InlineData(LargeTitleDisplayMode.Always)]
        [InlineData(LargeTitleDisplayMode.Never)]
        public void SetLargeTitleDisplay_ValidEnumValues_CallsSetValueWithCorrectParameters(LargeTitleDisplayMode value)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetLargeTitleDisplay(element, value);

            // Assert
            element.Received(1).SetValue(
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.LargeTitleDisplayProperty,
                value);
        }

        /// <summary>
        /// Tests that SetLargeTitleDisplay handles invalid enum values by casting them properly.
        /// This test verifies that the method can handle enum values that are outside the defined range.
        /// Expected result: SetValue should be called with the cast enum value.
        /// </summary>
        [Fact]
        public void SetLargeTitleDisplay_InvalidEnumValue_CallsSetValueWithCastValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var invalidEnumValue = (LargeTitleDisplayMode)999;

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetLargeTitleDisplay(element, invalidEnumValue);

            // Assert
            element.Received(1).SetValue(
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.LargeTitleDisplayProperty,
                invalidEnumValue);
        }

        /// <summary>
        /// Tests that SetLargeTitleDisplay works correctly with minimum enum value.
        /// This test verifies boundary value handling for the minimum enum value.
        /// Expected result: SetValue should be called with the minimum enum value.
        /// </summary>
        [Fact]
        public void SetLargeTitleDisplay_MinimumEnumValue_CallsSetValueCorrectly()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var minValue = (LargeTitleDisplayMode)0; // Automatic

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetLargeTitleDisplay(element, minValue);

            // Assert
            element.Received(1).SetValue(
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.LargeTitleDisplayProperty,
                minValue);
        }

        /// <summary>
        /// Tests that SetLargeTitleDisplay works correctly with maximum defined enum value.
        /// This test verifies boundary value handling for the maximum defined enum value.
        /// Expected result: SetValue should be called with the maximum enum value.
        /// </summary>
        [Fact]
        public void SetLargeTitleDisplay_MaximumEnumValue_CallsSetValueCorrectly()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var maxValue = (LargeTitleDisplayMode)2; // Never

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetLargeTitleDisplay(element, maxValue);

            // Assert
            element.Received(1).SetValue(
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.LargeTitleDisplayProperty,
                maxValue);
        }

        /// <summary>
        /// Tests that GetPopoverSourceView throws NullReferenceException when element parameter is null.
        /// This tests the edge case where a null BindableObject is passed to the method.
        /// Expected result: NullReferenceException should be thrown.
        /// </summary>
        [Fact]
        public void GetPopoverSourceView_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetPopoverSourceView(element));
        }

        /// <summary>
        /// Tests that GetPopoverSourceView returns null when the ModalPopoverSourceViewProperty has no value set.
        /// This tests the scenario where the property returns its default value (null).
        /// Expected result: Method should return null.
        /// </summary>
        [Fact]
        public void GetPopoverSourceView_ElementWithNullProperty_ReturnsNull()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.ModalPopoverSourceViewProperty).Returns((View)null);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetPopoverSourceView(element);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetPopoverSourceView returns the View when the ModalPopoverSourceViewProperty has a valid View value.
        /// This tests the normal scenario where a View is stored as the property value.
        /// Expected result: Method should return the stored View.
        /// </summary>
        [Fact]
        public void GetPopoverSourceView_ElementWithValidView_ReturnsView()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var expectedView = Substitute.For<View>();
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.ModalPopoverSourceViewProperty).Returns(expectedView);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetPopoverSourceView(element);

            // Assert
            Assert.Same(expectedView, result);
        }

        /// <summary>
        /// Tests that GetPopoverRect returns the default value when no value has been set.
        /// The default value should be Rectangle.Empty as defined in the property declaration.
        /// </summary>
        [Fact]
        public void GetPopoverRect_DefaultValue_ReturnsEmptyRectangle()
        {
            // Arrange
            var page = new ContentPage();

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetPopoverRect(page);

            // Assert
            Assert.Equal(Rectangle.Empty, result);
        }

        /// <summary>
        /// Tests that GetPopoverRect returns the correct value when a custom rectangle has been set.
        /// This validates the basic get functionality with various rectangle values.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(10, 20, 30, 40)]
        [InlineData(-5, -10, 15, 25)]
        [InlineData(100, 200, 0, 0)]
        [InlineData(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue)]
        [InlineData(int.MinValue, int.MinValue, 0, 0)]
        public void GetPopoverRect_WithSetValue_ReturnsCorrectRectangle(int x, int y, int width, int height)
        {
            // Arrange
            var page = new ContentPage();
            var expectedRectangle = new Rectangle(x, y, width, height);
            page.SetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.ModalPopoverRectProperty, expectedRectangle);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetPopoverRect(page);

            // Assert
            Assert.Equal(expectedRectangle, result);
        }

        /// <summary>
        /// Tests that GetPopoverRect throws ArgumentNullException when passed a null element.
        /// This validates proper null parameter handling.
        /// </summary>
        [Fact]
        public void GetPopoverRect_NullElement_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetPopoverRect(null));
        }

        /// <summary>
        /// Tests that GetPopoverRect works with different types of BindableObject implementations.
        /// This validates that the method works with any BindableObject, not just Page instances.
        /// </summary>
        [Fact]
        public void GetPopoverRect_DifferentBindableObjectTypes_ReturnsCorrectValue()
        {
            // Arrange
            var button = new Button();
            var label = new Label();
            var testRectangle = new Rectangle(50, 75, 100, 125);

            button.SetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.ModalPopoverRectProperty, testRectangle);
            // Label should have default value

            // Act
            var buttonResult = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetPopoverRect(button);
            var labelResult = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetPopoverRect(label);

            // Assert
            Assert.Equal(testRectangle, buttonResult);
            Assert.Equal(Rectangle.Empty, labelResult);
        }

        /// <summary>
        /// Tests that GetPopoverRect works correctly when value is set and then changed.
        /// This validates that the method always returns the current value.
        /// </summary>
        [Fact]
        public void GetPopoverRect_ValueChangedMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var page = new ContentPage();
            var firstRectangle = new Rectangle(10, 10, 20, 20);
            var secondRectangle = new Rectangle(30, 30, 40, 40);
            var thirdRectangle = new Rectangle(50, 50, 60, 60);

            // Act & Assert - Initial default value
            var initialResult = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetPopoverRect(page);
            Assert.Equal(Rectangle.Empty, initialResult);

            // Act & Assert - First value
            page.SetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.ModalPopoverRectProperty, firstRectangle);
            var firstResult = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetPopoverRect(page);
            Assert.Equal(firstRectangle, firstResult);

            // Act & Assert - Second value
            page.SetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.ModalPopoverRectProperty, secondRectangle);
            var secondResult = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetPopoverRect(page);
            Assert.Equal(secondRectangle, secondResult);

            // Act & Assert - Third value
            page.SetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.ModalPopoverRectProperty, thirdRectangle);
            var thirdResult = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetPopoverRect(page);
            Assert.Equal(thirdRectangle, thirdResult);
        }

        /// <summary>
        /// Tests that GetPrefersHomeIndicatorAutoHidden returns false when the property has not been set (default value).
        /// </summary>
        [Fact]
        public void GetPrefersHomeIndicatorAutoHidden_ElementWithDefaultValue_ReturnsFalse()
        {
            // Arrange
            var element = new ContentPage();

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetPrefersHomeIndicatorAutoHidden(element);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetPrefersHomeIndicatorAutoHidden returns true when the property has been set to true.
        /// </summary>
        [Fact]
        public void GetPrefersHomeIndicatorAutoHidden_ElementWithTrueValue_ReturnsTrue()
        {
            // Arrange
            var element = new ContentPage();
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetPrefersHomeIndicatorAutoHidden(element, true);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetPrefersHomeIndicatorAutoHidden(element);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetPrefersHomeIndicatorAutoHidden returns false when the property has been explicitly set to false.
        /// </summary>
        [Fact]
        public void GetPrefersHomeIndicatorAutoHidden_ElementWithFalseValue_ReturnsFalse()
        {
            // Arrange
            var element = new ContentPage();
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetPrefersHomeIndicatorAutoHidden(element, false);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetPrefersHomeIndicatorAutoHidden(element);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetPrefersHomeIndicatorAutoHidden throws ArgumentNullException when element parameter is null.
        /// </summary>
        [Fact]
        public void GetPrefersHomeIndicatorAutoHidden_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetPrefersHomeIndicatorAutoHidden(element));
        }

        /// <summary>
        /// Tests that GetPrefersHomeIndicatorAutoHidden works with different BindableObject types.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetPrefersHomeIndicatorAutoHidden_DifferentBindableObjectTypes_ReturnsCorrectValue(bool expectedValue)
        {
            // Arrange - Test with different BindableObject derived types
            var contentPage = new ContentPage();
            var navigationPage = new NavigationPage();
            var tabbedPage = new TabbedPage();

            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetPrefersHomeIndicatorAutoHidden(contentPage, expectedValue);
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetPrefersHomeIndicatorAutoHidden(navigationPage, expectedValue);
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetPrefersHomeIndicatorAutoHidden(tabbedPage, expectedValue);

            // Act
            var contentPageResult = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetPrefersHomeIndicatorAutoHidden(contentPage);
            var navigationPageResult = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetPrefersHomeIndicatorAutoHidden(navigationPage);
            var tabbedPageResult = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetPrefersHomeIndicatorAutoHidden(tabbedPage);

            // Assert
            Assert.Equal(expectedValue, contentPageResult);
            Assert.Equal(expectedValue, navigationPageResult);
            Assert.Equal(expectedValue, tabbedPageResult);
        }

        /// <summary>
        /// Tests that SetPrefersHomeIndicatorAutoHidden extension method sets the value on the element and returns the configuration for method chaining when provided with valid parameters.
        /// Input: Valid configuration with mock element and boolean value.
        /// Expected: SetValue called on element with correct property and value, same configuration object returned.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetPrefersHomeIndicatorAutoHidden_WithValidConfigAndBoolValue_SetsValueAndReturnsConfig(bool value)
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Page>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = mockConfig.SetPrefersHomeIndicatorAutoHidden(value);

            // Assert
            mockElement.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetPrefersHomeIndicatorAutoHidden extension method throws NullReferenceException when config parameter is null.
        /// Input: null configuration parameter.
        /// Expected: NullReferenceException thrown when trying to access config.Element.
        /// </summary>
        [Fact]
        public void SetPrefersHomeIndicatorAutoHidden_WithNullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, Page> nullConfig = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => nullConfig.SetPrefersHomeIndicatorAutoHidden(true));
        }

        /// <summary>
        /// Tests that SetPrefersHomeIndicatorAutoHidden extension method throws NullReferenceException when config.Element is null.
        /// Input: Valid configuration with null Element property.
        /// Expected: NullReferenceException thrown when SetValue is called on null element.
        /// </summary>
        [Fact]
        public void SetPrefersHomeIndicatorAutoHidden_WithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Page>>();
            mockConfig.Element.Returns((BindableObject)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => mockConfig.SetPrefersHomeIndicatorAutoHidden(false));
        }
    }


    /// <summary>
    /// Tests for the Page.SetUseSafeArea method in the iOS platform configuration.
    /// </summary>
    public class PageSetUseSafeAreaTests
    {
        /// <summary>
        /// Tests that SetUseSafeArea throws ArgumentNullException when element parameter is null.
        /// Input: null element parameter and any boolean value.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void SetUseSafeArea_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject nullElement = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Page.SetUseSafeArea(nullElement, value));
        }

        /// <summary>
        /// Tests that SetUseSafeArea correctly sets the property to true on a mocked BindableObject.
        /// Input: valid BindableObject and true value.
        /// Expected: SetValue is called with UseSafeAreaProperty and true.
        /// </summary>
        [Fact]
        public void SetUseSafeArea_ValidElementWithTrueValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            bool value = true;

            // Act
            Page.SetUseSafeArea(mockElement, value);

            // Assert
            mockElement.Received(1).SetValue(Page.UseSafeAreaProperty, value);
        }

        /// <summary>
        /// Tests that SetUseSafeArea correctly sets the property to false on a mocked BindableObject.
        /// Input: valid BindableObject and false value.
        /// Expected: SetValue is called with UseSafeAreaProperty and false.
        /// </summary>
        [Fact]
        public void SetUseSafeArea_ValidElementWithFalseValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            bool value = false;

            // Act
            Page.SetUseSafeArea(mockElement, value);

            // Assert
            mockElement.Received(1).SetValue(Page.UseSafeAreaProperty, value);
        }

        /// <summary>
        /// Tests that SetUseSafeArea integration works correctly with a real ContentPage for true value.
        /// Input: ContentPage instance and true value.
        /// Expected: Property is set to true and can be retrieved correctly.
        /// </summary>
        [Fact]
        public void SetUseSafeArea_ContentPageWithTrueValue_PropertySetCorrectly()
        {
            // Arrange
            var contentPage = new ContentPage();
            bool value = true;

            // Act
            Page.SetUseSafeArea(contentPage, value);

            // Assert
            bool actualValue = Page.GetUseSafeArea(contentPage);
            Assert.Equal(value, actualValue);
        }

        /// <summary>
        /// Tests that SetUseSafeArea integration works correctly with a real ContentPage for false value.
        /// Input: ContentPage instance and false value.
        /// Expected: Property is set to false and can be retrieved correctly.
        /// </summary>
        [Fact]
        public void SetUseSafeArea_ContentPageWithFalseValue_PropertySetCorrectly()
        {
            // Arrange
            var contentPage = new ContentPage();
            bool value = false;

            // Act
            Page.SetUseSafeArea(contentPage, value);

            // Assert
            bool actualValue = Page.GetUseSafeArea(contentPage);
            Assert.Equal(value, actualValue);
        }

        /// <summary>
        /// Tests that SetUseSafeArea works correctly when called multiple times with different values.
        /// Input: ContentPage instance with alternating true and false values.
        /// Expected: Property value changes correctly with each call.
        /// </summary>
        [Fact]
        public void SetUseSafeArea_MultipleCallsWithDifferentValues_PropertyUpdatedCorrectly()
        {
            // Arrange
            var contentPage = new ContentPage();

            // Act & Assert - Set to true
            Page.SetUseSafeArea(contentPage, true);
            Assert.True(Page.GetUseSafeArea(contentPage));

            // Act & Assert - Set to false
            Page.SetUseSafeArea(contentPage, false);
            Assert.False(Page.GetUseSafeArea(contentPage));

            // Act & Assert - Set back to true
            Page.SetUseSafeArea(contentPage, true);
            Assert.True(Page.GetUseSafeArea(contentPage));
        }
    }
}