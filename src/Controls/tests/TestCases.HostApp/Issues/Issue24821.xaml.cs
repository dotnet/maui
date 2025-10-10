using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 24821, "Support navigating with URLs for HybridWebView", PlatformAffected.All)]
	public partial class Issue24821 : ContentPage
	{
		public Issue24821()
		{
			InitializeComponent();
			statusLabel.Text = "Status: HybridWebView loaded";
		}

		private void OnNavigateClicked(object sender, EventArgs e)
		{
			try
			{
				var source = sourceEntry.Text?.Trim();
				if (string.IsNullOrEmpty(source))
				{
					statusLabel.Text = "Status: Please enter a route";
					return;
				}

				hybridWebView.Source = source;
				statusLabel.Text = $"Status: Navigated to {source}";
			}
			catch (Exception ex)
			{
				statusLabel.Text = $"Status: Error - {ex.Message}";
			}
		}

		private async void OnGetLocationClicked(object sender, EventArgs e)
		{
			try
			{
				var location = await hybridWebView.EvaluateJavaScriptAsync("window.location.href");
				statusLabel.Text = $"Status: Current location is {location}";
			}
			catch (Exception ex)
			{
				statusLabel.Text = $"Status: Error getting location - {ex.Message}";
			}
		}
	}
}