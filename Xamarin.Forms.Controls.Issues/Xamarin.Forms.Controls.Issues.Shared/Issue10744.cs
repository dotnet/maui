using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10744, "[Android] WebView.Eval crashes on Android with long string",
		PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.WebView)]
#endif
	public class Issue10744 : TestContentPage
	{
		Label _navigatedLabel;
		WebView _webView;

		protected override void Init()
		{
			_navigatedLabel = new Label()
			{
				AutomationId = "navigatedLabel"
			};

			_webView = new WebView()
			{
				Source = "https://dotnet.microsoft.com/apps/xamarin",
				Cookies = new System.Net.CookieContainer()
			};

			_webView.Navigating += (_, __) =>
			{
			};

			_webView.Navigated += (_, __) =>
			{
				if (_navigatedLabel.Text == "Navigated")
					return;

				_webView.Eval($"javascript:{String.Join(":", Enumerable.Range(0, 900000).ToArray())}");
				_navigatedLabel.Text = "Navigated";
			};

			Content = new StackLayout()
			{
				Children =
				{
					new Label(){ Text = "If App hasn't crashed after navigating to the web page then this test has passed"},
					_navigatedLabel,
					_webView
				}
			};
		}

#if UITEST
		[Test]
		[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
		public void WebViewEvalCrashesOnAndroidWithLongString()
		{
			RunningApp.WaitForElement("navigatedLabel");
		}
#endif
	}
}
