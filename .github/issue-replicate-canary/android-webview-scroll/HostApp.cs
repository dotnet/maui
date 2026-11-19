#if ANDROID
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38452, "Non-scrollable WebView forwards vertical gestures to its parent ScrollView", PlatformAffected.Android)]
public class Issue38452 : ContentPage
{
	public Issue38452()
	{
		var position = new Label { AutomationId = "OuterScrollPosition", Text = "0" };
		var readiness = new Label { AutomationId = "ShortWebViewState", Text = "Loading local HTML" };
		var shortWebView = new WebView
		{
			AutomationId = "ShortWebView",
			HeightRequest = 200,
			Source = new HtmlWebViewSource
			{
				Html = "<p style='font-family:sans-serif'>Short content that fits inside the WebView.</p>"
			}
		};
		var longWebView = new WebView
		{
			AutomationId = "LongWebView",
			HeightRequest = 200,
			Source = new HtmlWebViewSource
			{
				Html = "<div style='font-family:sans-serif'>" +
					string.Concat(Enumerable.Range(1, 20).Select(i => $"<p>Line {i}</p>")) + "</div>"
			}
		};
		var loaded = false;
		shortWebView.Navigated += (_, args) => loaded = args.Result == WebNavigationResult.Success;
		var scrollView = new ScrollView
		{
			AutomationId = "OuterScrollView",
			Content = new VerticalStackLayout
			{
				Padding = 16,
				Spacing = 16,
				Children =
				{
					new Label
					{
						AutomationId = "ControlSurface",
						Text = "Control: drag upward here. The outer page should scroll.",
						HeightRequest = 200,
						BackgroundColor = Colors.LightGray
					},
					new Label { Text = "Repro: drag upward on the short WebView below. Its HTML does not overflow." },
					shortWebView,
					new Label { Text = "The overflowing WebView remains in the author layout; its own scrolling is not asserted." },
					longWebView,
					new Label { Text = "Ordinary scrollable page content", HeightRequest = 200, BackgroundColor = Colors.LightGray },
					new Label { Text = "Bottom of page", HeightRequest = 600, BackgroundColor = Colors.LightGray }
				}
			}
		};
		scrollView.Scrolled += (_, args) => position.Text = args.ScrollY.ToString("F1", CultureInfo.InvariantCulture);
		var reset = new Button { AutomationId = "ResetOuterScroll", Text = "Reset scroll" };
		reset.Clicked += async (_, _) =>
		{
			await scrollView.ScrollToAsync(0, 0, false);
			position.Text = scrollView.ScrollY.ToString("F1", CultureInfo.InvariantCulture);
		};
		var probe = new Button { AutomationId = "ProbeShortWebView", Text = "Check HTML" };
		probe.Clicked += (_, _) =>
		{
			if (loaded && shortWebView.Handler?.PlatformView is global::Android.Webkit.WebView native &&
				native.Height > 0 && native.ContentHeight > 0)
			{
				readiness.Text = native.CanScrollVertically(1) || native.CanScrollVertically(-1)
					? "Short HTML overflows"
					: "Short HTML loaded; no vertical overflow";
			}
		};
		var layout = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			}
		};
		layout.Add(new HorizontalStackLayout { Children = { reset, probe } });
		layout.Add(new HorizontalStackLayout { Spacing = 12, Children = { position, readiness } }, 0, 1);
		layout.Add(scrollView, 0, 2);
		Content = layout;
	}
}
#endif
