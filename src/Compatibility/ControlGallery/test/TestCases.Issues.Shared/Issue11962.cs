using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11962, "[iOS] Cannot access a disposed object. Object name: 'WkWebViewRenderer",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github10000)]
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue11962 : TestShell
	{
		protected override void Init()
		{
			ContentPage contentPage = new ContentPage()
			{
				Content = new Button()
				{
					Text = "Go to the next page and back twice. If app doesn't crash test has passed.",
					Command = new Command(async () =>
					{
						await GoToAsync("//user");
					}),
					AutomationId = "NextButton"
				}
			};

			ContentPage webViewPage = new ContentPage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new WebView()
						{
							Source = new HtmlWebViewSource { Html = GetHtml("string some Text") },
							VerticalOptions = LayoutOptions.StartAndExpand
						},
						new Button()
						{
							Text = "Go Back",
							Command = new Command(async () =>
							{
								await GoToAsync("//usersearch");
							}),
							AutomationId = "BackButton"
						}
					}
				}
			};


			AddFlyoutItem(contentPage, "User Search").Route = "usersearch";
			AddFlyoutItem(webViewPage, "Web View Page").Route = "user";
		}

		string GetHtml(string uid)
		{
			var htmlHeader = @"<header><meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, minimum-scale=1.0, user-scalable=no'></header>";
			if ("UserOne".Equals(uid))
				return htmlHeader + @"<h3>Welcome <b style=""color:red;"">User One</b>,</h3><div>Displaying Html message</div>";
			return htmlHeader + @"<h3>Welcome <b style=""color:blue;"">User Two</b>,</h3><div>Displaying Html message</div>";
		}

#if UITEST
		[Test]
		public void WkWebViewDisposesProperly()
		{
			RunningApp.Tap("NextButton");
			RunningApp.Tap("BackButton");
			RunningApp.Tap("NextButton");
			RunningApp.Tap("BackButton");
			RunningApp.Tap("NextButton");
			RunningApp.Tap("BackButton");
		}
#endif
	}
}
