#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="Microsoft.Maui.Controls.View"/> that displays text.</summary>
	public partial class Label
	{
		static Label() => RemapForControls();

		private new static void RemapForControls()
		{
			VisualElement.RemapIfNeeded();

			// Adjust the mappings to preserve Controls.Label legacy behaviors
			// ILabel does not include the TextType property, so we map it here to handle HTML text
			// And we map some of the other property handlers to Controls-specific versions that avoid stepping on HTML text settings

			// these just refresh Text / FormattedText
			LabelHandler.Mapper.ReplaceMapping<Label, ILabelHandler>(nameof(TextType), MapTextType);
			LabelHandler.Mapper.ReplaceMapping<Label, ILabelHandler>(nameof(TextTransform), MapTextTransform);

			// these are really a single property
			LabelHandler.Mapper.ReplaceMapping<Label, ILabelHandler>(nameof(Text), MapText);
			LabelHandler.Mapper.ReplaceMapping<Label, ILabelHandler>(nameof(FormattedText), MapFormattedText);

			LabelHandler.Mapper.ReplaceMapping<Label, ILabelHandler>(nameof(LineBreakMode), MapLineBreakMode);
			LabelHandler.Mapper.ReplaceMapping<Label, ILabelHandler>(nameof(MaxLines), MapMaxLines);

#if ANDROID || IOS
			// these are for platforms that do no support view properties reaching spans
			LabelHandler.Mapper.ModifyMapping<Label, ILabelHandler>(nameof(ILabel.Font), MapFont);
			LabelHandler.Mapper.ModifyMapping<Label, ILabelHandler>(nameof(TextColor), MapTextColor);

			// these are for properties that should only apply to plain text (not spans nor html)
			LabelHandler.Mapper.ModifyMapping<Label, ILabelHandler>(nameof(TextDecorations), MapTextDecorations);
			LabelHandler.Mapper.ModifyMapping<Label, ILabelHandler>(nameof(CharacterSpacing), MapCharacterSpacing);
			LabelHandler.Mapper.ModifyMapping<Label, ILabelHandler>(nameof(LineHeight), MapLineHeight);
#endif

			// platform-specifics
#if WINDOWS
			LabelHandler.Mapper.ReplaceMapping<Label, ILabelHandler>(PlatformConfiguration.WindowsSpecific.InputView.DetectReadingOrderFromContentProperty.PropertyName, MapDetectReadingOrderFromContent);
#endif
		}


		// Some properties just need to re-evaluate the Text property
		// which then makes a decision about Text vs FormattedText:
		//  - TextType
		//  - TextTransform
		public static void MapTextType(LabelHandler handler, Label label) =>
			MapTextOrFormattedText(handler, label);
		public static void MapTextType(ILabelHandler handler, Label label) =>
			MapTextOrFormattedText(handler, label);
		static void MapTextTransform(ILabelHandler handler, Label label) =>
			MapTextOrFormattedText(handler, label);
		static void MapFormattedText(ILabelHandler handler, Label label)
		{
			if (label.IsConnectingHandler())
				return;

			MapText(handler, label);
		}

		static void MapTextOrFormattedText(ILabelHandler handler, Label label)
		{
			if (label.IsConnectingHandler())
				return;

			if (label.HasFormattedTextSpans)
				handler.UpdateValue(nameof(FormattedText));
			else
				handler.UpdateValue(nameof(Text));
		}

#if ANDROID || IOS

#if IOS // iOS had these public, so we cannot remove
		public static void MapTextDecorations(ILabelHandler handler, Label label) =>
			MapTextDecorations(handler, label, (h, v) => LabelHandler.MapTextDecorations(handler, label));

		public static void MapCharacterSpacing(ILabelHandler handler, Label label) =>
			MapCharacterSpacing(handler, label, (h, v) => LabelHandler.MapCharacterSpacing(handler, label));

		public static void MapLineHeight(ILabelHandler handler, Label label) =>
			MapLineHeight(handler, label, (h, v) => LabelHandler.MapLineHeight(handler, label));

		public static void MapFont(ILabelHandler handler, Label label) =>
			MapFont(handler, label, (h, v) => LabelHandler.MapFont(handler, label));

		public static void MapTextColor(ILabelHandler handler, Label label) =>
			MapTextColor(handler, label, (h, v) => LabelHandler.MapTextColor(handler, label));

		public static void MapTextDecorations(LabelHandler handler, Label label) =>
			MapTextDecorations((ILabelHandler)handler, label);

		public static void MapCharacterSpacing(LabelHandler handler, Label label) =>
			MapCharacterSpacing((ILabelHandler)handler, label);

		public static void MapLineHeight(LabelHandler handler, Label label) =>
			MapLineHeight((ILabelHandler)handler, label);

		public static void MapFont(LabelHandler handler, Label label) =>
			MapFont((ILabelHandler)handler, label);

		public static void MapTextColor(LabelHandler handler, Label label) =>
			MapTextColor((ILabelHandler)handler, label);
#endif

		// these are for properties that should only apply to plain text (not spans nor html)

		static void MapLineHeight(ILabelHandler handler, Label label, Action<IElementHandler, IElement> baseMethod)
		{
			if (!IsPlainText(label))
				return;

			baseMethod?.Invoke(handler, label);
		}

		static void MapTextDecorations(ILabelHandler handler, Label label, Action<IElementHandler, IElement> baseMethod)
		{
			if (!IsPlainText(label))
				return;

			baseMethod?.Invoke(handler, label);
		}

		static void MapCharacterSpacing(ILabelHandler handler, Label label, Action<IElementHandler, IElement> baseMethod)
		{
			if (!IsPlainText(label))
				return;

			baseMethod?.Invoke(handler, label);
		}

		// these are for platforms that do no support view properties reaching spans

		static void MapFont(ILabelHandler handler, Label label, Action<IElementHandler, IElement> baseMethod)
		{
			if (label.HasFormattedTextSpans)
			{
				// if there is formatted text,
				// then we re-apply the whole formatted text
				handler.UpdateValue(nameof(FormattedText));
			}
			else if (label.TextType == TextType.Text || !IsDefaultFont(label))
			{
				// if this is plain text or if the user specifically wants to override html,
				// then we fall back to the base implementation
				baseMethod?.Invoke(handler, label);
			}
		}

		static void MapTextColor(ILabelHandler handler, Label label, Action<IElementHandler, IElement> baseMethod)
		{
			if (label.HasFormattedTextSpans)
			{
				// if there is formatted text,
				// then we re-apply the whole formatted text
				handler.UpdateValue(nameof(FormattedText));
			}
			else if (label.TextType == TextType.Text || !label.TextColor.IsDefault())
			{
				// if this is plain text or if the user specifically wants to override html,
				// then we fall back to the base implementation
				baseMethod?.Invoke(handler, label);
			}
		}

#endif

		static bool IsPlainText(Label label)
		{
			if (label.HasFormattedTextSpans)
				return false;

			if (label.TextType != TextType.Text)
				return false;

			return true;
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
	}
}
