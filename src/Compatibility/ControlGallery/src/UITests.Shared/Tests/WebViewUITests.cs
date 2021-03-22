using NUnit.Framework;
using Microsoft.Maui.Controls.CustomAttributes;

namespace Microsoft.Maui.Controls.Compatibility.UITests
{
	[TestFixture]
	[Category(UITestCategories.WebView)]
	internal class WebViewUITests : _ViewUITests
	{
		public WebViewUITests()
		{
			PlatformViewType = Views.WebView;
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.WebViewGallery);
		}

		[Category(UITestCategories.ManualReview)]
		public override void _IsEnabled()
		{
			Assert.Inconclusive("Does not make sense for WebView");
		}

		[Test]
		[Category(UITestCategories.ManualReview)]
		[Ignore("Keep empty test from failing in Test Cloud")]
		public override void _IsVisible()
		{
		}

		[UiTestExempt(ExemptReason.CannotTest, "Invalid interaction with Label")]
		public override void _Focus()
		{
		}

		// TODO
		public override void _GestureRecognizers()
		{
		}

		[UiTestExempt(ExemptReason.CannotTest, "Invalid interaction with Label")]
		public override void _IsFocused()
		{
		}

		[Test]
		[Category(UITestCategories.ManualReview)]
		[Ignore("Keep empty test from failing in Test Cloud")]
		public override void _Opacity()
		{
		}

		[Test]
		[Category(UITestCategories.ManualReview)]
		[Ignore("Keep empty test from failing in Test Cloud")]
		public override void _Rotation()
		{
		}

		[Test]
		[Category(UITestCategories.ManualReview)]
		[Ignore("Keep empty test from failing in Test Cloud")]
		public override void _RotationX()
		{
		}

		[Test]
		[Category(UITestCategories.ManualReview)]
		[Ignore("Keep empty test from failing in Test Cloud")]
		public override void _RotationY()
		{
		}

		[Test]
		[Category(UITestCategories.ManualReview)]
		[Ignore("Keep empty test from failing in Test Cloud")]
		public override void _TranslationX()
		{
		}

		[Test]
		[Category(UITestCategories.ManualReview)]
		[Ignore("Keep empty test from failing in Test Cloud")]
		public override void _TranslationY()
		{
		}

		[Test]
		[Category(UITestCategories.ManualReview)]
		[Ignore("Keep empty test from failing in Test Cloud")]
		public override void _Scale()
		{
		}

		[UiTestExempt(ExemptReason.CannotTest, "Invalid interaction with Label")]
		public override void _UnFocus()
		{
		}

		// TODO
		// Implement control specific ui tests

		protected override void FixtureTeardown()
		{
			App.NavigateBack();
			base.FixtureTeardown();
		}
	}
}