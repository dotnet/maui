using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10744, "[Android] WebView.Eval crashes on Android with long string",
		PlatformAffected.Android)]
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
				Cookies = new System.Net.CookieContainer(),
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
				HeightRequest = 600
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
	}
}
