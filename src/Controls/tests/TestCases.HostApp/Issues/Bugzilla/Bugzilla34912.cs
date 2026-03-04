using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

// Note: Fails on UWP due to https://bugzilla.xamarin.com/show_bug.cgi?id=60521

[Issue(IssueTracker.Bugzilla, 34912, "ListView.IsEnabled has no effect on iOS")]
public class Bugzilla34912 : TestContentPage
{
	protected override void Init()
	{
		Padding = new Thickness(0, 20, 0, 0);

		var source = SetupList();

		var list = new ListView
		{
			ItemTemplate = new DataTemplate(typeof(TextCell))
			{
				Bindings = {
					{ TextCell.TextProperty, new Binding ("Name") }
				}
			},

			GroupDisplayBinding = new Binding("LongTitle"),
			// While using this GroupShortNameBinding property it throws an exception on Windows for more information:https://github.com/dotnet/maui/issues/26534. For this test case we don't need this property.
			// GroupShortNameBinding = new Binding("Title"),
			Header = "HEADER",
			Footer = "FOOTER",
			IsGroupingEnabled = true,
			ItemsSource = SetupList(),
		};

		list.ItemTapped += (sender, e) =>
		{
			var listItem = (Issue2777.ListItemValue)e.Item;
			DisplayAlertAsync(listItem.Name, "You tapped " + listItem.Name, "OK", "Cancel");
		};

		var btnDisable = new Button()
		{
			Text = "Disable ListView",
			AutomationId = "btnDisable"
		};
		btnDisable.Clicked += (object sender, EventArgs e) =>
		{
			if (list.IsEnabled == true)
			{
				list.IsEnabled = false;
				btnDisable.Text = "Enable ListView";
			}
			else
			{
				list.IsEnabled = true;
				btnDisable.Text = "Disable ListView";
			}
		};

#pragma warning disable CS0618 // Type or member is obsolete
		Content = new StackLayout
		{
			VerticalOptions = LayoutOptions.FillAndExpand,
			Children = { btnDisable, list }
		};
#pragma warning restore CS0618 // Type or member is obsolete
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
}
