using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 22972, "Win platform WebView cannot be release after its parent window get close")]
	public class Issue22972 : NavigationPage, INavigation
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

		public IReadOnlyList<Page> ModalStack => throw new NotImplementedException();

		public IReadOnlyList<Page> NavigationStack => throw new NotImplementedException();

		public void InsertPageBefore(Page page, Page before)
		{
			throw new NotImplementedException();
		}

		public Task<Page> PopModalAsync()
		{
			throw new NotImplementedException();
		}

		public Task<Page> PopModalAsync(bool animated)
		{
			throw new NotImplementedException();
		}

		public Task PushModalAsync(Page page)
		{
			throw new NotImplementedException();
		}

		public Task PushModalAsync(Page page, bool animated)
		{
			throw new NotImplementedException();
		}

		public void RemovePage(Page page)
		{
			throw new NotImplementedException();
		}
	}
}