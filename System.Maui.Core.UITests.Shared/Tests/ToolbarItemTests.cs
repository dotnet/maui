using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xamarin.Forms.Controls;
using Xamarin.Forms.CustomAttributes;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category(UITestCategories.ToolbarItem)]
	[Category(UITestCategories.UwpIgnore)]
	internal class ToolbarItemTests : BaseTestFixture
	{
		string btn1Id = "toolbaritem_primary";
		string btn2Id = "toolbaritem_primary2";
		string btn4Id = "toolbaritem_secondary2";
#if !__MACOS__
		string btn3Id = "toolbaritem_secondary";
#endif 

#if __ANDROID__
		bool isSecondaryMenuOpen()
		{
			Thread.Sleep(1000);
			var items = App.Query(btn4Id);
			return items.Length > 0;
		}
#endif
		void ShouldShowMenu()
		{
			App.TapOverflowMenuButton();
		}

		void ShouldHideMenu()
		{
#if __ANDROID__
			if (isSecondaryMenuOpen())
			{
				// slight pause in case menu hasn't quite closed
				Thread.Sleep(500);
				App.Back();
			}
#endif
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.ToolbarItemGallery);
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
			Assert.False(textLabel.Text == btn1Id);
			Assert.True(textLabel.Text == "Hello ContentPage");
		}

		[Test]
		public void ToolbarButtonsCommand()
		{
			ShouldShowMenu();

			App.WaitForElement(btn4Id);
			App.Tap(c => c.Marked(btn4Id));
			App.WaitForNoElement(c => c.Text("button 4 new text"));
#if __MACOS__
			App.Tap(c => c.Button().Index(6));
#else
			App.Tap(c => c.Marked(btn3Id));
#endif
#if __ANDROID__
			ShouldShowMenu();
#endif
			App.Tap(c => c.Marked(btn4Id));
			App.WaitForElement(c => c.Text("button 4 new text"));
#if __MACOS__
			App.Tap(c => c.Button().Index(6));
#else
#if __ANDROID__
			ShouldShowMenu();
#endif
			App.Tap(c => c.Marked(btn3Id));
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
			var btn1 = App.WaitForElement(c => c.Marked(btn1Id))[0];
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
			App.WaitForElement(c => c.Marked(btn1Id));
#endif

			App.WaitForElement(c => c.Marked(btn2Id));
			ShouldShowMenu();

#if __MACOS__
			var existsSecondary = App.Query(c => c.Button())[7];
			Assert.True(existsSecondary != null, "Toolbar Item 3 no name, not found");
#else
			App.WaitForElement(c => c.Marked(btn3Id));
#endif
			App.WaitForElement(c => c.Marked(btn4Id));
		}

		[Test]
		public void ToolbarButtons_2_Order()
		{
			ShouldHideMenu();
#if __MACOS__
			var btn1 = App.Query(c => c.Button())[4];
#else
			App.WaitForElement(c => c.Marked(btn1Id));
			var btn1 = App.Query(c => c.Marked(btn1Id))[0];
#endif
			ShouldShowMenu();
			App.WaitForElement(c => c.Marked(btn4Id));
			var btn2 = App.Query(c => c.Marked(btn4Id))[0];
#if __IOS__
			Assert.True(btn1.Rect.CenterY < btn2.Rect.CenterY);
#elif __MACOS__
			Assert.True(btn1.Rect.CenterX < btn2.Rect.CenterX);
#endif
		}


		protected override void TestTearDown()
		{
			base.TestTearDown();
			ResetApp();
			NavigateToGallery();
		}
	}
}

