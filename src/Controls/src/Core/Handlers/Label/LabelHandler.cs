namespace Microsoft.Maui.Controls.Handlers
{
	public partial class LabelHandler : Maui.Handlers.LabelHandler
	{
		public static PropertyMapper<Label, LabelHandler> ControlsLabelMapper = new(LabelMapper)
		{
			[nameof(Label.TextType)] = MapTextType,
			[nameof(Label.Text)] = MapText,
#if __IOS__
			[nameof(Label.TextDecorations)] = MapTextDecorations,
			[nameof(Label.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(Label.LineHeight)] = MapLineHeight,
			[nameof(ILabel.Font)] = MapFont,
			[nameof(ILabel.TextColor)] = MapTextColor
#endif
		};

		public LabelHandler() : base(ControlsLabelMapper) { }

		public LabelHandler(PropertyMapper mapper = null) : base(mapper ?? ControlsLabelMapper) { }
	}
}
