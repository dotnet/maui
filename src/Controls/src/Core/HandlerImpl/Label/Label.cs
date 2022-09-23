using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/Label.xml" path="Type[@FullName='Microsoft.Maui.Controls.Label']/Docs/*" />
	public partial class Label
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='ControlsLabelMapper']/Docs/*" />
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
		};

		static void AddLegacyMultilineMappings() 
		{
			var mapper = ControlsLabelMapper as PropertyMapper<Label, LabelHandler>;

			mapper.Add(nameof(Label.LineBreakMode), MapLineBreakMode);
			mapper.Add(nameof(Label.MaxLines),  MapMaxLines);

			// Does this even work? Why is there no "Remove"?
			mapper[nameof(IMultilineText.MaximumLines)] = null;
		}

		internal static void RemapForControls(CompatibilityOptions options = null)
		{
			// Adjust the mappings to preserve Controls.Label legacy behaviors
			// ILabel does not include the TextType property, so we map it here to handle HTML text
			// And we map some of the other property handlers to Controls-specific versions that avoid stepping on HTML text settings

			if (options != null && options.LabelMultilineLegacyBehavior)
			{
				AddLegacyMultilineMappings();
			}

			LabelHandler.Mapper = ControlsLabelMapper;
		}
	}
}
