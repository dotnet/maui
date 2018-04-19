using NUnit.Framework;
using Xamarin.Forms.Controls;
using Xamarin.Forms.CustomAttributes;

using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category(UITestCategories.ToolbarItem)]
	internal class ToolbarItemTests : BaseTestFixture
	{
		string btn1Id = "tb1";
		string btn4Id = "tb4";
#if __ANDROID__
		static bool isSecondaryMenuOpen = false;
#endif
		static void ShouldShowMenu()
		{
#if __ANDROID__
			isSecondaryMenuOpen = true;
			//show secondary menu
			App.Tap(c => c.Class("OverflowMenuButton"));
#endif
		}

		static void ShouldHideMenu()
		{
#if __ANDROID__
			if (isSecondaryMenuOpen)
			{
				isSecondaryMenuOpen = false;
				App.Back();
			}
#endif
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.ToolbarItemGallery);
#if __IOS__
			btn1Id = "menuIcon";
			btn4Id = "tb4";
			if (AppSetup.iOSVersion  >= 9)
			{
				btn1Id = "toolbaritem_primary";
				btn4Id = "toolbaritem_secondary2";
			}
#endif
		}

		[Test]
		public void ToolbarButtonsClick()
		{
			ShouldHideMenu();
#if __MACOS__
			App.Tap(c => c.Button().Index(4));
#else
			App.WaitForElement(btn1Id);
			App.Tap(c => c.Marked(btn1Id));
#endif
			var textLabel = App.Query((arg) => arg.Marked("label_id"))[0];
			Assert.False(textLabel.Text == "tb1");
			Assert.True(textLabel.Text == "Hello ContentPage");
		}

		[Test]
		public void ToolbarButtonsCommand()
		{
			ShouldShowMenu();
#if __ANDROID__
			//App.Query (c => c.Marked (btn4Id))[0];
#else
			App.WaitForElement(btn4Id);
			App.Tap(c => c.Marked(btn4Id));
			App.WaitForNoElement(c => c.Text("button 4 new text"));
#if __MACOS__
			App.Tap(c => c.Button().Index(6));
#else
			App.Tap(c => c.Marked("tb3"));
#endif
			App.Tap(c => c.Marked(btn4Id));
			App.WaitForElement(c => c.Text("button 4 new text"));
#if __MACOS__
			App.Tap(c => c.Button().Index(6));
#else
			App.Tap(c => c.Marked("tb3"));
#endif
#endif
		}

		[Test]
		public void ToolbarButtonsDisable()
		{
			ShouldHideMenu();
#if __MACOS__
			var result = App.Query(c => c.Button());
			var btn1 = result[4];
			var btn2 = App.Query(c => c.Marked(btn4Id))[0];
			Assert.False(btn2.Enabled, "Toolbar Item  should be disable");
#else
			var btn1 = App.Query(c => c.Marked(btn1Id))[0];
			ShouldShowMenu();
			//var btn2 = App.Query (c => c.Marked (btn4Id)) [0];		
			//TODO: how to check Enable for the textview
			//Assert.False (btn2.Enabled, "Toolbar Item  should be disable");
#endif
			Assert.False(btn1.Enabled, "Toolbar Item  should be disable");
		}

		[Test]
		public void ToolbarButtons_1_Exist()
		{
			ShouldHideMenu();
#if __MACOS__
			var existsPrimary = App.Query(c => c.Button())[4];
			Assert.True(existsPrimary != null, "Toolbar Item 1 no name, not found");
#else
			var existsPrimary = App.Query(c => c.Marked(btn1Id)).Length;
			Assert.True(existsPrimary > 0, "Toolbar Item 1 no name, not found");
#endif
			var existsPrimary2 = App.Query(c => c.Marked("tb2")).Length;
			Assert.True(existsPrimary2 > 0, "Toolbar Item 2, not found");
			ShouldShowMenu();

#if __MACOS__
			var existsSecondary = App.Query(c => c.Button())[7];
			Assert.True(existsSecondary != null, "Toolbar Item 3 no name, not found");
#else
			var existsSecondary = App.Query(c => c.Marked("tb3")).Length;
			Assert.True(existsSecondary > 0, "Toolbar Item 1 no name, not found");
#endif
			var existsSecondary2 = App.Query(c => c.Marked(btn4Id)).Length;
			Assert.True(existsSecondary2 > 0, "Toolbar Item 4, not found");
		}

		[Test]
		public void ToolbarButtons_2_Order()
		{
			ShouldHideMenu();
#if __MACOS__
			var btn1 = App.Query(c => c.Button())[4];
#else
			var btn1 = App.Query(c => c.Marked(btn1Id))[0];
#endif
			ShouldShowMenu();
			var btn2 = App.Query(c => c.Marked("tb4"))[0];
#if __IOS__
			Assert.True(btn1.Rect.CenterY < btn2.Rect.CenterY);
#elif __MACOS__
			Assert.True(btn1.Rect.CenterX < btn2.Rect.CenterX);
#endif
		}

	}
}

