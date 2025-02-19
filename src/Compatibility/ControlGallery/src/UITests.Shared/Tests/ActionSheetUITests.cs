using System;
using NUnit.Framework;
using Xamarin.UITest.Queries;

namespace Microsoft.Maui.Controls.Compatibility.UITests
{
	[TestFixture]
	[Category(UITestCategories.ActionSheet)]
	internal class ActionSheetUITests : BaseTestFixture
	{
		AppRect screenSize;

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.ActionSheetGallery);
		}

		static void CheckExtras()
		{
			App.WaitForElement(c => c.Marked("Extra One"));
			App.WaitForElement(c => c.Marked("Extra Six"));
		}

		protected override void TestSetup()
		{
			base.TestSetup();
#if !__MACOS__
			screenSize = App.Query(q => q.Marked("ActionSheetPage"))[0].Rect;
#else
			screenSize = new AppRect();
#endif


		}

		[Test]
		public void TestDisplayActionSheet()
		{
			ScrollAndTap("ActionSheet Extras");
			CheckExtras();
			App.Tap(c => c.Marked("Extra One"));
		}

		[Test]
		public void TestDisplayActionSheetCancel()
		{
			ScrollAndTap("ActionSheet Cancel");
			// iPad does not have a Cancel button for action sheet
			if (App.Query(q => q.Marked("Cancel")).Length > 0)
				App.Tap(c => c.Marked("Cancel"));
			else
				App.TapCoordinates(20, screenSize.Height / 2);
		}

		[Test]
		public void TestDisplayActionSheetCancelDestruction()
		{
			ScrollAndTap("ActionSheet Cancel Destruction");
			App.WaitForNoElement(c => c.Marked("Extra One"));
			App.WaitForElement(c => c.Marked("Destruction"));
			if (App.Query(q => q.Marked("Cancel")).Length > 0)
				App.Tap(c => c.Marked("Cancel"));
			else
				App.TapCoordinates(20, screenSize.Height / 2);
		}

		[Test]
		public void TestDisplayActionSheetCancelExtras()
		{
			ScrollAndTap("ActionSheet Cancel Extras");
			CheckExtras();
			if (App.Query(q => q.Marked("Cancel")).Length > 0)
				App.Tap(c => c.Marked("Cancel"));
			else
				App.TapCoordinates(20, screenSize.Height / 2);
		}

		[Test]
		public void TestDisplayActionSheetCancelExtrasDestruction()
		{
			ScrollAndTap("ActionSheet Cancel Destruction Extras");
			CheckExtras();
			App.WaitForElement(c => c.Marked("Destruction"));
			if (App.Query(q => q.Marked("Cancel")).Length > 0)
				App.Tap(c => c.Marked("Cancel"));
			else
				App.TapCoordinates(20, screenSize.Height / 2);
		}

		[Test]
		public void TestDisplayActionSheetDestruction()
		{
			ScrollAndTap("ActionSheet Destruction");
			App.WaitForNoElement(c => c.Marked("Extra One"));
			App.Tap(c => c.Marked("Destruction"));
		}

		[Test]
		public void TestDisplayActionSheetDestructionExtras()
		{
			ScrollAndTap("ActionSheet Destruction Extras");
			CheckExtras();
			App.Tap(c => c.Marked("Extra One"));
		}

		[Test]
		public void TestDisplayActionSheetTitleCancel()
		{
			ScrollAndTap("ActionSheet Title Cancel");
			App.WaitForElement(c => c.Marked("Title"));
			if (App.Query(q => q.Marked("Cancel")).Length > 0)
				App.Tap(c => c.Marked("Cancel"));
			else
				App.TapCoordinates(20, screenSize.Height / 2);
		}

		[Test]
		public void TestDisplayActionSheetTitleCancelDestruction()
		{
			ScrollAndTap("ActionSheet Title Cancel Destruction");
			App.WaitForElement(c => c.Marked("Title"));
			App.WaitForNoElement(c => c.Marked("Extra One"));
			App.Tap(c => c.Marked("Destruction"));
		}

		[Test]
		public void TestDisplayActionSheetTitleCancelDestructionExtras()
		{
			ScrollAndTap("ActionSheet Title Cancel Destruction Extras");
			App.WaitForElement(c => c.Marked("Title"));
			CheckExtras();
			App.Tap(c => c.Marked("Destruction"));
		}

		[Test]
		public void TestDisplayActionSheetTitleDestruction()
		{
			ScrollAndTap("ActionSheet Title Destruction");
			App.WaitForElement(c => c.Marked("Title"));
			App.WaitForNoElement(c => c.Marked("Extra One"));
			App.Tap(c => c.Marked("Destruction"));
		}

		[Test]
		public void TestDisplayActionSheetTitleDestructionExtras()
		{
			ScrollAndTap("ActionSheet Title Destruction Extras");
			App.WaitForElement(c => c.Marked("Title"));
			CheckExtras();
			App.Tap(c => c.Marked("Destruction"));
		}


		[Test]
		public void TestDisplayActionSheetTitleExtras()
		{
			ScrollAndTap("ActionSheet Title Extras");
			CheckExtras();
			App.Tap(c => c.Marked("Extra One"));
		}

		void ScrollAndTap(string actionSheet)
		{
			var queryString = $"* text:'{actionSheet}'";
			Func<AppQuery, AppQuery> actionSheetQuery = q => q.Raw(queryString);
#if WINDOWS
			App.ScrollDownTo(actionSheetQuery);
#elif __MACOS__
			App.Tap(actionSheetQuery);
#else
			App.ScrollForElement(queryString, new Drag(App.Query(q => q.Marked("ActionSheetPage"))[0].Rect, Drag.Direction.BottomToTop, Drag.DragLength.Long));
#endif
			App.Tap(actionSheetQuery);
		}

	}
}

