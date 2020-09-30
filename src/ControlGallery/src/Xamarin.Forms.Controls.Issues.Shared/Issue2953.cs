using System;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2953, "GroupHeaderCells disappear when item is removed from a group in ListView (iOS only) ")]
	public class Issue2953 : TestContentPage
	{
		protected override void Init()
		{
			var items = new ObservableCollection<GroupedItems>() {
				new GroupedItems ("Header 1") { "1.1", "1.2", "1.3" },
				new GroupedItems ("Header 2") { "2.1", "2.2", "2.3" },
				new GroupedItems ("Header 3") { "3.1", "3.2", "3.3" },
				new GroupedItems ("Header 4") { "4.1", "4.2", "4.3" },
			};

			var listview = new ListView
			{
				HasUnevenRows = true,
				IsGroupingEnabled = true
			};

			listview.GroupHeaderTemplate = new DataTemplate
				(typeof(HeaderCell));
			listview.ItemTemplate = new DataTemplate(typeof(ItemCell));
			listview.ItemsSource = items;

			var btnRemove = new Button() { Text = "Remove", AutomationId = "btnRemove" };
			btnRemove.Clicked += delegate
			{
				if (items[1].Count > 0)
				{
					items[1].RemoveAt(0);
				}
			};

			Content = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				Children = { listview, btnRemove }
			};
		}

		[Preserve(AllMembers = true)]
		internal class GroupedItems : ObservableCollection<string>
		{
			public GroupedItems(string groupName) { GroupName = groupName; }
			public string GroupName { get; private set; }
		}

		[Preserve(AllMembers = true)]
		internal class HeaderCell : ViewCell
		{
			public HeaderCell()
			{
				Height = 44;
				var label = new Label { BackgroundColor = Color.Pink };
				label.SetBinding(Label.TextProperty, "GroupName");
				View = label;
			}
		}

		[Preserve(AllMembers = true)]
		internal class ItemCell : ViewCell
		{
			public ItemCell()
			{
				var label = new Label { BackgroundColor = Color.Aqua };
				label.SetBinding(Label.TextProperty, ".");
				View = label;
			}
		}

#if UITEST
		[Test]
		public void Issue2953Test()
		{
			RunningApp.Screenshot("I am at Issue 2953");
			RunningApp.WaitForElement(q => q.Marked("Header 3"));
			RunningApp.Screenshot("I see the Header 3");
			RunningApp.Tap(q => q.Marked("btnRemove"));
			RunningApp.WaitForElement(q => q.Marked("Header 3"));
			RunningApp.Screenshot("I still see the Header 3");
		}
#endif
	}
}

// Note: this fails on UWP because we can't currently inspect listview headers	