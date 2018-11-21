using NUnit.Framework;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Core.UITests
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

		[Category(UITestCategories.UwpIgnore)]
		public override void _IsEnabled()
		{
			base._IsEnabled();
		} 

		 
		[Category(UITestCategories.UwpIgnore)] 
		public override void _IsVisible()
		{
			base._IsVisible();
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