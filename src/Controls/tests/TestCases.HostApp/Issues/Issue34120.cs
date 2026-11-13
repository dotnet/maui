namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34120, "Label text truncated in ScrollView when MaxLines is set", PlatformAffected.Android)]
public class Issue34120 : ContentPage
{
	// Reproduces the N3_Navigation layout: horizontal ScrollView with BindableLayout,
	// 200×200 Border cards, Image (HeightRequest=120), and a Label with MaxLines=2.
	record Issue34120MonkeyItem(string Name, string ImageUrl);

	public Issue34120()
	{
		// Long names ("Golden Snub-nosed Monkey", "Tonkin Snub-nosed Monkey") are the ones
		// that triggered truncation; "Baboon" is a short-name reference card.
		var monkeys = new List<Issue34120MonkeyItem>
		{
			new("Golden Snub-nosed Monkey", "golden.jpg"),
			new("Baboon",                   "papio.jpg"),
			new("Tonkin Snub-nosed Monkey", "bluemonkey.jpg"),
			new("Howler Monkey",            "alouatta.jpg"),
			new("Squirrel Monkey",          "saimiri.jpg"),
		};

		var itemTemplate = new DataTemplate(() =>
		{
			var image = new Image
			{
				Aspect = Aspect.AspectFit,
				HeightRequest = 120,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			};
			image.SetBinding(Image.SourceProperty, "ImageUrl");

			var nameLabel = new Label
			{
				FontSize = 14,
				FontAttributes = FontAttributes.Bold,
				BackgroundColor = Color.FromArgb("#AAFFFFFF"),
				TextColor = Colors.Black,
				Padding = new Thickness(4, 2),
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				HorizontalTextAlignment = TextAlignment.Center,
				LineBreakMode = LineBreakMode.WordWrap,
				MaxLines = 2,
			};
			nameLabel.SetBinding(Label.TextProperty, "Name");
			nameLabel.SetBinding(Label.AutomationIdProperty, "Name");

			var card = new Border
			{
				Padding = new Thickness(10),
				Stroke = Colors.LightGray,
				StrokeThickness = 1,
				WidthRequest = 200,
				HeightRequest = 200,
				BackgroundColor = Colors.White,
				Content = new VerticalStackLayout
				{
					Spacing = 10,
					Children = { image, nameLabel }
				}
			};
			return card;
		});

		var horizontalStack = new HorizontalStackLayout
		{
			Spacing = 15,
			Padding = new Thickness(5),
		};
		BindableLayout.SetItemTemplate(horizontalStack, itemTemplate);
		BindableLayout.SetItemsSource(horizontalStack, monkeys);

		Content = new ScrollView
		{
			Orientation = ScrollOrientation.Horizontal,
			Content = horizontalStack,
		};
	}
}
