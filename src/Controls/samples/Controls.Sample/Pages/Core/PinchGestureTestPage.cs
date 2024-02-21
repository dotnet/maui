using System;
using System.Linq;
using Maui.Controls.Sample.Pages.Base;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public class PinchToZoomContainer : VerticalStackLayout
	{
		public PinchToZoomContainer()
		{

		}

		public View? Content
		{
			get => Children.OfType<View>().LastOrDefault();
			set
			{
				if (Children.Count > 0)
					Remove(Children[0]);

				Add(value);
			}
		}

		public void AddPinch()
		{

			var pinch = new PinchGestureRecognizer();

			double xOffset = 0;
			double yOffset = 0;
			double startScale = 1;

			pinch.PinchUpdated += (sender, e) =>
			{

				if (e.Status == GestureStatus.Started)
				{
					startScale = Content!.Scale;
					Content.AnchorX = Content.AnchorY = 0;
				}
				if (e.Status == GestureStatus.Running)
				{

					_currentScale += (e.Scale - 1) * startScale;
					_currentScale = Math.Max(1, _currentScale);

					var renderedX = Content!.X + xOffset;
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
					xOffset = Content!.TranslationX;
					yOffset = Content!.TranslationY;
				}
			};

			GestureRecognizers.Add(pinch);
		}

		public bool AlwaysZoomCenter { get; set; }

		double _currentScale = 1;
	}

	public class PinchGestureTestPage : BasePage
	{
		public PinchGestureTestPage()
		{
			var stack = new StackLayout { VerticalOptions = LayoutOptions.Start, HorizontalOptions = LayoutOptions.Center };
			var textBoxScale = new Label { VerticalOptions = LayoutOptions.Start, HorizontalOptions = LayoutOptions.Center };
			var textBox = new Label { VerticalOptions = LayoutOptions.Start, HorizontalOptions = LayoutOptions.Center };
			var textBoxPoint = new Label { VerticalOptions = LayoutOptions.Start, HorizontalOptions = LayoutOptions.Center };
			stack.Children.Add(textBox);
			stack.Children.Add(textBoxScale);
			stack.Children.Add(textBoxPoint);

			var box = new Image { Source = "crimson.jpg", BackgroundColor = Colors.Red, WidthRequest = 200, HeightRequest = 200, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };

			var zoomContainer = new PinchToZoomContainer();
			zoomContainer.Content = box;

			var btn = new Button { Text = "add pinch gesture", Command = new Command(() => zoomContainer.AddPinch()) };
			var btnRemove = new Button { Text = "remove pinch gesture", Command = new Command(() => zoomContainer.GestureRecognizers.Clear()) };

			Content = new StackLayout { Children = { btn, btnRemove, new Grid { Children = { zoomContainer }, Padding = new Thickness(20) } } };
		}

	}
}

