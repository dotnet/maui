using System.Collections.Generic;
using System.Collections.ObjectModel;
using Maui.Controls.Sample.Issues;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 30924, "CollectionView GroupHeaders do not resize if you hide the content", PlatformAffected.iOS | PlatformAffected.Android)]
	public class Issue30924 : TestContentPage
	{
		const string HeaderAutomationId = "30924GroupHeader";
		const string ToggleAutomationId = "30924HeaderToggle";

		protected override void Init()
		{
			var headerToggle = new Switch
			{
				AutomationId = ToggleAutomationId,
				IsToggled = true
			};

			// CollectionView2 pins the page to the supported handler, so the test behaves
			// the same in the CollectionView1 CI configuration (which uses the legacy handler).
			var collectionView = new CollectionView2
			{
				AutomationId = "30924CollectionView",
				IsGrouped = true,
				ItemsSource = CreateGroups(),
				ItemTemplate = new DataTemplate(() =>
				{
					var itemLabel = new Label
					{
						HeightRequest = 60,
						Padding = new Thickness(10, 0),
						VerticalTextAlignment = TextAlignment.Center
					};

					itemLabel.SetBinding(Label.TextProperty, ".");
					itemLabel.SetBinding(AutomationIdProperty, ".");

					return itemLabel;
				}),
				GroupHeaderTemplate = new DataTemplate(() =>
				{
					var headerLabel = new Label
					{
						AutomationId = HeaderAutomationId,
						BackgroundColor = Colors.LightGray,
						HeightRequest = 80,
						Padding = new Thickness(10, 0),
						VerticalTextAlignment = TextAlignment.Center
					};

					headerLabel.SetBinding(Label.TextProperty, nameof(Issue30924Group.Name));
					headerLabel.SetBinding(IsVisibleProperty, new Binding(nameof(Switch.IsToggled), source: headerToggle));

					return headerLabel;
				})
			};

			var layout = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition(GridLength.Auto),
					new RowDefinition(GridLength.Star)
				},
				Children =
				{
					new HorizontalStackLayout
					{
						Padding = 10,
						Spacing = 10,
						Children =
						{
							new Label
							{
								Text = "Group header visible",
								VerticalTextAlignment = TextAlignment.Center
							},
							headerToggle
						}
					},
					collectionView
				}
			};

			Grid.SetRow(collectionView, 1);

			Content = layout;
		}

		static ObservableCollection<Issue30924Group> CreateGroups()
		{
			return new ObservableCollection<Issue30924Group>
			{
				new Issue30924Group("Group A", new[] { "A1", "A2", "A3" }),
				new Issue30924Group("Group B", new[] { "B1", "B2", "B3" })
			};
		}
	}

	public class Issue30924Group : List<string>
	{
		public Issue30924Group(string name, IEnumerable<string> items) : base(items)
		{
			Name = name;
		}

		public string Name { get; }
	}
}
