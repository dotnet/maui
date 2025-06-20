using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Performance;

namespace Maui.Controls.Sample.Pages;

public class PerformanceProfilerGallery : ContentPage
{
	public PerformanceProfilerGallery()
	{
		var descriptionLabel =
			new Label { Text = "Performance Profiler Galleries", Margin = new Thickness(2, 2, 2, 2) };

		Title = "Performance Profiler Galleries";

		Content = new ScrollView
		{
			Content = new StackLayout
			{
				Children =
				{
					descriptionLabel,
					GalleryBuilder.NavButton("Get stats by request", () =>
						new StatsByRequestPage(), Navigation),
					GalleryBuilder.NavButton("Realtime stats", () =>
						new RealtimeStatsPage(), Navigation),
					GalleryBuilder.NavButton("Get Performance Warnings", () =>
						new PerformanceWarningsPage(), Navigation),
				}
			}
		};
	}
}