using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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
	[Issue(IssueTracker.Github, 1939, "ArgumentOutOfRangeException on clearing a group on a grouped ListView on Android", PlatformAffected.Android)]
	public class Issue1939 : TestContentPage
	{
		ObservableCollection<GroupedData> Data { get; set; } = new ObservableCollection<GroupedData>();

		readonly GroupedData _temp1 = new GroupedData() { GroupName = $"Group #1", HasHeader = true };
		readonly GroupedData _temp2 = new GroupedData() { GroupName = $"Group #2", HasHeader = false };

		protected override void Init()
		{
			var listView = new ListView
			{
				IsGroupingEnabled = true,
				ItemTemplate = new DataTemplate(typeof(GroupItemTemplate)),
				GroupHeaderTemplate = new MyDataTemplateSelector(),
				ItemsSource = Data
			};

			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children = {
					new Label { Text = "This test adds two groups to this list and then clears the items from one of them. If the test crashes, this test has failed." },
					listView
				}
			};
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			FillResults(_temp1, 5, true);

			Data.Add(_temp1);
			Data.Add(_temp2);

			FillResults(_temp2, 5, false);
		}

		async void FillResults(GroupedData results, int items, bool clear)
		{
			results.Clear();

			await Task.Delay(200);

			for (int i = 0; i < items; i++)
			{
				results.Add(new GroupItem { DisplayText = $"Text for ListView item {i}" });
			}

			if (!clear)
				return;

			await Task.Delay(1000);

			results.Clear();
		}

		[Preserve(AllMembers = true)]
		public class MyDataTemplateSelector : DataTemplateSelector
		{
			readonly DataTemplate firstGroupTemplate;
			readonly DataTemplate secondGroupTemplate;

			public MyDataTemplateSelector()
			{
				firstGroupTemplate = new DataTemplate(typeof(GroupNoHeaderTemplate));
				secondGroupTemplate = new DataTemplate(typeof(GroupHeaderTemplate));
			}

			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
			{
				if (!(item is GroupedData model))
				{
					return null;
				}

				if (model.HasHeader)
					return secondGroupTemplate;

				return firstGroupTemplate;
			}
		}

		[Preserve(AllMembers = true)]
		public class GroupItem
		{
			public string DisplayText { get; set; }
		}

		[Preserve(AllMembers = true)]
		public class GroupedData : ObservableCollection<GroupItem>
		{
			public string GroupName { get; set; }
			public bool HasHeader { get; set; }
		}

		[Preserve(AllMembers = true)]
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

		[Preserve(AllMembers = true)]
		public class GroupNoHeaderTemplate : ViewCell
		{
			public GroupNoHeaderTemplate()
			{
				View = new StackLayout
				{
					BackgroundColor = Color.White,
				};
			}
		}

#if UITEST
		[Test]
		public void Issue1939Test()
		{
			RunningApp.WaitForElement(q => q.Marked("Group #1"));
		}
#endif
	}
}