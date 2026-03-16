using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33333, "CollectionView Scrolled event is triggered on the initial app load", PlatformAffected.Android)]
public class Issue33333 : ContentPage
{
	int _scrolledEventCount = 0;

	public Issue33333()
	{
		var items = new ObservableCollection<string>();
		for (int i = 0; i < 50; i++)
		{
			items.Add($"Item {i}");
		}

		var scrollCountLabel = new Label
		{
			AutomationId = "ScrollCountLabel",
			Text = "Scrolled Event Count: 0",
			FontSize = 18,
			Padding = new Thickness(10),
			BackgroundColor = Colors.LightYellow
		};

		var collectionView = new CollectionView
		{
			AutomationId = "TestCollectionView",
			ItemsSource = items,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = new Thickness(10),
					VerticalOptions = LayoutOptions.Center
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		collectionView.Scrolled += (sender, e) =>
		{
			_scrolledEventCount++;
			scrollCountLabel.Text = $"Scrolled Event Count: {_scrolledEventCount}";
		};

		var instructionLabel = new Label
		{
			AutomationId = "InstructionLabel",
			Text = "The Scrolled event count should remain 0 on initial load. On Android, it incorrectly fires immediately.",
			Padding = new Thickness(10),
			FontSize = 14,
			TextColor = Colors.Gray
		};

		Content = new StackLayout
		{
			Children = { scrollCountLabel, instructionLabel, collectionView }
		};
	}
}
