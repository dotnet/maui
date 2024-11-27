﻿namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 19657, "CarouselView Content disappears when 'Loop' is false and inside ScrollView", PlatformAffected.iOS)]
	public partial class Issue19657 : ContentPage
	{

		public Issue19657()
		{
			InitializeComponent();
			var exampleItems = new List<SampleCarouselItem1>
			{
				new SampleCarouselItem1( "First", "First CarouselView item" ),
				new SampleCarouselItem1( "Second", "Second CarouselView item" ),
				new SampleCarouselItem1( "Third", "Third CarouselView item" ),
				new SampleCarouselItem1( "Fourth", "Fourth CarouselView item" ),
				new SampleCarouselItem1( "Fifth", "Fifth CarouselView item" ),
			};

			carousel.ItemsSource = exampleItems;
		}

		class SampleCarouselItem1
		{
			public SampleCarouselItem1(string title, string description)
			{
				Title = title;
				Description = description;
			}

			public string Title { get; set; }
			public string Description { get; set; }
			public Color Color { get; set; }
		}
	}
}