namespace Xamarin.Forms
{
	public interface IWebViewDelegate
	{
		void LoadHtml(string html, string baseUrl);
		void LoadUrl(string url);
	}
}