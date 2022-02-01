using System.Threading.Tasks;

namespace Microsoft.Maui.Platform
{
	public static class WebViewExtensions
	{
		public static Task<string> EvaluateJavaScriptAsync(this object nativeWebView, IWebView webView, string script)
		{
			return nativeWebView.EvaluateJavaScriptAsync(script);
		}

		public static Task<string> EvaluateJavaScriptAsync(this object nativeWebView, string script)
		{
			return Task.FromResult(string.Empty);
		}
	}
}