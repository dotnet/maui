using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31874, "Incorrect Intermediate CurrentItem updates with CarouselView Scroll Animation Enabled", PlatformAffected.Android)]
public class Issue31874 : ContentPage
{
	const string InitialItem = "0";
	const string TargetItem = "4";
	
	Label resultStatus;
	Label selectedItemLabel;
	CarouselView carouselView;
	public Issue31874()
	{
		carouselView = new CarouselView
		{
			HeightRequest = 150,
			Loop = false,
			IsScrollAnimated = true,
			ItemsSource = new ObservableCollection<string>{ "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11" },
			ItemTemplate = new DataTemplate(() =>
			{
				Border border = new Border
				{
					BackgroundColor = Colors.LightGray,
					Content = new Label
					{
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Center,
						FontSize = 80
					}
				};
				border.Content.SetBinding(Label.TextProperty, ".");
				return border;
			})
		};
		carouselView.PropertyChanged += CarouselView_PropertyChanged;

		selectedItemLabel = new Label
		{
			Text = $"Selected Item : {InitialItem}",
			FontSize = 16
		};

		Button selectButton = new Button
		{
			AutomationId = "Issue31874ScrollBtn",
			Text = $"Select item {TargetItem}",
			FontSize = 12
		};
		
		selectButton.Clicked += (s, e) => 
		{
			carouselView.CurrentItem = TargetItem;
			selectedItemLabel.Text = $"Selected Item : {carouselView.CurrentItem}";
		};

		resultStatus = new Label
		{
			AutomationId = "Issue31874ResultLabel",
			Text = "Success"
		};

		Content = new VerticalStackLayout
		{
			Padding = 25,
			Spacing = 15,
			Children =
			{
				carouselView,
				selectedItemLabel,
				selectButton,
				resultStatus
			}
		};
	}

	void CarouselView_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(CarouselView.CurrentItem))
		{
			var currentItemValue = carouselView.CurrentItem?.ToString();

			if (!string.IsNullOrEmpty(currentItemValue) && currentItemValue != InitialItem &&
			    currentItemValue != TargetItem)
			{
				resultStatus.Text = "Failure";
			}
		}
	}
}