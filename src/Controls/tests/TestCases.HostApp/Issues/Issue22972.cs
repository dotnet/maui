using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 22972, "Win platform WebView cannot be release after its parent window get close")]
	public class Issue22972 : NavigationPage
	{
		public Issue22972() 
		{
            this.RunMemoryTest(() =>
            {
                return new WebView
                {
                    HeightRequest = 500, // NOTE: non-zero size required for Windows
                    Source = new HtmlWebViewSource { Html = "<p>hi</p>" },
                };
            });
        }
	}
}
