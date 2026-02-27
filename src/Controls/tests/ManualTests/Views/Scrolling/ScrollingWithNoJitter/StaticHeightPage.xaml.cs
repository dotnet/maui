namespace Microsoft.Maui.ManualTests.Views
{
	public partial class StaticHeightPage : ContentPage
	{
		public StaticHeightPage()
		{
			InitializeComponent();
			var random = new Random();
			this.RandomStaticRects.AddRange(Enumerable.Range(0, 100).Select(_ => new RandomStaticRect
			{
				Background = Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255))
			}));
			this.CollectionView.ItemsSource = this.RandomStaticRects;
		}

		public List<RandomStaticRect> RandomStaticRects { get; } = new();

		public class RandomStaticRect
		{
			public Color Background { get; set; }
		}
	}
}