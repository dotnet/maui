using System;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, NView>
	{

		protected override NView CreatePlatformView() => new();

		static void Reload(IPickerHandler handler)
		{
			if (handler.VirtualView == null || handler.PlatformView == null)
				return;

			handler.PlatformView.UpdatePicker(handler.VirtualView);
		}

		// Uncomment me on NET7 [Obsolete]
		public static void MapReload(IPickerHandler handler, IPicker picker, object? args) => Reload(handler);

		internal static void MapItems(IPickerHandler handler, IPicker picker) => Reload(handler);

		public static void MapTitleColor(IPickerHandler handler, IPicker picker)
		{
		}

		public static void MapFont(IPickerHandler handler, IPicker picker)
		{
		}

		public static void MapHorizontalTextAlignment(IPickerHandler handler, IPicker picker)
		{
		}

		public static void MapVerticalTextAlignment(IPickerHandler handler, IPicker picker)
		{
		}

		public static void MapTextColor(IPickerHandler handler, IPicker picker)
		{
		}

		public static void MapTitle(IPickerHandler handler, IPicker picker)
		{
		}

		public static void MapSelectedIndex(IPickerHandler handler, IPicker picker)
		{
		}

		[MissingMapper]
		public static void MapCharacterSpacing(PickerHandler handler, IPicker picker) { }
	}
}