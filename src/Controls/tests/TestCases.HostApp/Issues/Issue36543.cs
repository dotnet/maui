namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36543, "[Android] RTL CollectionView content is truncated / shifted into the display cutout after landscape rotation", PlatformAffected.Android)]
public class Issue36543 : ContentPage
{
	static readonly IReadOnlyList<Issue36543Monkey> MonkeyData = new[]
	{
		new Issue36543Monkey("Baboon", "Africa & Asia"),
		new Issue36543Monkey("Capuchin Monkey", "Central & South America"),
		new Issue36543Monkey("Blue Monkey", "Central and East Africa"),
		new Issue36543Monkey("Squirrel Monkey", "Central & South America"),
		new Issue36543Monkey("Golden Lion Tamarin", "Brazil"),
		new Issue36543Monkey("Howler Monkey", "South America"),
	};

	public Issue36543()
	{
		Title = "Issue36543";
		FlowDirection = FlowDirection.RightToLeft;
		SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);

		Content = new Grid
		{
			Margin = 20,
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star },
			},
			Children =
			{
				new Label
				{
					FlowDirection = FlowDirection.LeftToRight,
					Text = "RTL CollectionView content should not be truncated / shifted into the display cutout after landscape rotation.",
				},
				new CollectionView
				{
					AutomationId = "Issue36543CollectionView",
					ItemsSource = MonkeyData,
					ItemTemplate = new DataTemplate(CreateItemTemplate),
				}.Row(1),
			},
		};
	}

	static View CreateItemTemplate()
	{
		var image = new Image
		{
			Source = "dotnet_bot.png",
			Aspect = Aspect.AspectFill,
			HeightRequest = 60,
			WidthRequest = 60,
		};
		Grid.SetRowSpan(image, 2);

		var nameLabel = new Label { FontAttributes = FontAttributes.Bold }.Column(1);
		nameLabel.SetBinding(Label.TextProperty, nameof(Issue36543Monkey.Name));

		var locationLabel = new Label { FontAttributes = FontAttributes.Italic, VerticalOptions = LayoutOptions.End }.Row(1).Column(1);
		locationLabel.SetBinding(Label.TextProperty, nameof(Issue36543Monkey.Location));

		return new Grid
		{
			Padding = 10,
			RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
			ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
			Children = { image, nameLabel, locationLabel },
		};
	}

	class Issue36543Monkey
	{
		public Issue36543Monkey(string name, string location)
		{
			Name = name;
			Location = location;
		}

		public string Name { get; }
		public string Location { get; }
	}
}
