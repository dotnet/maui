using System.Threading;
using NUnit.Framework;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category(UITestCategories.TimePicker)]
	internal class TimePickerUITests : _ViewUITests
	{
		public TimePickerUITests()
		{
			PlatformViewType = Views.TimePicker;
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.TimePickerGallery);

			Thread.Sleep(4000);
		}

		// TODO
		public override void _Focus()
		{
		}

		[UiTestExempt(ExemptReason.CannotTest, "Invalid interaction")]
		public override void _GestureRecognizers()
		{
		}

		// TODO
		public override void _IsFocused()
		{
		}

		// TODO
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