using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14141, "Incorrect Intermediate CurrentItem updates with CarouselView Scroll Animation Enabled", PlatformAffected.Android)]
public class Issue14141 : ContentPage
{
	Label resultStatus;
	Label selectedItemLabel;
	CarouselView carouselView;
	public Issue14141()
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
			Text = "Selected Item : 0",
			FontSize = 16
		};

		Button selectButton = new Button
		{
			AutomationId = "Issue14141ScrollBtn",
			Text = "Select item 4",
			FontSize = 12
		};
		
		selectButton.Clicked += (s, e) => 
		{
			carouselView.CurrentItem = "4";
			selectedItemLabel.Text = $"Selected Item : {carouselView.CurrentItem}";
		};

		resultStatus = new Label
		{
			AutomationId = "Issue14141ResultLabel",
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

			if (!string.IsNullOrEmpty(currentItemValue) && currentItemValue != "0" &&
			    currentItemValue != "4")
			{
				resultStatus.Text = "Failure";
			}
		}
	}
}