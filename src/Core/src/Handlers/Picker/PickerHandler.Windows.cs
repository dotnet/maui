using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, ComboBox>
	{
		protected override ComboBox CreateNativeView() => new ComboBox();

		[MissingMapper]
		public static void MapTitle(PickerHandler handler, IPicker view) { }

		[MissingMapper]
		public static void MapSelectedIndex(PickerHandler handler, IPicker view) { }

		[MissingMapper]
		public static void MapCharacterSpacing(PickerHandler handler, IPicker view) { }

		[MissingMapper]
		public static void MapFont(PickerHandler handler, IPicker view) { }

		[MissingMapper]
		public static void MapTextColor(PickerHandler handler, IPicker view) { }

		[MissingMapper]
		public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry) { }
	}
}