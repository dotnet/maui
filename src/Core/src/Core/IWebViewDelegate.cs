﻿namespace Microsoft.Maui
{
	public interface IWebViewDelegate
	{
		void LoadHtml(string? html, string? baseUrl);
		void LoadUrl(string? url);
		bool LoadFile(string? url);
	}
}