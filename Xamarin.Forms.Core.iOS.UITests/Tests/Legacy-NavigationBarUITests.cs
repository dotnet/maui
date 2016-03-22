using NUnit.Framework;
using Xamarin.UITest;
using System;
using System.Threading;
using Xamarin.UITest.Queries;
using System.Diagnostics;

namespace Xamarin.Forms.Core.UITests
{
	//[TestFixture]
	//[Category ("NavigationPage")]
	internal class NavigationBarGalleryTests : BaseTestFixture
	{
		// TODO: Port to new conventions

		public NavigationBarGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			//App.NavigateToGallery (GalleryQueries.NavigationBarGalleryLegacy);
		}

		//[Test]
		//[Category ("ManualReview")]
		//[Description ("Change NavigationPage Bar colors")]
		public void NavigationBarGalleryTestsChangeColors ()
		{
		//	App.Screenshot ("Background should be green, Text and back button / arrow should be yellow");

		//	App.Tap (q => q.Button ("Change BarTextColor"));
		//	App.Screenshot ("Text / back button / arrow should change to teal");
		//	App.Tap (q => q.Button ("Change BarTextColor"));
		//	App.Screenshot ("Text / back button / arrow should change to Color.Default");
		//	App.Tap (q => q.Button ("Change BarTextColor"));
		//	App.Screenshot ("Text / back button / arrow should change to teal");

		//	App.Tap (q => q.Button ("Change BarBackgroundColor"));
		//	App.Screenshot ("Background color should change to navy");
		//	App.Tap (q => q.Button ("Change BarBackgroundColor"));
		//	App.Screenshot ("Background color should change to Color.Default");
		//	App.Tap (q => q.Button ("Change BarBackgroundColor"));
		//	App.Screenshot ("Background color should change to navy");

		//	App.Tap (q => q.Button ("Change Both to default"));
		//	App.Screenshot ("Background color / text / back button / arrow should change to Color.Default");

		//	App.Tap (q => q.Button ("Make sure Tint still works"));
		//	App.Screenshot ("Background arrow should change to red");

		//	App.Tap (q => q.Button ("Black background, white text"));
		//	App.Screenshot ("Status bar should be white on iOS");
		}
	}
}
