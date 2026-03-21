using System;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		[Obsolete("Use Microsoft.Maui.Handlers.PickerHandler.MapItems instead")]
		public static void MapReload(IPickerHandler handler, IPicker picker, object? args) { }
		internal static void MapItems(IPickerHandler handler, IPicker picker) { }

		public static void MapTitle(IPickerHandler handler, IPicker view) { }
		public static void MapTitleColor(IPickerHandler handler, IPicker view) { }
		public static void MapSelectedIndex(IPickerHandler handler, IPicker view) { }
		public static void MapCharacterSpacing(IPickerHandler handler, IPicker view) { }
		public static void MapFont(IPickerHandler handler, IPicker view) { }
		public static void MapTextColor(IPickerHandler handler, IPicker view) { }
		public static void MapHorizontalTextAlignment(IPickerHandler handler, IPicker view) { }
		public static void MapVerticalTextAlignment(IPickerHandler handler, IPicker view) { }
		internal static void MapIsOpen(IPickerHandler handler, IPicker picker) { }
	}
}