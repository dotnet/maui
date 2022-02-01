namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/WebNavigatingEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.WebNavigatingEventArgs']/Docs" />
	public class WebNavigatingEventArgs : WebNavigationEventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/WebNavigatingEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public WebNavigatingEventArgs(WebNavigationEvent navigationEvent, WebViewSource source, string url) : base(navigationEvent, source, url)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebNavigatingEventArgs.xml" path="//Member[@MemberName='Cancel']/Docs" />
		public bool Cancel { get; set; }
	}
}