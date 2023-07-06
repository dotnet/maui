#nullable disable
using System;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class Label
	{
		public static void MapTextType(LabelHandler handler, Label label) => MapTextType((ILabelHandler)handler, label);
		public static void MapText(LabelHandler handler, Label label) => MapText((ILabelHandler)handler, label);
		public static void MapCharacterSpacing(LabelHandler handler, Label label) => MapCharacterSpacing((ILabelHandler)handler, label);
		public static void MapTextDecorations(LabelHandler handler, Label label) => MapTextDecorations((ILabelHandler)handler, label);
		public static void MapLineHeight(LabelHandler handler, Label label) => MapLineHeight((ILabelHandler)handler, label);
		public static void MapFont(LabelHandler handler, Label label) => MapFont((ILabelHandler)handler, label);
		public static void MapTextColor(LabelHandler handler, Label label) => MapTextColor((ILabelHandler)handler, label);


		public static void MapTextType(ILabelHandler handler, Label label)
		{
			handler.UpdateValue(nameof(ILabel.Text));
		}

		public static void MapText(ILabelHandler handler, Label label)
		{
			Platform.LabelExtensions.UpdateText(handler.PlatformView, label);

			MapFormatting(handler, label);
		}

		public static void MapTextDecorations(ILabelHandler handler, Label label)
		{
			if (!IsPlainText(label))
				return;

			LabelHandler.MapTextDecorations(handler, label);
		}

		public static void MapCharacterSpacing(ILabelHandler handler, Label label)
		{
			if (!IsPlainText(label))
				return;

			LabelHandler.MapCharacterSpacing(handler, label);
		}

		public static void MapLineHeight(ILabelHandler handler, Label label)
		{
			if (!IsPlainText(label))
				return;

			LabelHandler.MapLineHeight(handler, label);
		}

		public static void MapFont(ILabelHandler handler, Label label)
		{
			if (label.HasFormattedTextSpans)
				return;

			if (label.TextType == TextType.Html && IsDefaultFont(label))
			{
				// If no explicit font has been specified and we're displaying HTML,
				// let the HTML determine the font
				return;
			}

			LabelHandler.MapFont(handler, label);
		}

		public static void MapTextColor(ILabelHandler handler, Label label)
		{
			if (label.HasFormattedTextSpans)
				return;

			if (label.TextType == TextType.Html && label.TextColor.IsDefault())
			{
				// If no explicit text color has been specified and we're displaying HTML,
				// let the HTML determine the colors
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

		static void MapFormatting(ILabelHandler handler, Label label)
		{
			handler.UpdateValue(nameof(ILabel.TextColor));
			handler.UpdateValue(nameof(ILabel.Font));
		}

		static bool IsDefaultFont(Label label)
		{
			if (label.IsSet(Label.FontAttributesProperty))
				return false;

			if (label.IsSet(Label.FontFamilyProperty))
				return false;

			if (label.IsSet(Label.FontSizeProperty))
				return false;

			return true;
		}

		static bool IsPlainText(Label label)
		{
			if (label.HasFormattedTextSpans)
				return false;

			if (label.TextType != TextType.Text)
				return false;

			return true;
		}
	}
}
