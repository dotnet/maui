using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Performance;

namespace Maui.Controls.Sample.Pages;

/// <summary>
/// A sample page intentionally designed to simulate poor navigation performance
/// by introducing a 2-second blocking delay during page construction. 
/// Useful for testing and validating MAUI's performance tracking, particularly 
/// navigation duration thresholds and warning generation.
/// </summary>
public class SimulatedLatencyPage : ContentPage
{
	readonly IPerformanceProfiler _performanceProfiler;
	
	public SimulatedLatencyPage()
	{
		Content = new Label
		{
			Text = "Simulated Latency Page",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
		
		var serviceProvider = IPlatformApplication.Current?.Services;
		_performanceProfiler = serviceProvider?.GetService<IPerformanceProfiler>() 
		                       ?? throw new InvalidOperationException("IPerformanceProfiler service not found");
		
		_performanceProfiler.Warnings.WarningRaised += OnPerformanceWarning;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		
		// Introduce a 2-second delay to simulate slow page navigation
		Thread.Sleep(2000);
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_performanceProfiler.Warnings.WarningRaised -= OnPerformanceWarning;
	}

	void OnPerformanceWarning(object sender, PerformanceWarningEventArgs e)
	{
		if (e.Warning.Level != PerformanceWarningLevel.Info)
		{
			DisplayAlert("Performance Issue", $"Information: {e.Warning.Message}, Recommendation: {e.Warning.Recommendation}", "OK");
		}
	}
}