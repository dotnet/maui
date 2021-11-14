using System;

namespace Microsoft.Maui.Controls
{
	public interface IWebNavigationEventArgs
	{
		public WebNavigationEvent NavigationEvent { get; }

		public WebViewSource Source { get; }

		public string Url { get; }
	}
}