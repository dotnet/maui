using System;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

public class WkWebView : WebView { }

namespace Xamarin.Forms.Controls.Issues
{

#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1666, "Use WKWebView on iOS", PlatformAffected.iOS)]
	public class Issue1666 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{
			var buttonBack = new Button() { Text = "<", BackgroundColor = Color.LightBlue, AutomationId = "buttonBack" };
			var buttonNext = new Button() { Text = ">", BackgroundColor = Color.LightBlue, AutomationId = "buttonNext" };
			var buttonClear = new Button() { Text = "--", BackgroundColor = Color.LightBlue, AutomationId = "buttonClear" };
			var buttonState = new Button() { Text = "?", BackgroundColor = Color.LightBlue, AutomationId = "buttonState" };
			var buttonStop = new Button() { Text = "X", BackgroundColor = Color.LightBlue, AutomationId = "buttonStop" };

			var buttonJsAlert = new Button() { Text = "ALERT", BackgroundColor = Color.LightBlue, AutomationId = "buttonJsAlert" };
			var buttonJsPrompt = new Button() { Text = "PROMPT", BackgroundColor = Color.LightBlue, AutomationId = "buttonJsPrompt" };
			var buttonJsConfirm = new Button() { Text = "CONFIRM", BackgroundColor = Color.LightBlue, AutomationId = "buttonJsConfirm" };

			var buttonA = new Button() { Text = "GO", BackgroundColor = Color.LightBlue, AutomationId = "buttonA" };
			var buttonB = new Button() { Text = "HTML", BackgroundColor = Color.LightBlue, AutomationId = "buttonB" };
			var buttonC = new Button() { Text = "EVAL", BackgroundColor = Color.LightBlue, AutomationId = "buttonC" };
			var buttonD = new Button() { Text = "AEVAL", BackgroundColor = Color.LightBlue, AutomationId = "buttonD" };

			var url = "https://www.microsoft.com/";
			var html = $"<html><body><a href=\"{url}\">Link</a></body></html>";

			var webView = new WkWebView()
			{
				HeightRequest = 40,
				Source = new HtmlWebViewSource { Html = html }
			};

			var vcr = new Grid();
			vcr.Children.AddHorizontal(new[] { buttonBack, buttonNext, buttonClear, buttonState, buttonStop });

			var evals = new Grid();
			evals.Children.AddHorizontal(new[] { buttonA, buttonB, buttonC, buttonD });

			var entry = new Entry() { AutomationId = "entry" };
			entry.BackgroundColor = Color.Wheat;

			var jsAlerts = new Grid();
			jsAlerts.Children.AddHorizontal(new[] { buttonJsAlert, buttonJsPrompt, buttonJsConfirm });

			var buttons = new Grid();
			buttons.Children.AddVertical(vcr);
			buttons.Children.AddVertical(evals);
			buttons.Children.AddVertical(jsAlerts);
			buttons.Children.AddVertical(entry);

			var console = new Label()
			{
				AutomationId = "console",
				Text = "Loaded\n"
			};
			Action<string> log = s => { console.Text = s + "\n" + console.Text; };

			var grid = new Grid();
			grid.Children.AddVertical(webView);
			grid.Children.AddVertical(buttons);
			grid.Children.AddVertical(new ScrollView() { Content = console });

			buttonA.Clicked += (s, e) => 
			{
				webView.Source = new UrlWebViewSource() { Url = url };
			};

			buttonB.Clicked += (s, e) => {
				webView.Source = new HtmlWebViewSource()
				{
					Html = html
				};
			};

			var js = "1 + 2";
			buttonC.Clicked += (s, e) => {
				log($"Eval: {js}");
				webView.Eval(js);
			};

			webView.EvalRequested += (s, e) =>
			{
				log($"EvalRequested: {e.Script}");
			};

			buttonD.Clicked += (s, e) => {
				log($"AEval: {js}");
				var promise = webView.EvaluateJavaScriptAsync(js);
				promise.ContinueWith(a => Device.BeginInvokeOnMainThread(() => log($"Evaled: {a.Result}")));
			};

			bool cancel = false;
			buttonNext.Clicked += (s, e) => { webView.GoForward(); log($"GoForward: {webView.CanGoBack}/{webView.CanGoForward}"); };
			buttonBack.Clicked += (s, e) => { webView.GoBack(); log($"GoBack: {webView.CanGoBack}/{webView.CanGoForward}"); };
			buttonClear.Clicked += (s, e) => { console.Text = ""; };
			buttonStop.Clicked += (s, e) => { cancel = true; log("Cancelling navigation"); };
			buttonState.Clicked += (s, e) => {
				log($"F/B: {webView.CanGoBack}/{webView.CanGoForward}");
				log($"Source: {webView.Source.ToString()}");
			};

			buttonJsAlert.Clicked += async (s, e) => { await webView.EvaluateJavaScriptAsync("alert('foo')"); };
			buttonJsPrompt.Clicked += async (s, e) => { log($"{await webView.EvaluateJavaScriptAsync("prompt('enter something:')")} was enterred"); };
			buttonJsConfirm.Clicked += async (s, e) => { log($"{await webView.EvaluateJavaScriptAsync("confirm('choose')")} was chosen"); };

			bool navigating = false;
			webView.Navigating += (s, e) =>
			{
				entry.Text = e.Url;
				entry.BackgroundColor = Color.LightPink;

				if (!navigating)
				{
					log("Navigating");
					navigating = true;
				}

				if (cancel)
				{
					e.Cancel = true;
					log("Cancel navigation");
					cancel = false;
				}
			};

			webView.Navigated += (s, e) =>
			{
				var text = $"Navigated {e.NavigationEvent}, ";
				text += $"Result: {e.Result}";
				log(text);

				entry.Text = e.Url;
				entry.BackgroundColor = Color.LightBlue;

				cancel = false;
				navigating = false;
			};

			// Initialize ui here instead of ctor
			Content = grid;
			BackgroundColor = Color.Gray;
		}
	}
}