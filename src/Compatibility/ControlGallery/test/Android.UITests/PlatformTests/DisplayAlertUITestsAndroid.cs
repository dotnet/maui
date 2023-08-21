//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Compatibility.UITests
{
	[TestFixture]
	[Category(UITestCategories.DisplayAlert)]
	internal class DisplayAlertUITestsAndroid : BaseTestFixture
	{

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.DisplayAlertGallery);
		}

		[Test]
		public void TestTapOff()
		{
			App.Tap(c => c.Marked("Alert Override2"));
			App.Screenshot("Display Alert");
			App.TapCoordinates(100, 100);
			App.WaitForElement(c => c.Marked("Result: False"));
			App.Screenshot("Alert Dismissed");
		}
	}
}

