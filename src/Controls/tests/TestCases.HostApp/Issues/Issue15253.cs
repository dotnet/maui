using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 15253, "[Windows]HorizontalScrollBarVisibility doesnot works", PlatformAffected.UWP)]

public class Issue15253 : ContentPage
{
	ObservableCollection<Model15253> Items;
	public Issue15253()
	{
		Items = new ObservableCollection<Model15253>();
		Items.Add(new Model15253 { Name = "one", AutomationId = "15253One" });
		Items.Add(new Model15253 { Name = "two", AutomationId = "15253Two" });
		Items.Add(new Model15253 { Name = "three", AutomationId = "15253Three" });
		var carouselView = new CarouselView
		{
			WidthRequest = 200,
			HeightRequest = 200,
			AutomationId = "15253CarouselView",
			Loop = false,
			VerticalScrollBarVisibility = ScrollBarVisibility.Never,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
			BackgroundColor = Colors.LightBlue,
			ItemsSource = Items,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					FontSize = 32,
					TextColor = Colors.Black,
					HorizontalTextAlignment = TextAlignment.Center,
					BackgroundColor = Colors.LightBlue

				};
				label.SetBinding(Label.TextProperty, "Name");
				label.SetBinding(Label.AutomationIdProperty, "AutomationId");

				var stack = new StackLayout
				{
					Orientation = StackOrientation.Vertical,
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					WidthRequest = 200,
					Children = { label }
				};
				return new ContentView { Content = stack };
			})
		};


		var toggleButton = new Button
		{
			Text = "Toggle Scrollbar",
			AutomationId = "15253Button"
		};

		toggleButton.Clicked += (s, e) =>
		{
			carouselView.HorizontalScrollBarVisibility = carouselView.HorizontalScrollBarVisibility == ScrollBarVisibility.Never
				? ScrollBarVisibility.Always
				: ScrollBarVisibility.Never;
		};

		var outerStack = new StackLayout
		{
			Orientation = StackOrientation.Vertical,
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			Spacing = 8,
			Padding = 8,
			Children = { toggleButton, carouselView }
		};

		Content = outerStack;
	}

}
public class Model15253
{
	public string Name { get; set; }
	public string AutomationId { get; set; }
}