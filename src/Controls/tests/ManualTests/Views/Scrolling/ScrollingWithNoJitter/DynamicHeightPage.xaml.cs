namespace Microsoft.Maui.ManualTests.Views
{
	public partial class DynamicHeightPage : ContentPage
	{
		public DynamicHeightPage()
		{
			InitializeComponent();
			var random = new Random();
			this.RandomDynamicRects.AddRange(Enumerable.Range(0, 100).Select(_ => new RandomDynamicRect
			{
				Height = random.Next(50, 200),
				Background = Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255))
			}));
			this.CollectionView.ItemsSource = this.RandomDynamicRects;
		}

		public List<RandomDynamicRect> RandomDynamicRects { get; } = new();

		public class RandomDynamicRect
		{
			public int Height { get; set; }

			public Color Background { get; set; }
		}
	}
}