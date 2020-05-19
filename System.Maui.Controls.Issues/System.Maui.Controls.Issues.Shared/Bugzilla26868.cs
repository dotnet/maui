using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 26868, "GroupHeaders do not extend on Windows Phone")]
	public class Bugzilla26868 : TestContentPage
	{
		protected override void Init ()
		{
			List<GroupedData> groups = new List<GroupedData> ();

			var group1 = new GroupedData { GroupName = "Group #1" };
			group1.Add (new GroupItem { DisplayText = "Text for ListView item 1.1" });
			group1.Add (new GroupItem { DisplayText = "Text for ListView item 1.2" });
			groups.Add (group1);

			var group2 = new GroupedData { GroupName = "Group #2" };
			group2.Add (new GroupItem { DisplayText = "Text for ListVIew item 2.1" });
			group2.Add (new GroupItem { DisplayText = "Text for ListView item 2.2" });
			groups.Add (group2);

			var itemTemplate = new DataTemplate(typeof(GroupItemTemplate));
			itemTemplate.CreateContent();

			var groupHeaderTemplate = new DataTemplate(typeof(GroupHeaderTemplate));
			groupHeaderTemplate.CreateContent();

			var listView = new ListView {
				IsGroupingEnabled = true,
				GroupDisplayBinding = new Binding("GroupName"),
				GroupShortNameBinding = new Binding("GroupName"),
				HasUnevenRows = Device.RuntimePlatform == Device.Android,

				ItemTemplate = itemTemplate,
				GroupHeaderTemplate = groupHeaderTemplate,

				ItemsSource = groups
			};

			Content = new StackLayout {
				VerticalOptions = LayoutOptions.Center,
				Children = {
					new Label {
						Text = "The group headers below should extend to the width of the screen. If they aren't the width of the screen, this test has failed."
					},
					new ContentView {
						Content = listView,
						HorizontalOptions = LayoutOptions.FillAndExpand,
						Padding = 0
					}
				}
			};
		}

		[Preserve (AllMembers = true)]
		public class GroupItem
		{
			public string DisplayText { get; set; }
		}

		[Preserve (AllMembers = true)]
		public class GroupedData : List<GroupItem>
		{
			public string GroupName { get; set; }
		}

		[Preserve (AllMembers=true)]
		public class GroupItemTemplate : ViewCell
		{
			public GroupItemTemplate()
			{
				var title = new Label() { FontSize = 14 };
				title.SetBinding(Label.TextProperty, new Binding("DisplayText", BindingMode.OneWay));

				View = new StackLayout
				{
					Orientation = StackOrientation.Horizontal,
					Padding = new Thickness(8),
					Children = { title } 
				};
			}
		}

		[Preserve(AllMembers = true)]
		public class GroupHeaderTemplate : ViewCell
		{
			public GroupHeaderTemplate()
			{
				var title = new Label { TextColor = Color.White, FontSize = 16 };
				title.SetBinding(Label.TextProperty, new Binding("GroupName", BindingMode.OneWay));

				View = new StackLayout
				{
					Padding = new Thickness(8, 0),
					VerticalOptions = LayoutOptions.StartAndExpand,
					BackgroundColor = Color.FromHex("#6D91BA"),
					Orientation = StackOrientation.Horizontal,
					Children = { title },
				};
			}
		}
	}
}
