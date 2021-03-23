using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Compatibility.UITests
{
	[Ignore("Test only meant for Mac and iOS, besides needing API keys for Android and Windows is also failing on iOS9, should be enable later when iOS9 support is dropped")]
	[TestFixture]
	[Category(UITestCategories.Maps)]
	[Category(UITestCategories.ManualReview)]
	internal class MapUITests : BaseTestFixture
	{
		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.MapGalleryLegacy);
		}

		[Test]
		public void MapGalleryPinClicked()
		{
			App.Tap(PlatformQueries.Pin);

			App.WaitForElement(c => c.Text("Colosseum"));
			App.Tap(c => c.Text("Colosseum"));

			App.Screenshot("Alert displayed as result of PinClicked event");

			// Dismiss alert
			App.Tap(c => c.Text("OK"));
		}

		protected override void FixtureTeardown()
		{
			App.NavigateBack();
			base.FixtureTeardown();
		}
	}
}
