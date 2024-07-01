namespace Maui.Controls.Sample
{
	public class DynamicTapGestureGallery : Microsoft.Maui.Controls.ContentView
	{
		public DynamicTapGestureGallery()
		{
			AutomationId = nameof(DynamicTapGestureGallery);

			var layout = new VerticalStackLayout { Margin = 10, Spacing = 10 };

			var singleTapResults = new Label { AutomationId = "DynamicTapGestureResults", Text = "0" };
			var singleTapSurface = new Grid()
			{
				HeightRequest = 100,
				WidthRequest = 200,
				BackgroundColor = Microsoft.Maui.Graphics.Colors.AliceBlue,
				Children = { new Label { Text = "DynamicTapSurface", AutomationId = "DynamicTapSurface" } }
			};

			var tapCount = 0;

			var singleTapRecognizer = new TapGestureRecognizer();
			singleTapRecognizer.Tapped += (sender, args) => {
				tapCount++;
				singleTapResults.Text = tapCount.ToString();

				var dynamicTapRecognizer = new TapGestureRecognizer();
				dynamicTapRecognizer.Tapped += (sender, args) =>
				{
					Console.WriteLine("DynamicTap"); 
				};
				singleTapSurface.GestureRecognizers.Add(dynamicTapRecognizer);
			};
			singleTapSurface.GestureRecognizers.Add(singleTapRecognizer);

			layout.Add(singleTapSurface);
			layout.Add(singleTapResults);

			Content = layout;
		}
	}
}