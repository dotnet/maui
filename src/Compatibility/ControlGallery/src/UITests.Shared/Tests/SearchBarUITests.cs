using Microsoft.Maui.Controls.CustomAttributes;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Compatibility.UITests
{
	[TestFixture]
	[Category(UITestCategories.SearchBar)]
	internal class SearchBarUITests : _ViewUITests
	{
		public SearchBarUITests()
		{
			PlatformViewType = Views.SearchBar;
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.SearchBarGallery);
		}

		// TODO
		public override void _Focus() { }

		[UiTestExempt(ExemptReason.CannotTest, "Invalid interaction")]
		public override void _GestureRecognizers() { }

		// TODO
		public override void _IsFocused() { }

		// TODO
		public override void _UnFocus() { }

		// TODO
		// Implement control specific ui tests
		protected override void FixtureTeardown()
		{
			App.NavigateBack();
			base.FixtureTeardown();
		}

#if __ANDROID__ || __IOS__
		[Ignore("This is covered by the platform tests")]
		public override void _Opacity() { }
#endif

#if __ANDROID__ || __IOS__ || WINDOWS
		[Ignore("This is covered by the platform tests")]
		public override void _IsEnabled() { }
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
#else
		[FailsOnMauiAndroid]
		public override void _Scale() { }
#endif

#if __ANDROID__ || __IOS__
		[Ignore("This is covered by the platform tests")]
		public override void _IsVisible() { }
#endif
	}
}