using Microsoft.Maui;
using Controls.Core.Platform.iOS.Extensions;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class LabelHandler : Maui.Handlers.LabelHandler
	{
		public static void MapTextType(LabelHandler handler, Label label)
		{
			handler.NativeView?.UpdateText(label);
		}

		public static void MapLabelTextDecorations(LabelHandler handler, Label label)
		{
			handler.NativeView?.UpdateTextDecorations(label);
		}
	}
}