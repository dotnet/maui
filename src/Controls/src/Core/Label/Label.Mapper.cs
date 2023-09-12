#nullable disable
using System;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/Label.xml" path="Type[@FullName='Microsoft.Maui.Controls.Label']/Docs/*" />
	public partial class Label
	{
		[Obsolete("Use LabelHandler.Mapper instead.")]
		public static IPropertyMapper<ILabel, LabelHandler> ControlsLabelMapper = new PropertyMapper<Label, LabelHandler>(LabelHandler.Mapper);

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.Label legacy behaviors
			// ILabel does not include the TextType property, so we map it here to handle HTML text
			// And we map some of the other property handlers to Controls-specific versions that avoid stepping on HTML text settings

			LabelHandler.Mapper.ReplaceMapping<Label, ILabelHandler>(nameof(TextType), MapTextType);
			LabelHandler.Mapper.ReplaceMapping<Label, ILabelHandler>(nameof(Text), MapText);
			LabelHandler.Mapper.ReplaceMapping<Label, ILabelHandler>(nameof(FormattedText), MapText);
			LabelHandler.Mapper.ReplaceMapping<Label, ILabelHandler>(nameof(TextTransform), MapText);
#if WINDOWS
			LabelHandler.Mapper.ReplaceMapping<Label, ILabelHandler>(PlatformConfiguration.WindowsSpecific.InputView.DetectReadingOrderFromContentProperty.PropertyName, MapDetectReadingOrderFromContent);
#endif
#if IOS
			LabelHandler.Mapper.ModifyMapping<Label, ILabelHandler>(nameof(TextDecorations), MapTextDecorations);
			LabelHandler.Mapper.ModifyMapping<Label, ILabelHandler>(nameof(CharacterSpacing), MapCharacterSpacing);
			LabelHandler.Mapper.ModifyMapping<Label, ILabelHandler>(nameof(LineHeight), MapLineHeight);
			LabelHandler.Mapper.ModifyMapping<Label, ILabelHandler>(nameof(ILabel.Font), MapFont);
			LabelHandler.Mapper.ModifyMapping<Label, ILabelHandler>(nameof(TextColor), MapTextColor);
#endif
			LabelHandler.Mapper.ReplaceMapping<Label, ILabelHandler>(nameof(Label.LineBreakMode), MapLineBreakMode);
			LabelHandler.Mapper.ReplaceMapping<Label, ILabelHandler>(nameof(Label.MaxLines), MapMaxLines);
		}
	}
}
