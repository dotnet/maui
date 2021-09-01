#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler
	{
		public static IPropertyMapper<ILabel, LabelHandler> LabelMapper = new PropertyMapper<ILabel, LabelHandler>(ViewHandler.ViewMapper)
		{
#if WINDOWS || __IOS__
			[nameof(ILabel.Background)] = MapBackground,
#endif
			[nameof(ILabel.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ILabel.Font)] = MapFont,
			[nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(ITextAlignment.VerticalTextAlignment)] = MapVerticalTextAlignment,
			[nameof(ILabel.LineBreakMode)] = MapLineBreakMode,
			[nameof(ILabel.LineHeight)] = MapLineHeight,
			[nameof(ILabel.MaxLines)] = MapMaxLines,
			[nameof(ILabel.Padding)] = MapPadding,
			[nameof(ILabel.Text)] = MapText,
			[nameof(ILabel.TextColor)] = MapTextColor,
			[nameof(ILabel.TextDecorations)] = MapTextDecorations,
		};

		static LabelHandler()
		{
#if __IOS__
			LabelMapper.PrependToMapping(nameof(IView.FlowDirection), (h, __) => h.UpdateValue(nameof(ITextAlignment.HorizontalTextAlignment)));
#endif
		}

		public LabelHandler() : base(LabelMapper)
		{

		}

		public LabelHandler(IPropertyMapper? mapper = null) : base(mapper ?? LabelMapper)
		{

		}
	}
}