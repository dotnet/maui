#nullable disable

using System;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class FlyoutPageUnitTests : BaseTestFixture
    {
        MockDeviceDisplay mockDeviceDisplay;
        MockDeviceInfo mockDeviceInfo;


        public FlyoutPageUnitTests()
        {

            DeviceDisplay.SetCurrent(mockDeviceDisplay = new MockDeviceDisplay());
            DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
        }

        [Fact]
        public void TestConstructor()
        {
            FlyoutPage page = new FlyoutPage();

            Assert.Null(page.Flyout);
            Assert.Null(page.Detail);
        }

        [Fact]
        public void TestFlyoutSetter()
        {
            FlyoutPage page = new FlyoutPage();
            var child = new ContentPage { Content = new Label(), Title = "Foo" };
            page.Flyout = child;

            Assert.Equal(child, page.Flyout);
        }

        [Fact]
        public void TestFlyoutSetNull()
        {
            FlyoutPage page = new FlyoutPage();
            var child = new ContentPage { Content = new Label(), Title = "Foo" };
            page.Flyout = child;

            Assert.Throws<ArgumentNullException>(() => { page.Flyout = null; });
        }

        [Fact]
        public void TestFlyoutChanged()
        {
            FlyoutPage page = new FlyoutPage();
            var child = new ContentPage { Content = new Label(), Title = "Foo" };

            bool changed = false;
            page.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Flyout")
                    changed = true;
            };

            page.Flyout = child;

            Assert.True(changed);
        }

        [Fact]
        public void TestDetailSetter()
        {
            FlyoutPage page = new FlyoutPage();
            var child = new ContentPage { Content = new Label() };
            page.Detail = child;

            Assert.Equal(child, page.Detail);
        }

        [Fact]
        public void TestDetailSetNull()
        {
            FlyoutPage page = new FlyoutPage();
            var child = new ContentPage { Content = new Label() };
            page.Detail = child;

            Assert.Throws<ArgumentNullException>(() => { page.Detail = null; });
        }

        [Fact]
        public void TestDetailChanged()
        {
            FlyoutPage page = new FlyoutPage();
            var child = new ContentPage { Content = new Label() };

            bool changed = false;
            page.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Detail")
                    changed = true;
            };

            page.Detail = child;

            Assert.True(changed);
        }

        [Theory, InlineData(null), InlineData("")]
        public void ThrowsWhenFlyoutSetWithoutValidTitle(string title)
        {
            var page = new FlyoutPage();
            Assert.Throws<InvalidOperationException>(() => page.Flyout = new ContentPage { Title = title });
        }

        [Fact]
        public void TestThrowsWhenPackedWithoutSetting()
        {
            FlyoutPage page = new FlyoutPage();
            Assert.Throws<InvalidOperationException>(() => new TabbedPage { Children = { page } });
        }

        [Fact]
        public void TestDoesNotThrowWhenPackedWithSetting()
        {
            FlyoutPage page = new FlyoutPage
            {
                Flyout = new ContentPage { Content = new View(), Title = "Foo" },
                Detail = new ContentPage { Content = new View() }
            };
            _ = new TabbedPage { Children = { page } };
        }

        [Fact]
        public void TestFlyoutVisible()
        {
            var page = new FlyoutPage();

            Assert.False(page.IsPresented);

            bool signaled = false;
            page.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == FlyoutPage.IsPresentedProperty.PropertyName)
                    signaled = true;
            };

            page.IsPresented = true;

            Assert.True(page.IsPresented);
            Assert.True(signaled);
        }

        [Fact]
        public void TestFlyoutVisibleDoubleSet()
        {
            var page = new FlyoutPage();

            bool signaled = false;
            page.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == FlyoutPage.IsPresentedProperty.PropertyName)
                    signaled = true;
            };

            page.IsPresented = page.IsPresented;

            Assert.False(signaled);
        }

        [Fact]
        public void ThorwsInSetDetailBoundsWithNullDetail()
        {
            var page = new FlyoutPage
            {
                Flyout = new ContentPage { Content = new View(), Title = "Foo" },
                IsPlatformEnabled = true,
            };

            Assert.Throws<InvalidOperationException>(() => ((IFlyoutPageController)page).DetailBounds = new Rect(0, 0, 200, 200));
        }

        [Fact]
        public void ThrowsInSetFlyoutBoundsWithNullFlyout()
        {
            var page = new FlyoutPage
            {
                Detail = new ContentPage { Content = new View() },
                IsPlatformEnabled = true,
            };

            Assert.Throws<InvalidOperationException>(() => ((IFlyoutPageController)page).FlyoutBounds = new Rect(0, 0, 200, 200));
        }

        [Fact]
        public void ThrowsInSetIsPresentOnSplitModeOnTablet()
        {
            mockDeviceInfo.Idiom = DeviceIdiom.Tablet;
            var page = new FlyoutPage
            {
                Flyout = new ContentPage { Content = new View(), IsPlatformEnabled = true, Title = "Foo" },
                Detail = new ContentPage { Content = new View(), IsPlatformEnabled = true },
                IsPlatformEnabled = true,
                FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split
            };

            Assert.Throws<InvalidOperationException>(() => page.IsPresented = false);
        }

        [Fact]
        public void ThrowsInSetIsPresentOnSplitPortraitModeOnTablet()
        {
            mockDeviceInfo.Idiom = DeviceIdiom.Tablet;
            mockDeviceDisplay.SetMainDisplayOrientation(DisplayOrientation.Portrait);

            var page = new FlyoutPage
            {
                Flyout = new ContentPage { Content = new View(), IsPlatformEnabled = true, Title = "Foo" },
                Detail = new ContentPage { Content = new View(), IsPlatformEnabled = true },
                FlyoutLayoutBehavior = FlyoutLayoutBehavior.SplitOnPortrait
            };

            Assert.Throws<InvalidOperationException>(() => page.IsPresented = false);
        }

        [Fact]
        public void TestSetIsPresentedOnPopoverMode()
        {
            mockDeviceDisplay.SetMainDisplayOrientation(DisplayOrientation.Landscape);

            var page = new FlyoutPage
            {
                Flyout = new ContentPage { Content = new View(), IsPlatformEnabled = true, Title = "Foo" },
                Detail = new ContentPage { Content = new View(), IsPlatformEnabled = true },
                IsPlatformEnabled = true,
                FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover
            };
            page.IsPresented = true;

            Assert.True(page.IsPresented);
        }

        [Fact]
        public void SendsBackEventToPresentedFlyoutFirst()
        {
            var detail = new BackButtonPage() { Handle = true };
            var Flyout = new BackButtonPage() { Title = "Flyout" };
            var mdp = new FlyoutPage()
            {
                Detail = detail,
                Flyout = Flyout,
                IsPresented = true,
                IsPlatformEnabled = true,
            };

            ((IFlyoutPageController)mdp).BackButtonPressed += (sender, args) =>
            {
                args.Handled = mdp.IsPresented;
                mdp.IsPresented = false;
            };

            var detailEmitted = false;
            var FlyoutEmitted = false;

            detail.BackPressed += (sender, args) => detailEmitted = true;
            Flyout.BackPressed += (sender, args) => FlyoutEmitted = true;

            var result = mdp.SendBackButtonPressed();

            Assert.True(FlyoutEmitted);
            Assert.False(detailEmitted);
            Assert.True(result);
        }

        [Fact]
        public void EmitsCorrectlyWhenPresentedOnBackPressed()
        {
            var detail = new BackButtonPage();
            var Flyout = new BackButtonPage { Title = "Flyout" };
            var mdp = new FlyoutPage
            {
                Detail = detail,
                Flyout = Flyout,
                IsPresented = true,
                IsPlatformEnabled = true,
            };

            ((IFlyoutPageController)mdp).BackButtonPressed += (sender, args) =>
            {
                args.Handled = mdp.IsPresented;
                mdp.IsPresented = false;
            };

            var detailEmitted = false;
            var FlyoutEmitted = false;

            detail.BackPressed += (sender, args) => detailEmitted = true;
            Flyout.BackPressed += (sender, args) => FlyoutEmitted = true;

            var result = mdp.SendBackButtonPressed();

            Assert.True(FlyoutEmitted);
            Assert.False(detailEmitted);
            Assert.True(result);
        }

        [Fact]
        public void ThrowsExceptionWhenAddingAlreadyParentedDetail()
        {
            var detail = new ContentPage { };

            // give detail a parent
            var nav = new NavigationPage(detail);

            var mdp = new FlyoutPage();
            Assert.Throws<InvalidOperationException>(() => mdp.Detail = detail);
        }

        [Fact]
        public void ThrowsExceptionWhenAddingAlreadyParentedFlyout()
        {
            var Flyout = new ContentPage { Title = "Foo" };

            // give Flyout a parent
            var nav = new NavigationPage(Flyout);

            var mdp = new FlyoutPage();
            Assert.Throws<InvalidOperationException>(() => mdp.Flyout = Flyout);
        }

        [Fact]
        public void FlyoutPageAppearingAndDisappearingPropagatesToFlyout()
        {
            int disappearing = 0;
            int appearing = 0;

            var flyout = new ContentPage() { Title = "flyout" };
            var flyoutPage = new FlyoutPage()
            {
                Flyout = flyout,
                Detail = new ContentPage() { Title = "detail" }
            };

            _ = new TestWindow(flyoutPage);
            flyout.Appearing += (_, __) => appearing++;
            flyout.Disappearing += (_, __) => disappearing++;

            Assert.Equal(0, disappearing);
            Assert.Equal(0, appearing);

            flyoutPage.SendDisappearing();

            Assert.Equal(1, disappearing);
            Assert.Equal(0, appearing);

            flyoutPage.SendAppearing();

            Assert.Equal(1, disappearing);
            Assert.Equal(1, appearing);
        }

        [Fact]
        public void FlyoutPageAppearingAndDisappearingPropagatesToDetail()
        {
            int disappearing = 0;
            int appearing = 0;

            var detail = new ContentPage() { Title = "detail" };
            var flyoutPage = new FlyoutPage()
            {
                Flyout = new ContentPage() { Title = "flyout" },
                Detail = detail
            };

            _ = new TestWindow(flyoutPage);

            detail.Appearing += (_, __) => appearing++;
            detail.Disappearing += (_, __) => disappearing++;

            Assert.Equal(0, disappearing);
            Assert.Equal(0, appearing);

            flyoutPage.SendDisappearing();

            Assert.Equal(1, disappearing);
            Assert.Equal(0, appearing);

            flyoutPage.SendAppearing();

            Assert.Equal(1, disappearing);
            Assert.Equal(1, appearing);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task VerifyToolbarButtonVisibilityWhenFlyoutReset(int depth)
        {
            ContentPage detailContentPage = new ContentPage();

            Page flyout = new ContentPage()
            {
                Title = "Initial Flyout"
            };

            NavigationPage.SetHasNavigationBar(flyout, false);

            var flyoutContentPage = flyout;

            for (int i = 0; i < depth; i++)
            {
                flyout = new NavigationPage(flyout)
                {
                    Title = "Flyout " + i
                };
            }

            FlyoutPage flyoutPage = new FlyoutPage
            {
                Flyout = flyout,
                Detail = new NavigationPage(detailContentPage)
            };

            _ = new TestWindow(flyoutPage);

            Toolbar flyoutToolBar = flyoutPage.Toolbar;
            Assert.True(flyoutToolBar.IsVisible);

            if (depth >= 1)
            {
                var page = new ContentPage();
                NavigationPage.SetHasNavigationBar(page, false);
                await flyoutContentPage.Navigation.PushAsync(page);
            }
            else
            {
                var page = new ContentPage { Title = "Reborn Flyout" };
                NavigationPage.SetHasNavigationBar(page, false);
                flyoutPage.Flyout = new ContentPage { Title = "Reborn Flyout" };
            }

            Assert.True(flyoutToolBar.IsVisible);
        }

        [Fact]
        public void FlyoutPageDetailNavigation_EventsWithContentPage()
        {
            // Test that navigation events are fired directly on the page when Detail is a ContentPage
            var firstDetail = new NavigationObserverPage { Title = "First Detail" };
            var secondDetail = new NavigationObserverPage { Title = "Second Detail" };
            var flyoutPage = new FlyoutPage
            {
                Flyout = new ContentPage { Title = "Flyout" },
                Detail = firstDetail
            }.AddToTestWindow();

            flyoutPage.Detail = secondDetail;

            // Verify NavigatingFrom was called on the first detail directly  
            Assert.NotNull(firstDetail.NavigatingFromArgs);
            Assert.Equal(secondDetail, firstDetail.NavigatingFromArgs.DestinationPage);
            Assert.Equal(NavigationType.Replace, firstDetail.NavigatingFromArgs.NavigationType);

            // Verify NavigatedFrom was called on the first detail directly
            Assert.NotNull(firstDetail.NavigatedFromArgs);
            Assert.Equal(secondDetail, firstDetail.NavigatedFromArgs.DestinationPage);
            Assert.Equal(NavigationType.Replace, firstDetail.NavigatedFromArgs.NavigationType);

            // Verify NavigatedTo was called on the second detail directly
            Assert.NotNull(secondDetail.NavigatedToArgs);
            Assert.Equal(firstDetail, secondDetail.NavigatedToArgs.PreviousPage);
            Assert.Equal(NavigationType.Replace, secondDetail.NavigatedToArgs.NavigationType);
        }

        [Fact]
        public void FlyoutPageDetailNavigation_EventsWithNavigationPage()
        {
            // Test that navigation events are fired on NavigationPage directly, NOT on CurrentPage
            var firstDetailContent = new NavigationObserverPage { Title = "First Detail Content" };
            var firstDetail = new TrackingNavigationPage(firstDetailContent) { Title = "First Detail Nav" };
            var secondDetailContent = new NavigationObserverPage { Title = "Second Detail Content" };
            var secondDetail = new TrackingNavigationPage(secondDetailContent) { Title = "Second Detail Nav" };
            var flyoutPage = new FlyoutPage
            {
                Flyout = new ContentPage { Title = "Flyout" },
                Detail = firstDetail
            }.AddToTestWindow();

            // Clear any initial navigation args on the content pages to focus on Detail replacement
            firstDetailContent.ClearNavigationArgs();
            secondDetailContent.ClearNavigationArgs();
            firstDetail.ClearNavigationArgs();
            secondDetail.ClearNavigationArgs();

            flyoutPage.Detail = secondDetail;

            // Verify that navigation events WERE fired on the NavigationPage containers
            Assert.NotNull(firstDetail.NavigatingFromArgs);
            Assert.Equal(secondDetail, firstDetail.NavigatingFromArgs.DestinationPage);
            Assert.Equal(NavigationType.Replace, firstDetail.NavigatingFromArgs.NavigationType);

            Assert.NotNull(firstDetail.NavigatedFromArgs);
            Assert.Equal(secondDetail, firstDetail.NavigatedFromArgs.DestinationPage);
            Assert.Equal(NavigationType.Replace, firstDetail.NavigatedFromArgs.NavigationType);

            Assert.NotNull(secondDetail.NavigatedToArgs);
            Assert.Equal(firstDetail, secondDetail.NavigatedToArgs.PreviousPage);
            Assert.Equal(NavigationType.Replace, secondDetail.NavigatedToArgs.NavigationType);
        }

        [Fact]
        public void FlyoutPageDetailNavigation_MixedPageTypes()
        {
            // Test that navigation events work correctly when mixing ContentPage and NavigationPage
            var firstDetail = new NavigationObserverPage { Title = "First Detail" };
            var secondDetailContent = new NavigationObserverPage { Title = "Second Detail Content" };
            var secondDetail = new TrackingNavigationPage(secondDetailContent) { Title = "Second Detail Nav" };
            var flyoutPage = new FlyoutPage
            {
                Flyout = new ContentPage { Title = "Flyout" },
                Detail = firstDetail
            }.AddToTestWindow();

            // Clear initial navigation args
            secondDetailContent.ClearNavigationArgs();
            secondDetail.ClearNavigationArgs();

            flyoutPage.Detail = secondDetail;

            // Verify NavigatingFrom was called on the ContentPage directly
            Assert.NotNull(firstDetail.NavigatingFromArgs);
            Assert.Equal(secondDetail, firstDetail.NavigatingFromArgs.DestinationPage);
            Assert.Equal(NavigationType.Replace, firstDetail.NavigatingFromArgs.NavigationType);

            // Verify NavigatedFrom was called on the ContentPage directly
            Assert.NotNull(firstDetail.NavigatedFromArgs);
            Assert.Equal(secondDetail, firstDetail.NavigatedFromArgs.DestinationPage);
            Assert.Equal(NavigationType.Replace, firstDetail.NavigatedFromArgs.NavigationType);

            // Verify NavigatedTo was called on the NavigationPage container
            Assert.NotNull(secondDetail.NavigatedToArgs);
            Assert.Equal(firstDetail, secondDetail.NavigatedToArgs.PreviousPage);
            Assert.Equal(NavigationType.Replace, secondDetail.NavigatedToArgs.NavigationType);
        }

        [Fact]
        public void FlyoutPageDetailNavigation_NavigationPageToContentPage()
        {
            // Test navigation from NavigationPage to ContentPage
            var firstDetailContent = new NavigationObserverPage { Title = "First Detail Content" };
            var firstDetail = new TrackingNavigationPage(firstDetailContent) { Title = "First Detail Nav" };
            var secondDetail = new NavigationObserverPage { Title = "Second Detail" };
            var flyoutPage = new FlyoutPage
            {
                Flyout = new ContentPage { Title = "Flyout" },
                Detail = firstDetail
            }.AddToTestWindow();

            // Clear any initial navigation args
            firstDetailContent.ClearNavigationArgs();
            firstDetail.ClearNavigationArgs();
            secondDetail.ClearNavigationArgs();

            flyoutPage.Detail = secondDetail;

            // Verify NavigatingFrom was called on the NavigationPage container
            Assert.NotNull(firstDetail.NavigatingFromArgs);
            Assert.Equal(secondDetail, firstDetail.NavigatingFromArgs.DestinationPage);
            Assert.Equal(NavigationType.Replace, firstDetail.NavigatingFromArgs.NavigationType);

            // Verify NavigatedFrom was called on the NavigationPage container
            Assert.NotNull(firstDetail.NavigatedFromArgs);
            Assert.Equal(secondDetail, firstDetail.NavigatedFromArgs.DestinationPage);
            Assert.Equal(NavigationType.Replace, firstDetail.NavigatedFromArgs.NavigationType);

            // Verify NavigatedTo was called on the ContentPage with NavigationPage as previous page
            Assert.NotNull(secondDetail.NavigatedToArgs);
            Assert.Equal(firstDetail, secondDetail.NavigatedToArgs.PreviousPage);
            Assert.Equal(NavigationType.Replace, secondDetail.NavigatedToArgs.NavigationType);
        }

        [Fact]
        public void FlyoutPageFlyoutNavigation_EventsWithContentPage()
        {
            // Test that navigation events are fired directly on Flyout pages when they are ContentPages
            var firstFlyout = new NavigationObserverPage { Title = "First Flyout" };
            var secondFlyout = new NavigationObserverPage { Title = "Second Flyout" };
            var flyoutPage = new FlyoutPage
            {
                Flyout = firstFlyout,
                Detail = new ContentPage { Title = "Detail" }
            }.AddToTestWindow();

            flyoutPage.Flyout = secondFlyout;

            // Verify NavigatingFrom was called on the first flyout directly
            Assert.NotNull(firstFlyout.NavigatingFromArgs);
            Assert.Equal(secondFlyout, firstFlyout.NavigatingFromArgs.DestinationPage);
            Assert.Equal(NavigationType.Replace, firstFlyout.NavigatingFromArgs.NavigationType);

            // Verify NavigatedFrom was called on the first flyout directly
            Assert.NotNull(firstFlyout.NavigatedFromArgs);
            Assert.Equal(secondFlyout, firstFlyout.NavigatedFromArgs.DestinationPage);
            Assert.Equal(NavigationType.Replace, firstFlyout.NavigatedFromArgs.NavigationType);

            // Verify NavigatedTo was called on the second flyout directly
            Assert.NotNull(secondFlyout.NavigatedToArgs);
            Assert.Equal(firstFlyout, secondFlyout.NavigatedToArgs.PreviousPage);
            Assert.Equal(NavigationType.Replace, secondFlyout.NavigatedToArgs.NavigationType);
        }

        [Fact]
        public void FlyoutPageFlyoutNavigation_EventsWithNavigationPage()
        {
            // Test that navigation events are fired on NavigationPage directly for Flyout, NOT on CurrentPage
            var firstFlyoutContent = new NavigationObserverPage { Title = "First Flyout Content" };
            var firstFlyout = new TrackingNavigationPage(firstFlyoutContent) { Title = "First Flyout Nav" };
            var secondFlyoutContent = new NavigationObserverPage { Title = "Second Flyout Content" };
            var secondFlyout = new TrackingNavigationPage(secondFlyoutContent) { Title = "Second Flyout Nav" };
            var flyoutPage = new FlyoutPage
            {
                Flyout = firstFlyout,
                Detail = new ContentPage { Title = "Detail" }
            }.AddToTestWindow();

            // Clear any initial navigation args on the content pages
            firstFlyoutContent.ClearNavigationArgs();
            secondFlyoutContent.ClearNavigationArgs();
            firstFlyout.ClearNavigationArgs();
            secondFlyout.ClearNavigationArgs();

            flyoutPage.Flyout = secondFlyout;

            // Verify that navigation events WERE fired on the NavigationPage containers
            Assert.NotNull(firstFlyout.NavigatingFromArgs);
            Assert.Equal(secondFlyout, firstFlyout.NavigatingFromArgs.DestinationPage);
            Assert.Equal(NavigationType.Replace, firstFlyout.NavigatingFromArgs.NavigationType);

            Assert.NotNull(firstFlyout.NavigatedFromArgs);
            Assert.Equal(secondFlyout, firstFlyout.NavigatedFromArgs.DestinationPage);
            Assert.Equal(NavigationType.Replace, firstFlyout.NavigatedFromArgs.NavigationType);

            Assert.NotNull(secondFlyout.NavigatedToArgs);
            Assert.Equal(firstFlyout, secondFlyout.NavigatedToArgs.PreviousPage);
            Assert.Equal(NavigationType.Replace, secondFlyout.NavigatedToArgs.NavigationType);
        }

        [Fact]
        public void FlyoutPageDetailNavigation_CorrectNavigationType()
        {
            // Test that all navigation events use NavigationType.Replace
            var firstDetail = new NavigationObserverPage { Title = "First Detail" };
            var secondDetail = new NavigationObserverPage { Title = "Second Detail" };
            var flyoutPage = new FlyoutPage
            {
                Flyout = new ContentPage { Title = "Flyout" },
                Detail = firstDetail
            }.AddToTestWindow();

            flyoutPage.Detail = secondDetail;

            Assert.Equal(NavigationType.Replace, firstDetail.NavigatingFromArgs.NavigationType);
            Assert.Equal(NavigationType.Replace, firstDetail.NavigatedFromArgs.NavigationType);
            Assert.Equal(NavigationType.Replace, secondDetail.NavigatedToArgs.NavigationType);
        }

        [Fact]
        public void FlyoutPageFlyoutNavigation_CorrectNavigationType()
        {
            // Test that all Flyout navigation events use NavigationType.Replace
            var firstFlyout = new NavigationObserverPage { Title = "First Flyout" };
            var secondFlyout = new NavigationObserverPage { Title = "Second Flyout" };
            var flyoutPage = new FlyoutPage
            {
                Flyout = firstFlyout,
                Detail = new ContentPage { Title = "Detail" }
            }.AddToTestWindow();

            flyoutPage.Flyout = secondFlyout;

            Assert.Equal(NavigationType.Replace, firstFlyout.NavigatingFromArgs.NavigationType);
            Assert.Equal(NavigationType.Replace, firstFlyout.NavigatedFromArgs.NavigationType);
            Assert.Equal(NavigationType.Replace, secondFlyout.NavigatedToArgs.NavigationType);
        }

        [Fact]
        public void FlyoutPageDetailNavigation_NullPreviousDetailHandled()
        {
            // Test that navigation events are handled correctly when there's no previous Detail
            var detail = new NavigationObserverPage { Title = "Detail" };
            var flyoutPage = new FlyoutPage
            {
                Flyout = new ContentPage { Title = "Flyout" }
            };

            // Set detail first, which should trigger NavigatedTo with null previous page
            flyoutPage.Detail = detail;

            // Then add to window after both properties are set
            var window = flyoutPage.AddToTestWindow();

            // Verify NavigatedTo was called with null previous page  
            Assert.NotNull(detail.NavigatedToArgs);
            Assert.Null(detail.NavigatedToArgs.PreviousPage);
            Assert.Equal(NavigationType.Replace, detail.NavigatedToArgs.NavigationType);
        }

        [Fact]
        public void FlyoutPageFlyoutNavigation_NullPreviousFlyoutHandled()
        {
            // Test that navigation events are handled correctly when there's no previous Flyout
            var flyout = new NavigationObserverPage { Title = "Flyout" };
            var flyoutPage = new FlyoutPage
            {
                Detail = new ContentPage { Title = "Detail" }
            };

            // Set flyout first, which should trigger NavigatedTo with null previous page
            flyoutPage.Flyout = flyout;

            // Then add to window after both properties are set
            var window = flyoutPage.AddToTestWindow();

            // Verify NavigatedTo was called with null previous page
            Assert.NotNull(flyout.NavigatedToArgs);
            Assert.Null(flyout.NavigatedToArgs.PreviousPage);
            Assert.Equal(NavigationType.Replace, flyout.NavigatedToArgs.NavigationType);
        }

        public class NavigationObserverPage : ContentPage
        {
            public NavigatedFromEventArgs NavigatedFromArgs { get; private set; }
            public NavigatingFromEventArgs NavigatingFromArgs { get; private set; }
            public NavigatedToEventArgs NavigatedToArgs { get; private set; }

            public void ClearNavigationArgs()
            {
                NavigatedFromArgs = null;
                NavigatingFromArgs = null;
                NavigatedToArgs = null;
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

        public class TrackingNavigationPage : NavigationPage
        {
            public NavigatedFromEventArgs NavigatedFromArgs { get; private set; }
            public NavigatingFromEventArgs NavigatingFromArgs { get; private set; }
            public NavigatedToEventArgs NavigatedToArgs { get; private set; }

            public TrackingNavigationPage(Page root) : base(root) { }

            public void ClearNavigationArgs()
            {
                NavigatedFromArgs = null;
                NavigatingFromArgs = null;
                NavigatedToArgs = null;
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

    public partial class FlyoutPageTests
    {
        /// <summary>
        /// Tests that the FlyoutPage constructor initializes the instance correctly with default values.
        /// Verifies that Flyout and Detail properties are null after construction and basic properties are accessible.
        /// </summary>
        [Fact]
        public void Constructor_DefaultInitialization_InitializesPropertiesCorrectly()
        {
            // Arrange & Act
            var flyoutPage = new FlyoutPage();

            // Assert
            Assert.Null(flyoutPage.Flyout);
            Assert.Null(flyoutPage.Detail);
            Assert.True(flyoutPage.IsGestureEnabled);
            Assert.False(flyoutPage.IsPresented);
            Assert.Equal(FlyoutLayoutBehavior.Default, flyoutPage.FlyoutLayoutBehavior);
        }

        /// <summary>
        /// Tests that the FlyoutPage constructor properly initializes the platform configuration registry.
        /// Verifies that the On method can be called without throwing exceptions after construction.
        /// </summary>
        [Fact]
        public void Constructor_PlatformConfigurationRegistry_IsInitializedAndAccessible()
        {
            // Arrange & Act
            var flyoutPage = new FlyoutPage();

            // Assert
            Assert.NotNull(flyoutPage.On<Android>());
            Assert.NotNull(flyoutPage.On<iOS>());
        }

        /// <summary>
        /// Tests that creating multiple FlyoutPage instances works correctly and each instance is independent.
        /// Verifies that multiple instances can be created without interference.
        /// </summary>
        [Fact]
        public void Constructor_MultipleInstances_EachInstanceIsIndependent()
        {
            // Arrange & Act
            var flyoutPage1 = new FlyoutPage();
            var flyoutPage2 = new FlyoutPage();

            // Assert
            Assert.NotSame(flyoutPage1, flyoutPage2);
            Assert.NotSame(flyoutPage1.On<Android>(), flyoutPage2.On<Android>());
            Assert.Null(flyoutPage1.Flyout);
            Assert.Null(flyoutPage2.Flyout);
            Assert.Null(flyoutPage1.Detail);
            Assert.Null(flyoutPage2.Detail);
        }

        /// <summary>
        /// Tests that the FlyoutPage constructor does not throw any exceptions during instantiation.
        /// Verifies basic construction stability.
        /// </summary>
        [Fact]
        public void Constructor_BasicInstantiation_DoesNotThrowException()
        {
            // Arrange, Act & Assert
            var exception = Record.Exception(() => new FlyoutPage());
            Assert.Null(exception);
        }
    }
}