using System;

namespace Microsoft.Maui.Gtk.Web
{
	/// <summary>Runs JavaScript in a WebKitGTK web view and hands the result back to a MAUI request.</summary>
	internal static class GtkWebViewScripting
	{
		/// <summary>Runs <paramref name="script"/> and ignores its result.</summary>
		/// <param name="view">The web view.</param>
		/// <param name="script">The script.</param>
		internal static void Run(global::WebKit.WebView view, string script)
		{
			view.EvaluateJavascript(script, null, null, null, (source, result, userData) =>
			{
				try
				{
					view.EvaluateJavascriptFinish(result);
				}
				catch (Exception)
				{
					// A failing script has nobody to tell: Eval is fire-and-forget.
				}
			});
		}

		/// <summary>Runs <paramref name="script"/> and completes <paramref name="request"/> with its result (a string, or JSON for other values).</summary>
		/// <param name="view">The web view.</param>
		/// <param name="script">The script.</param>
		/// <param name="request">The request to complete.</param>
		internal static void Evaluate(global::WebKit.WebView view, string script, EvaluateJavaScriptAsyncRequest request)
		{
			try
			{
				view.EvaluateJavascript(script, null, null, null, (source, result, userData) =>
				{
					try
					{
						var converted = GtkWebViewScripts.ConvertResult(view.EvaluateJavascriptFinish(result));
						request.TrySetResult(converted!);
					}
					catch (Exception e)
					{
						request.TrySetException(e);
					}
				});
			}
			catch (Exception e)
			{
				request.TrySetException(e);
			}
		}
	}
}
