using NUnit.Framework;
using Xamarin.UITest;
using System;
using System.Threading;


namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("DisplayAlert")]
	internal class DisplayAlertUITests : BaseTestFixture
	{

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.DisplayAlertGallery);
		}

		[Test]
		public void TestWithCancelButton ()
		{
		
			App.Tap (c => c.Marked ("Alert Override1"));
			App.Screenshot ("Display Alert");
			App.WaitForElement (c => c.Marked ("TheAlertTitle"));
			App.WaitForElement (c => c.Marked ("TheAlertMessage"));
			App.WaitForElement (c => c.Marked ("TheCancelButton"));
			App.Screenshot ("Display Alert Closed");
			App.Tap (c => c.Marked ("TheCancelButton"));
			App.WaitForNoElement (c => c.Marked ("TheAlertTitle"));
		}

		[Test]
		public void TestWithCancelAndOkButton ()
		{
			App.Tap (c => c.Marked ("Alert Override2"));
			App.Screenshot ("Display Alert");
			App.WaitForElement (c => c.Marked ("TheAlertTitle"));
			App.WaitForElement (c => c.Marked ("TheAlertMessage"));
			App.WaitForElement (c => c.Marked ("TheAcceptButton"));
			App.WaitForElement (c => c.Marked ("TheCancelButton"));
			App.Tap (c => c.Marked ("TheCancelButton"));
			App.Screenshot ("Display Alert Closed");
			App.WaitForNoElement (c => c.Marked ("TheAlertTitle"));
		}

		[Test]
		public void TestOkAndCancelResults ()
		{
			App.Tap (c => c.Marked ("Alert Override2"));
			App.Screenshot ("Display Alert");
			App.WaitForElement (c => c.Marked ("TheCancelButton"));
			App.Tap (c => c.Marked ("TheCancelButton"));
			App.Screenshot ("Display Alert Closed with cancel");
			App.WaitForElement (c => c.Marked ("Result: False"));
			App.Tap (c => c.Marked ("test2"));
			App.Screenshot ("Display Alert");
			App.WaitForElement (c => c.Marked ("TheAcceptButton"));
			App.Tap (c => c.Marked ("TheAcceptButton"));
			App.Screenshot ("Display Alert Closed with True");
			App.WaitForElement (c => c.Marked ("Result: True"));
		}
			
//		[Test]
//		public void TestRotation ()
//		{
//			App.SetOrientationLandscape ();
//
//			//App.Tap (c => c.Marked ("Alert Override1"));
//			// 27-01-2015 14:30:02.125 -08:00 - 28524 - Tapping first element (2 total) matching Marked("Alert Override1") at coordinates [ 512, 113 ]. 
//
//			App.TapCoordinates (665, 512);
//
//			App.WaitForElement (c => c.Marked ("TheAlertTitle"));
//
//			App.Screenshot ("Display Alert After Rotation");
//			App.WaitForElement (c => c.Marked ("TheAlertTitle"));
//			App.Tap (c => c.Marked("TheCancelButton"));
//			App.WaitForNoElement (c => c.Marked("TheCancelButton"));
//			App.SetOrientationPortrait ();
//
//		}

	}
}
