namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20042, "[iOS] ScrollView HorizontalOptions=StartAndExpand does not expand to fill screen width in landscape", PlatformAffected.iOS)]
public class Issue20042 : ContentPage
{
	public Issue20042()
	{
		var scrollView = new ScrollView
		{
			Orientation = ScrollOrientation.Vertical,
			HorizontalOptions = LayoutOptions.StartAndExpand,
			VerticalOptions = LayoutOptions.Fill,
		};

		var innerGrid = new Grid
		{
			BackgroundColor = Colors.LightBlue,
			AutomationId = "InnerGrid",
		};

		var label = new Label
		{
			Text = "ScrollView with HorizontalOptions=StartAndExpand\nIn landscape, this blue area must fill the full screen width.",
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center,
			Margin = new Thickness(16),
		};

		innerGrid.Add(label);
		scrollView.Content = innerGrid;

		// A reference element that always fills the full page width — used to compare against InnerGrid.
		var referenceBar = new BoxView
		{
			Color = Colors.Red,
			HeightRequest = 4,
			HorizontalOptions = LayoutOptions.Fill,
			AutomationId = "ReferenceBar",
		};

		Content = new Grid
		{
			RowDefinitions = new RowDefinitionCollection
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star },
			},
			Children =
			{
				referenceBar,
				scrollView,
			}
		};

		Grid.SetRow(referenceBar, 0);
		Grid.SetRow(scrollView, 1);
	}
}
