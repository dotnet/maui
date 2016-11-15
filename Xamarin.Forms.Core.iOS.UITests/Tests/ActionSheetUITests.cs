using NUnit.Framework;
using Xamarin.UITest;
using System;
using System.Threading;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category(UITestCategories.ActionSheet)]
	internal class ActionSheetUITests : BaseTestFixture
	{
		AppRect screenSize;

		public ActionSheetUITests ()
		{

		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.ActionSheetGallery);
		}

		static void CheckExtras ()
		{
			App.WaitForElement(c => c.Marked("Extra One"));
			App.WaitForElement(c => c.Marked("Extra Six"));
		}

		protected override void TestSetup ()
		{
			base.TestSetup ();
			screenSize = App.Query (q => q.Marked ("ActionSheetPage"))[0].Rect;
		}

		[Test]
		public void TestDisplayActionSheet ()
		{
			ScrollAndTap ("ActionSheet Extras");
			CheckExtras ();
			App.Tap (c => c.Marked ("Extra One"));
		}
			
		[Test]
		public void TestDisplayActionSheetCancel ()
		{
			ScrollAndTap ("ActionSheet Cancel");
			// iPad does not have a Cancel button for action sheet
			if (App.Query (q => q.Marked ("Cancel")).Length > 0)
				App.Tap (c => c.Marked ("Cancel"));
			else
				App.TapCoordinates (20, screenSize.Height / 2);
		}

		[Test]
		public void TestDisplayActionSheetCancelDestruction ()
		{
			ScrollAndTap ("ActionSheet Cancel Destruction");
			App.WaitForNoElement (c => c.Marked ("Extra One"));
			App.WaitForElement (c => c.Marked ("Destruction"));
			if (App.Query (q => q.Marked ("Cancel")).Length > 0)
				App.Tap (c => c.Marked ("Cancel"));
			else
				App.TapCoordinates (20, screenSize.Height / 2);
		}

		[Test]
		public void TestDisplayActionSheetCancelExtras ()
		{
			ScrollAndTap ("ActionSheet Cancel Extras");
			CheckExtras ();
			if (App.Query (q => q.Marked ("Cancel")).Length > 0)
				App.Tap (c => c.Marked ("Cancel"));
			else
				App.TapCoordinates (20, screenSize.Height / 2);
		}

		[Test]
		public void TestDisplayActionSheetCancelExtrasDestruction ()
		{
			ScrollAndTap ("ActionSheet Cancel Destruction Extras");
			CheckExtras ();
			App.WaitForElement (c => c.Marked ("Destruction"));
			if (App.Query (q => q.Marked ("Cancel")).Length > 0)
				App.Tap (c => c.Marked ("Cancel"));
			else
				App.TapCoordinates (20, screenSize.Height / 2);
		}

		[Test]
		public void TestDisplayActionSheetDestruction ()
		{
			ScrollAndTap ("ActionSheet Destruction");
			App.WaitForNoElement (c => c.Marked ("Extra One"));
			App.Tap (c => c.Marked ("Destruction"));
		}

		[Test]
		public void TestDisplayActionSheetDestructionExtras ()
		{
			ScrollAndTap ("ActionSheet Destruction Extras");
			CheckExtras ();
			App.Tap (c => c.Marked ("Extra One"));
		}

		[Test]
		public void TestDisplayActionSheetTitleCancel ()
		{
			ScrollAndTap ("ActionSheet Title Cancel");
			App.WaitForElement (c => c.Marked ("Title"));
			if (App.Query (q => q.Marked ("Cancel")).Length > 0)
				App.Tap (c => c.Marked ("Cancel"));
			else
				App.TapCoordinates (20, screenSize.Height / 2);
		}

		[Test]
		public void TestDisplayActionSheetTitleCancelDestruction ()
		{
			ScrollAndTap ("ActionSheet Title Cancel Destruction");
			App.WaitForElement (c => c.Marked ("Title"));
			App.WaitForNoElement (c => c.Marked ("Extra One"));
			App.Tap (c => c.Marked ("Destruction"));
		}

		[Test]
		public void TestDisplayActionSheetTitleCancelDestructionExtras ()
		{
			ScrollAndTap ("ActionSheet Title Cancel Destruction Extras");
			App.WaitForElement (c => c.Marked ("Title"));
			CheckExtras ();
			App.Tap (c => c.Marked ("Destruction"));
		}

		[Test]
		public void TestDisplayActionSheetTitleDestruction ()
		{
			ScrollAndTap ("ActionSheet Title Destruction");
			App.WaitForElement (c => c.Marked ("Title"));
			App.WaitForNoElement (c => c.Marked ("Extra One"));
			App.Tap (c => c.Marked ("Destruction"));
		}

		[Test]
		public void TestDisplayActionSheetTitleDestructionExtras ()
		{
			ScrollAndTap ("ActionSheet Title Destruction Extras");
			App.WaitForElement (c => c.Marked ("Title"));
			CheckExtras ();
			App.Tap (c => c.Marked ("Destruction"));
		}


		[Test]
		public void TestDisplayActionSheetTitleExtras ()
		{
			ScrollAndTap ("ActionSheet Title Extras");
			CheckExtras ();
			App.Tap (c => c.Marked ("Extra One"));
		}

		void ScrollAndTap(string actionSheet) 
		{
			App.ScrollForElement(string.Format("* text:'{0}'", actionSheet), new Drag(App.Query(q => q.Marked("ActionSheetPage"))[0].Rect, Drag.Direction.BottomToTop, Drag.DragLength.Long));
			App.Tap(q=>q.Raw(string.Format("* text:'{0}'", actionSheet)));
		}

	}
}

