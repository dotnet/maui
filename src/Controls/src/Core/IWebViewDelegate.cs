namespace Microsoft.Maui.Controls
{
	public interface IWebViewDelegate
	{
		void LoadHtml(string html, string baseUrl);
		void LoadUrl(string url);
	}
}