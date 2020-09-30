using System;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public interface IWebViewController : IViewController
	{
		bool CanGoBack { get; set; }
		bool CanGoForward { get; set; }
		event EventHandler<EvalRequested> EvalRequested;
		event EvaluateJavaScriptDelegate EvaluateJavaScriptRequested;
		event EventHandler GoBackRequested;
		event EventHandler GoForwardRequested;
		event EventHandler ReloadRequested;
		void SendNavigated(WebNavigatedEventArgs args);
		void SendNavigating(WebNavigatingEventArgs args);
	}
}