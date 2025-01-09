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
		private static Dictionary<string, Func<IElement, bool>> initSkipChecks = new(StringComparer.Ordinal)
		{
			{ nameof(ILabel.Height), SkipCheckHeight },
			{ nameof(ILabel.Opacity), SkipCheckOpacity },

			{ nameof(ILabel.CharacterSpacing), SkipCheckCharacterSpacing },
			{ nameof(ILabel.HorizontalTextAlignment), SkipCheckHorizontalTextAlignment },
			{ nameof(ILabel.VerticalTextAlignment), SkipCheckVerticalTextAlignment },
			{ nameof(ILabel.Padding), SkipCheckPadding },
			{ nameof(ILabel.TextDecorations), SkipCheckTextDecorations },
		};

		public static IPropertyMapper<ILabel, ILabelHandler> Mapper = new PropertyMapper<ILabel, ILabelHandler>(initSkipChecks, ViewHandler.ViewMapper)
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

		internal static bool SkipCheckHeight(IElement label)
		{
			return double.IsNaN(((ILabel)label).Height);
		}
		
		internal static bool SkipCheckOpacity(IElement label)
		{
			return ((ILabel)label).Opacity == 1;
		}
		
		internal static bool SkipCheckTextDecorations(IElement label)
		{
			return ((ILabel)label).TextDecorations == TextDecorations.None;
		}

		internal static bool SkipCheckCharacterSpacing(IElement label)
		{
			return ((ILabel)label).CharacterSpacing == 0;
		}
		
		internal static bool SkipCheckPadding(IElement label)
		{
			return ((ILabel)label).Padding.IsEmpty;
		}
		
		internal static bool SkipCheckHorizontalTextAlignment(IElement label)
		{
			return ((ILabel)label).HorizontalTextAlignment == TextAlignment.Start;
		}

		internal static bool SkipCheckVerticalTextAlignment(IElement label)
		{
			return ((ILabel)label).VerticalTextAlignment == TextAlignment.Start;
		}
	}
}