using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Label
	{
		public static IPropertyMapper<ILabel, LabelHandler> ControlsLabelMapper = new PropertyMapper<Label, LabelHandler>(LabelHandler.LabelMapper)
		{
			[nameof(TextType)] = MapTextType,
			[nameof(Text)] = MapText,
#if __IOS__
			[nameof(TextDecorations)] = MapTextDecorations,
			[nameof(CharacterSpacing)] = MapCharacterSpacing,
			[nameof(LineHeight)] = MapLineHeight,
			[nameof(ILabel.Font)] = MapFont,
			[nameof(TextColor)] = MapTextColor
#endif
		};

		public static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.Label legacy behaviors
			// ILabel does not include the TextType property, so we map it here to handle HTML text
			// And we map some of the other property handlers to Controls-specific versions that avoid stepping on HTML text settings

			LabelHandler.LabelMapper = ControlsLabelMapper;
		}
	}
}
