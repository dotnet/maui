using System;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class ContextFlyoutPage
	{
		public ContextFlyoutPage()
		{
			InitializeComponent();

			ImageContextCommand = new Command(
				execute: async (object arg) =>
				{
					await DisplayAlert(
						title: "Image",
						message: $"The image's context menu was clicked via a command with parameter: {arg}",
						cancel: "OK");
				});

			BindingContext = this;

			ContextMenuWebView.HandlerChanged += OnWebViewHandlerChanged;
		}


		void OnWebViewHandlerChanged(object sender, EventArgs e)
		{
			if (ContextMenuWebView.Handler != null)
			{
#if WINDOWS
				var webView2 = (Microsoft.UI.Xaml.Controls.WebView2)ContextMenuWebView.Handler.PlatformView;
				webView2.CoreWebView2Initialized += OnWebView2CoreWebView2Initialized;
#elif MACCATALYST
				var wkWebView = (WebKit.WKWebView)ContextMenuWebView.Handler.PlatformView;
				// TODO: Need to figure out how to disable default WKWebView context menu so that
				// the custom context flyout is shown instead. (It does sometimes show up for a second
				// but then it goes back to the default web context menu.)
#endif
			}
		}

#if WINDOWS
		void OnWebView2CoreWebView2Initialized(Microsoft.UI.Xaml.Controls.WebView2 sender, Microsoft.UI.Xaml.Controls.CoreWebView2InitializedEventArgs args)
		{
			sender.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
		}
#endif

		public ICommand ImageContextCommand { get; init; }

		int count;

		void OnIncrementByOneClicked(object sender, EventArgs e)
		{
			count++;
			OnPropertyChanged(nameof(CounterValue));
		}

		void OnIncrementMenuItemClicked(object sender, EventArgs e)
		{
			var menuItem = (MenuFlyoutItem)sender;
			var incrementAmount = int.Parse((string)menuItem.CommandParameter);
			count += incrementAmount;
			OnPropertyChanged(nameof(CounterValue));
		}

		public string CounterValue => count.ToString("N0");

		async void OnEntryShowTextClicked(object sender, EventArgs e)
		{
			await DisplayAlert(
				title: "Entry",
				message: $"The entry's text is: {EntryWithContextFlyout.Text}",
				cancel: "OK");
		}

		void OnEntryAddTextClicked(object sender, EventArgs e)
		{
			EntryWithContextFlyout.Text += " more text!";
		}

		void OnEntryClearTextClicked(object sender, EventArgs e)
		{
			EntryWithContextFlyout.Text = "";
		}

		async void OnImageContextClicked(object sender, EventArgs e)
		{
			await DisplayAlert(
				title: "Image",
				message: $"The image's context menu was clicked",
				cancel: "OK");
		}

		void OnWebViewGoToSiteClicked(object sender, EventArgs e)
		{
			ContextMenuWebView.Source = new UrlWebViewSource() { Url = "https://github.com/dotnet/maui", };
		}

		async void OnWebViewInvokeJSClicked(object sender, EventArgs e)
		{
			await ContextMenuWebView.EvaluateJavaScriptAsync(@"alert('help, i\'m being invoked!');");
		}

		void OnAddMenuClicked(object sender, EventArgs e)
		{
			var contextFlyout = ((MenuFlyoutItem)sender).Parent as MenuFlyout;
			contextFlyout.Add(new MenuFlyoutItem() { Text = "Thank you for adding me" });
		}

		void OnSubMenuClicked(object sender, EventArgs e)
		{
			var subMenu = ((MenuFlyoutSubItem)sender);
			subMenu.Add(new MenuFlyoutItem() { Text = "Thank you for adding me" });
		}
	}
}
