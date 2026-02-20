using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32348, "CollectionView VSM Background and BackgroundColor work similarly", PlatformAffected.iOS | PlatformAffected.Android | PlatformAffected.macOS)]
public class Issue32348 : ContentPage
{
	public ObservableCollection<string> BackgroundItems { get; set; }
	public ObservableCollection<string> BackgroundColorItems { get; set; }

	public Issue32348()
	{
		BackgroundItems = new ObservableCollection<string>
		{
			"Item 1",
			"Item 2",
			"Item 3"
		};

		BackgroundColorItems = new ObservableCollection<string>
		{
			"Item 4",
			"Item 5",
			"Item 6"
		};

		BindingContext = this;

		var collectionView1 = new CollectionView
		{
			SelectionMode = SelectionMode.Multiple,
			HeightRequest = 44,
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				ItemSpacing = 10
			}
		};

		collectionView1.ItemsSource = BackgroundItems;

		collectionView1.ItemTemplate = new DataTemplate(() =>
		{
			var label = new Label
			{
				TextColor = Colors.White,
				FontSize = 16,
				VerticalOptions = LayoutOptions.Center,
				VerticalTextAlignment = TextAlignment.Center,
				HorizontalTextAlignment = TextAlignment.Center,
				AutomationId = "ItemLabel"
			};

			label.SetBinding(Label.TextProperty, ".");

			var border = new Border
			{
				StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(22) },
				HeightRequest = 44,
				VerticalOptions = LayoutOptions.Center,
				Padding = new Thickness(18, 0, 18, 8),
				Content = label,
				Background = Colors.Purple
			};

			var normalState = new VisualState { Name = "Normal" };
			normalState.Setters.Add(new Setter
			{
				Property = Border.BackgroundProperty,
				Value = Colors.Purple
			});

			var selectedState = new VisualState { Name = "Selected" };
			selectedState.Setters.Add(new Setter
			{
				Property = Border.BackgroundProperty,
				Value = Colors.DarkSlateGray
			});

			var commonStates = new VisualStateGroup { Name = "CommonStates" };
			commonStates.States.Add(normalState);
			commonStates.States.Add(selectedState);

			var visualStateGroups = new VisualStateGroupList
			{
				commonStates
			};

			VisualStateManager.SetVisualStateGroups(border, visualStateGroups);

			return border;
		});

		var collectionView2 = new CollectionView
		{
			SelectionMode = SelectionMode.Multiple,
			HeightRequest = 44,
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				ItemSpacing = 10
			}
		};

		collectionView2.ItemsSource = BackgroundColorItems;

		collectionView2.ItemTemplate = new DataTemplate(() =>
		{
			var label = new Label
			{
				TextColor = Colors.White,
				FontSize = 16,
				VerticalOptions = LayoutOptions.Center,
				VerticalTextAlignment = TextAlignment.Center,
				HorizontalTextAlignment = TextAlignment.Center,
				AutomationId = "ItemLabel"
			};

			label.SetBinding(Label.TextProperty, ".");

			var border = new Border
			{
				StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(22) },
				HeightRequest = 44,
				VerticalOptions = LayoutOptions.Center,
				Padding = new Thickness(18, 0, 18, 8),
				Content = label,
				BackgroundColor = Colors.Purple
			};

			var normalState = new VisualState { Name = "Normal" };
			normalState.Setters.Add(new Setter
			{
				Property = Border.BackgroundColorProperty,
				Value = Colors.Purple
			});

			var selectedState = new VisualState { Name = "Selected" };
			selectedState.Setters.Add(new Setter
			{
				Property = Border.BackgroundColorProperty,
				Value = Colors.DarkSlateGray
			});

			var commonStates = new VisualStateGroup { Name = "CommonStates" };
			commonStates.States.Add(normalState);
			commonStates.States.Add(selectedState);

			var visualStateGroups = new VisualStateGroupList
			{
				commonStates
			};

			VisualStateManager.SetVisualStateGroups(border, visualStateGroups);

			return border;
		});

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children =
			{
				new Label { Text = "CollectionView with Background property", FontAttributes = FontAttributes.Bold },
				collectionView1,
				new Label { Text = "CollectionView with BackgroundColor property", FontAttributes = FontAttributes.Bold },
				collectionView2
			}
		};
	}
}