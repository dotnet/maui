using NUnit.Framework;
using System.Threading;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("ListView")]
	internal class GroupedListActionsGalleryTests : BaseTestFixture
	{
		// TODO Group item actions, isolate specific indices (iOS is by 1, Android by two for cells)
		// TODO: Port to new conventions

		public GroupedListActionsGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.GroupedListActionsGalleryLegacy);
		}

		[Test]
		[Description ("All of the grouped list actions are displayed")]
		public void GroupedListActionsGalleryAllElementsExist ()
		{
//			var actions = new [] {
//				"General",
//				"Change group name",
//				"Change group short name",
//				"Child item actions",
//				"Clear this group",
//				"Insert group item",
//				"Insert 2 group items",
//				"Remove next item",
//				"Dummy item RDI",
//				"Remove next 2 dummy items",
//				"Dummy item RmDI-1",
//				"Dummy item RmDI-2",
//				"Replace dummy item",
//				"Dummy item RpDI",
//				"Replace next two dummy items",
//				"Dummy item RpDI-1",
//				"Dummy item RpDI-2",
//				"Select next dummy item",
//				"Dummy item SI",
//				"Move dummy above this one",
//				"Dummy item MDI",
//				"Move last 2 items above this one",
//				"Dummy item M2DI-1",
//				"Dummy item M2DI-2",
//				"Group item actions",
//				"Clear all",
//				"Insert group",
//				"Insert 2 groups",
//				"Remove previous dummy group",
//				"Remove previous 2 dummy groups",
//				"Replace previous dummy group",
//				"Replace previous 2 dummy groups",
//				"Move next group above",
//				"Move next 2 groups above"
//			};

//			foreach (var action in actions) {
//				App.ScrollDownForElement (q => q.Marked (action), 40);
//			}

//			App.Screenshot ("All actions are preset");
		}

//		[Test]
//		[Description ("Change group name")]
//		public void GroupedListActionsGalleryChangeGroupName ()
//		{
//			App.ScrollDownForElement (q => q.Marked ("General"), 40);
//			App.Screenshot ("Before changing group name");
//			App.Tap (q => q.Marked ("Change group name"));
//			App.WaitForElement (q => q.Marked ("General (changed)"));
//			App.Screenshot ("After changing group name");
//		}

//		[Test]
//		[Description ("Child actions - clear group")]
//		public void GroupedListActionsGalleryChildClearGroup ()
//		{
//			App.ScrollDownForElement (q => q.Marked ("Clear this group"), 40);
//			App.Screenshot ("Before clearing group");
//			App.Tap (q => q.Marked ("Clear this group"));
//			App.WaitForNoElement (q => q.Marked ("Insert group item"));
//			App.WaitForNoElement (q => q.Marked ("Insert 2 group items"));
//			App.Screenshot ("After clearing the group");
//		}

//		[Test]
//		[Description ("Child actions - insert item(s)")]
//		public void GroupedListActionsGalleryChildInsertItems ()
//		{
//			App.ScrollDownForElement (q => q.Marked ("Insert group item"), 40);
//			App.Screenshot ("Before inserting item");

//			App.Tap (q => q.Marked ("Insert group item"));
//			App.ScrollUpForElement (q => q.Marked ("Inserted item S"), 2);
//			App.WaitForElement (q => q.Marked ("Inserted item S"));
//			App.Screenshot ("After inserting item");

//			App.ScrollDownForElement (q => q.Marked ("Insert 2 group items"), 2);
//			App.Tap (q => q.Marked ("Insert 2 group items"));
//			App.ScrollUpForElement (q => q.Marked ("Inserted item D 0"), 2);
//			App.WaitForElement (q => q.Marked ("Inserted item D 0"));
//			App.WaitForElement (q => q.Marked ("Inserted item D 1"));
//			App.Screenshot ("After inserting 2 items");
//		}
			
			
//		[Test]
//		[Description ("Child actions - remove item(s)")]
//		public void GroupedListActionsGalleryChildRemoveItems ()
//		{
//			App.ScrollDownForElement (q => q.Marked ("Remove next item"), 40);
//			App.Screenshot ("Before removing item");

//			App.Tap (q => q.Marked ("Remove next item"));
//			App.WaitForNoElement (q => q.Marked ("Dummy item RDI"));
//			App.Screenshot ("After removing item");

//			App.ScrollDownForElement (q => q.Marked ("Remove next 2 dummy items"), 40);
//			App.Tap (q => q.Marked ("Remove next 2 dummy items"));
//			App.WaitForNoElement (q => q.Marked ("Dummy item RmDI-1"));
//			App.WaitForNoElement (q => q.Marked ("Dummy item RmDI-2"));
//			App.Screenshot ("After removing 2 items");
//		}

//		[Test]
//		[Description ("Child actions - replace item(s)")]
//		public void GroupedListActionsGalleryChildReplaceItems ()
//		{
//			App.ScrollDownForElement (q => q.Marked ("Replace dummy item"), 40);
//			App.Screenshot ("Before replacing item");

//			App.Tap (q => q.Marked ("Replace dummy item"));
//			App.WaitForNoElement (q => q.Marked ("Dummy item RpDI"));
//			App.ScrollDownForElement (q => q.Marked ("Replaced item"), 2);
//			App.Screenshot ("After replacing item");

//			App.ScrollDownForElement (q => q.Marked ("Replace next two dummy items"), 40);
//			App.Tap (q => q.Marked ("Replace next two dummy items"));
//			App.WaitForNoElement (q => q.Marked ("Dummy item RpDI-1"));
//			App.WaitForNoElement (q => q.Marked ("Dummy item RpDI-2"));
//			App.WaitForElement (q => q.Marked ("Replaced items 0"));
//			App.WaitForElement (q => q.Marked ("Replaced items 1"));
//			App.Screenshot ("After replacing 2 items");
//		}

//		[Test]
//		[Category ("ManualReview")]
//		[Description ("Child actions - select item(s)")]
//		public void GroupedListActionsGalleryChildSelectItems ()
//		{
//			App.ScrollDownForElement (q => q.Marked ("Select next dummy item"), 40);
//			App.Screenshot ("Before selecting item");
//			App.Tap (q => q.Marked ("Select next dummy item"));
//			App.Screenshot ("After selecting item");
//		}

//		[Test]
//		[Category ("ManualReview")]
//		[Description ("Child actions - move item(s)")]
//		public void GroupedListActionsGalleryChildMoveItems ()
//		{
//			App.ScrollDownForElement (q => q.Marked ("Move dummy above this one"), 40);
//			App.Screenshot ("Before moving item");

//			App.Tap (q => q.Marked ("Move dummy above this one"));
//			App.Screenshot ("Dummy item MDI should now be above 'Move dummy above this one'");

//			App.Tap (q => q.Marked ("Move last 2 items above this one"));
//			App.Screenshot ("Dummy item M2DI-1 and M2DI-2 should now be above 'Move last 2 items above this one'");

//		}
			
//		[Test]
//		[Description ("Child item action test - landscape")]
//		public void GroupedListActionsGalleryAllElementsExistLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			GroupedListActionsGalleryAllElementsExist ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to portrait");
//		}

//		[Test]
//		[Description ("Change group name - landscape")]
//		public void GroupedListActionsGalleryChangeGroupNameLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			GroupedListActionsGalleryChangeGroupName ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to portrait");
//		}

//		[Test]
//		[Description ("Child actions - clear group - landscape")]
//		public void GroupedListActionsGalleryChildClearGroupLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			GroupedListActionsGalleryChildClearGroup ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to portrait");
//		}

//		[Test]
//		[Description ("Child actions - insert item(s) - landscape")]
//		public void GroupedListActionsGalleryChildInsertItemsLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			GroupedListActionsGalleryChildInsertItems ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to portrait");
//		}


//		[Test]
//		[Description ("Child actions - remove item(s) - landscape")]
//		public void GroupedListActionsGalleryChildRemoveItemsLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			GroupedListActionsGalleryChildRemoveItems ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to portrait");
//		}

//		[Test]
//		[Description ("Child actions - replace item(s) - landscape")]
//		public void GroupedListActionsGalleryChildReplaceItemsLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			GroupedListActionsGalleryChildReplaceItems ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to portrait");
//		}

//		[Test]
//		[Category ("ManualReview")]
//		[Description ("Child actions - select item(s) - landscape")]
//		public void GroupedListActionsGalleryChildSelectItemsLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			GroupedListActionsGalleryChildSelectItems ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to portrait");
//		}

//		[Test]
//		[Category ("ManualReview")]
//		[Description ("Child actions - move item(s) - landscape")]
//		public void GroupedListActionsGalleryChildMoveItemsLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			GroupedListActionsGalleryChildMoveItems ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to portrait");
//		}

	}
}