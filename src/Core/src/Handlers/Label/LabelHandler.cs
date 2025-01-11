#nullable enable
using System;
using System.Collections.Generic;
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
		internal static readonly IPlatformPropertyDefaults<ILabel> LabelPropertyDefaults = new PlatformPropertyDefaults<ILabel>(ViewHandler.ViewPropertyDefaults)
		{
			[nameof(ILabel.CharacterSpacing)] = HasDefaultCharacterSpacing,
			[nameof(ILabel.HorizontalTextAlignment)] = HasDefaultHorizontalTextAlignment,
			[nameof(ILabel.VerticalTextAlignment)] = HasDefaultVerticalTextAlignment,
			[nameof(ILabel.TextDecorations)] = HasDefaultTextDecorations,
		};

		public static IPropertyMapper<ILabel, ILabelHandler> Mapper = new PropertyMapper<ILabel, ILabelHandler>(ViewHandler.ViewMapper)
		{
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
			[nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ITextStyle.Font)] = MapFont,
			[nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(ITextAlignment.VerticalTextAlignment)] = MapVerticalTextAlignment,
			[nameof(ILabel.LineHeight)] = MapLineHeight,
			[nameof(ILabel.Padding)] = MapPadding,
			[nameof(ILabel.Text)] = MapText,
			[nameof(ITextStyle.TextColor)] = MapTextColor,
			[nameof(ILabel.TextDecorations)] = MapTextDecorations,
#if ANDROID
			[nameof(ILabel.Background)] = MapBackground,
#endif
		};

		public static CommandMapper<ILabel, ILabelHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public LabelHandler() : base(Mapper, CommandMapper)
		{
			PlatformPropertyDefaults = LabelPropertyDefaults;
		}

		public LabelHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
			PlatformPropertyDefaults = LabelPropertyDefaults;
		}

		public LabelHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
			PlatformPropertyDefaults = LabelPropertyDefaults;
		}

		ILabel ILabelHandler.VirtualView => VirtualView;

		PlatformView ILabelHandler.PlatformView => PlatformView;
				
		internal static bool HasDefaultTextDecorations(ILabel label)
		{
			return label.TextDecorations == TextDecorations.None;
		}

		internal static bool HasDefaultCharacterSpacing(ILabel label)
		{
			return label.CharacterSpacing == 0;
		}

		internal static bool HasDefaultHorizontalTextAlignment(ILabel label)
		{
			return label.HorizontalTextAlignment == TextAlignment.Start;
		}

		internal static bool HasDefaultVerticalTextAlignment(ILabel label)
		{
			return label.VerticalTextAlignment == TextAlignment.Start;
		}
	}
}