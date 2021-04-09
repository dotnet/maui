using System.ComponentModel;

namespace Microsoft.Maui
{
	public class HtmlWebViewSource : WebViewSource
	{
		public string? BaseUrl { get; set; }

		public string? Html { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void Load(IWebViewDelegate renderer)
		{
			renderer.LoadHtml(Html, BaseUrl);
		}
	}
}