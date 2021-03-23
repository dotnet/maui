namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler
	{
		public static PropertyMapper<ILabel, LabelHandler> LabelMapper = new PropertyMapper<ILabel, LabelHandler>(ViewHandler.ViewMapper)
		{
			[nameof(ILabel.TextColor)] = MapTextColor,
			[nameof(ILabel.Text)] = MapText,
			[nameof(ILabel.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ILabel.MaxLines)] = MapMaxLines,
			[nameof(ILabel.Font)] = MapFont,
			[nameof(ILabel.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(ILabel.LineBreakMode)] = MapLineBreakMode,
			[nameof(ILabel.Padding)] = MapPadding,
			[nameof(ILabel.TextDecorations)] = MapTextDecorations,
			[nameof(ILabel.LineHeight)] = MapLineHeight
		};

		public LabelHandler() : base(LabelMapper)
		{

		}

		public LabelHandler(PropertyMapper? mapper = null) : base(mapper ?? LabelMapper)
		{

		}
	}
}