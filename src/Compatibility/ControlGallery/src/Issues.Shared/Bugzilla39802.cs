﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39802, "Gap between ListView cells even if SeparatorVisablity is set to none ",
		PlatformAffected.iOS)]
	public class Bugzilla39802 : TestContentPage
	{
		protected override void Init()
		{
			BackgroundColor = Colors.Yellow;

			var list = new ObservableCollection<GroupedData>();

			for (int i = 1; i <= 2; i++)
			{
				var group = new GroupedData { GroupName = $"Group #{i}" };

				for (int j = 1; j < 30; j++)
				{
					var item = new MyItem { Title = $"Item: #{i}-{j}", Color = (j % 2 == 0) ? Colors.Blue : Colors.Red };

					group.Add(item);
				}
				list.Add(group);
			}

			ListItems = list;

			BindingContext = this;
			var lst = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				BackgroundColor = Colors.Transparent,
				ItemTemplate = new DataTemplate(typeof(ItemTemplate)),
				GroupHeaderTemplate = new DataTemplate(typeof(GroupHeaderTemplate)),
				IsGroupingEnabled = true,
				GroupDisplayBinding = new Binding(nameof(GroupedData.GroupName)),
				GroupShortNameBinding = new Binding(nameof(GroupedData.GroupName)),
			};
			lst.SeparatorVisibility = SeparatorVisibility.None;
			lst.SeparatorColor = Colors.Green;
			lst.SetBinding(ListView.ItemsSourceProperty, nameof(ListItems));
			Content = lst;
		}

		[Preserve(AllMembers = true)]
		public class ItemTemplate : ViewCell
		{
			public ItemTemplate()
			{
				var stk = new StackLayout
				{
					Padding = new Thickness(15, 0, 0, 0)
				};
				stk.SetBinding(VisualElement.BackgroundColorProperty, nameof(MyItem.Color));
				var lbl = new Label
				{
					TextColor = Colors.Yellow,
					VerticalOptions = LayoutOptions.CenterAndExpand
				};
				lbl.SetBinding(Label.TextProperty, nameof(MyItem.Title));
				stk.Children.Add(lbl);
				View = stk;
			}
		}

		[Preserve(AllMembers = true)]
		public class GroupHeaderTemplate : ViewCell
		{
			public GroupHeaderTemplate()
			{
				var title = new Label { TextColor = Colors.White, FontSize = 16 };
				title.SetBinding(Label.TextProperty, new Binding(nameof(GroupedData.GroupName), BindingMode.OneWay));

				View = new StackLayout
				{
					Padding = new Thickness(8, 0),
					VerticalOptions = LayoutOptions.StartAndExpand,
					BackgroundColor = Colors.Pink,
					Orientation = StackOrientation.Horizontal,
					Children = { title },
				};
			}
		}

		public ObservableCollection<GroupedData> ListItems { get; set; }

		[Preserve(AllMembers = true)]
		public class MyItem
		{
			public string Title { get; set; }

			public Color Color { get; set; }
		}

		[Preserve(AllMembers = true)]
		public class GroupedData : List<MyItem>
		{
			public string GroupName { get; set; }
		}
	}
}
