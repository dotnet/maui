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

		public static void MapText(LabelHandler handler, Label label)
		{
			handler.NativeView?.UpdateText(label);
		}

		public static void MapTextDecorations(LabelHandler handler, Label label)
		{
			if (label?.TextType == TextType.Html)
				return;

			Maui.Handlers.LabelHandler.MapTextDecorations(handler, label);
		}

		public static void MapCharacterSpacing(LabelHandler handler, Label label)
		{
			if (label?.TextType == TextType.Html)
				return;

			Maui.Handlers.LabelHandler.MapCharacterSpacing(handler, label);
		}

		public static void MapLineHeight(LabelHandler handler, Label label)
		{
			if (label?.TextType == TextType.Html)
				return;

			Maui.Handlers.LabelHandler.MapLineHeight(handler, label);
		}

		public static void MapFont(LabelHandler handler, Label label)
		{
			if (label?.TextType == TextType.Html)
				return;

			Maui.Handlers.LabelHandler.MapFont(handler, label);
		}

		public static void MapTextColor(LabelHandler handler, Label label)
		{
			if (label?.TextType == TextType.Html)
				return;

			Maui.Handlers.LabelHandler.MapTextColor(handler, label);
		}
	}
}
