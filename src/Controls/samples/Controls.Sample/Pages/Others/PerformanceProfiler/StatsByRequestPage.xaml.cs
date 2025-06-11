using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Performance;

namespace Maui.Controls.Sample.Pages
{
	public partial class StatsByRequestPage
	{
		readonly IPerformanceProfiler _performanceProfiler;
		
		public StatsByRequestPage()
		{
			InitializeComponent();

			var serviceProvider = IPlatformApplication.Current?.Services;
			_performanceProfiler = serviceProvider?.GetService<IPerformanceProfiler>() 
			                       ?? throw new InvalidOperationException("IPerformanceProfiler service not found");
		}

		void OnGetStatsClicked(object? sender, EventArgs e)
		{
			var imageStats = _performanceProfiler.Image.GetStats();
			ImageStatsLabelTitle.Text = "Layout Performance Stats";
			ImageStatsLabel.Text = $"LoadDuration: {imageStats.LoadDuration} ms";
			
			var layoutStats = _performanceProfiler.Layout.GetStats();
			LayoutStatsLabelTitle.Text = "Layout Performance Stats";
			LayoutStatsLabel.Text = $"ArrangeDuration: {layoutStats.ArrangeDuration}, MeasureDuration: {layoutStats.MeasureDuration}";
		}
	}
}