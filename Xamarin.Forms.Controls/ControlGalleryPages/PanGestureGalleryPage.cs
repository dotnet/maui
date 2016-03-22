using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class PanGestureGalleryPage : ContentPage
	{
		public class PanContainer : ContentView
		{
			public PanContainer ()
			{
				var pan = new PanGestureRecognizer
				{
					TouchPoints = 1
				};

				pan.PanUpdated += (object s, PanUpdatedEventArgs e) =>
				{
					switch (e.StatusType) {

						case GestureStatus.Started: break;

						case GestureStatus.Running:
							Content.TranslationX = e.TotalX;
							Content.TranslationY = e.TotalY;
							break;

						default:
							Content.TranslationX = Content.TranslationY = 0;
							break;
					}
				};

				var pinch = new PinchGestureRecognizer ();

				double xOffset = 0;
				double yOffset = 0;
				double startScale = 1;

				pinch.PinchUpdated += (sender, e) =>
				{

					if (e.Status == GestureStatus.Started) {
						startScale = Content.Scale;
						Content.AnchorX = Content.AnchorY = 0;
					}
					if (e.Status == GestureStatus.Running) {

						_currentScale += (e.Scale - 1) * startScale;
						_currentScale = Math.Max (1, _currentScale);

						var renderedX = Content.X + xOffset;
						var deltaX = renderedX / Width;
						var deltaWidth = Width / (Content.Width * startScale);
						var originX = (e.ScaleOrigin.X - deltaX) * deltaWidth;

						var renderedY = Content.Y + yOffset;
						var deltaY = renderedY / Height;
						var deltaHeight = Height / (Content.Height * startScale);
						var originY = (e.ScaleOrigin.Y - deltaY) * deltaHeight;

						double targetX = xOffset - (originX * Content.Width) * (_currentScale - startScale);
						double targetY = yOffset - (originY * Content.Height) * (_currentScale - startScale);

						Content.TranslationX = targetX.Clamp (-Content.Width * (_currentScale - 1), 0);
						Content.TranslationY = targetY.Clamp (-Content.Height * (_currentScale - 1), 0);

						Content.Scale = _currentScale;
					}
					if (e.Status == GestureStatus.Completed) {
						xOffset = Content.TranslationX;
						yOffset = Content.TranslationY;
					}
				};

				GestureRecognizers.Add (pinch);

				GestureRecognizers.Add (pan);
			}

			double _currentScale = 1;
		}

		public PanGestureGalleryPage ()
		{
			var image = new Image { Source = "http://placehold.it/2000x2000", BackgroundColor = Color.Gray, WidthRequest = 2000, HeightRequest = 2000, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };

			var panme = new PanContainer { Content = image };

			Content = new StackLayout { Children = { new Label { Text = "Use two fingers to pinch. Use one finger to pan." }, panme }, Padding = new Thickness (20) };
		}
	}
}