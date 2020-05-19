using System;
using Xamarin.Forms.CustomAttributes;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ListView)]
#endif

	// Note: Fails on UWP due to https://bugzilla.xamarin.com/show_bug.cgi?id=60521

	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 34912, "ListView.IsEnabled has no effect on iOS")]
	public class Bugzilla34912 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init ()
		{
			Padding = new Thickness (0, 20, 0, 0);

			var source = SetupList ();

			var list = new ListView
			{
				ItemTemplate = new DataTemplate(typeof(TextCell))
				{
					Bindings = {
						{ TextCell.TextProperty, new Binding ("Name") }
					}
				},

				GroupDisplayBinding = new Binding("LongTitle"),
				GroupShortNameBinding = new Binding("Title"),
				Header = "HEADER",
				Footer = "FOOTER",
				IsGroupingEnabled = true,
				ItemsSource = SetupList(),
			};

			list.ItemTapped += (sender, e) =>
			{
				var listItem = (Issue2777.ListItemValue)e.Item;
				DisplayAlert(listItem.Name, "You tapped " + listItem.Name, "OK", "Cancel");
			};

			var btnDisable = new Button () {
				Text = "Disable ListView",
				AutomationId = "btnDisable"
			};
			btnDisable.Clicked += (object sender, EventArgs e) => {
				if (list.IsEnabled == true){
					list.IsEnabled = false;
					btnDisable.Text = "Enable ListView";
				}
				else {
					list.IsEnabled = true;
					btnDisable.Text = "Disable ListView";
				}
			};

			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = { btnDisable, list }
			};
		}

		ObservableCollection<Issue2777.ListItemCollection> SetupList()
		{
			var allListItemGroups = new ObservableCollection<Issue2777.ListItemCollection>();

			foreach (var item in Issue2777.ListItemCollection.GetSortedData())
			{
				// Attempt to find any existing groups where theg group title matches the first char of our ListItem's name.
				var listItemGroup = allListItemGroups.FirstOrDefault(g => g.Title == item.Label);

				// If the list group does not exist, we create it.
				if (listItemGroup == null)
				{
					listItemGroup = new Issue2777.ListItemCollection(item.Label);
					listItemGroup.Add(item);
					allListItemGroups.Add(listItemGroup);
				}
				else
				{ // If the group does exist, we simply add the demo to the existing group.
					listItemGroup.Add(item);
				}
			}
			return allListItemGroups;
		}

#if UITEST
		[Test]
		public void Bugzilla34912Test ()
		{
			RunningApp.Tap (q => q.Marked ("Allen"));
			RunningApp.WaitForElement (q => q.Marked ("You tapped Allen"));
			RunningApp.Tap (q => q.Marked ("OK"));
			RunningApp.Tap (q => q.Marked ("btnDisable"));
			RunningApp.Tap (q => q.Marked ("Allen"));
			RunningApp.WaitForNoElement (q => q.Marked ("You tapped Allen"));
		}
#endif
	}
}
