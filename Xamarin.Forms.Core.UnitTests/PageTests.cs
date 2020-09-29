using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class PageTests : BaseTestFixture
	{
		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			MessagingCenter.ClearSubscribers();
		}

		[Test]
		public void TestConstructor()
		{
			var child = new Label();
			Page root = new ContentPage { Content = child };

			Assert.AreEqual(((IElementController)root).LogicalChildren.Count, 1);
			Assert.AreSame(((IElementController)root).LogicalChildren.First(), child);
		}

		[Test]
		public void TestChildFillBehavior()
		{
			var child = new Label();
			Page root = new ContentPage { Content = child };
			root.IsPlatformEnabled = child.IsPlatformEnabled = true;

			root.Layout(new Rectangle(0, 0, 200, 500));

			Assert.AreEqual(child.Width, 200);
			Assert.AreEqual(child.Height, 500);
		}

		[Test]
		public void TestSizedChildBehavior()
		{
			var child = new Label { IsPlatformEnabled = true, WidthRequest = 100, HorizontalOptions = LayoutOptions.Center };
			var root = new ContentPage { IsPlatformEnabled = true, Content = child };

			root.Layout(new Rectangle(0, 0, 200, 500));

			Assert.AreEqual(50, child.X);
			Assert.AreEqual(100, child.Width);
			Assert.AreEqual(500, child.Height);

			child = new Label()
			{
				IsPlatformEnabled = true,
				HeightRequest = 100,
				VerticalOptions = LayoutOptions.Center
			};

			root = new ContentPage
			{
				IsPlatformEnabled = true,
				Content = child
			};

			root.Layout(new Rectangle(0, 0, 200, 500));

			Assert.AreEqual(0, child.X);
			Assert.AreEqual(200, child.Y);
			Assert.AreEqual(200, child.Width);
			Assert.AreEqual(100, child.Height);

			child = new Label();
			child.IsPlatformEnabled = true;
			child.HeightRequest = 100;

			root = new ContentPage
			{
				Content = child,
				IsPlatformEnabled = true
			};

			root.Layout(new Rectangle(0, 0, 200, 500));

			Assert.AreEqual(0, child.X);
			Assert.AreEqual(0, child.Y);
			Assert.AreEqual(200, child.Width);
			Assert.AreEqual(500, child.Height);
		}

		[Test]
		public void NativeSizedChildBehavior()
		{
			var child = new Label { IsPlatformEnabled = true, HorizontalOptions = LayoutOptions.Center };
			var root = new ContentPage { IsPlatformEnabled = true, Content = child };

			root.Layout(new Rectangle(0, 0, 200, 500));

			Assert.AreEqual(50, child.X);
			Assert.AreEqual(100, child.Width);
			Assert.AreEqual(500, child.Height);

			child = new Label()
			{
				IsPlatformEnabled = true,
				VerticalOptions = LayoutOptions.Center
			};

			root = new ContentPage
			{
				IsPlatformEnabled = true,
				Content = child
			};

			root.Layout(new Rectangle(0, 0, 200, 500));

			Assert.AreEqual(0, child.X);
			Assert.AreEqual(240, child.Y);
			Assert.AreEqual(200, child.Width);
			Assert.AreEqual(20, child.Height);
		}

		[Test]
		public void TestContentPageSetContent()
		{
			View child;
			var page = new ContentPage { Content = child = new View() };

			Assert.AreEqual(child, page.Content);

			bool fired = false;
			page.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Content")
					fired = true;
			};

			page.Content = child;
			Assert.False(fired);

			page.Content = new View();
			Assert.True(fired);

			page.Content = null;
			Assert.Null(page.Content);
		}

		[Test]
		public void TestLayoutChildrenFill()
		{
			View child;
			var page = new ContentPage
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 200,
					IsPlatformEnabled = true
				},
				IsPlatformEnabled = true,
			};

			page.Layout(new Rectangle(0, 0, 800, 800));

			Assert.AreEqual(new Rectangle(0, 0, 800, 800), child.Bounds);

			page.Layout(new Rectangle(0, 0, 50, 50));

			Assert.AreEqual(new Rectangle(0, 0, 50, 50), child.Bounds);
		}

		[Test]
		public void TestLayoutChildrenStart()
		{
			View child;
			var page = new ContentPage
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 200,
					HorizontalOptions = LayoutOptions.Start,
					VerticalOptions = LayoutOptions.Start,
					IsPlatformEnabled = true
				},
				IsPlatformEnabled = true,
			};

			page.Layout(new Rectangle(0, 0, 800, 800));

			Assert.AreEqual(new Rectangle(0, 0, 100, 200), child.Bounds);

			page.Layout(new Rectangle(0, 0, 50, 50));

			Assert.AreEqual(new Rectangle(0, 0, 50, 50), child.Bounds);
		}

		[Test]
		public void TestLayoutChildrenEnd()
		{
			View child;
			var page = new ContentPage
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 200,
					HorizontalOptions = LayoutOptions.End,
					VerticalOptions = LayoutOptions.End,
					IsPlatformEnabled = true
				},
				IsPlatformEnabled = true,
			};

			page.Layout(new Rectangle(0, 0, 800, 800));

			Assert.AreEqual(new Rectangle(700, 600, 100, 200), child.Bounds);

			page.Layout(new Rectangle(0, 0, 50, 50));

			Assert.AreEqual(new Rectangle(0, 0, 50, 50), child.Bounds);
		}

		[Test]
		public void TestLayoutChildrenCenter()
		{
			View child;
			var page = new ContentPage
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 200,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					IsPlatformEnabled = true
				},
				IsPlatformEnabled = true,
			};

			page.Layout(new Rectangle(0, 0, 800, 800));

			Assert.AreEqual(new Rectangle(350, 300, 100, 200), child.Bounds);

			page.Layout(new Rectangle(0, 0, 50, 50));

			Assert.AreEqual(new Rectangle(0, 0, 50, 50), child.Bounds);
		}

		[Test]
		public void TestLayoutWithContainerArea()
		{
			View child;
			var page = new ContentPage
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 200,
					IsPlatformEnabled = true
				},
				IsPlatformEnabled = true,
			};

			page.Layout(new Rectangle(0, 0, 800, 800));

			Assert.AreEqual(new Rectangle(0, 0, 800, 800), child.Bounds);
			((IPageController)page).ContainerArea = new Rectangle(10, 10, 30, 30);

			Assert.AreEqual(new Rectangle(10, 10, 30, 30), child.Bounds);

			page.Layout(new Rectangle(0, 0, 50, 50));

			Assert.AreEqual(new Rectangle(10, 10, 30, 30), child.Bounds);
		}

		[Test]
		public void TestThrowOnInvalidAlignment()
		{
			bool thrown = false;

			try
			{
				new ContentPage
				{
					Content = new View
					{
						WidthRequest = 100,
						HeightRequest = 200,
						HorizontalOptions = new LayoutOptions((LayoutAlignment)int.MaxValue, false),
						VerticalOptions = LayoutOptions.Center,
						IsPlatformEnabled = true
					},
					IsPlatformEnabled = true,
				};
			}
			catch (ArgumentOutOfRangeException)
			{
				thrown = true;
			}

			Assert.True(thrown);
		}

		[Test]
		public void BusyNotSentWhenNotVisible()
		{
			var sent = false;
			MessagingCenter.Subscribe<Page, bool>(this, Page.BusySetSignalName, (p, b) => sent = true);

			new ContentPage { IsBusy = true };

			Assert.That(sent, Is.False, "Busy message sent while not visible");
		}

		[Test]
		public void BusySentWhenBusyPageAppears()
		{
			var sent = false;
			MessagingCenter.Subscribe<Page, bool>(this, Page.BusySetSignalName, (p, b) =>
			{
				Assert.That(b, Is.True);
				sent = true;
			});

			var page = new ContentPage { IsBusy = true, IsPlatformEnabled = true };

			Assert.That(sent, Is.False, "Busy message sent while not visible");

			((IPageController)page).SendAppearing();

			Assert.That(sent, Is.True, "Busy message not sent when visible");
		}

		[Test]
		public void BusySentWhenBusyPageDisappears()
		{
			var page = new ContentPage { IsBusy = true };
			((IPageController)page).SendAppearing();

			var sent = false;
			MessagingCenter.Subscribe<Page, bool>(this, Page.BusySetSignalName, (p, b) =>
			{
				Assert.That(b, Is.False);
				sent = true;
			});

			((IPageController)page).SendDisappearing();

			Assert.That(sent, Is.True, "Busy message not sent when visible");
		}

		[Test]
		public void BusySentWhenVisiblePageSetToBusy()
		{
			var sent = false;
			MessagingCenter.Subscribe<Page, bool>(this, Page.BusySetSignalName, (p, b) => sent = true);

			var page = new ContentPage();
			((IPageController)page).SendAppearing();

			Assert.That(sent, Is.False, "Busy message sent appearing while not busy");

			page.IsBusy = true;

			Assert.That(sent, Is.True, "Busy message not sent when visible");
		}

		[Test]
		public void DisplayAlert()
		{
			var page = new ContentPage() { IsPlatformEnabled = true };

			AlertArguments args = null;
			MessagingCenter.Subscribe(this, Page.AlertSignalName, (Page sender, AlertArguments e) => args = e);

			var task = page.DisplayAlert("Title", "Message", "Accept", "Cancel");

			Assert.AreEqual("Title", args.Title);
			Assert.AreEqual("Message", args.Message);
			Assert.AreEqual("Accept", args.Accept);
			Assert.AreEqual("Cancel", args.Cancel);

			bool completed = false;
			var continueTask = task.ContinueWith(t => completed = true);

			args.SetResult(true);
			continueTask.Wait();
			Assert.True(completed);
		}

		[Test]
		public void DisplayActionSheet()
		{
			var page = new ContentPage() { IsPlatformEnabled = true };

			ActionSheetArguments args = null;
			MessagingCenter.Subscribe(this, Page.ActionSheetSignalName, (Page sender, ActionSheetArguments e) => args = e);

			var task = page.DisplayActionSheet("Title", "Cancel", "Destruction", "Other 1", "Other 2");

			Assert.AreEqual("Title", args.Title);
			Assert.AreEqual("Destruction", args.Destruction);
			Assert.AreEqual("Cancel", args.Cancel);
			Assert.AreEqual("Other 1", args.Buttons.First());
			Assert.AreEqual("Other 2", args.Buttons.Skip(1).First());

			bool completed = false;
			var continueTask = task.ContinueWith(t => completed = true);

			args.SetResult("Cancel");
			continueTask.Wait();
			Assert.True(completed);
		}

		class PageTestApp : Application { }

		[Test]
		public void SendApplicationPageAppearing()
		{
			var app = new PageTestApp();
			var page = new ContentPage();

			Page actual = null;
			app.MainPage = page;
			app.PageAppearing += (sender, args) => actual = args;

			((IPageController)page).SendAppearing();

			Assert.AreSame(page, actual);
		}

		[Test]
		public void SendApplicationPageDisappearing()
		{
			var app = new PageTestApp();
			var page = new ContentPage();

			Page actual = null;
			app.MainPage = page;
			app.PageDisappearing += (sender, args) => actual = args;

			((IPageController)page).SendAppearing();
			((IPageController)page).SendDisappearing();

			Assert.AreSame(page, actual);
		}

		[Test]
		public void SendAppearing()
		{
			var page = new ContentPage();

			bool sent = false;
			page.Appearing += (sender, args) => sent = true;

			((IPageController)page).SendAppearing();

			Assert.True(sent);
		}

		[Test]
		public void SendDisappearing()
		{
			var page = new ContentPage();

			((IPageController)page).SendAppearing();

			bool sent = false;
			page.Disappearing += (sender, args) => sent = true;

			((IPageController)page).SendDisappearing();

			Assert.True(sent);
		}

		[Test]
		public void SendAppearingDoesntGetCalledMultipleTimes()
		{
			var page = new ContentPage();

			int countAppearing = 0;
			page.Appearing += (sender, args) => countAppearing++;

			((IPageController)page).SendAppearing();
			((IPageController)page).SendAppearing();

			Assert.That(countAppearing, Is.EqualTo(1));
		}

		[Test]
		public void IsVisibleWorks()
		{
			var page = new ContentPage();
			page.IsVisible = false;
			Assert.False(page.IsVisible);
		}

		[Test]
		public void SendAppearingToChildrenAfter()
		{
			var page = new ContentPage();

			var navPage = new NavigationPage(page);

			bool sentNav = false;
			bool sent = false;
			page.Appearing += (sender, args) =>
			{
				if (sentNav)
					sent = true;
			};
			navPage.Appearing += (sender, e) => sentNav = true;

			((IPageController)navPage).SendAppearing();

			Assert.True(sentNav);
			Assert.True(sent);

		}

		[Test]
		public void SendDisappearingToChildrenPageFirst()
		{
			var page = new ContentPage();

			var navPage = new NavigationPage(page);
			((IPageController)navPage).SendAppearing();

			bool sentNav = false;
			bool sent = false;
			page.Disappearing += (sender, args) =>
			{
				sent = true;
			};
			navPage.Disappearing += (sender, e) =>
			{
				if (sent)
					sentNav = true;
			};
			((IPageController)navPage).SendDisappearing();

			Assert.True(sentNav);
			Assert.True(sent);
		}
	}
}