using System;
using NUnit.Framework;
using Xamarin.Forms.CustomAttributes;
using Xamarin.UITest.Android;
using Xamarin.UITest.iOS;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("ToolbarItem")]
	internal class ToolbarItemTests : BaseTestFixture
	{
		string btn1Id = "tb1";
		string btn4Id = "tb4";

		static bool isSecondaryMenuOpen = false;
		static void ShouldShowMenu ()
		{
			if (App is AndroidApp) {
				isSecondaryMenuOpen = true;
				//show secondary menu
				App.Tap (c => c.Class ("android.support.v7.widget.ActionMenuPresenter$OverflowMenuButton"));
			}
		}

		static void ShouldHideMenu ()
		{
			if (App is AndroidApp && isSecondaryMenuOpen) {
				isSecondaryMenuOpen = false;
				App.Back ();	
			}
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.ToolbarItemGallery);
			if (App is iOSApp) {
				btn1Id = "menuIcon";
				btn4Id = "tb4";
			}
		}

		[Test]
		public void ToolbarButtonsClick ()
		{
			ShouldHideMenu ();
			App.Tap (c => c.Marked (btn1Id));
		}

		[Test]
		public void ToolbarButtonsCommand ()
		{
			ShouldShowMenu ();
			if (App is AndroidApp) {
				//App.Query (c => c.Marked (btn4Id))[0];
			}
			else {
				App.Tap (c => c.Marked (btn4Id));
			}
		}

		[Test]
		public void ToolbarButtonsDisable ()
		{
			ShouldHideMenu ();
			var btn1 = App.Query (c => c.Marked (btn1Id)) [0];
			ShouldShowMenu ();
			//var btn2 = App.Query (c => c.Marked (btn4Id)) [0];
			Assert.False (btn1.Enabled, "Toolbar Item  should be disable");
			//TODO: how to check Enable for the textview
			//Assert.False (btn2.Enabled, "Toolbar Item  should be disable");
		}

		[Test]
		public void ToolbarButtonsExist ()
		{
			ShouldHideMenu ();
			var existsPrimary = App.Query (c => c.Marked (btn1Id)).Length;
			var existsPrimary2 = App.Query (c => c.Marked ("tb2")).Length;
			ShouldShowMenu ();
			var existsSecondary = App.Query (c => c.Marked ("tb3")).Length;
			var existsSecondary2 = App.Query (c => c.Marked (btn4Id)).Length;
			Assert.True (existsPrimary > 0, "Toolbar Item 1 no name, not found");
			Assert.True (existsPrimary2 > 0, "Toolbar Item 2, not found");
			Assert.True (existsSecondary > 0, "Toolbar Item 1 no name, not found");
			Assert.True (existsSecondary2 > 0, "Toolbar Item 1, not found");
		}

		[Test]
		public void ToolbarButtonsOrder ()
		{
			ShouldHideMenu ();
			var btn1 = App.Query (c => c.Marked (btn1Id)) [0];
			ShouldShowMenu ();
			var btn2 = App.Query (c => c.Marked ("tb4")) [0];
			if(App is iOSApp)
				Assert.True (btn1.Rect.CenterY < btn2.Rect.CenterY);
		}

	}
}

