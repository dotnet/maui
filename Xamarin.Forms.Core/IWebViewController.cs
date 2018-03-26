using System;
using Xamarin.Forms.Internals;
using System.Threading.Tasks;

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
		void SendNavigated(WebNavigatedEventArgs args);
		void SendNavigating(WebNavigatingEventArgs args);
	}
}