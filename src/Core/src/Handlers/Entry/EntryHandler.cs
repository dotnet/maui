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
		public static IPropertyMapper<IEntry, IEntryHandler> Mapper = new PropertyMapper<IEntry, IEntryHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IEntry.Background)] = MapBackground,
			[nameof(IEntry.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(IEntry.ClearButtonVisibility)] = MapClearButtonVisibility,
			[nameof(IEntry.Font)] = MapFont,
			[nameof(IEntry.IsPassword)] = MapIsPassword,
			[nameof(IEntry.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(IEntry.VerticalTextAlignment)] = MapVerticalTextAlignment,
			[nameof(IEntry.IsReadOnly)] = MapIsReadOnly,
			[nameof(IEntry.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
			[nameof(IEntry.Keyboard)] = MapKeyboard,
			[nameof(IEntry.MaxLength)] = MapMaxLength,
			[nameof(IEntry.Placeholder)] = MapPlaceholder,
			[nameof(IEntry.PlaceholderColor)] = MapPlaceholderColor,
			[nameof(IEntry.ReturnType)] = MapReturnType,
			[nameof(IEntry.Text)] = MapText,
			[nameof(IEntry.TextColor)] = MapTextColor,
			[nameof(IEntry.CursorPosition)] = MapCursorPosition,
			[nameof(IEntry.SelectionLength)] = MapSelectionLength
		};

		public static CommandMapper<IEntry, IEntryHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public EntryHandler() : base(Mapper)
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
