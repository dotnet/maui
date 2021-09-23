using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : ViewHandler<IButton, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapText(IButtonHandler handler, IText button) { }
		public static void MapTextColor(IButtonHandler handler, ITextStyle button) { }
		public static void MapCharacterSpacing(IButtonHandler handler, ITextStyle button) { }
		public static void MapFont(IButtonHandler handler, ITextStyle button) { }
		public static void MapPadding(IButtonHandler handler, IButton button) { }
		public static void MapImageSource(IButtonHandler handler, IButton image) { }
		void OnSetImageSource(object? obj)
		{
			throw new NotImplementedException();
		}
	}
}