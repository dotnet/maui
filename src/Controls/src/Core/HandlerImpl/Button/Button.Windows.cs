#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using WButton = Microsoft.UI.Xaml.Controls.Button;

namespace Microsoft.Maui.Controls
{
	public partial class Button
	{
		WButton _button;

		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();

			if (Handler is not null)
			{
				if (Handler is ButtonHandler buttonHandler && buttonHandler.PlatformView is WButton button)
				{
					_button = button;
					_button.SizeChanged += OnButtonSizeChanged;
				}
			}
			else if (_button is not null)
			{
				_button.SizeChanged -= OnButtonSizeChanged;
				_button = null;
			}
		}

		public static void MapImageSource(ButtonHandler handler, Button button) =>
			MapImageSource((IButtonHandler)handler, button);

		public static void MapText(IButtonHandler handler, Button button)
		{
			var text = TextTransformUtilites.GetTransformedText(button.Text, button.TextTransform);
			handler.PlatformView?.UpdateText(text);
			button.Handler?.UpdateValue(nameof(Button.ContentLayout));
		}

		public static void MapLineBreakMode(IButtonHandler handler, Button button)
		{
			handler.PlatformView?.UpdateLineBreakMode(button);
		}

		public static void MapImageSource(IButtonHandler handler, Button button)
		{
			ButtonHandler.MapImageSource(handler, button);
			button.Handler?.UpdateValue(nameof(Button.ContentLayout));
		}

		public static void MapText(ButtonHandler handler, Button button) =>
			MapText((IButtonHandler)handler, button);

		void OnButtonSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (Handler is ButtonHandler buttonHandler)
			{
				var platformView = buttonHandler.PlatformView;
				var virtualView = buttonHandler.VirtualView as Button;

				if (platformView is null || virtualView is null)
					return;

				platformView.UpdateContentSize(virtualView);
			}
		}
	}
}
