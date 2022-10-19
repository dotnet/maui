#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiTextView;
#elif MONOANDROID
using PlatformView = AndroidX.AppCompat.Widget.AppCompatEditText;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.TextBox;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.NUI.Editor;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : IEditorHandler
	{
		public static IPropertyMapper<IEditor, IEditorHandler> Mapper = new PropertyMapper<IEditor, IEditorHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IEditor.Background)] = MapBackground,
			[nameof(IEditor.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(IEditor.Font)] = MapFont,
			[nameof(IEditor.IsReadOnly)] = MapIsReadOnly,
			[nameof(IEditor.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
			[nameof(IEditor.MaxLength)] = MapMaxLength,
			[nameof(IEditor.Placeholder)] = MapPlaceholder,
			[nameof(IEditor.PlaceholderColor)] = MapPlaceholderColor,
			[nameof(IEditor.Text)] = MapText,
			[nameof(IEditor.TextColor)] = MapTextColor,
			[nameof(IEditor.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(IEditor.VerticalTextAlignment)] = MapVerticalTextAlignment,
			[nameof(IEditor.Keyboard)] = MapKeyboard,
			[nameof(IEditor.CursorPosition)] = MapCursorPosition,
			[nameof(IEditor.SelectionLength)] = MapSelectionLength,
#if IOS
			[nameof(IEditor.IsEnabled)] = MapIsEnabled,
#endif
		};

		public static CommandMapper<IEditor, IEditorHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public EditorHandler() : base(Mapper)
		{
		}

		public EditorHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public EditorHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IEditor IEditorHandler.VirtualView => VirtualView;

		PlatformView IEditorHandler.PlatformView => PlatformView;
	}
}
