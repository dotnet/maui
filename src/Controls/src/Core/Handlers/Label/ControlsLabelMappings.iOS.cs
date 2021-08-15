using Microsoft.Maui;
using Controls.Core.Platform.iOS.Extensions;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers
{
	public static partial class ControlsLabelMappings 
	{
		public static void MapTextType(LabelHandler handler, Label label)
		{
			// We could conceivably make this work by casting label to Label,
			// but that seems gross
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
