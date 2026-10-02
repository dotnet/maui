using System;
using WebView2Control = Microsoft.UI.Xaml.Controls.WebView2;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public partial class BlazorWebView
	{
		/// <inheritdoc/>
		protected override void OnPropertyChanging(string? propertyName = null)
		{
			base.OnPropertyChanging(propertyName);

			if (propertyName == nameof(Window) && Window is not null)
				Window.Destroying -= Window_Destroying;
		}

		/// <inheritdoc/>
		protected override void OnPropertyChanged(string? propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == nameof(Window) && Window is not null)
				Window.Destroying += Window_Destroying;
		}

		private void Window_Destroying(object? sender, EventArgs e)
		{
			// see: https://github.com/microsoft/microsoft-ui-xaml/issues/6872
			if (Handler?.PlatformView is WebView2Control webView)
				webView.Close();
		}
	}
}
