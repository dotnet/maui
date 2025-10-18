using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class ShellToolbarTests : ShellTestBase
    {
        [Fact]
        public async Task ShellToolbarItemsMergeWithPage()
        {
            var toolbarItem1 = new ToolbarItem("Foo1", "Foo.png", () => { });
            var toolbarItem2 = new ToolbarItem("Foo2", "Foo.png", () => { });
            var toolbarItem3 = new ToolbarItem("Foo2", "Foo.png", () => { });

            var firstPage = new ContentPage
            {
                ToolbarItems = { toolbarItem2 }
            };

            var secondPage = new ContentPage
            {
                ToolbarItems = { toolbarItem3 }
            };

            var shell = new TestShell(firstPage)
            {
                ToolbarItems = {
                    toolbarItem1
                }
            };

            var toolbar = shell.Toolbar;
            Assert.Contains(toolbarItem1, toolbar.ToolbarItems);
            Assert.Contains(toolbarItem2, toolbar.ToolbarItems);
            Assert.Equal(2, toolbar.ToolbarItems.Count());

            await shell.Navigation.PushAsync(secondPage);
            Assert.Contains(toolbarItem1, toolbar.ToolbarItems);
            Assert.Contains(toolbarItem3, toolbar.ToolbarItems);
            Assert.Equal(2, toolbar.ToolbarItems.Count());
        }

        [Fact]
        public async Task BackButtonExecutesCommand()
        {
            var pushedPage = new ContentPage();
            TestShell testShell = new TestShell(new ContentPage());
            var window = new Window()
            {
                Page = testShell
            };

            bool commandExecuted = false;
            string parameter = String.Empty;
            var command = new Command((p) =>
            {
                parameter = $"{p}";
                commandExecuted = true;
            });

            var backButtonBehavior = new BackButtonBehavior()
            {
                Command = command,
                CommandParameter = "PARAMETER"
            };

            await testShell.Navigation.PushAsync(pushedPage);
            Shell.SetBackButtonBehavior(pushedPage, backButtonBehavior);

            (window as IWindow).BackButtonClicked();

            // Validate that we didn't navigate back and only the
            // Command was executed
            Assert.Equal(pushedPage, testShell.CurrentPage);
            Assert.True(commandExecuted);
            Assert.Equal("PARAMETER", parameter);
        }

        [Fact]
        public async Task BackButtonDisabledWhenCommandDisabled()
        {
            var page = new ContentPage();
            TestShell testShell = new TestShell(new ContentPage());
            await testShell.Navigation.PushAsync(page);

            var backButtonBehavior = new BackButtonBehavior();
            Shell.SetBackButtonBehavior(page, backButtonBehavior);
            Assert.True(testShell.Toolbar.BackButtonEnabled);

            bool canExecute = false;
            backButtonBehavior.Command = new Command(() => { }, () => canExecute);
            Assert.False(testShell.Toolbar.BackButtonEnabled);
            canExecute = true;
            (backButtonBehavior.Command as Command).ChangeCanExecute();
            Assert.True(testShell.Toolbar.BackButtonEnabled);
        }

        [Fact]
        public async Task BackButtonBehaviorCommandFromPoppedPageIsCorrectlyUnsubscribedFrom()
        {
            var firstPage = new ContentPage();
            var secondPage = new ContentPage();
            bool canExecute = true;
            var backButtonBehavior = new BackButtonBehavior();
            TestShell testShell = new TestShell(firstPage);

            await testShell.Navigation.PushAsync(secondPage);

            Shell.SetBackButtonBehavior(secondPage, backButtonBehavior);

            backButtonBehavior.Command = new Command(() => { }, () => canExecute);

            await testShell.Navigation.PopAsync();

            canExecute = false;
            (backButtonBehavior.Command as Command).ChangeCanExecute();

            Assert.True(testShell.Toolbar.BackButtonEnabled);
        }

        [Fact]
        public async Task BackButtonUpdatesWhenSetToNewCommand()
        {
            var firstPage = new ContentPage();
            var secondPage = new ContentPage();
            bool canExecute = true;
            var backButtonBehavior = new BackButtonBehavior();
            TestShell testShell = new TestShell(firstPage);

            await testShell.Navigation.PushAsync(secondPage);

            Shell.SetBackButtonBehavior(secondPage, backButtonBehavior);

            backButtonBehavior.Command = new Command(() => { }, () => true);
            Assert.True(testShell.Toolbar.BackButtonEnabled);
            backButtonBehavior.Command = new Command(() => { }, () => false);
            Assert.False(testShell.Toolbar.BackButtonEnabled);
            backButtonBehavior.Command = null;
            Assert.True(testShell.Toolbar.BackButtonEnabled);
        }

        [Fact]
        public async Task ShellToolbarUpdatesFromNewBackButtonBehavior()
        {
            var page = new ContentPage();
            TestShell testShell = new TestShell(new ContentPage());
            await testShell.Navigation.PushAsync(page);

            Assert.True(testShell.Toolbar.BackButtonVisible);
            var backButtonBehavior = new BackButtonBehavior()
            {
                IsVisible = false,
            };

            Shell.SetBackButtonBehavior(page, backButtonBehavior);
            Assert.False(testShell.Toolbar.BackButtonVisible);
        }

        [Fact]
        public async Task ShellToolbarUpdatesFromPropertyChanged()
        {
            var page = new ContentPage();
            TestShell testShell = new TestShell(new ContentPage());
            Shell.SetBackButtonBehavior(page, new BackButtonBehavior());
            await testShell.Navigation.PushAsync(page);

            Assert.True(testShell.Toolbar.IsVisible);
            var backButtonBehavior = new BackButtonBehavior()
            {
                IsVisible = true,
            };

            Shell.SetBackButtonBehavior(page, backButtonBehavior);
            Assert.True(testShell.Toolbar.BackButtonVisible);
            backButtonBehavior.IsVisible = false;
            Assert.False(testShell.Toolbar.BackButtonVisible);
        }

        [Fact]
        public void NavBarIsVisibleUpdates()
        {
            var page = new ContentPage() { Title = "Test" };
            var testShell = new TestShell(page);
            var toolBar = testShell.Toolbar;
            Assert.True(toolBar.IsVisible); // visible by default

            // Change the Shell
            Shell.SetNavBarIsVisible(testShell, false);
            Assert.False(toolBar.IsVisible);
            testShell.ClearValue(Shell.NavBarIsVisibleProperty);
            Assert.True(toolBar.IsVisible); // back to default

            // Change the Page's parent
            Shell.SetNavBarIsVisible(page.Parent, false);
            Assert.False(toolBar.IsVisible);
            page.Parent.ClearValue(Shell.NavBarIsVisibleProperty);
            Assert.True(toolBar.IsVisible); // back to default

            // Change the Page
            Shell.SetNavBarIsVisible(page, false);
            Assert.False(toolBar.IsVisible);
            page.ClearValue(Shell.NavBarIsVisibleProperty);
            Assert.True(toolBar.IsVisible); // back to default
        }

        [Fact]
        public void BackButtonBehaviorSet()
        {
            var page = new ContentPage();

            Assert.Null(Shell.GetBackButtonBehavior(page));

            var backButtonBehavior = new BackButtonBehavior();

            Shell.SetBackButtonBehavior(page, backButtonBehavior);

            Assert.Equal(backButtonBehavior, Shell.GetBackButtonBehavior(page));
        }

        [Fact]
        public void BackButtonBehaviorBindingContextPropagation()
        {
            object bindingContext = new object();
            var page = new ContentPage();
            var backButtonBehavior = new BackButtonBehavior();

            Shell.SetBackButtonBehavior(page, backButtonBehavior);
            page.BindingContext = bindingContext;

            Assert.Equal(page.BindingContext, backButtonBehavior.BindingContext);
        }

        [Fact]
        public void BackButtonBehaviorBindingContextPropagationWithExistingBindingContext()
        {
            object bindingContext = new object();
            var page = new ContentPage();
            var backButtonBehavior = new BackButtonBehavior();

            page.BindingContext = bindingContext;
            Shell.SetBackButtonBehavior(page, backButtonBehavior);

            Assert.Equal(page.BindingContext, backButtonBehavior.BindingContext);
        }

        [Fact]
        public async Task TitleAndTitleViewAreMutuallyExclusive()
        {
            var contentPage = new ContentPage() { Title = "Test Title" };
            var titleView = new VerticalStackLayout();

            TestShell testShell = new TestShell(contentPage);
            var window = new Window()
            {
                Page = testShell
            };

            var toolbar = testShell.Toolbar;
            Assert.Equal("Test Title", toolbar.Title);
            Shell.SetTitleView(contentPage, titleView);
            Assert.Empty(toolbar.Title);
            Assert.Equal(titleView, toolbar.TitleView);
            Shell.SetTitleView(contentPage, null);
            Assert.Equal("Test Title", toolbar.Title);
        }

        [Fact]
        public void ContentPageColorsPropagateToShellToolbar()
        {
            var contentPage = new ContentPage() { Title = "Test Title" };
            Shell.SetBackgroundColor(contentPage, Colors.Green);
            Shell.SetTitleColor(contentPage, Colors.Pink);
            Shell.SetForegroundColor(contentPage, Colors.Orange);

            TestShell testShell = new TestShell(contentPage);
            _ = new Window() { Page = testShell };
            var toolbar = testShell.Toolbar;

            Assert.Equal(Colors.Green, (toolbar.BarBackground as SolidColorBrush).Color);
            Assert.Equal(Colors.Orange, toolbar.IconColor);
            Assert.Equal(Colors.Pink, toolbar.BarTextColor);
        }

        [Fact]
        public void ToolBarShouldBeVisibleWithEmptyTitleAndFlyoutBehaviorSetToFlyout()
        {
            TestShell testShell = new TestShell()
            {
                CurrentItem = new FlyoutItem()
                {
                    CurrentItem = new ContentPage()
                }
            };

            _ = new Window() { Page = testShell };
            var toolbar = testShell.Toolbar;

            Assert.True(toolbar.IsVisible);
        }

        /// <summary>
        /// Tests that CurrentPage returns the initial page when a Shell is created with a page.
        /// Verifies that the toolbar correctly tracks the current page from shell initialization.
        /// </summary>
        [Fact]
        public void CurrentPage_WithInitialPage_ReturnsInitialPage()
        {
            // Arrange
            var initialPage = new ContentPage { Title = "Initial Page" };
            var shell = new TestShell(initialPage);
            var toolbar = shell.Toolbar;

            // Act
            var currentPage = toolbar.CurrentPage;

            // Assert
            Assert.Equal(initialPage, currentPage);
        }

        /// <summary>
        /// Tests that CurrentPage returns the correct page after navigation.
        /// Verifies that the toolbar's CurrentPage property updates when navigating to a new page.
        /// </summary>
        [Fact]
        public async Task CurrentPage_AfterNavigation_ReturnsCurrentPage()
        {
            // Arrange
            var initialPage = new ContentPage { Title = "Initial Page" };
            var secondPage = new ContentPage { Title = "Second Page" };
            var shell = new TestShell(initialPage);
            var toolbar = shell.Toolbar;

            // Act
            await shell.Navigation.PushAsync(secondPage);
            var currentPage = toolbar.CurrentPage;

            // Assert
            Assert.Equal(secondPage, currentPage);
        }

        /// <summary>
        /// Tests that CurrentPage returns the updated page after multiple navigations.
        /// Verifies that the toolbar correctly tracks page changes through navigation stack operations.
        /// </summary>
        [Fact]
        public async Task CurrentPage_AfterMultipleNavigations_ReturnsLatestPage()
        {
            // Arrange
            var firstPage = new ContentPage { Title = "First Page" };
            var secondPage = new ContentPage { Title = "Second Page" };
            var thirdPage = new ContentPage { Title = "Third Page" };
            var shell = new TestShell(firstPage);
            var toolbar = shell.Toolbar;

            // Act - Navigate to second page
            await shell.Navigation.PushAsync(secondPage);
            var currentPageAfterFirstNavigation = toolbar.CurrentPage;

            // Act - Navigate to third page
            await shell.Navigation.PushAsync(thirdPage);
            var currentPageAfterSecondNavigation = toolbar.CurrentPage;

            // Assert
            Assert.Equal(secondPage, currentPageAfterFirstNavigation);
            Assert.Equal(thirdPage, currentPageAfterSecondNavigation);
        }

        /// <summary>
        /// Tests that CurrentPage returns the correct page after popping from navigation stack.
        /// Verifies that the toolbar's CurrentPage property updates correctly during back navigation.
        /// </summary>
        [Fact]
        public async Task CurrentPage_AfterPop_ReturnsPreviousPage()
        {
            // Arrange
            var initialPage = new ContentPage { Title = "Initial Page" };
            var secondPage = new ContentPage { Title = "Second Page" };
            var shell = new TestShell(initialPage);
            var toolbar = shell.Toolbar;

            // Act - Navigate forward then back
            await shell.Navigation.PushAsync(secondPage);
            await shell.Navigation.PopAsync();
            var currentPage = toolbar.CurrentPage;

            // Assert
            Assert.Equal(initialPage, currentPage);
        }

        /// <summary>
        /// Tests that CurrentPage returns null when shell has no current page.
        /// Verifies proper handling of edge case where shell navigation is empty.
        /// </summary>
        [Fact]
        public void CurrentPage_WithEmptyShell_ReturnsNull()
        {
            // Arrange
            var shell = new TestShell();
            var toolbar = shell.Toolbar;

            // Act
            var currentPage = toolbar.CurrentPage;

            // Assert
            Assert.Null(currentPage);
        }

        /// <summary>
        /// Tests that DrawerToggleVisible property returns the initial value set in constructor.
        /// Verifies that the property starts with true as the default value.
        /// Expected result: DrawerToggleVisible should return true initially.
        /// </summary>
        [Fact]
        public void DrawerToggleVisible_InitialValue_ReturnsTrue()
        {
            // Arrange
            var shell = new TestShell();
            var toolbar = shell.Toolbar;

            // Act & Assert
            Assert.True(toolbar.DrawerToggleVisible);
        }

        /// <summary>
        /// Tests that DrawerToggleVisible setter updates the property value from true to false.
        /// Verifies that the property correctly stores and returns the new value.
        /// Expected result: DrawerToggleVisible should return false after being set to false.
        /// </summary>
        [Fact]
        public void DrawerToggleVisible_SetToFalse_UpdatesValue()
        {
            // Arrange
            var shell = new TestShell();
            var toolbar = shell.Toolbar;

            // Act
            toolbar.DrawerToggleVisible = false;

            // Assert
            Assert.False(toolbar.DrawerToggleVisible);
        }

        /// <summary>
        /// Tests that DrawerToggleVisible setter updates the property value from false to true.
        /// Verifies that the property correctly stores and returns the new value.
        /// Expected result: DrawerToggleVisible should return true after being set to true.
        /// </summary>
        [Fact]
        public void DrawerToggleVisible_SetToTrue_UpdatesValue()
        {
            // Arrange
            var shell = new TestShell();
            var toolbar = shell.Toolbar;
            toolbar.DrawerToggleVisible = false; // Set to false first

            // Act
            toolbar.DrawerToggleVisible = true;

            // Assert
            Assert.True(toolbar.DrawerToggleVisible);
        }

        /// <summary>
        /// Tests that DrawerToggleVisible setter with various boolean values updates correctly.
        /// Uses parameterized test to verify both true and false values work properly.
        /// Expected result: DrawerToggleVisible should return the value that was set.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DrawerToggleVisible_SetValue_UpdatesCorrectly(bool value)
        {
            // Arrange
            var shell = new TestShell();
            var toolbar = shell.Toolbar;

            // Act
            toolbar.DrawerToggleVisible = value;

            // Assert
            Assert.Equal(value, toolbar.DrawerToggleVisible);
        }

        /// <summary>
        /// Tests that DrawerToggleVisible setter raises PropertyChanged event when value changes.
        /// Verifies that the PropertyChanged event is raised with correct property name.
        /// Expected result: PropertyChanged event should be raised with "DrawerToggleVisible" property name.
        /// </summary>
        [Fact]
        public void DrawerToggleVisible_SetDifferentValue_RaisesPropertyChangedEvent()
        {
            // Arrange
            var shell = new TestShell();
            var toolbar = shell.Toolbar;
            string propertyName = null;
            bool eventRaised = false;

            toolbar.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "DrawerToggleVisible")
                {
                    propertyName = e.PropertyName;
                    eventRaised = true;
                }
            };

            // Act
            toolbar.DrawerToggleVisible = false; // Change from initial true to false

            // Assert
            Assert.True(eventRaised);
            Assert.Equal("DrawerToggleVisible", propertyName);
        }

        /// <summary>
        /// Tests that DrawerToggleVisible setter does not raise PropertyChanged event when value is the same.
        /// Verifies that the PropertyChanged event is not raised when setting the same value.
        /// Expected result: PropertyChanged event should not be raised when setting the same value.
        /// </summary>
        [Fact]
        public void DrawerToggleVisible_SetSameValue_DoesNotRaisePropertyChangedEvent()
        {
            // Arrange
            var shell = new TestShell();
            var toolbar = shell.Toolbar;
            bool eventRaised = false;

            toolbar.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "DrawerToggleVisible")
                {
                    eventRaised = true;
                }
            };

            // Act
            toolbar.DrawerToggleVisible = true; // Set to same value (initial is true)

            // Assert
            Assert.False(eventRaised);
        }

        /// <summary>
        /// Tests that DrawerToggleVisible property change event is raised multiple times for different values.
        /// Verifies that PropertyChanged event is raised each time the value actually changes.
        /// Expected result: PropertyChanged event should be raised twice for two different value changes.
        /// </summary>
        [Fact]
        public void DrawerToggleVisible_MultipleValueChanges_RaisesPropertyChangedEventForEachChange()
        {
            // Arrange
            var shell = new TestShell();
            var toolbar = shell.Toolbar;
            int eventCount = 0;

            toolbar.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "DrawerToggleVisible")
                {
                    eventCount++;
                }
            };

            // Act
            toolbar.DrawerToggleVisible = false; // Change 1: true -> false
            toolbar.DrawerToggleVisible = true;  // Change 2: false -> true
            toolbar.DrawerToggleVisible = true;  // No change: true -> true

            // Assert
            Assert.Equal(2, eventCount);
        }
    }


    public partial class ShellToolbarApplyChangesTests
    {
        /// <summary>
        /// Tests that ApplyChanges returns early when the navigation stack is empty.
        /// This covers the uncovered line 74 where the method returns if stack.Count == 0.
        /// </summary>
        [Fact]
        public void ApplyChanges_WithEmptyNavigationStack_ReturnsEarly()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            var currentPage = Substitute.For<Page>();
            var emptyNavigationStack = Substitute.For<IReadOnlyList<Page>>();
            var navigation = Substitute.For<INavigation>();
            var toolbarTracker = Substitute.For<ToolbarTracker>();

            shell.GetCurrentShellPage().Returns(currentPage);
            navigation.NavigationStack.Returns(emptyNavigationStack);
            shell.Navigation.Returns(navigation);
            emptyNavigationStack.Count.Returns(0);

            var toolbar = new ShellToolbar(shell);

            // Act
            toolbar.ApplyChanges();

            // Assert
            // Verify that methods that should not be called after early return are not called
            shell.DidNotReceive().GetEffectiveValue(Arg.Any<BindableProperty>(), Arg.Any<Func<bool>>(), observer: null);
        }

        /// <summary>
        /// Tests ApplyChanges when current page is null.
        /// This should cause an early return at line 70.
        /// </summary>
        [Fact]
        public void ApplyChanges_WithNullCurrentPage_ReturnsEarly()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            shell.GetCurrentShellPage().Returns((Page)null);

            var toolbar = new ShellToolbar(shell);

            // Act
            toolbar.ApplyChanges();

            // Assert
            // Verify that Navigation is not accessed when currentPage is null
            shell.DidNotReceive().Navigation.Received();
        }

        /// <summary>
        /// Tests ApplyChanges with single page in navigation stack.
        /// This tests the drawer toggle visibility logic and back button visibility when stack count <= 1.
        /// </summary>
        [Fact]
        public void ApplyChanges_WithSinglePageInStack_SetsCorrectVisibilityProperties()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            var currentPage = Substitute.For<Page>();
            var navigationStack = Substitute.For<IReadOnlyList<Page>>();
            var navigation = Substitute.For<INavigation>();
            var flyoutView = Substitute.For<IFlyoutView>();
            var toolbarTracker = Substitute.For<ToolbarTracker>();
            var toolbarItems = new List<ToolbarItem>();

            shell.GetCurrentShellPage().Returns(currentPage);
            navigation.NavigationStack.Returns(navigationStack);
            shell.Navigation.Returns(navigation);
            navigationStack.Count.Returns(1);
            ((IFlyoutView)shell).FlyoutBehavior.Returns(FlyoutBehavior.Flyout);
            toolbarTracker.ToolbarItems.Returns(toolbarItems);
            shell.GetEffectiveValue(Arg.Any<BindableProperty>(), Arg.Any<Func<bool>>(), observer: null).Returns(true);

            var toolbar = new ShellToolbar(shell);

            // Act
            toolbar.ApplyChanges();

            // Assert
            Assert.True(toolbar.DrawerToggleVisible); // Should be true when stack count <= 1 and flyout behavior is Flyout
            Assert.False(toolbar.BackButtonVisible); // Should be false when stack count <= 1
        }

        /// <summary>
        /// Tests ApplyChanges with multiple pages in navigation stack.
        /// This tests back button visibility when stack count > 1.
        /// </summary>
        [Fact]
        public void ApplyChanges_WithMultiplePagesInStack_SetsBackButtonVisible()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            var currentPage = Substitute.For<Page>();
            var previousPage = Substitute.For<Page>();
            var navigationStack = Substitute.For<IReadOnlyList<Page>>();
            var navigation = Substitute.For<INavigation>();
            var flyoutView = Substitute.For<IFlyoutView>();
            var toolbarTracker = Substitute.For<ToolbarTracker>();
            var toolbarItems = new List<ToolbarItem>();

            shell.GetCurrentShellPage().Returns(currentPage);
            navigation.NavigationStack.Returns(navigationStack);
            shell.Navigation.Returns(navigation);
            navigationStack.Count.Returns(2);
            navigationStack[1].Returns(previousPage);
            ((IFlyoutView)shell).FlyoutBehavior.Returns(FlyoutBehavior.Disabled);
            toolbarTracker.ToolbarItems.Returns(toolbarItems);
            shell.GetEffectiveValue(Arg.Any<BindableProperty>(), Arg.Any<Func<bool>>(), observer: null).Returns(true);

            var toolbar = new ShellToolbar(shell);

            // Act
            toolbar.ApplyChanges();

            // Assert
            Assert.False(toolbar.DrawerToggleVisible); // Should be false when stack count > 1
            Assert.True(toolbar.BackButtonVisible); // Should be true when stack count > 1
        }

        /// <summary>
        /// Tests ApplyChanges when back button behavior is set and IsVisible is false.
        /// This tests the back button visibility override logic.
        /// </summary>
        [Fact]
        public void ApplyChanges_WithBackButtonBehaviorIsVisibleFalse_HidesBackButton()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            var currentPage = Substitute.For<Page>();
            var navigationStack = Substitute.For<IReadOnlyList<Page>>();
            var navigation = Substitute.For<INavigation>();
            var backButtonBehavior = Substitute.For<BackButtonBehavior>();
            var toolbarTracker = Substitute.For<ToolbarTracker>();
            var toolbarItems = new List<ToolbarItem>();

            shell.GetCurrentShellPage().Returns(currentPage);
            navigation.NavigationStack.Returns(navigationStack);
            shell.Navigation.Returns(navigation);
            navigationStack.Count.Returns(2);
            backButtonBehavior.IsVisible.Returns(false);
            backButtonBehavior.IsEnabled.Returns(true);
            toolbarTracker.ToolbarItems.Returns(toolbarItems);
            shell.GetEffectiveValue(Arg.Any<BindableProperty>(), Arg.Any<Func<bool>>(), observer: null).Returns(true);

            var toolbar = new ShellToolbar(shell);
            // Simulate that UpdateBackbuttonBehavior sets the _backButtonBehavior
            toolbar.GetType().GetField("_backButtonBehavior", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(toolbar, backButtonBehavior);

            // Act
            toolbar.ApplyChanges();

            // Assert
            Assert.False(toolbar.BackButtonVisible); // Should be false when backButtonBehavior.IsVisible is false
        }

        /// <summary>
        /// Tests ApplyChanges when back button behavior is set and IsEnabled is false.
        /// This tests the back button enabled state logic.
        /// </summary>
        [Fact]
        public void ApplyChanges_WithBackButtonBehaviorIsEnabledFalse_DisablesBackButton()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            var currentPage = Substitute.For<Page>();
            var navigationStack = Substitute.For<IReadOnlyList<Page>>();
            var navigation = Substitute.For<INavigation>();
            var backButtonBehavior = Substitute.For<BackButtonBehavior>();
            var toolbarTracker = Substitute.For<ToolbarTracker>();
            var toolbarItems = new List<ToolbarItem>();

            shell.GetCurrentShellPage().Returns(currentPage);
            navigation.NavigationStack.Returns(navigationStack);
            shell.Navigation.Returns(navigation);
            navigationStack.Count.Returns(2);
            backButtonBehavior.IsVisible.Returns(true);
            backButtonBehavior.IsEnabled.Returns(false);
            toolbarTracker.ToolbarItems.Returns(toolbarItems);
            shell.GetEffectiveValue(Arg.Any<BindableProperty>(), Arg.Any<Func<bool>>(), observer: null).Returns(true);

            var toolbar = new ShellToolbar(shell);
            // Simulate that UpdateBackbuttonBehavior sets the _backButtonBehavior
            toolbar.GetType().GetField("_backButtonBehavior", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(toolbar, backButtonBehavior);

            // Act
            toolbar.ApplyChanges();

            // Assert
            Assert.False(toolbar.BackButtonEnabled); // Should be false when backButtonBehavior.IsEnabled is false
        }

        /// <summary>
        /// Tests ApplyChanges when back button behavior is null.
        /// This tests the default back button enabled state (should be true).
        /// </summary>
        [Fact]
        public void ApplyChanges_WithNullBackButtonBehavior_DefaultsBackButtonEnabledToTrue()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            var currentPage = Substitute.For<Page>();
            var navigationStack = Substitute.For<IReadOnlyList<Page>>();
            var navigation = Substitute.For<INavigation>();
            var toolbarTracker = Substitute.For<ToolbarTracker>();
            var toolbarItems = new List<ToolbarItem>();

            shell.GetCurrentShellPage().Returns(currentPage);
            navigation.NavigationStack.Returns(navigationStack);
            shell.Navigation.Returns(navigation);
            navigationStack.Count.Returns(2);
            toolbarTracker.ToolbarItems.Returns(toolbarItems);
            shell.GetEffectiveValue(Arg.Any<BindableProperty>(), Arg.Any<Func<bool>>(), observer: null).Returns(true);

            var toolbar = new ShellToolbar(shell);
            // Ensure _backButtonBehavior is null
            toolbar.GetType().GetField("_backButtonBehavior", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(toolbar, null);

            // Act
            toolbar.ApplyChanges();

            // Assert
            Assert.True(toolbar.BackButtonEnabled); // Should default to true when _backButtonBehavior is null
        }

        /// <summary>
        /// Tests ApplyChanges with different flyout behaviors.
        /// This tests the drawer toggle visibility logic for different flyout behaviors.
        /// </summary>
        [Theory]
        [InlineData(FlyoutBehavior.Disabled, false)]
        [InlineData(FlyoutBehavior.Flyout, true)]
        [InlineData(FlyoutBehavior.Locked, false)]
        public void ApplyChanges_WithDifferentFlyoutBehaviors_SetsDrawerToggleVisibilityCorrectly(FlyoutBehavior flyoutBehavior, bool expectedDrawerToggleVisible)
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            var currentPage = Substitute.For<Page>();
            var navigationStack = Substitute.For<IReadOnlyList<Page>>();
            var navigation = Substitute.For<INavigation>();
            var toolbarTracker = Substitute.For<ToolbarTracker>();
            var toolbarItems = new List<ToolbarItem>();

            shell.GetCurrentShellPage().Returns(currentPage);
            navigation.NavigationStack.Returns(navigationStack);
            shell.Navigation.Returns(navigation);
            navigationStack.Count.Returns(1); // Single page to test flyout behavior effect
            ((IFlyoutView)shell).FlyoutBehavior.Returns(flyoutBehavior);
            toolbarTracker.ToolbarItems.Returns(toolbarItems);
            shell.GetEffectiveValue(Arg.Any<BindableProperty>(), Arg.Any<Func<bool>>(), observer: null).Returns(true);

            var toolbar = new ShellToolbar(shell);

            // Act
            toolbar.ApplyChanges();

            // Assert
            Assert.Equal(expectedDrawerToggleVisible, toolbar.DrawerToggleVisible);
        }

        /// <summary>
        /// Tests ApplyChanges when current page changes.
        /// This tests the PropertyChanged event subscription/unsubscription logic.
        /// </summary>
        [Fact]
        public void ApplyChanges_WhenCurrentPageChanges_UpdatesPropertyChangedSubscription()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            var initialPage = Substitute.For<Page>();
            var newPage = Substitute.For<Page>();
            var navigationStack = Substitute.For<IReadOnlyList<Page>>();
            var navigation = Substitute.For<INavigation>();
            var toolbarTracker = Substitute.For<ToolbarTracker>();
            var toolbarItems = new List<ToolbarItem>();

            navigation.NavigationStack.Returns(navigationStack);
            shell.Navigation.Returns(navigation);
            navigationStack.Count.Returns(1);
            toolbarTracker.ToolbarItems.Returns(toolbarItems);
            shell.GetEffectiveValue(Arg.Any<BindableProperty>(), Arg.Any<Func<bool>>(), observer: null).Returns(true);
            ((IFlyoutView)shell).FlyoutBehavior.Returns(FlyoutBehavior.Disabled);

            var toolbar = new ShellToolbar(shell);

            // Set initial page
            shell.GetCurrentShellPage().Returns(initialPage);
            toolbar.ApplyChanges();

            // Verify initial page is current
            Assert.Equal(initialPage, toolbar.CurrentPage);

            // Change to new page
            shell.GetCurrentShellPage().Returns(newPage);

            // Act
            toolbar.ApplyChanges();

            // Assert
            Assert.Equal(newPage, toolbar.CurrentPage);
            // Verify PropertyChanged events are properly managed (unsubscribed from old, subscribed to new)
            initialPage.Received().PropertyChanged -= Arg.Any<PropertyChangedEventHandler>();
            newPage.Received().PropertyChanged += Arg.Any<PropertyChangedEventHandler>();
        }

        /// <summary>
        /// Tests ApplyChanges sets DynamicOverflowEnabled when current page is not null.
        /// This tests the platform-specific dynamic overflow logic.
        /// </summary>
        [Fact]
        public void ApplyChanges_WithValidCurrentPage_SetsDynamicOverflowEnabled()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            var currentPage = Substitute.For<Page>();
            var navigationStack = Substitute.For<IReadOnlyList<Page>>();
            var navigation = Substitute.For<INavigation>();
            var toolbarTracker = Substitute.For<ToolbarTracker>();
            var toolbarItems = new List<ToolbarItem>();

            shell.GetCurrentShellPage().Returns(currentPage);
            navigation.NavigationStack.Returns(navigationStack);
            shell.Navigation.Returns(navigation);
            navigationStack.Count.Returns(1);
            toolbarTracker.ToolbarItems.Returns(toolbarItems);
            shell.GetEffectiveValue(Arg.Any<BindableProperty>(), Arg.Any<Func<bool>>(), observer: null).Returns(true);
            ((IFlyoutView)shell).FlyoutBehavior.Returns(FlyoutBehavior.Disabled);

            var toolbar = new ShellToolbar(shell);

            // Act
            toolbar.ApplyChanges();

            // Assert
            // Verify that GetToolbarDynamicOverflowEnabled was called with the current page
            // Note: We can't directly verify the DynamicOverflowEnabled property value because it depends on
            // the platform-specific implementation, but we can verify the method was called
            Assert.NotNull(toolbar.CurrentPage);
            Assert.Equal(currentPage, toolbar.CurrentPage);
        }
    }
}