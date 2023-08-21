// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/WebNavigatedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.WebNavigatedEventArgs']/Docs/*" />
	public class WebNavigatedEventArgs : WebNavigationEventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/WebNavigatedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public WebNavigatedEventArgs(WebNavigationEvent navigationEvent, WebViewSource source, string url, WebNavigationResult result) : base(navigationEvent, source, url)
		{
			Result = result;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebNavigatedEventArgs.xml" path="//Member[@MemberName='Result']/Docs/*" />
		public WebNavigationResult Result { get; private set; }
	}
}