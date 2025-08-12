#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiTextField;
#elif MONOANDROID
using PlatformView = AndroidX.AppCompat.Widget.AppCompatEditText;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.TextBox;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.NUI.Entry;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

using System;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : IEntryHandler
	{
		private static readonly IPropertyMapper<IEntry, IEntryHandler> TextMapper = new PropertyMapper<IEntry, IEntryHandler>
		{
			[nameof(IEntry.ClearButtonVisibility)] = MapClearButtonVisibility,
			[nameof(IEntry.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(IEntry.VerticalTextAlignment)] = MapVerticalTextAlignment,
			// Ensure Text is mapped before LineHeight/Decorations/CharacterSpacing/HorizontalTextAlignment/TextColor/Font
			// due to them being applied to the native object (i.e. AttributedText on iOS) created by mapping Text
			[nameof(IEntry.Text)] = MapText,
			[nameof(IEntry.MaxLength)] = MapMaxLength,
			[nameof(IEntry.Font)] = MapFont,
			[nameof(IEntry.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(IEntry.TextColor)] = MapTextColor
		};

		public static IPropertyMapper<IEntry, IEntryHandler> Mapper = new PropertyMapper<IEntry, IEntryHandler>(TextMapper, ViewHandler.ViewMapper)
		{
			[nameof(IEntry.Background)] = MapBackground,
			[nameof(IEntry.IsPassword)] = MapIsPassword,
			[nameof(IEntry.IsReadOnly)] = MapIsReadOnly,
			[nameof(IEntry.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
			[nameof(IEntry.IsSpellCheckEnabled)] = MapIsSpellCheckEnabled,
			[nameof(IEntry.Keyboard)] = MapKeyboard,
			[nameof(IEntry.Placeholder)] = MapPlaceholder,
			[nameof(IEntry.PlaceholderColor)] = MapPlaceholderColor,
			[nameof(IEntry.ReturnType)] = MapReturnType,
			[nameof(IEntry.CursorPosition)] = MapCursorPosition,
			[nameof(IEntry.SelectionLength)] = MapSelectionLength
		};

		public static CommandMapper<IEntry, IEntryHandler> CommandMapper = new(ViewCommandMapper)
		{
#if ANDROID
			[nameof(IEntry.Focus)] = MapFocus
#endif
		};

		public EntryHandler() : this(Mapper)
		{
		}

		public EntryHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public EntryHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IEntry IEntryHandler.VirtualView => VirtualView;

		PlatformView IEntryHandler.PlatformView => PlatformView;
	}
}