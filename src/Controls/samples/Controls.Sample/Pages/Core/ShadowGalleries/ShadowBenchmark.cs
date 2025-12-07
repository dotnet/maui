using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages;

public class ShadowBenchmark : ContentPage
{
	event Action timerUpdateEvent;

	public ShadowBenchmark()
	{
		Title = "Shadow Benchmark";

		double fps = 60;

		var timer = Dispatcher.CreateTimer();
		timer.Interval = TimeSpan.FromSeconds(1 / fps);
		double time = 0;
		DateTime currentTickDateTime = DateTime.Now;
		double deltaTime = 0;

		timer.Tick += delegate
		{
			deltaTime = (DateTime.Now - currentTickDateTime).TotalSeconds;
			currentTickDateTime = DateTime.Now;
			time += deltaTime;
			timerUpdateEvent?.Invoke();
		};

		timer.Start();

		AbsoluteLayout rootAbs = new AbsoluteLayout();
		Content = rootAbs;

		Label fpsLabel = new Label
		{
			BackgroundColor = Colors.Black,
			TextColor = Colors.White
		};

		AbsoluteLayout abs = new AbsoluteLayout();
		rootAbs.Add(abs);
		rootAbs.Add(fpsLabel);

		VerticalStackLayout vert = new VerticalStackLayout();
		vert.BackgroundColor = Colors.LightGray;
		abs.Add(vert);

		AbsoluteLayout abs2 = new AbsoluteLayout();
		vert.Add(abs2);

		int numBorders = 3;
		List<Border> borders = new List<Border>();

		for (int i = 0; i < numBorders; i++)
		{
			Border border = new Border
			{
				HeightRequest = 200,
				WidthRequest = 500,
				BackgroundColor = Colors.DimGray
			};
			abs2.Add(border);
			borders.Add(border);
			border.Shadow = new Shadow() { Brush = new SolidColorBrush(Colors.Black), Offset = new Point(5, 5), Radius = 5 };
		}

		Border borderBot = new Border
		{
			HeightRequest = 200,
			WidthRequest = 50,
			BackgroundColor = Colors.DarkGray
		};
		vert.Add(borderBot);

		timerUpdateEvent += delegate
		{
			double sinVal = (Math.Sin(time) + 1) * 0.5;
			double height = 20 + sinVal * 800;
			for (int i = 0; i < borders.Count; i++)
			{
				borders[i].HeightRequest = height;
			}
		};

		SizeChanged += delegate
		{
			if (Width > 0)
			{
				abs.WidthRequest = Width;
				abs.HeightRequest = Height;
				vert.WidthRequest = Width;

				for (int i = 0; i < borders.Count; i++)
				{
					borders[i].WidthRequest = Width;
					borders[i].TranslationX = Width * i;
				}
			}
		};

		DateTime lastUpdateDateTime = DateTime.Now;
		abs2.SizeChanged += delegate
		{
			if (lastUpdateDateTime != DateTime.Now)
			{
				string fps = "FPS " + 1 / (DateTime.Now - lastUpdateDateTime).TotalSeconds;
				fpsLabel.Text = fps;

				lastUpdateDateTime = DateTime.Now;
			}
		};
	}
}
