using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : AbstractViewHandler<IButton, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapText(ButtonHandler handler, IButton button) { }
		public static void MapTextColor(ButtonHandler handler, IButton button) { }
		public static void MapFont(ButtonHandler handler, IButton button) { }
		public static void MapPadding(ButtonHandler handler, IButton button) { }
	}
}