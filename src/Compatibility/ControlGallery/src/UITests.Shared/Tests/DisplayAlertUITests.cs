using NUnit.Framework;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category(UITestCategories.DisplayAlert)]
	internal class DisplayAlertUITests : BaseTestFixture
	{
		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.DisplayAlertGallery);
		}

		[Test]
		public void TestWithCancelButton()
		{
			App.Tap(c => c.Marked("Alert Override1"));
			App.Screenshot("Display Alert");
			App.WaitForElement(c => c.Marked("TheAlertTitle"));
			App.WaitForElement(c => c.Marked("TheAlertMessage"));
			App.WaitForElement(c => c.Marked("TheCancelButton"));
			App.Screenshot("Display Alert Closed");
			App.Tap(c => c.Marked("TheCancelButton"));
			App.WaitForNoElement(c => c.Marked("TheAlertTitle"));
		}

		[Test]
		public void TestWithCancelAndOkButton()
		{
			App.Tap(c => c.Marked("Alert Override2"));
			App.Screenshot("Display Alert");
			App.WaitForElement(c => c.Marked("TheAlertTitle"));
			App.WaitForElement(c => c.Marked("TheAlertMessage"));
			App.WaitForElement(c => c.Marked("TheAcceptButton"));
			App.WaitForElement(c => c.Marked("TheCancelButton"));
			App.Tap(c => c.Marked("TheCancelButton"));
			App.Screenshot("Display Alert Closed");
			App.WaitForNoElement(c => c.Marked("TheAlertTitle"));
		}

		[Test]
		public void TestOkAndCancelResults()
		{
			App.Tap(c => c.Marked("Alert Override2"));
			App.Screenshot("Display Alert");
			App.WaitForElement(c => c.Marked("TheCancelButton"));
			App.Tap(c => c.Marked("TheCancelButton"));
			App.Screenshot("Display Alert Closed with cancel");
			App.WaitForElement(c => c.Marked("Result: False"));
			App.Tap(c => c.Marked("test2"));
			App.Screenshot("Display Alert");
			App.WaitForElement(c => c.Marked("TheAcceptButton"));
			App.Tap(c => c.Marked("TheAcceptButton"));
			App.Screenshot("Display Alert Closed with True");
			App.WaitForElement(c => c.Marked("Result: True"));
		}
	}
}