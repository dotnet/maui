using static Controls.Core.Platform.Android.Extensions.TextViewExtensions;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class LabelHandler : Microsoft.Maui.Handlers.LabelHandler
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
