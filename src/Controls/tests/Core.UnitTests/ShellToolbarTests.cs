using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
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
	}
}
