namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 13436,
		"[Bug] Java.Lang.IllegalArgumentException in CarouselView adjusting PeekAreaInsets in OnSizeAllocated using XF 5.0",
		PlatformAffected.Android)]
	public partial class CarouselViewAdjustPeekAreaInsets : ContentPage
	{
		double _prevWidth;

		public CarouselViewAdjustPeekAreaInsets()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			Carousel.ItemsSource = new List<Issue13436Model>
			{
				new Issue13436Model
				{
					Name = "N1",
					Desc = "D1",
					Color = Colors.Yellow
				},
				new Issue13436Model
				{
					Name = "N2",
					Desc = "D2",
					Color = Colors.Orange
				},
				new Issue13436Model
				{
					Name = "N3",
					Desc = "D3",
					Color = Colors.AliceBlue
				}
			};
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);

			if (Math.Abs(width - _prevWidth) < .1)
			{
				return;
			}

			_prevWidth = width;
			Carousel.PeekAreaInsets = width * .15;
		}
	}

	public class Issue13436Model
	{
		public string Name { get; set; }
		public string Desc { get; set; }
		public Color Color { get; set; }
		public double Scale { get; set; }
	}
}