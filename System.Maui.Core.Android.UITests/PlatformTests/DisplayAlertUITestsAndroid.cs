using System;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category(UITestCategories.DisplayAlert)]
	internal class DisplayAlertUITestsAndroid : BaseTestFixture
	{

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.DisplayAlertGallery);
		}
			
		[Test]
		public void TestTapOff ()
		{
			App.Tap (c => c.Marked ("Alert Override2"));
			App.Screenshot ("Display Alert");
			App.TapCoordinates (100, 100);
			App.WaitForElement (c => c.Marked ("Result: False"));
			App.Screenshot ("Alert Dismissed");
		}
	}
}

