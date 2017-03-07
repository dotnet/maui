using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	public class PanGestureGalleryPage : ContentPage
	{
		public class PanCompleteArgs : EventArgs
		{
			public PanCompleteArgs(string message) { Message = message; }
			public string Message
			{
				get; private set;
			}
		}

		public class PanContainer : ContentView
		{
			double _x, _y;
			double _currentScale = 1;

			public EventHandler<PanCompleteArgs> PanCompleted;

			public PanContainer()
			{
				GestureRecognizers.Add(GetPinch());
				GestureRecognizers.Add(GetPan());
			}

			PanGestureRecognizer GetPan()
			{
				var pan = new PanGestureRecognizer();
				pan.PanUpdated += (s, e) =>
				{
					switch (e.StatusType)
					{
						case GestureStatus.Running:
							Content.TranslationX = e.TotalX;
							Content.TranslationY = e.TotalY;
							break;

						case GestureStatus.Completed:
							_x = Content.TranslationX;
							_y = Content.TranslationY;

							PanCompleted?.Invoke(s, new PanCompleteArgs($"x: {_x}, y: {_y}"));
							break;
					}
				};
				return pan;
			}

			PinchGestureRecognizer GetPinch()
			{
				var pinch = new PinchGestureRecognizer();

				double xOffset = 0;
				double yOffset = 0;
				double startScale = 1;

				pinch.PinchUpdated += (sender, e) =>
				{

					if (e.Status == GestureStatus.Started)
					{
						startScale = Content.Scale;
						Content.AnchorX = Content.AnchorY = 0;
					}

					if (e.Status == GestureStatus.Running)
					{
						_currentScale += (e.Scale - 1) * startScale;
						_currentScale = Math.Max(1, _currentScale);

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

						Content.TranslationX = targetX.Clamp(-Content.Width * (_currentScale - 1), 0);
						Content.TranslationY = targetY.Clamp(-Content.Height * (_currentScale - 1), 0);

						Content.Scale = _currentScale;
					}

					if (e.Status == GestureStatus.Completed)
					{
						xOffset = Content.TranslationX;
						yOffset = Content.TranslationY;
					}
				};
				return pinch;
			}
		}

		public PanGestureGalleryPage()
		{
			var box = new Image
			{
				BackgroundColor = Color.Gray,
				WidthRequest = 2000,
				HeightRequest = 2000,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			};

			var label = new Label { Text = "Use two fingers to pinch. Use one finger to pan." };

			var panme = new PanContainer { Content = box };
			panme.PanCompleted += (s, e) =>
			{
				label.Text = e.Message;
			};

			Content = new StackLayout { Children = { label, panme }, Padding = new Thickness(20) };
		}
	}
}