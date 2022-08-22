using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{

	public partial class Label
	{

		public static void MapTextType(LabelHandler handler, Label label)
		{
			handler.NativeView?.UpdateText(label, label.TextType);
		}

		public static void MapText(LabelHandler handler, Label label)
		{
			handler.NativeView?.UpdateText(label, label.TextType);
		}

	}

}