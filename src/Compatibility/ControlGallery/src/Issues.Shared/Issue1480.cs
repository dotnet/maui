﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1480, "[iOS] Change CALayer.Transform in MainThread", PlatformAffected.iOS)]
	public class Issue1480 : TestContentPage
	{
		protected override void Init()
		{
			Content = new ContentView();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			var checkWebView = new WebView
			{
				Source = new HtmlWebViewSource
				{
					Html = @"<html>
<body>
<h1>Hey, I'm still here.\n\n</h1>
<h2>There is no crash</h2>
<h3>WebView is shifted a little to the bottom.</h3>
<h4>If I'm not shifted, i will crash in short time :(</h4>
</body>
</html>"
				}
			};
			var checkPage = new ContentPage
			{
				Content = checkWebView
			};
			checkPage.Appearing += (s, e) => checkWebView.TranslateTo(0, 100);
			Navigation.PushModalAsync(checkPage);
		}

#if UITEST

#endif
	}
}