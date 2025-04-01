namespace Maui.Controls.Sample
{
	public class SingleTapGallery : Microsoft.Maui.Controls.ContentView
	{
		public SingleTapGallery()
		{
			AutomationId = nameof(SingleTapGallery);

			var layout = new VerticalStackLayout { Margin = 10, Spacing = 10 };

			var singleTapResults = new Label { AutomationId = "SingleTapGestureResults", Text = "Should succeed." };
			var singleTapSurface = new Grid()
			{
				HeightRequest = 100,
				WidthRequest = 200,
				BackgroundColor = Microsoft.Maui.Graphics.Colors.AliceBlue,
				Children = { new Label { Text = "SingleTapSurface", AutomationId = "SingleTapSurface" } }
			};

			var singleTapRecognizer = new TapGestureRecognizer();
			singleTapRecognizer.Tapped += (sender, args) => { singleTapResults.Text = "Success"; };
			singleTapSurface.GestureRecognizers.Add(singleTapRecognizer);

			layout.Add(singleTapSurface);
			layout.Add(singleTapResults);


			// Now add a tap surface that is disabled and a results label that should not change when the surface is tapped
			var disabledTapResults = new Label { AutomationId = "DisabledTapGestureResults", Text = "Should not change when tapped" };
			var disabledTapSurface = new Grid()
			{
				IsEnabled = false, // Disabled
				HeightRequest = 100,
				WidthRequest = 200,
				BackgroundColor = Microsoft.Maui.Graphics.Colors.Bisque,
				Children = { new Label { Text = "DisabledTapSurface", AutomationId = "DisabledTapSurface" } }
			};

			// Wire up the tap gesture recognizer, it should never fire
			var disabledTapRecognizer = new TapGestureRecognizer();
			disabledTapRecognizer.Tapped += (sender, args) => { disabledTapResults.Text = "Failed"; };
			disabledTapSurface.GestureRecognizers.Add(disabledTapRecognizer);

			layout.Add(disabledTapSurface);
			layout.Add(disabledTapResults);

			Content = layout;
		}
	}
}