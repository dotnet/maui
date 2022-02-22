#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiLabel;
#elif MONOANDROID
using PlatformView = AndroidX.AppCompat.Widget.AppCompatTextView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.TextBlock;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : ILabelHandler
	{
		public static IPropertyMapper<ILabel, ILabelHandler> LabelMapper = new PropertyMapper<ILabel, ILabelHandler>(ViewHandler.ViewMapper)
		{
#if WINDOWS || __IOS__
			[nameof(ILabel.Background)] = MapBackground,
			[nameof(ILabel.Opacity)] = MapOpacity,
#endif
			[nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ITextStyle.Font)] = MapFont,
			[nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(ITextAlignment.VerticalTextAlignment)] = MapVerticalTextAlignment,
			[nameof(ILabel.LineBreakMode)] = MapLineBreakMode,
			[nameof(ILabel.LineHeight)] = MapLineHeight,
			[nameof(ILabel.MaxLines)] = MapMaxLines,
			[nameof(ILabel.Padding)] = MapPadding,
			[nameof(ILabel.Text)] = MapText,
			[nameof(ITextStyle.TextColor)] = MapTextColor,
			[nameof(ILabel.TextDecorations)] = MapTextDecorations,
		};

		public static CommandMapper<IActivityIndicator, ILabelHandler> CommandMapper = new(ViewCommandMapper)
		{
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

		ILabel ILabelHandler.VirtualView => VirtualView;

		PlatformView ILabelHandler.PlatformView => PlatformView;
	}
}