using System;
using System.Linq;
using Maui.Controls.Sample.Pages.Base;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public class PanGesturePlaygroundGallery : BasePage
	{
		public class PanCompleteArgs : EventArgs
		{
			public PanCompleteArgs(string message) { Message = message; }

			public string Message
			{
				get; private set;
			}
		}

		public class PanContainer : VerticalStackLayout
		{
			double _x, _y;
			double _currentScale = 1;

			public EventHandler<PanCompleteArgs>? PanCompleted;

			public PanContainer()
			{
				GestureRecognizers.Add(GetPinch());
				GestureRecognizers.Add(GetPan());
			}

			public View Content
			{
				get => (View)Children.Last();
				set
				{
					if (Children.Count > 0)
						Remove(Children[0]);

					Add(value);
				}
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

						Content.TranslationX = Math.Clamp(targetX, -Content.Width * (_currentScale - 1), 0);
						Content.TranslationY = Math.Clamp(targetY, -Content.Height * (_currentScale - 1), 0);

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

		public PanGesturePlaygroundGallery()
		{
			Title = "PanGesture Playground";

			var box = new Image
			{
				BackgroundColor = Colors.Gray,
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