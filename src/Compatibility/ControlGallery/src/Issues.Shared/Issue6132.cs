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
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6132, "[Bug] [Forms 4.0] [Android] WebView.EnableZoomControls platform-specific doesn't work ", PlatformAffected.Android)]
	public class Issue6132 : TestContentPage
	{
		protected override void Init()
		{
			Title = "WebView Zoom Controls";
			WebView webView = new WebView
			{
				Source = "https://www.xamarin.com"
			};

			webView.On<Android>().EnableZoomControls(true);
			webView.On<Android>().DisplayZoomControls(true);

			Content = webView;
		}
	}
}