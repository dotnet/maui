using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2777, "When add GroupHeaderTemplate in XAML the group header does not show up")]
	public partial class Issue2777 : TestContentPage
	{
		public Issue2777()
		{
			InitializeComponent();
			var list = SetupList();
			itemListView.ItemsSource = list;
		}

		protected override void Init()
		{

		}

		internal void OnItemTapped(object sender, ItemTappedEventArgs ea)
		{
			var listItem = (ListItemValue)ea.Item;
			DisplayAlert(listItem.Name, "You tapped " + listItem.Name, "OK", "Cancel");
		}

		ObservableCollection<ListItemCollection> SetupList()
		{
			var allListItemGroups = new ObservableCollection<ListItemCollection>();

			foreach (var item in ListItemCollection.GetSortedData())
			{
				// Attempt to find any existing groups where theg group title matches the first char of our ListItem's name.
				var listItemGroup = allListItemGroups.FirstOrDefault(g => g.Title == item.Label);

				// If the list group does not exist, we create it.
				if (listItemGroup == null)
				{
					listItemGroup = new ListItemCollection(item.Label);
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

		// Represents a group of items in our list.
		[Preserve(AllMembers = true)]
		public class ListItemCollection : ObservableCollection<ListItemValue>
		{
			public string Title { get; private set; }

			public string LongTitle { get { return "The letter " + Title; } }

			public ListItemCollection(string title)
			{
				Title = title;
			}

			public static List<ListItemValue> GetSortedData()
			{
				var items = ListItems;
				items.Sort();
				return items;
			}

			// Data used to populate our list.
			static readonly List<ListItemValue> ListItems = new List<ListItemValue>() {
				new ListItemValue ("Babbage"),
				new ListItemValue ("Boole"),
				new ListItemValue ("Berners-Lee"),
				new ListItemValue ("Atanasoff"),
				new ListItemValue ("Allen"),
				new ListItemValue ("Cormack"),
				new ListItemValue ("Cray"),
				new ListItemValue ("Dijkstra"),
				new ListItemValue ("Dix"),
				new ListItemValue ("Dewey"),
				new ListItemValue ("Erdős"),
			};
		}

		// Represents one item in our list.
		[Preserve(AllMembers = true)]
		public class ListItemValue : IComparable<ListItemValue>
		{
			public string Name { get; private set; }


			public ListItemValue(string name)
			{
				Name = name;
			}

			int IComparable<ListItemValue>.CompareTo(ListItemValue value)
			{
				return Name.CompareTo(value.Name);
			}

			public string Label
			{
				get
				{
					return Name[0].ToString();
				}
			}
		}
	}

	// Note: this fails on UWP because we can't currently inspect listview headers
}

