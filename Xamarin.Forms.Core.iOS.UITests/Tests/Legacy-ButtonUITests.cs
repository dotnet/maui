using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UITests
{
	// Run these to test in CI before full suite
	[TestFixture]
	[Category ("Button")]
	internal class ButtonGalleryTests : BaseTestFixture
	{
		// TODO: Rotate Button - test rotation
		// TODO: Port to new conventions

		public ButtonGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.ButtonGalleryLegacy);
		}

		[Test]
		[Description ("Disabled Button")]
		public void ButtonGalleryOnOffDisbledButton ()
		{
			App.Screenshot ("At Gallery");

		//	App.ScrollDownForElement (q => q.Marked ("Cannot Tap"), 3);
		//	App.Tap (q => q.Button ("Disabled Button"));
		//	Assert.AreEqual (1, App.Query (q => q.Button ("Disabled Button")).Length);
		//	App.Screenshot ("Tried to tap disabled button");
		//	App.Tap (PlatformQueries.Switch);
		//	App.Screenshot ("Tapped switch");
		//	App.Tap (q => q.Button ("Disabled Button"));
		//	App.WaitForElement (q => q.Marked ("TAPPED!"));
		//	App.Screenshot ("Disabled button should not be enabled and change labels text");
		}

		//[Test]
		//[UiTest (Test.Device.StartTimer)]
		//[Description ("Clicking the 'Timer Button'")]
		//public void ButtonGalleryTimerButton ()
		//{
		//	App.ScrollDownForElement (q => q.Marked ("Timer"), 10);
		//	App.Screenshot ("Press 'Timer' Button");

		//	App.Tap (q => q.Button ("Timer"));

		//	App.WaitForElement (q => q.Marked ("Timer Elapsed 3"), "Timeout : Timer Elapsed 3");
		//	App.Screenshot ("Timer button elapsed 3");
		//}

		//[Test]
		//[UiTest (Test.Page.DisplayAlert)]
		//public void ButtonGalleryAlertAccepted ()
		//{
		//	App.ScrollDownForElement (q => q.Marked ("Alert"), 10);

		//	App.Screenshot ("Press 'Alert' Button");

		//	App.Tap (q => q.Marked ("Alert"));
		//	App.WaitForElement (q => q.Marked ("Accept"), "Timeout : Accept");

		//	App.Screenshot ("Press 'Accept' or 'Cancel'");

		//	App.Tap (q => q.Marked ("Accept"));
		//	App.WaitForElement (q => q.Button ("Accepted"), "Timeout : Accepted");

		//	App.Screenshot ("See 'Accepted' or 'Cancelled'");
		//}


		//[Test]
		//[UiTest (Test.Page.DisplayAlert)]
		//public void ButtonGalleryAlertCancelled ()
		//{
		//	App.ScrollDownForElement (q => q.Marked ("Alert"), 10);

		//	App.Screenshot ("Press 'Alert' Button");

		//	App.Tap (q => q.Marked ("Alert"));
		//	App.WaitForElement (q => q.Marked ("Cancel"), "Timeout : Cancel");

		//	App.Screenshot ("Press 'Accept' or 'Cancel'");

		//	App.Tap (q => q.Marked ("Cancel"));
		//	App.WaitForElement (q => q.Button ("Cancelled"), "Timeout : Cancelled");

		//	App.Screenshot ("See 'Accepted' or 'Cancelled'");
		//}
	}
}
