using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : AbstractViewHandler<IButton, Button>
	{
		protected override Button CreateNativeView() => new Button();

		public static void MapText(ButtonHandler handler, IButton button)
		{
			handler.TypedNativeView?.UpdateText(button);
		}

		public static void MapTextColor(ButtonHandler handler, IButton button) { }

		public static void MapFont(ButtonHandler handler, IButton button)
		{
			_ = handler.Services ?? throw new InvalidOperationException($"{nameof(Services)} should have been set by base class.");

			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			handler.TypedNativeView?.UpdateFont(button, fontManager);
		}

		public static void MapPadding(ButtonHandler handler, IButton button) { }
	}
}