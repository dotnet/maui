#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler
	{
		public static PropertyMapper<ILabel, LabelHandler> LabelMapper = new PropertyMapper<ILabel, LabelHandler>(ViewHandler.ViewMapper)
		{
#if WINDOWS || __IOS__
			[nameof(ILabel.Background)] = MapBackground,
#endif
			[nameof(ILabel.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ILabel.Font)] = MapFont,
			[nameof(ILabel.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(ILabel.LineBreakMode)] = MapLineBreakMode,
			[nameof(ILabel.LineHeight)] = MapLineHeight,
			[nameof(ILabel.MaxLines)] = MapMaxLines,
			[nameof(ILabel.Padding)] = MapPadding,
			[nameof(ILabel.Text)] = MapText,
			[nameof(ILabel.TextColor)] = MapTextColor,
			[nameof(ILabel.TextDecorations)] = MapTextDecorations,
		};

		public LabelHandler() : base(LabelMapper)
		{

		}

		public LabelHandler(PropertyMapper? mapper = null) : base(mapper ?? LabelMapper)
		{

		}
	}
}