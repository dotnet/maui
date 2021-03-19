using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler : AbstractViewHandler<ISwitch, ToggleSwitch>
	{
		protected override ToggleSwitch CreateNativeView() => new ToggleSwitch();

		public static void MapIsToggled(SwitchHandler handler, ISwitch view) { }
		public static void MapTrackColor(SwitchHandler handler, ISwitch view) { }
		public static void MapThumbColor(SwitchHandler handler, ISwitch view) { }
	}
}