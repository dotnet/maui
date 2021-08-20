using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Label
	{
		public static void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.Label legacy behaviors
			// ILabel does not include the TextType property, so we map it here to handle HTML text
			// And we map some of the other property handlers to Controls-specific versions that avoid steppingon HTML text settings

			IPropertyMapper<ILabel, LabelHandler> ControlsLabelMapper = new PropertyMapper<Label, LabelHandler>(LabelHandler.LabelMapper)
			{
				[nameof(TextType)] = MapTextType,
				[nameof(Text)] = MapText,
				[nameof(TextDecorations)] = MapTextDecorations,
				[nameof(CharacterSpacing)] = MapCharacterSpacing,
				[nameof(LineHeight)] = MapLineHeight,
				[nameof(ILabel.Font)] = MapFont,
				[nameof(TextColor)] = MapTextColor
			};

			LabelHandler.LabelMapper = ControlsLabelMapper;
		}

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
			{
				return;
			}

			LabelHandler.MapTextDecorations(handler, label);
		}

		public static void MapCharacterSpacing(LabelHandler handler, Label label)
		{
			if (label?.TextType == TextType.Html)
			{
				return;
			}

			LabelHandler.MapCharacterSpacing(handler, label);
		}

		public static void MapLineHeight(LabelHandler handler, Label label)
		{
			if (label?.TextType == TextType.Html)
			{
				return;
			}

			LabelHandler.MapLineHeight(handler, label);
		}

		public static void MapFont(LabelHandler handler, Label label)
		{
			if (label?.TextType == TextType.Html)
			{
				return;
			}

			LabelHandler.MapFont(handler, label);
		}

		public static void MapTextColor(LabelHandler handler, Label label)
		{
			if (label?.TextType == TextType.Html)
			{
				return;
			}

			LabelHandler.MapTextColor(handler, label);
		}
	}
}
