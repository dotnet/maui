using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using Xamarin.Forms.Core.UITests;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ListView)]
#endif

	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 26032, " ListView ItemTapped doesn't get called for the selected item ", PlatformAffected.iOS)]
	public partial class Bugzilla26032 : TestContentPage
	{
		#if APP
		public Bugzilla26032 ()
		{
			
			InitializeComponent ();
			var data = new[] { "1", "2", "3", "4", "5" };
			var dataContext = new[] { "1 Context", "2 Context", "3 Context", "4 Context", "5 Context" };
			List1.ItemsSource = data;
			List2.ItemsSource = dataContext;
		}


		public void OnItemTapped(object sender, ItemTappedEventArgs e)
		{
			Log.Text = string.Format("Item '{0}' tapped\n{1}", e.Item, Log.Text);
		}

		public void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			Log.Text = string.Format("Item '{0}' selected\n{1}", e.SelectedItem, Log.Text);
		}

		#endif

		protected override void Init ()
		{
		}


		#if UITEST
		[Test]
		public void SelectedItemTap ()
		{
			var id = "1";
			var idContext = "1 Context";
			var initalLog = "Item '{0}' tapped\nItem '{0}' selected\n";

			var forId1 = string.Format (initalLog, id);
			RunningApp.WaitForElement (q => q.Marked (id));
			RunningApp.Tap (q => q.Marked (id));
			RunningApp.WaitForElement (q => q.Text (forId1));
			RunningApp.Tap (q => q.Marked (id));
			forId1 = string.Format ( "Item '{0}' tapped\n" + initalLog, id);
			RunningApp.WaitForElement (q => q.Text (forId1));

			var forIdContext = string.Format (initalLog, idContext);
			RunningApp.WaitForElement (q => q.Marked (idContext));
			RunningApp.Tap (q => q.Marked (idContext));
			RunningApp.WaitForElement (q => q.Text (forIdContext + forId1));
			RunningApp.Tap (q => q.Marked (idContext));
			forIdContext = string.Format ( "Item '{0}' tapped\n" + initalLog, idContext);
			RunningApp.WaitForElement (q => q.Text (forIdContext + forId1));
		}
		#endif
	}
}

