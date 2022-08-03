#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiTextField;
#elif MONOANDROID
using PlatformView = AndroidX.AppCompat.Widget.AppCompatEditText;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.TextBox;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.ElmSharp.Entry;
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

		static EntryHandler()
		{
#if __IOS__
			Mapper.PrependToMapping(nameof(IView.FlowDirection), (h, __) => h.UpdateValue(nameof(ITextAlignment.HorizontalTextAlignment)));
#endif
		}

		public EntryHandler() : base(Mapper)
		{
		}

		public EntryHandler(IPropertyMapper? mapper = null) : base(mapper ?? Mapper)
		{
		}

		IEntry IEntryHandler.VirtualView => VirtualView;

		PlatformView IEntryHandler.PlatformView => PlatformView;
	}
}
