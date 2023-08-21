//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4919, "Webview Navigation cancel not working", PlatformAffected.Android)]
	public class Issue4919 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			var url = "https://www.microsoft.com/";
			var cancel = true;
			var log = new Label
			{
				VerticalOptions = LayoutOptions.StartAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Text = ""
			};
			var webView = new WebView()
			{
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Source = url
			};
			webView.Navigating += (_, e) =>
			{
				e.Cancel = cancel;
				var resultText = cancel ? "[Canceled]" : "[OK]";
				log.Text += $"{resultText} {e.Url}{System.Environment.NewLine}";
			};

			Content = new StackLayout
			{
				Children =
				{
					new Label { Text = "WebView must be empty on init" },
					webView,
					new Button
					{
						Text = "Go to github",
						Command = new Command(() => webView.Source = "https://github.com/xamarin/Xamarin.Forms")
					},
					new Button
					{
						Text = "Toggle cancel navigation",
						Command = new Command(() => cancel = !cancel)
					},
					new ScrollView
					{
						VerticalOptions = LayoutOptions.EndAndExpand,
						HorizontalOptions = LayoutOptions.FillAndExpand,
						Content = log
					}
				}
			};
		}
	}
}