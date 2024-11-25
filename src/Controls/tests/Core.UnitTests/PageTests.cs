using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class PageTests : BaseTestFixture
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				MessagingCenter.ClearSubscribers();
			}

			base.Dispose(disposing);
		}

		[Fact]
		public void TestConstructor()
		{
			var child = new Label();
			Page root = new ContentPage { Content = child };

			Assert.Equal(root, child.Parent);

			Assert.Single(((IElementController)root).LogicalChildren);
			Assert.Same(((IElementController)root).LogicalChildren.First(), child);

			((ContentPage)root).Content = null;
			Assert.Null(child.Parent);
		}

		[Fact]
		public void TestChildFillBehavior()
		{
			var child = MockPlatformSizeService.Sub<Label>();
			Page root = new ContentPage { Content = child };
			root.IsPlatformEnabled = child.IsPlatformEnabled = true;

			root.Layout(new Rect(0, 0, 200, 500));


			Assert.Equal(200, child.Width);
			Assert.Equal(500, child.Height);
		}

		[Fact]
		public void TestSizedChildBehavior()
		{
			var child = MockPlatformSizeService.Sub<Label>(width: 100, horizOpts: LayoutOptions.Center);
			var root = new ContentPage { IsPlatformEnabled = true, Content = child };

			root.Layout(new Rect(0, 0, 200, 500));

			Assert.Equal(50, child.X);
			Assert.Equal(100, child.Width);
			Assert.Equal(500, child.Height);

			child = child = MockPlatformSizeService.Sub<Label>(height: 100, vertOpts: LayoutOptions.Center);

			root = new ContentPage
			{
				IsPlatformEnabled = true,
				Content = child
			};

			root.Layout(new Rect(0, 0, 200, 500));

			Assert.Equal(0, child.X);
			Assert.Equal(200, child.Y);
			Assert.Equal(200, child.Width);
			Assert.Equal(100, child.Height);

			child = MockPlatformSizeService.Sub<Label>(height: 100);

			root = new ContentPage
			{
				Content = child,
				IsPlatformEnabled = true
			};

			root.Layout(new Rect(0, 0, 200, 500));

			Assert.Equal(0, child.X);
			Assert.Equal(0, child.Y);
			Assert.Equal(200, child.Width);
			Assert.Equal(500, child.Height);
		}

		[Fact]
		public void NativeSizedChildBehavior()
		{
			var child = MockPlatformSizeService.Sub<Label>(horizOpts: LayoutOptions.Center);
			var root = new ContentPage { IsPlatformEnabled = true, Content = child };

			root.Layout(new Rect(0, 0, 200, 500));

			Assert.Equal(50, child.X);
			Assert.Equal(100, child.Width);
			Assert.Equal(500, child.Height);

			child = MockPlatformSizeService.Sub<Label>(vertOpts: LayoutOptions.Center);

			root = new ContentPage
			{
				IsPlatformEnabled = true,
				Content = child
			};

			root.Layout(new Rect(0, 0, 200, 500));

			Assert.Equal(0, child.X);
			Assert.Equal(240, child.Y);
			Assert.Equal(200, child.Width);
			Assert.Equal(20, child.Height);
		}

		[Fact]
		public void TestContentPageSetContent()
		{
			View child;
			var page = new ContentPage { Content = child = new View() };

			Assert.Equal(child, page.Content);

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

		[Fact]
		public void TestLayoutChildrenFill()
		{
			View child;
			var page = new ContentPage
			{
				Content = child = MockPlatformSizeService.Sub<View>(width: 100, height: 200),
				IsPlatformEnabled = true,
			};

			page.Layout(new Rect(0, 0, 800, 800));

			Assert.Equal(new Rect(0, 0, 800, 800), child.Bounds);

			page.Layout(new Rect(0, 0, 50, 50));

			Assert.Equal(new Rect(0, 0, 50, 50), child.Bounds);
		}

		[Fact]
		public void TestLayoutChildrenStart()
		{
			View child;
			var page = new ContentPage
			{
				Content = child = MockPlatformSizeService.Sub<View>(
					width: 100,
					height: 200,
					vertOpts: LayoutOptions.Start,
					horizOpts: LayoutOptions.Start),
				IsPlatformEnabled = true,
			};

			page.Layout(new Rect(0, 0, 800, 800));

			Assert.Equal(new Rect(0, 0, 100, 200), child.Bounds);

			page.Layout(new Rect(0, 0, 50, 50));

			Assert.Equal(new Rect(0, 0, 50, 50), child.Bounds);
		}

		[Fact]
		public void TestLayoutChildrenEnd()
		{
			View child;
			var page = new ContentPage
			{
				Content = child = MockPlatformSizeService.Sub<View>(
					width: 100,
					height: 200,
					vertOpts: LayoutOptions.End,
					horizOpts: LayoutOptions.End),
				IsPlatformEnabled = true,
			};

			page.Layout(new Rect(0, 0, 800, 800));

			Assert.Equal(new Rect(700, 600, 100, 200), child.Bounds);

			page.Layout(new Rect(0, 0, 50, 50));

			Assert.Equal(new Rect(0, 0, 50, 50), child.Bounds);
		}

		[Fact]
		public void TestLayoutChildrenCenter()
		{
			View child;
			var page = new ContentPage
			{
				Content = child = MockPlatformSizeService.Sub<View>(
					width: 100,
					height: 200,
					vertOpts: LayoutOptions.Center,
					horizOpts: LayoutOptions.Center),
				IsPlatformEnabled = true,
			};

			page.Layout(new Rect(0, 0, 800, 800));

			Assert.Equal(new Rect(350, 300, 100, 200), child.Bounds);

			page.Layout(new Rect(0, 0, 50, 50));

			Assert.Equal(new Rect(0, 0, 50, 50), child.Bounds);
		}

		[Fact]
		public void TestLayoutWithContainerArea()
		{
			View child;
			var page = new ContentPage
			{
				Content = child = MockPlatformSizeService.Sub<View>(width: 100, height: 200),
				IsPlatformEnabled = true,
			};

			page.Layout(new Rect(0, 0, 800, 800));

			Assert.Equal(new Rect(0, 0, 800, 800), child.Bounds);
			((IPageController)page).ContainerArea = new Rect(10, 10, 30, 30);

			Assert.Equal(new Rect(10, 10, 30, 30), child.Bounds);

			page.Layout(new Rect(0, 0, 50, 50));

			Assert.Equal(new Rect(10, 10, 30, 30), child.Bounds);
		}

		[Fact]
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

		[Fact]
		public void BusyNotSentWhenNotVisible()
		{
			var sent = false;
			MessagingCenter.Subscribe<Page, bool>(this, Page.BusySetSignalName, (p, b) => sent = true);

			new ContentPage { IsBusy = true };

			Assert.False(sent);
		}

		[Fact]
		public void BusySentWhenBusyPageAppears()
		{
			var sent = false;
			MessagingCenter.Subscribe<Page, bool>(this, Page.BusySetSignalName, (p, b) =>
			{
				Assert.True(b);
				sent = true;
			});

			var page = new ContentPage { IsBusy = true, IsPlatformEnabled = true };

			Assert.False(sent);

			_ = new TestWindow(page);

			Assert.True(sent, "Busy message not sent when visible");
		}

		[Fact]
		public void BusySentWhenBusyPageDisappears()
		{
			var page = new ContentPage { IsBusy = true };
			_ = new TestWindow(page);
			((IPageController)page).SendAppearing();

			var sent = false;
			MessagingCenter.Subscribe<Page, bool>(this, Page.BusySetSignalName, (p, b) =>
			{
				Assert.False(b);
				sent = true;
			});

			((IPageController)page).SendDisappearing();

			Assert.True(sent, "Busy message not sent when visible");
		}

		[Fact]
		public void BusySentWhenVisiblePageSetToBusy()
		{
			var sent = false;
			MessagingCenter.Subscribe<Page, bool>(this, Page.BusySetSignalName, (p, b) => sent = true);

			var page = new ContentPage();
			_ = new TestWindow(page);
			((IPageController)page).SendAppearing();

			Assert.False(sent, "Busy message sent appearing while not busy");

			page.IsBusy = true;

			Assert.True(sent, "Busy message not sent when visible");
		}

		[Fact]
		public void DisplayAlert()
		{
			var page = new ContentPage() { IsPlatformEnabled = true };

			AlertArguments args = null;
			MessagingCenter.Subscribe(this, Page.AlertSignalName, (Page sender, AlertArguments e) => args = e);

			var task = page.DisplayAlert("Title", "Message", "Accept", "Cancel");

			Assert.Equal("Title", args.Title);
			Assert.Equal("Message", args.Message);
			Assert.Equal("Accept", args.Accept);
			Assert.Equal("Cancel", args.Cancel);

			bool completed = false;
			var continueTask = task.ContinueWith(t => completed = true);

			args.SetResult(true);
			continueTask.Wait();
			Assert.True(completed);
		}

		[Fact]
		public void DisplayActionSheet()
		{
			var page = new ContentPage() { IsPlatformEnabled = true };

			ActionSheetArguments args = null;
			MessagingCenter.Subscribe(this, Page.ActionSheetSignalName, (Page sender, ActionSheetArguments e) => args = e);

			var task = page.DisplayActionSheet("Title", "Cancel", "Destruction", "Other 1", "Other 2");

			Assert.Equal("Title", args.Title);
			Assert.Equal("Destruction", args.Destruction);
			Assert.Equal("Cancel", args.Cancel);
			Assert.Equal("Other 1", args.Buttons.First());
			Assert.Equal("Other 2", args.Buttons.Skip(1).First());

			bool completed = false;
			var continueTask = task.ContinueWith(t => completed = true);

			args.SetResult("Cancel");
			continueTask.Wait();
			Assert.True(completed);
		}

		class PageTestApp : Application { }

		[Fact]
		public void SendApplicationPageAppearing()
		{
			var app = new PageTestApp();
			var page = new ContentPage();

			Page actual = null;
			app.LoadPage(page);
			app.PageAppearing += (sender, args) => actual = args;

			((IPageController)page).SendDisappearing();
			((IPageController)page).SendAppearing();

			Assert.Same(page, actual);
		}

		[Fact]
		public void SendApplicationPageDisappearing()
		{
			var app = new PageTestApp();
			var page = new ContentPage();

			Page actual = null;
			app.LoadPage(page);
			app.PageDisappearing += (sender, args) => actual = args;

			((IPageController)page).SendAppearing();
			((IPageController)page).SendDisappearing();

			Assert.Same(page, actual);
		}

		[Fact]
		public void SendAppearing()
		{
			var page = new ContentPage();

			bool sent = false;
			page.Appearing += (sender, args) => sent = true;

			_ = new TestWindow(page);

			Assert.True(sent);
		}

		[Fact]
		public void SendDisappearing()
		{
			var page = new ContentPage();
			_ = new TestWindow(page);

			((IPageController)page).SendAppearing();

			bool sent = false;
			page.Disappearing += (sender, args) => sent = true;

			((IPageController)page).SendDisappearing();

			Assert.True(sent);
		}

		[Fact]
		public void SendAppearingDoesntGetCalledMultipleTimes()
		{
			var page = new ContentPage();

			int countAppearing = 0;
			page.Appearing += (sender, args) => countAppearing++;

			_ = new TestWindow(page);
			((IPageController)page).SendAppearing();

			Assert.Equal(1, countAppearing);
		}

		[Fact]
		public void IsVisibleWorks()
		{
			var page = new ContentPage();
			page.IsVisible = false;
			Assert.False(page.IsVisible);
		}

		[Fact]
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

			_ = new TestWindow(navPage);

			Assert.True(sentNav);
			Assert.True(sent);

		}

		[Fact]
		public void SendDisappearingToChildrenPageFirst()
		{
			var page = new ContentPage();

			var navPage = new NavigationPage(page);
			_ = new TestWindow(navPage);
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

		[Fact]
		public void LogicalChildrenDontAddToPagesInternalChildren()
		{
			var page = new ContentPage()
			{
				Content = new VerticalStackLayout()
			};

			var window = new TestWindow(page);

			var customControl = new VerticalStackLayout();
			Shell.SetTitleView(page, new VerticalStackLayout());
			page.AddLogicalChild(customControl);

			Assert.Equal(window, customControl.Window);
			Assert.Single(page.InternalChildren);
			Assert.Contains(customControl, page.LogicalChildrenInternal);
			Assert.Contains(customControl, ((IVisualTreeElement)page).GetVisualChildren());
		}

		[Fact]
		public void MeasureInvalidatedPropagatesUpTree()
		{
			var label = new Label()
			{
				IsPlatformEnabled = true
			};

			var scrollView = new ScrollViewInvalidationMeasureCheck()
			{
				Content = new VerticalStackLayout()
				{
					Children = { new ContentView { Content = label, IsPlatformEnabled = true } },
					IsPlatformEnabled = true
				},
				IsPlatformEnabled = true
			};

			var page = new InvalidatePageInvalidateMeasureCheck()
			{
				Content = scrollView
			};

			var window = new TestWindow(page);

			int fired = 0;
			page.MeasureInvalidated += (sender, args) =>
			{
				fired++;
			};

			page.InvalidateMeasureCount = 0;
			scrollView.InvalidateMeasureCount = 0;
			label.InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
			Assert.Equal(1, fired);
			Assert.Equal(0, page.InvalidateMeasureCount);
			Assert.Equal(0, scrollView.InvalidateMeasureCount);
			page.Content.InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
			Assert.Equal(1, page.InvalidateMeasureCount);
		}

		class LabelInvalidateMeasureCheck : Label
		{
			public int InvalidateMeasureCount { get; set; }

			public LabelInvalidateMeasureCheck()
			{

			}

			internal override void InvalidateMeasureInternal(InvalidationEventArgs trigger)
			{
				base.InvalidateMeasureInternal(trigger);
				InvalidateMeasureCount++;
			}
		}

		class ScrollViewInvalidationMeasureCheck : ScrollView
		{
			public int InvalidateMeasureCount { get; set; }

			public ScrollViewInvalidationMeasureCheck()
			{

			}

			internal override void InvalidateMeasureInternal(InvalidationEventArgs trigger)
			{
				base.InvalidateMeasureInternal(trigger);
				InvalidateMeasureCount++;
			}
		}

		class InvalidatePageInvalidateMeasureCheck : ContentPage
		{
			public int InvalidateMeasureCount { get; set; }

			public InvalidatePageInvalidateMeasureCheck()
			{

			}

			internal override void InvalidateMeasureInternal(InvalidationEventArgs trigger)
			{
				base.InvalidateMeasureInternal(trigger);
				InvalidateMeasureCount++;
			}
		}
	}
}