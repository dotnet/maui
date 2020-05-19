using System;
using Xamarin.Forms.CustomAttributes;
using System.Diagnostics;
using Xamarin.Forms.Internals;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3019, "Grouped ListView Header empty for adding items", PlatformAffected.UWP)]
#if UITEST
	[Category(UITestCategories.ListView)]
#endif
	public class Issue3019 : TestContentPage
	{
		ListView _listViewIsGrouped;


		[Preserve(AllMembers = true)]
		class MyHeaderViewCell : ViewCell
		{
			public MyHeaderViewCell()
			{
				Height = 25;
				var label = new Label { VerticalOptions = LayoutOptions.Center };
				label.SetBinding(Label.TextProperty, nameof(GroupedItem.Name));
				View = new StackLayout()
				{
					Children =
					{
						label
					}
				};
			}
		}

		[Preserve(AllMembers = true)]
		class Item
		{
			static int counter = 0;
			public Item()
			{
				Text = $"Grouped Item: {counter++}";
			}

			public string Text { get; }

		}

		[Preserve(AllMembers = true)]
		class GroupedItem : List<Item>
		{
			public GroupedItem()
			{
				AddRange(Enumerable.Range(0, 1).Select(i => new Item()));
			}

			public string Name { get; set; }
		}


		void LoadData()
		{
			_listViewIsGrouped.ItemsSource = new ObservableCollection<GroupedItem>(Enumerable.Range(0, 1).Select(x => new GroupedItem() { Name = $"Group {x}" }));
		}


		void AddData()
		{
			var list = _listViewIsGrouped.ItemsSource as IList<GroupedItem>;
			list.Add(new GroupedItem() { Name = $"Group {list.Count}" });
		}

		void ReloadListViews()
		{
			StackLayout content = Content as StackLayout;

			if (_listViewIsGrouped != null)
			{
				content.Children.Remove(_listViewIsGrouped);
			}

			_listViewIsGrouped = new ListView
			{
				IsGroupingEnabled = true,
				GroupHeaderTemplate = new DataTemplate(typeof(MyHeaderViewCell)),
				ItemTemplate = new DataTemplate(() =>
				{
					Label nameLabel = new Label();
					nameLabel.SetBinding(Label.TextProperty, "Text");
					var cell = new ViewCell
					{
						View = nameLabel,
					};
					return cell;
				}),
				ItemsSource = new ObservableCollection<GroupedItem>()
			};

			content.Children.Add(_listViewIsGrouped);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			AddData();
		}

		protected override void Init()
		{
			Label label = new Label() { Text = "If you see two group headers and can click on each row without crashing test has passed" };

			Content = new StackLayout
			{
				Children =
				{
					label,
					new Button()
					{
						Text = "Click to add more rows",
						Command = new Command(() =>
						{
							AddData();
						})
					}
				},
			};

			ReloadListViews();
			LoadData();

			_listViewIsGrouped.ItemSelected += (sender, args) =>
			{
				label.Text = (args.SelectedItem as Item).Text + " Clicked";
			};
		}

#if UITEST
		[Test]
		public void MakeSureListGroupShowsUpAndItemsAreClickable()
		{
			RunningApp.WaitForElement("Group 1");

			RunningApp.Tap(x => x.Marked("Grouped Item: 0"));
			RunningApp.Tap(x => x.Marked("Grouped Item: 1"));
			RunningApp.Tap(x => x.Marked("Grouped Item: 1 Clicked"));

		}
#endif

	}
}
