using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif


namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 2777, "When add GroupHeaderTemplate in XAML the group header does not show up")]
	public partial class Issue2777 : TestContentPage
	{
		public Issue2777 ()
		{
			#if APP
			InitializeComponent ();
			var list = SetupList ();
			itemListView.ItemsSource = list;
			#endif
		
		}

		protected override void Init ()
		{
			
		}

		internal void OnItemTapped (object sender, ItemTappedEventArgs ea)
		{
			var listItem = (ListItemValue)ea.Item;
			DisplayAlert (listItem.Name, "You tapped " + listItem.Name, "OK", "Cancel");
		}

		ObservableCollection<ListItemCollection> SetupList ()
		{
			var allListItemGroups = new ObservableCollection<ListItemCollection> ();

			foreach (var item in ListItemCollection.GetSortedData()) {
				// Attempt to find any existing groups where theg group title matches the first char of our ListItem's name.
				var listItemGroup = allListItemGroups.FirstOrDefault (g => g.Title == item.Label);

				// If the list group does not exist, we create it.
				if (listItemGroup == null) {
					listItemGroup = new ListItemCollection (item.Label);
					listItemGroup.Add (item);
					allListItemGroups.Add (listItemGroup);
				} else { // If the group does exist, we simply add the demo to the existing group.
					listItemGroup.Add (item);
				}
			}
			return allListItemGroups;
		}

		// Represents a group of items in our list.
		[Preserve (AllMembers = true)]
		public class ListItemCollection : ObservableCollection<ListItemValue>
		{
			public string Title { get; private set; }

			public string LongTitle { get { return "The letter " + Title; } }

			public ListItemCollection (string title)
			{
				Title = title;
			}

			public static List<ListItemValue> GetSortedData ()
			{
				var items = ListItems;
				items.Sort ();
				return items;
			}

			// Data used to populate our list.
			static readonly List<ListItemValue> ListItems = new List<ListItemValue> () {
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
		[Preserve (AllMembers = true)]
		public class ListItemValue : IComparable<ListItemValue>
		{
			public string Name { get; private set; }


			public ListItemValue (string name)
			{
				Name = name;
			}

			int IComparable<ListItemValue>.CompareTo (ListItemValue value)
			{
				return Name.CompareTo (value.Name);
			}

			public string Label {
				get {
					return Name [0].ToString ();
				}
			}
		}
	

		#if UITEST
		[Test]
		public void Issue2777Test ()
		{
			RunningApp.Screenshot ("I am at Issue 2965");
			RunningApp.WaitForElement (q => q.Marked ("The letter A"));
		}
		#endif
	}

	// Note: this fails on UWP because we can't currently inspect listview headers
}

