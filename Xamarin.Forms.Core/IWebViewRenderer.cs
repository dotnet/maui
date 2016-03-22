namespace Xamarin.Forms
{
	internal interface IWebViewRenderer
	{
		void LoadHtml(string html, string baseUrl);
		void LoadUrl(string url);
	}
}