using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Label
	{
		public static void MapTextType(LabelHandler handler, Label label) => MapText((ILabelHandler)handler, label);
		public static void MapText(LabelHandler handler, Label label) => MapText((ILabelHandler)handler, label);
		public static void MapCharacterSpacing(LabelHandler handler, Label label) => MapCharacterSpacing((ILabelHandler)handler, label);
		public static void MapTextDecorations(LabelHandler handler, Label label) => MapTextDecorations((ILabelHandler)handler, label);
		public static void MapLineHeight(LabelHandler handler, Label label) => MapLineHeight((ILabelHandler)handler, label);
		public static void MapFont(LabelHandler handler, Label label) => MapFont((ILabelHandler)handler, label);
		public static void MapTextColor(LabelHandler handler, Label label) => MapTextColor((ILabelHandler)handler, label);


		public static void MapTextType(ILabelHandler handler, Label label)
		{
			Platform.LabelExtensions.UpdateText(handler.PlatformView, label);
		}

		public static void MapText(ILabelHandler handler, Label label)
		{
			Platform.LabelExtensions.UpdateText(handler.PlatformView, label);
		}

		public static void MapTextDecorations(ILabelHandler handler, Label label)
		{
			if (label?.HasFormattedTextSpans ?? false)
				return;

			if (label?.TextType == TextType.Html)
			{
				return;
			}

			LabelHandler.MapTextDecorations(handler, label);
		}

		public static void MapCharacterSpacing(ILabelHandler handler, Label label)
		{
			if (label?.HasFormattedTextSpans ?? false)
				return;

			if (label?.TextType == TextType.Html)
			{
				return;
			}

			LabelHandler.MapCharacterSpacing(handler, label);
		}

		public static void MapLineHeight(ILabelHandler handler, Label label)
		{
			if (label?.HasFormattedTextSpans ?? false)
				return;

			if (label?.TextType == TextType.Html)
			{
				return;
			}

			LabelHandler.MapLineHeight(handler, label);
		}

		public static void MapFont(ILabelHandler handler, Label label)
		{
			if (label?.HasFormattedTextSpans ?? false)
				return;

			if (label?.TextType == TextType.Html)
			{
				return;
			}

			LabelHandler.MapFont(handler, label);
		}

		public static void MapTextColor(ILabelHandler handler, Label label)
		{
			if (label?.HasFormattedTextSpans ?? false)
				return;

			if (label?.TextType == TextType.Html)
			{
				return;
			}

			LabelHandler.MapTextColor(handler, label);
		}

		public static void MapLineBreakMode(ILabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateLineBreakMode(label);
		}

		public static void MapMaxLines(ILabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateMaxLines(label);
		}
	}
}
