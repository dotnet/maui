using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class FlyoutPageUnitTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			var mockDeviceInfo = new TestDeviceInfo();
			Device.Info = mockDeviceInfo;
		}

		[Test]
		public void TestConstructor()
		{

			FlyoutPage page = new FlyoutPage();

			Assert.Null(page.Flyout);
			Assert.Null(page.Detail);
		}

		[Test]
		public void TestFlyoutSetter()
		{
			FlyoutPage page = new FlyoutPage();
			var child = new ContentPage { Content = new Label(), Title = "Foo" };
			page.Flyout = child;

			Assert.AreEqual(child, page.Flyout);
		}

		[Test]
		public void TestFlyoutSetNull()
		{
			FlyoutPage page = new FlyoutPage();
			var child = new ContentPage { Content = new Label(), Title = "Foo" };
			page.Flyout = child;

			Assert.Throws<ArgumentNullException>(() => { page.Flyout = null; });
		}

		[Test]
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

		[Test]
		public void TestDetailSetter()
		{
			FlyoutPage page = new FlyoutPage();
			var child = new ContentPage { Content = new Label() };
			page.Detail = child;

			Assert.AreEqual(child, page.Detail);
		}

		[Test]
		public void TestDetailSetNull()
		{
			FlyoutPage page = new FlyoutPage();
			var child = new ContentPage { Content = new Label() };
			page.Detail = child;

			Assert.Throws<ArgumentNullException>(() => { page.Detail = null; });
		}

		[Test]
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

		[Test]
		public void ThrowsWhenFlyoutSetWithoutValidTitle([Values(null, "")] string title)
		{
			var page = new FlyoutPage();
			Assert.Throws<InvalidOperationException>(() => page.Flyout = new ContentPage { Title = title });
		}

		[Test]
		public void TestThrowsWhenPackedWithoutSetting()
		{
			FlyoutPage page = new FlyoutPage();
			Assert.Throws<InvalidOperationException>(() => new TabbedPage { Children = { page } });
		}

		[Test]
		public void TestDoesNotThrowWhenPackedWithSetting()
		{
			FlyoutPage page = new FlyoutPage
			{
				Flyout = new ContentPage { Content = new View(), Title = "Foo" },
				Detail = new ContentPage { Content = new View() }
			};
			Assert.DoesNotThrow(() => new TabbedPage { Children = { page } });
		}

		[Test]
		public void TestFlyoutVisible()
		{
			var page = new FlyoutPage();

			Assert.AreEqual(false, page.IsPresented);

			bool signaled = false;
			page.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == FlyoutPage.IsPresentedProperty.PropertyName)
					signaled = true;
			};

			page.IsPresented = true;

			Assert.AreEqual(true, page.IsPresented);
			Assert.True(signaled);
		}

		[Test]
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

		[Test]
		public void TestSetFlyoutBounds()
		{
			var page = new FlyoutPage
			{
				Flyout = new ContentPage { Content = new View(), Title = "Foo" },
				Detail = new ContentPage { Content = new View() }
			};

			((IFlyoutPageController)page).FlyoutBounds = new Rectangle(0, 0, 100, 100);
			Assert.AreEqual(new Rectangle(0, 0, 100, 100), page.Flyout.Bounds);
			Assert.AreEqual(new Rectangle(0, 0, 100, 100), ((IFlyoutPageController)page).FlyoutBounds);
		}

		[Test]
		public void TestSetDetailBounds()
		{
			var page = new FlyoutPage
			{
				Flyout = new ContentPage { Content = new View(), Title = "Foo" },
				Detail = new ContentPage { Content = new View() }
			};

			((IFlyoutPageController)page).DetailBounds = new Rectangle(0, 0, 100, 100);
			Assert.AreEqual(new Rectangle(0, 0, 100, 100), page.Detail.Bounds);
			Assert.AreEqual(new Rectangle(0, 0, 100, 100), ((IFlyoutPageController)page).DetailBounds);
		}

		[Test]
		public void TestLayoutChildren()
		{
			var page = new FlyoutPage
			{
				Flyout = new ContentPage { Content = new View(), IsPlatformEnabled = true, Title = "Foo" },
				Detail = new ContentPage { Content = new View(), IsPlatformEnabled = true },
				IsPlatformEnabled = true,
			};

			((IFlyoutPageController)page).FlyoutBounds = new Rectangle(0, 0, 100, 200);
			((IFlyoutPageController)page).DetailBounds = new Rectangle(0, 0, 100, 100);

			page.Flyout.Layout(new Rectangle(0, 0, 1, 1));
			page.Detail.Layout(new Rectangle(0, 0, 1, 1));

			page.Layout(new Rectangle(0, 0, 200, 200));

			Assert.AreEqual(new Rectangle(0, 0, 100, 200), page.Flyout.Bounds);
			Assert.AreEqual(new Rectangle(0, 0, 100, 100), page.Detail.Bounds);
		}

		[Test]
		public void ThorwsInLayoutChildrenWithNullDetail()
		{
			var page = new FlyoutPage
			{
				Flyout = new ContentPage { Content = new View(), IsPlatformEnabled = true, Title = "Foo" },
				IsPlatformEnabled = true,
			};

			Assert.Throws<InvalidOperationException>(() => page.Layout(new Rectangle(0, 0, 200, 200)));
		}

		[Test]
		public void ThorwsInLayoutChildrenWithNullFlyout()
		{
			var page = new FlyoutPage
			{
				Detail = new ContentPage { Content = new View(), IsPlatformEnabled = true },
				IsPlatformEnabled = true,
			};

			Assert.Throws<InvalidOperationException>(() => page.Layout(new Rectangle(0, 0, 200, 200)));
		}

		[Test]
		public void ThorwsInSetDetailBoundsWithNullDetail()
		{
			var page = new FlyoutPage
			{
				Flyout = new ContentPage { Content = new View(), Title = "Foo" },
				IsPlatformEnabled = true,
			};

			Assert.Throws<InvalidOperationException>(() => ((IFlyoutPageController)page).DetailBounds = new Rectangle(0, 0, 200, 200));
		}

		[Test]
		public void ThrowsInSetFlyoutBoundsWithNullFlyout()
		{
			var page = new FlyoutPage
			{
				Detail = new ContentPage { Content = new View() },
				IsPlatformEnabled = true,
			};

			Assert.Throws<InvalidOperationException>(() => ((IFlyoutPageController)page).FlyoutBounds = new Rectangle(0, 0, 200, 200));
		}

		[Test]
		public void ThrowsInSetIsPresentOnSplitModeOnTablet()
		{
			Device.Idiom = TargetIdiom.Tablet;
			var page = new FlyoutPage
			{
				Flyout = new ContentPage { Content = new View(), IsPlatformEnabled = true, Title = "Foo" },
				Detail = new ContentPage { Content = new View(), IsPlatformEnabled = true },
				IsPlatformEnabled = true,
				FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split
			};

			Assert.Throws<InvalidOperationException>(() => page.IsPresented = false);
		}

		[Test]
		public void ThorwsInSetIsPresentOnSplitPortraitModeOnTablet()
		{
			Device.Idiom = TargetIdiom.Tablet;
			Device.Info.CurrentOrientation = DeviceOrientation.Portrait;

			var page = new FlyoutPage
			{
				Flyout = new ContentPage { Content = new View(), IsPlatformEnabled = true, Title = "Foo" },
				Detail = new ContentPage { Content = new View(), IsPlatformEnabled = true },
				IsPlatformEnabled = true,
				FlyoutLayoutBehavior = FlyoutLayoutBehavior.SplitOnPortrait
			};

			Assert.Throws<InvalidOperationException>(() => page.IsPresented = false);
		}

		[Test]
		public void TestSetIsPresentedOnPopoverMode()
		{
			Device.Info.CurrentOrientation = DeviceOrientation.Landscape;

			var page = new FlyoutPage
			{
				Flyout = new ContentPage { Content = new View(), IsPlatformEnabled = true, Title = "Foo" },
				Detail = new ContentPage { Content = new View(), IsPlatformEnabled = true },
				IsPlatformEnabled = true,
				FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover
			};
			page.IsPresented = true;

			Assert.AreEqual(true, page.IsPresented);
		}

		[Test]
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

		[Test]
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

		[Test]
		public void ThrowsExceptionWhenAddingAlreadyParentedDetail()
		{
			var detail = new ContentPage { };

			// give detail a parent
			var nav = new NavigationPage(detail);

			var mdp = new FlyoutPage();
			Assert.Throws<InvalidOperationException>(() => mdp.Detail = detail);
		}

		[Test]
		public void ThrowsExceptionWhenAddingAlreadyParentedFlyout()
		{
			var Flyout = new ContentPage { Title = "Foo" };

			// give Flyout a parent
			var nav = new NavigationPage(Flyout);

			var mdp = new FlyoutPage();
			Assert.Throws<InvalidOperationException>(() => mdp.Flyout = Flyout);
		}
	}

}
