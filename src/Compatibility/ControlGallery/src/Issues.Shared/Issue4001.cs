﻿using System;
using System.Linq;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4001, "[MacOS] NullRef in WebViewRenderer", PlatformAffected.macOS, NavigationBehavior.PushAsync)]
	public class Issue4001 : TestContentPage
	{

		protected override void Init()
		{
			Content = new StackLayout
			{
				Children = {
					new Button
					{
						HeightRequest = 100,
						BackgroundColor = Colors.Red,
						FontSize = 25,
						FontAttributes = FontAttributes.Bold,
						TextColor = Colors.Black,
						Text = "Click Me and wait at least 5 sec [No crash expected]",
						Command = new Command(() => {
							Navigation.PushAsync(new ContentPage());
						})
					}
				}
			};
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			(Content as StackLayout).Children.Insert(0, new WebView
			{
				Source = "https://github.com/xamarin/Xamarin.Forms/issues/4001",
				HeightRequest = 400
			});
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			var webView = (Content as StackLayout).Children.First() as WebView;
			webView.Source = "https://en.wikipedia.org/wiki/Xamarin";
			(Content as StackLayout).Children.Remove(webView);
		}

#if UITEST
#endif

	}
}
