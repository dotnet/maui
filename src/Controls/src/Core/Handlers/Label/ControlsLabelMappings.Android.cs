using Microsoft.Maui.Handlers;
using static Controls.Core.Platform.Android.Extensions.TextViewExtensions;

namespace Microsoft.Maui.Controls.Handlers
{
	public static partial class ControlsLabelMappings
	{
		public static void MapTextType(LabelHandler handler, Label label)
		{
			handler.NativeView?.UpdateText(label);
		}

		public static void MapText(LabelHandler handler, Label label)
		{
			handler.NativeView?.UpdateText(label);
		}
	}
}
