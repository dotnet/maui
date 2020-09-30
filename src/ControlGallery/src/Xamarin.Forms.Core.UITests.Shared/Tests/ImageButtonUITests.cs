using System.Threading;
using NUnit.Framework;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category(UITestCategories.Image)]
	internal class ImageButtonUITests : _ViewUITests
	{
		public ImageButtonUITests()
		{
			PlatformViewType = Views.ImageButton;
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.ImageButtonGallery);

			// let remote images load
			Thread.Sleep(2000);
		}

		[UiTestExempt(ExemptReason.CannotTest, "Invalid interaction")]
		public override void _Focus()
		{
		}

		// TODO
		public override void _GestureRecognizers()
		{
		}

		[UiTestExempt(ExemptReason.CannotTest, "Invalid interaction")]
		public override void _IsFocused()
		{
		}

		[UiTestExempt(ExemptReason.CannotTest, "Invalid interaction")]
		public override void _UnFocus()
		{
		}

		// TODO
		// Implement control specific ui tests

		// TODO
		// Tests for remote images

		protected override void FixtureTeardown()
		{
			App.NavigateBack();
			base.FixtureTeardown();
		}




		// ImageButton Tests
		[Test]
		[UiTest(typeof(ImageButton), "BorderColor")]
		[UiTestBroken(BrokenReason.CalabashAndroidUnsupported, "Figure out how to get Android Drawables")]
		[UiTestBroken(BrokenReason.CalabashiOSUnsupported, "iOS nil result")]
		public void BorderColor()
		{
			//TODO iOS
			var remote = new ViewContainerRemote(App, Test.ImageButton.BorderColor, PlatformViewType);
			remote.GoTo();
		}

		[Test]
		[UiTest(typeof(ImageButton), "BorderWidth")]
		[UiTestBroken(BrokenReason.CalabashAndroidUnsupported, "Figure out how to get Android Drawables")]
		public void BorderWidth()
		{
			var remote = new ViewContainerRemote(App, Test.ImageButton.BorderWidth, PlatformViewType);
			remote.GoTo();

#if __IOS__
			var borderWidth = remote.GetProperty<float>(ImageButton.BorderWidthProperty);
			Assert.AreEqual(15.0f, borderWidth);
#endif
		}

		[Test]
		[UiTest(typeof(ImageButton), "Clicked")]
		[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
		public void Clicked()
		{
			var remote = new EventViewContainerRemote(App, Test.ImageButton.Clicked, PlatformViewType);
			remote.GoTo();

			var textBeforeClick = remote.GetEventLabel().Text;
			Assert.AreEqual("Event: Clicked (none)", textBeforeClick);

			// Click ImageButton
			remote.TapView();

			var textAfterClick = remote.GetEventLabel().Text;
			Assert.AreEqual("Event: Clicked (fired 1)", textAfterClick);
		}

		[Test]
		[UiTest(typeof(ImageButton), "Pressed")]
		[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
		public void Pressed()
		{
			var remote = new EventViewContainerRemote(App, Test.ImageButton.Pressed, PlatformViewType);
			remote.GoTo();

			var textBeforeClick = remote.GetEventLabel().Text;
			Assert.AreEqual("Event: Pressed (none)", textBeforeClick);

			// Press ImageButton
			remote.TouchAndHoldView();

			var textAfterClick = remote.GetEventLabel().Text;
			Assert.AreEqual("Event: Pressed (fired 1)", textAfterClick);
		}

		[Test]
		[UiTest(typeof(ImageButton), "Command")]
		public void Command()
		{
			var remote = new ViewContainerRemote(App, Test.ImageButton.Command, PlatformViewType);
			remote.GoTo();

			remote.TapView();

			App.WaitForElement(q => q.Marked("Hello Command"));
			App.Tap(q => q.Marked("Destroy"));
		}

		[Test]
		[UiTest(typeof(ImageButton), "CornerRadius")]
		[UiTestBroken(BrokenReason.CalabashAndroidUnsupported, "Figure out how to get Android Drawables")]
		public void CornerRadius()
		{
			var remote = new ViewContainerRemote(App, Test.ImageButton.CornerRadius, PlatformViewType);
			remote.GoTo();

#if __IOS__
			var cornerRadius = remote.GetProperty<float>(ImageButton.CornerRadiusProperty);
			Assert.AreEqual(20.0f, cornerRadius);
#endif
		}

		[Test]
		[UiTest(typeof(ImageButton), "Image")]
		[UiTestExempt(ExemptReason.TimeConsuming, "Need way to check Android resources")]
		public void Image()
		{
			//TODO iOS
			var remote = new ViewContainerRemote(App, Test.ImageButton.Image, PlatformViewType);
			remote.GoTo();
		}

#if __ANDROID__ || __IOS__
		[Ignore("This is covered by the platform tests")]
		public override void _Opacity() { }
#endif

#if __ANDROID__ || __IOS__ || __WINDOWS__
		[Ignore("This is covered by the platform tests")]
		public override void _IsEnabled() { }
#endif

#if __ANDROID__ || __IOS__ || __WINDOWS__
		[Ignore("This is covered by the platform tests")]
		public override void _Rotation() { }

		[Ignore("This is covered by the platform tests")]
		public override void _RotationX() { }

		[Ignore("This is covered by the platform tests")]
		public override void _RotationY() { }
#endif

#if __ANDROID__
		[Ignore("This is covered by the platform tests")]
		public override void _TranslationX() { }

		[Ignore("This is covered by the platform tests")]
		public override void _TranslationY() { }
#endif

#if __IOS__ || __WINDOWS__
		[Ignore("This is covered by the platform tests")]
		public override void _Scale() { }
#endif

#if __ANDROID__ || __IOS__
		[Ignore("This is covered by the platform tests")]
		public override void _IsVisible() { }
#endif
	}
}