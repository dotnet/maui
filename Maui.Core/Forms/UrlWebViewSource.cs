using System.ComponentModel;

namespace System.Maui
{
	public class UrlWebViewSource : WebViewSource
	{
		public string Url { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void Load(IWebViewDelegate renderer)
		{
			renderer.LoadUrl(Url);
		}
	}
}