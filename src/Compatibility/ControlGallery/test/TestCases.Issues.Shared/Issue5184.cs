using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	public class ListViewEntry
	{
		public int Number { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class ListViewGroup : List<ListViewEntry>
	{
		public string Heading { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class MainPageViewModel
	{
		List<ListViewGroup> _groups = new List<ListViewGroup>();

		public MainPageViewModel()
		{
			for (int i = 0; i < 20; ++i)
			{
				var group = new ListViewGroup
				{
					Heading = $"Group {i * 5} - {(i + 1) * 5 - 1}"
				};

				for (int j = 0; j < 5; ++j)
				{
					group.Add(new ListViewEntry { Number = i * 5 + j });
				}

				_groups.Add(group);
			}
		}

		public IReadOnlyList<ListViewGroup> Groups
		{
			get { return _groups; }
		}
	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5184, "Items get mixed up when fast scrolling", PlatformAffected.Android)]
	public class Issue5184 : TestContentPage
	{
		protected override void Init()
		{
			BindingContext = new MainPageViewModel();
			var listView = new ListView
			{
				Margin = 20,
				IsGroupingEnabled = true,
				GroupHeaderTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, nameof(ListViewGroup.Heading));
					var grid = new Grid
					{
						HorizontalOptions = LayoutOptions.FillAndExpand,
						ColumnDefinitions = new ColumnDefinitionCollection {
							new ColumnDefinition { Width = GridLength.Star }
						},
						Children = { label }
					};
					return new ViewCell { View = grid };
				}),
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, nameof(ListViewEntry.Number));
					var grid = new Grid
					{
						HorizontalOptions = LayoutOptions.FillAndExpand,
						ColumnDefinitions = new ColumnDefinitionCollection {
							new ColumnDefinition { Width = GridLength.Star }
						},
						Children = { label }
					};
					return new ViewCell { View = grid };
				})
			};
			listView.SetBinding(ListView.ItemsSourceProperty, nameof(MainPageViewModel.Groups));

			Content = new StackLayout
			{
				Children = {
					new Label { Text = "Quickly scroll down and back. Check that all items are correct." },
					listView
				}
			};
		}
	}
}
