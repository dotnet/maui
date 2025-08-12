#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiLabel;
#elif MONOANDROID
using PlatformView = AndroidX.AppCompat.Widget.AppCompatTextView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.TextBlock;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.NUI.Label;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : ILabelHandler
	{
		private static readonly IPropertyMapper<ILabel, ILabelHandler> TextMapper = new PropertyMapper<ILabel, ILabelHandler>
		{
			// Ensure Text is mapped before LineHeight/Decorations/CharacterSpacing/HorizontalTextAlignment/TextColor/Font
			// due to them being applied to the native object (i.e. AttributedText on iOS) created by mapping Text
			[nameof(ILabel.Text)] = MapText,
			[nameof(ITextStyle.Font)] = MapFont,
			[nameof(ILabel.LineHeight)] = MapLineHeight,
			[nameof(ILabel.TextDecorations)] = MapTextDecorations,
			[nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(ITextAlignment.VerticalTextAlignment)] = MapVerticalTextAlignment,
			[nameof(ITextStyle.TextColor)] = MapTextColor,
		};

		public static IPropertyMapper<ILabel, ILabelHandler> Mapper = new PropertyMapper<ILabel, ILabelHandler>(TextMapper, ViewHandler.ViewMapper)
		{
			[nameof(ILabel.Padding)] = MapPadding,
#if IOS || TIZEN
			[nameof(ILabel.Background)] = MapBackground,
			[nameof(ILabel.Opacity)] = MapOpacity,
#elif WINDOWS
			[nameof(ILabel.Background)] = MapBackground,
			[nameof(ILabel.Height)] = MapHeight,
			[nameof(ILabel.Opacity)] = MapOpacity,
#endif
#if TIZEN
			[nameof(ILabel.Shadow)] = MapShadow,
#endif
#if ANDROID
			[nameof(ILabel.Background)] = MapBackground,
#endif
		};

		public static CommandMapper<ILabel, ILabelHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public LabelHandler() : base(Mapper, CommandMapper)
		{
		}

		public LabelHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public LabelHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		ILabel ILabelHandler.VirtualView => VirtualView;

		PlatformView ILabelHandler.PlatformView => PlatformView;
	}
}