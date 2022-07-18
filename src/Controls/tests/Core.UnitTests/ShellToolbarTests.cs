using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class ShellToolbarTests : ShellTestBase
	{
		[Test]
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
			Assert.True(toolbar.ToolbarItems.Contains(toolbarItem1));
			Assert.True(toolbar.ToolbarItems.Contains(toolbarItem2));
			Assert.AreEqual(2, toolbar.ToolbarItems.Count());

			await shell.Navigation.PushAsync(secondPage);
			Assert.True(toolbar.ToolbarItems.Contains(toolbarItem1));
			Assert.True(toolbar.ToolbarItems.Contains(toolbarItem3));
			Assert.AreEqual(2, toolbar.ToolbarItems.Count());
		}

		[Test]
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
			Assert.AreEqual(pushedPage, testShell.CurrentPage);
			Assert.IsTrue(commandExecuted);
			Assert.AreEqual("PARAMETER", parameter);
		}

		[Test]
		public async Task BackButtonDisabledWhenCommandDisabled()
		{
			var page = new ContentPage();
			TestShell testShell = new TestShell(new ContentPage());
			await testShell.Navigation.PushAsync(page);

			var backButtonBehavior = new BackButtonBehavior();
			Shell.SetBackButtonBehavior(page, backButtonBehavior);
			Assert.IsTrue(testShell.Toolbar.BackButtonEnabled);

			bool canExecute = false;
			backButtonBehavior.Command = new Command(() => { }, () => canExecute);
			Assert.IsFalse(testShell.Toolbar.BackButtonEnabled);
			canExecute = true;
			(backButtonBehavior.Command as Command).ChangeCanExecute();
			Assert.IsTrue(testShell.Toolbar.BackButtonEnabled);
		}

		[Test]
		public async Task ShellToolbarUpdatesFromNewBackButtonBehavior()
		{
			var page = new ContentPage();
			TestShell testShell = new TestShell(new ContentPage());
			await testShell.Navigation.PushAsync(page);

			Assert.IsTrue(testShell.Toolbar.BackButtonVisible);
			var backButtonBehavior = new BackButtonBehavior()
			{
				IsVisible = false,
			};

			Shell.SetBackButtonBehavior(page, backButtonBehavior);
			Assert.IsFalse(testShell.Toolbar.BackButtonVisible);
		}

		[Test]
		public async Task ShellToolbarUpdatesFromPropertyChanged()
		{
			var page = new ContentPage();
			TestShell testShell = new TestShell(new ContentPage());
			Shell.SetBackButtonBehavior(page, new BackButtonBehavior());
			await testShell.Navigation.PushAsync(page);

			Assert.IsTrue(testShell.Toolbar.IsVisible);
			var backButtonBehavior = new BackButtonBehavior()
			{
				IsVisible = true,
			};

			Shell.SetBackButtonBehavior(page, backButtonBehavior);
			Assert.IsTrue(testShell.Toolbar.BackButtonVisible);
			backButtonBehavior.IsVisible = false;
			Assert.IsFalse(testShell.Toolbar.BackButtonVisible);
		}

		[Test]
		public void NavBarIsVisibleUpdates()
		{
			var page = new ContentPage() { Title = "Test" };
			TestShell testShell = new TestShell(page);
			var toolBar = testShell.Toolbar;
			Assert.IsTrue(toolBar.IsVisible);
			Shell.SetNavBarIsVisible(page, false);
			Assert.False(toolBar.IsVisible);
		}

		[Test]
		public void BackButtonBehaviorSet()
		{
			var page = new ContentPage();

			Assert.IsNull(Shell.GetBackButtonBehavior(page));

			var backButtonBehavior = new BackButtonBehavior();

			Shell.SetBackButtonBehavior(page, backButtonBehavior);

			Assert.AreEqual(backButtonBehavior, Shell.GetBackButtonBehavior(page));
		}

		[Test]
		public void BackButtonBehaviorBindingContextPropagation()
		{
			object bindingContext = new object();
			var page = new ContentPage();
			var backButtonBehavior = new BackButtonBehavior();

			Shell.SetBackButtonBehavior(page, backButtonBehavior);
			page.BindingContext = bindingContext;

			Assert.AreEqual(page.BindingContext, backButtonBehavior.BindingContext);
		}

		[Test]
		public void BackButtonBehaviorBindingContextPropagationWithExistingBindingContext()
		{
			object bindingContext = new object();
			var page = new ContentPage();
			var backButtonBehavior = new BackButtonBehavior();

			page.BindingContext = bindingContext;
			Shell.SetBackButtonBehavior(page, backButtonBehavior);

			Assert.AreEqual(page.BindingContext, backButtonBehavior.BindingContext);
		}

		[Test]
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
			Assert.AreEqual("Test Title", toolbar.Title);
			Shell.SetTitleView(contentPage, titleView);
			Assert.IsEmpty(toolbar.Title);
			Assert.AreEqual(titleView, toolbar.TitleView);
			Shell.SetTitleView(contentPage, null);
			Assert.AreEqual("Test Title", toolbar.Title);
		}

		[Test]
		public void ContentPageColorsPropagateToShellToolbar()
		{
			var contentPage = new ContentPage() { Title = "Test Title" };
			Shell.SetBackgroundColor(contentPage, Colors.Green);
			Shell.SetTitleColor(contentPage, Colors.Pink);
			Shell.SetForegroundColor(contentPage, Colors.Orange);

			TestShell testShell = new TestShell(contentPage);
			_ = new Window() { Page = testShell };
			var toolbar = testShell.Toolbar;

			Assert.AreEqual(Colors.Green, (toolbar.BarBackground as SolidColorBrush).Color);
			Assert.AreEqual(Colors.Orange, toolbar.IconColor);
			Assert.AreEqual(Colors.Pink, toolbar.BarTextColor);
		}

		[Test]
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

			Assert.IsTrue(toolbar.IsVisible);
		}
	}
}
