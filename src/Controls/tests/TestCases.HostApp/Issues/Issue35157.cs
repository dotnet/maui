using Microsoft.AspNetCore.Components.WebView.Maui;
using System.Runtime.Versioning;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35157, "Target blank link with data URI image crashes BlazorWebView", PlatformAffected.Android)]
[SupportedOSPlatform("android24.0")]
public class Issue35157 : ContentPage
{
	public Issue35157()
	{
		var blazorWebView = new BlazorWebView
		{
			AutomationId = "Issue35157BlazorWebView",
			HostPage = "wwwroot/index.html",
			HeightRequest = 300,
			WidthRequest = 300
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Children =
			{
				new Label
				{
					AutomationId = "Issue35157Instructions",
					Text = "Tap the image to open the link in a browser."
				},
				blazorWebView,
				new Label
				{
					AutomationId = "Issue35157SurvivalLabel",
					Text = "App is running"
				}
			}
		};
	}
}
