#if IOS
using System.Collections.Generic;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38276, "CollectionView height does not shrink after replacing ItemsSource on iOS", PlatformAffected.iOS)]
public class Issue38276 : TestContentPage
{
	protected override void Init()
	{
		var heightLabel = new Label
		{
			AutomationId = "Issue38276Height",
			Text = "Height: not measured",
			FontSize = 16,
			TextColor = Colors.Blue
		};
		var sourceStatus = new Label
		{
			AutomationId = "Issue38276SourceStatus",
			Text = "Items: 0; replacement: 0",
			FontSize = 12
		};
		var replacement = 0;
		var collectionView = new CollectionView
		{
			AutomationId = "Issue38276Collection",
			Background = Colors.Blue,
			SelectionMode = SelectionMode.None,
			Margin = new Thickness(0),
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
			{
				ItemSpacing = 2
			}
		};

		collectionView.SizeChanged += (_, _) =>
			heightLabel.Text = $"Height: {collectionView.Height:F2}";

		collectionView.ItemTemplate = new DataTemplate(() =>
		{
			var border = new Border
			{
				StrokeThickness = 0,
				Padding = new Thickness(8, 4),
				Margin = new Thickness(0, 2),
				StrokeShape = new RoundRectangle
				{
					CornerRadius = 3
				}
			};
			var label = new Label
			{
				FontSize = 13,
				FontAttributes = FontAttributes.Bold,
				LineBreakMode = LineBreakMode.TailTruncation
			};
			label.SetBinding(Label.TextProperty, ".");
			border.Content = label;
			return border;
		});

		var grid = new Grid
		{
			RowSpacing = 6,
			MaximumHeightRequest = 180,
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Star }
			}
		};
		grid.Add(collectionView);

		void ReplaceItems(List<string> items)
		{
			collectionView.ItemsSource = items;
			replacement++;
			sourceStatus.Text = $"Items: {items.Count}; replacement: {replacement}";
		}

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Children =
			{
				new Button
				{
					AutomationId = "Issue38276TenItems",
					Text = "10 Items",
					Command = new Command(() => ReplaceItems(new List<string>
					{
						"Appointment 1",
						"Appointment 2",
						"Appointment 3",
						"Appointment 4",
						"Appointment 5",
						"Appointment 6",
						"Appointment 7",
						"Appointment 8",
						"Appointment 9",
						"Appointment 10"
					}))
				},
				new Button
				{
					AutomationId = "Issue38276OneItem",
					Text = "one Item",
					Command = new Command(() => ReplaceItems(new List<string>
					{
						"Appointment 1"
					}))
				},
				heightLabel,
				sourceStatus,
				grid
			}
		};
	}
}
#endif
