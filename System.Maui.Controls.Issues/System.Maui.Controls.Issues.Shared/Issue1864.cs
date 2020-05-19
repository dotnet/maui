using System.Maui.CustomAttributes;
using System.Maui.Internals;
using System.Collections.ObjectModel;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif


namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1864, "[WPF] System.Maui WPF load local html throw ArgumentException with message 'Relative URIs are not allowed'", PlatformAffected.WPF)]
	public class Issue1864 : TestContentPage
	{
		protected override void Init()
		{
			WebView webView = new WebView();
			var source = new HtmlWebViewSource()
			{
				Html = @"<html><body> <h1>System.Maui</h1> <p>Welcome to WebView.</p> </body> </html>"
			};
			webView.Source = source;
			Content = webView;
		}
	}
}
