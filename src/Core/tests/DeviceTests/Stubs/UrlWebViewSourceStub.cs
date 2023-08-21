// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class UrlWebViewSourceStub : IWebViewSource
	{
		public string Url { get; set; }

		public void Load(IWebViewDelegate webViewDelegate)
		{
			webViewDelegate.LoadUrl(Url);
		}
	}
}