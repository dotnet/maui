#nullable enable
using System;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler
	{
		public static IPropertyMapper<IEntry, EntryHandler> EntryMapper = new PropertyMapper<IEntry, EntryHandler>(ViewHandler.ViewMapper)
		{
#if __ANDROID__
			[nameof(IEntry.Background)] = MapBackground,
#endif
			[nameof(IEntry.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(IEntry.ClearButtonVisibility)] = MapClearButtonVisibility,
			[nameof(IEntry.Font)] = MapFont,
			[nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(ITextAlignment.VerticalTextAlignment)] = MapVerticalTextAlignment,
			[nameof(IEntry.IsPassword)] = MapIsPassword,
			[nameof(IEntry.IsReadOnly)] = MapIsReadOnly,
			[nameof(IEntry.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
			[nameof(IEntry.Keyboard)] = MapKeyboard,
			[nameof(IEntry.MaxLength)] = MapMaxLength,
			[nameof(IEntry.Placeholder)] = MapPlaceholder,
			[nameof(IEntry.ReturnType)] = MapReturnType,
			[nameof(IEntry.Text)] = MapText,
			[nameof(IEntry.TextColor)] = MapTextColor,
			[nameof(IEntry.CursorPosition)] = MapCursorPosition,
			[nameof(IEntry.SelectionLength)] = MapSelectionLength
		};


		static EntryHandler()
		{
#if __IOS__
			EntryMapper.PrependToMapping(nameof(IView.FlowDirection), (h, __) => h.UpdateValue(nameof(ITextAlignment.HorizontalTextAlignment)));
#endif
		}

		public EntryHandler() : base(EntryMapper)
		{

		}

		public EntryHandler(IPropertyMapper? mapper = null) : base(mapper ?? EntryMapper)
		{

		}
	}
}