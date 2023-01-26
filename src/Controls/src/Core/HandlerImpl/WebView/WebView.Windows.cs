#nullable enable
using System;

namespace Microsoft.Maui.Controls
{
	public partial class WebView
	{
		protected override void OnPropertyChanging(string? propertyName = null)
		{
			base.OnPropertyChanging(propertyName);

			if (propertyName == nameof(Window) && Window is not null)
				Window.Destroying -= OnWindowDestroying;
		}

		void OnWindowDestroying(object? sender, EventArgs e)
		{
			Handler?.DisconnectHandler();
		}
	}
}