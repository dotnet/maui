using System;

namespace Xamarin.Platform.Handlers
{
	public partial class ButtonHandler : AbstractViewHandler<IButton, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapText(ButtonHandler handler, IButton button) { }
		public static void MapTextColor(ButtonHandler handler, IButton button) { }
	}
}