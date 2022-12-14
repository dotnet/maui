using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/Label.xml" path="Type[@FullName='Microsoft.Maui.Controls.Label']/Docs/*" />
	public partial class Label
	{
		public static IPropertyMapper<ILabel, LabelHandler> ControlsLabelMapper = new PropertyMapper<Label, LabelHandler>(LabelHandler.Mapper)
		{
			[nameof(TextType)] = MapTextType,
			[nameof(Text)] = MapText,
			[nameof(FormattedText)] = MapText,
			[nameof(TextTransform)] = MapText,
#if WINDOWS
			[PlatformConfiguration.WindowsSpecific.InputView.DetectReadingOrderFromContentProperty.PropertyName] = MapDetectReadingOrderFromContent,
#endif
#if ANDROID
			[nameof(TextColor)] = MapTextColor,
#endif
#if IOS
			[nameof(TextDecorations)] = MapTextDecorations,
			[nameof(CharacterSpacing)] = MapCharacterSpacing,
			[nameof(LineHeight)] = MapLineHeight,
			[nameof(ILabel.Font)] = MapFont,
			[nameof(TextColor)] = MapTextColor,
#endif
			[nameof(Label.LineBreakMode)] = MapLineBreakMode,
			[nameof(Label.MaxLines)] = MapMaxLines,
		};

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.Label legacy behaviors
			// ILabel does not include the TextType property, so we map it here to handle HTML text
			// And we map some of the other property handlers to Controls-specific versions that avoid stepping on HTML text settings

			LabelHandler.Mapper = ControlsLabelMapper;
		}
	}
}
