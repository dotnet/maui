using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Label
	{
		public static void MapTextType(LabelHandler handler, Label label)
		{
			Platform.LabelExtensions.UpdateText(handler.PlatformView, label);
		}

		public static void MapText(LabelHandler handler, Label label)
		{
			Platform.LabelExtensions.UpdateText(handler.PlatformView, label);
		}

		public static void MapTextDecorations(LabelHandler handler, Label label)
		{
			if (label?.HasFormattedTextSpans ?? false)
				return;

			if (label?.TextType == TextType.Html)
			{
				return;
			}

			LabelHandler.MapTextDecorations(handler, label);
		}

		public static void MapCharacterSpacing(LabelHandler handler, Label label)
		{
			if (label?.HasFormattedTextSpans ?? false)
				return;

			if (label?.TextType == TextType.Html)
			{
				return;
			}

			LabelHandler.MapCharacterSpacing(handler, label);
		}

		public static void MapLineHeight(LabelHandler handler, Label label)
		{
			if (label?.HasFormattedTextSpans ?? false)
				return;

			if (label?.TextType == TextType.Html)
			{
				return;
			}

			LabelHandler.MapLineHeight(handler, label);
		}

		public static void MapFont(LabelHandler handler, Label label)
		{
			if (label?.HasFormattedTextSpans ?? false)
				return;

			if (label?.TextType == TextType.Html)
			{
				return;
			}

			LabelHandler.MapFont(handler, label);
		}

		public static void MapTextColor(LabelHandler handler, Label label)
		{
			if (label?.HasFormattedTextSpans ?? false)
				return;

			if (label?.TextType == TextType.Html)
			{
				return;
			}

			LabelHandler.MapTextColor(handler, label);
		}
	}
}
