using Microsoft.AspNetCore.Components.WebView.WebView2;
using Microsoft.Web.WebView2.Core;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal class WinUICoreWebView2SettingsWrapper : ICoreWebView2SettingsWrapper
	{
		private readonly CoreWebView2Settings _settings;

		public WinUICoreWebView2SettingsWrapper(CoreWebView2Settings settings)
		{
			_settings = settings;
		}

		public bool IsScriptEnabled
		{
			get => _settings.IsScriptEnabled;
			set => _settings.IsScriptEnabled = value;
		}
		public bool IsWebMessageEnabled
		{
			get => _settings.IsWebMessageEnabled;
			set => _settings.IsWebMessageEnabled = value;
		}
		public bool AreDefaultScriptDialogsEnabled
		{
			get => _settings.AreDefaultScriptDialogsEnabled;
			set => _settings.AreDefaultScriptDialogsEnabled = value;
		}
		public bool IsStatusBarEnabled
		{
			get => _settings.IsStatusBarEnabled;
			set => _settings.IsStatusBarEnabled = value;
		}
		public bool AreDevToolsEnabled
		{
			get => _settings.AreDevToolsEnabled;
			set => _settings.AreDevToolsEnabled = value;
		}
		public bool AreDefaultContextMenusEnabled
		{
			get => _settings.AreDefaultContextMenusEnabled;
			set => _settings.AreDefaultContextMenusEnabled = value;
		}
		public bool AreHostObjectsAllowed
		{
			get => _settings.AreHostObjectsAllowed;
			set => _settings.AreHostObjectsAllowed = value;
		}
		public bool IsZoomControlEnabled
		{
			get => _settings.IsZoomControlEnabled;
			set => _settings.IsZoomControlEnabled = value;
		}
		public bool IsBuiltInErrorPageEnabled
		{
			get => _settings.IsBuiltInErrorPageEnabled;
			set => _settings.IsBuiltInErrorPageEnabled = value;
		}
	}
}
