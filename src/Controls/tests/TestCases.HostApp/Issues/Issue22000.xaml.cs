namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 22000, "[Windows] Carousel changes the current position when window is resized", PlatformAffected.UWP)]
	public partial class Issue22000 : ContentPage
	{
		readonly Random _random;

		public Issue22000()
		{
			InitializeComponent();

			_random = new Random();

			var exampleItems = new List<Issue22000Model>
			{
				new Issue22000Model( "First", "First CarouselView item", Colors.Red ),
				new Issue22000Model( "Second", "Second CarouselView item", Colors.LightBlue ),
				new Issue22000Model( "Third", "Third CarouselView item", Colors.Pink ),
				new Issue22000Model( "Fourth", "Fourth CarouselView item", Colors.GreenYellow ),
				new Issue22000Model( "Fifth", "Fifth CarouselView item", Colors.Purple ),
			};

			TestCarouselView.ItemsSource = exampleItems;

			UpdateCarouselViewSize();
		}

		void OnUpdateSizeClicked(object sender, EventArgs e)
		{
			UpdateCarouselViewSize();
		}

		void UpdateCarouselViewSize()
		{
			var currentWidth = TestCarouselView.WidthRequest;

			if (currentWidth == 400)
				TestCarouselView.WidthRequest = 300;
			else
				TestCarouselView.WidthRequest = 200;
		}
	}

	class Issue22000Model
	{
		public Issue22000Model(string title, string description, Color color)
		{
			Title = title;
			Description = description;
			Color = color;
		}

		public string Title { get; set; }
		public string Description { get; set; }
		public Color Color { get; set; }
	}
}