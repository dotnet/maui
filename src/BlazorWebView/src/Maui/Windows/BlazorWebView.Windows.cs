using System;

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
			// Routed through the shared guarded helper because DisconnectHandler may also
			// close the native CoreWebView2 around the same time.
			var platformView = ((BlazorWebViewHandler?)Handler)?.PlatformView;
			if (platformView is not null)
			{
				BlazorWebViewHandler.CloseWebView2(platformView);
			}
		}
	}
}
