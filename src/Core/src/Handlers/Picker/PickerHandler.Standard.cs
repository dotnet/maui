using System;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapReload(PickerHandler handler, IPicker picker, object? args) { }
		public static void MapTitle(PickerHandler handler, IPicker view) { }
		public static void MapTitleColor(PickerHandler handler, IPicker view) { }
		public static void MapSelectedIndex(PickerHandler handler, IPicker view) { }
		public static void MapCharacterSpacing(PickerHandler handler, IPicker view) { }
		public static void MapFont(PickerHandler handler, IPicker view) { }
		public static void MapTextColor(PickerHandler handler, IPicker view) { }
		public static void MapHorizontalTextAlignment(PickerHandler handler, IPicker view) { }
		public static void MapVerticalTextAlignment(PickerHandler handler, IPicker view) { }
	}
}