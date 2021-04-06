using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, ComboBox>
	{
		protected override ComboBox CreateNativeView() => new ComboBox();

		public static void MapTitle(PickerHandler handler, IPicker view) { }
		public static void MapSelectedIndex(PickerHandler handler, IPicker view) { }
		public static void MapCharacterSpacing(PickerHandler handler, IPicker view) { }
	}
}