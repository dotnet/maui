using Microsoft.Maui.Controls.CustomAttributes;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Compatibility.UITests
{
	[TestFixture]
	[Category(UITestCategories.Stepper)]
	internal class StepperUITests : _ViewUITests
	{
		public StepperUITests()
		{
			PlatformViewType = Views.Stepper;
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.StepperGallery);
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

#if __ANDROID__ || __IOS__ || WINDOWS
		[Ignore("This is covered by the platform tests")]
#endif
		[Category(UITestCategories.UwpIgnore)]
		public override void _IsEnabled()
		{
			base._IsEnabled();
		}


#if __ANDROID__ || __IOS__
		[Ignore("This is covered by the platform tests")]
		public override void _IsVisible() { }
#else
		[Category(UITestCategories.UwpIgnore)]
		public override void _IsVisible()
		{
			base._IsVisible();
		}
#endif

		// TODO
		// Implement control specific ui tests
		protected override void FixtureTeardown()
		{
			App.NavigateBack();
			base.FixtureTeardown();
		}

#if __ANDROID__ || __IOS__
		[Ignore("This is covered by the platform opacity tests")]
		public override void _Opacity() { }
#endif

#if __ANDROID__ || __IOS__ || WINDOWS
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

#if __IOS__ || WINDOWS
		[Ignore("This is covered by the platform tests")]
		public override void _Scale() { }
#endif


	}
}