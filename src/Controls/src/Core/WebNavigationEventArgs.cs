using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/WebNavigationEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.WebNavigationEventArgs']/Docs" />
	public class WebNavigationEventArgs : EventArgs
	{
		protected WebNavigationEventArgs(WebNavigationEvent navigationEvent, WebViewSource source, string url)
		{
			NavigationEvent = navigationEvent;
			Source = source;
			Url = url;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebNavigationEventArgs.xml" path="//Member[@MemberName='NavigationEvent']/Docs" />
		public WebNavigationEvent NavigationEvent { get; internal set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/WebNavigationEventArgs.xml" path="//Member[@MemberName='Source']/Docs" />
		public WebViewSource Source { get; internal set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/WebNavigationEventArgs.xml" path="//Member[@MemberName='Url']/Docs" />
		public string Url { get; internal set; }
	}
}