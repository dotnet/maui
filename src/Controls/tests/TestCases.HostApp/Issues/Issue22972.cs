using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 22972, "Win platform WebView cannot be release after its parent window get close")]
	public class Issue22972 : NavigationPage
	{
        ContentPage _rootPage = new ContentPage { Title = "Page 1" };
		public Issue22972() 
		{
            PushAsync(_rootPage);
            _rootPage.Content = new VerticalStackLayout()
            {
                new Label
                {
                    Text = "If you don't see a success label this test has failed"
                }
            };

            _rootPage.Loaded += OnPageLoaded;
        }

		async void OnPageLoaded(object sender, EventArgs e)
		{
		    var references = new List<WeakReference>();
            CurrentPage.Loaded -= OnPageLoaded;

            {
                var webView = new WebView
                {
                    HeightRequest = 500, // NOTE: non-zero size required for Windows
                    Source = new HtmlWebViewSource { Html = "<p>hi</p>" },
                };
                var page = new ContentPage
                {
                    Content = new VerticalStackLayout { webView }
                };
                await Navigation.PushAsync(page);
                await Task.Delay(1000); // give the WebView time to load

                references.Add(new(webView));
                references.Add(new(webView.Handler));
                references.Add(new(webView.Handler.PlatformView));

                await Navigation.PopAsync();
            }


            try
            {
                _rootPage.Content = new VerticalStackLayout()
                {
                    new Label
                    {
                        Text = "Waiting for resources to cleanup"
                    }
                };


                // Assert *before* the Window is closed
                await GarbageCollectionHelper.WaitForGC(references.ToArray());
                _rootPage.Content = new VerticalStackLayout()
                {
                    new Label
                    {
                        Text = "Success, everything has been cleaned up",
                        AutomationId = "Success"
                    }
                };
            }
            catch
            {
                var stillAlive = references.Where(x=> x.IsAlive).Select(x=> x.Target).ToList();
                _rootPage.Content = new VerticalStackLayout()
                {
                    new Label
                    {
                        Text = "Failed to cleanup: " + string.Join(", ", stillAlive),
                        AutomationId = "Failed"
                    }
                };
            }
		}
	}
}
