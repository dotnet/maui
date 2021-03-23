using System;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : AbstractViewHandler<IPicker, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapTitle(PickerHandler handler, IPicker view) { }
		public static void MapSelectedIndex(PickerHandler handler, IPicker view) { }
		public static void MapCharacterSpacing(PickerHandler handler, IPicker view) { }
	}
}