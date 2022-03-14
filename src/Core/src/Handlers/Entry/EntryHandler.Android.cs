using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using static Android.Views.View;
using static Android.Widget.TextView;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : ViewHandler<IEntry, AppCompatEditText>
	{
		Drawable? _clearButtonDrawable;
		ColorStateList? _defaultPlaceholderColors;

		protected override AppCompatEditText CreatePlatformView()
		{
			var nativeEntry = new AppCompatEditText(Context);
			_defaultPlaceholderColors = nativeEntry.HintTextColors;
			return nativeEntry;
		}

		// Returns the default 'X' char drawable in the AppCompatEditText.
		protected virtual Drawable GetClearButtonDrawable() =>
			_clearButtonDrawable ??= ContextCompat.GetDrawable(Context, Resource.Drawable.abc_ic_clear_material);

		protected override void ConnectHandler(AppCompatEditText platformView)
		{
			platformView.TextChanged += OnTextChanged;
			platformView.FocusChange += OnFocusedChange;
			platformView.Touch += OnTouch;
			platformView.EditorAction += OnEditorAction;
		}

		protected override void DisconnectHandler(AppCompatEditText platformView)
		{
			_clearButtonDrawable = null;
			platformView.TextChanged -= OnTextChanged;
			platformView.FocusChange -= OnFocusedChange;
			platformView.Touch -= OnTouch;
			platformView.EditorAction -= OnEditorAction;
		}

		public static void MapBackground(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateBackground(entry);

		public static void MapText(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateText(entry);

		public static void MapTextColor(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateTextColor(entry);

		public static void MapIsPassword(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsPassword(entry);

		public static void MapHorizontalTextAlignment(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(entry);

		public static void MapVerticalTextAlignment(IEntryHandler handler, IEntry entry) =>
			handler?.PlatformView?.UpdateVerticalTextAlignment(entry);

		public static void MapIsTextPredictionEnabled(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsTextPredictionEnabled(entry);

		public static void MapMaxLength(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateMaxLength(entry);

		public static void MapPlaceholder(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdatePlaceholder(entry);

		public static void MapPlaceholderColor(IEntryHandler handler, IEntry entry)
		{
			if (handler is EntryHandler platformHandler)
				handler.PlatformView?.UpdatePlaceholderColor(entry, platformHandler._defaultPlaceholderColors);
		}

		public static void MapFont(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateFont(entry, handler.GetRequiredService<IFontManager>());

		public static void MapIsReadOnly(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsReadOnly(entry);

		public static void MapKeyboard(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateKeyboard(entry);

		public static void MapReturnType(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateReturnType(entry);

		public static void MapCharacterSpacing(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateCharacterSpacing(entry);

		public static void MapCursorPosition(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateCursorPosition(entry);

		public static void MapSelectionLength(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateSelectionLength(entry);

		public static void MapClearButtonVisibility(IEntryHandler handler, IEntry entry)
		{
			if (handler is EntryHandler platformHandler)
				handler.PlatformView?.UpdateClearButtonVisibility(entry, platformHandler.GetClearButtonDrawable);
		}

		void OnTextChanged(object? sender, TextChangedEventArgs e) =>
			VirtualView?.UpdateText(e);

		// This will eliminate additional native property setting if not required.
		void OnFocusedChange(object? sender, FocusChangeEventArgs e)
		{
			if (VirtualView?.ClearButtonVisibility == ClearButtonVisibility.WhileEditing)
				UpdateValue(nameof(IEntry.ClearButtonVisibility));
		}

		// Check whether the touched position inbounds with clear button.
		void OnTouch(object? sender, TouchEventArgs e) =>
			e.Handled =
				VirtualView?.ClearButtonVisibility == ClearButtonVisibility.WhileEditing &&
				PlatformView.HandleClearButtonTouched(VirtualView.FlowDirection, e, GetClearButtonDrawable);

		void OnEditorAction(object? sender, EditorActionEventArgs e)
		{
			if (e.IsCompletedAction())
			{
				// TODO: Dismiss keyboard for hardware / physical keyboards

				VirtualView?.Completed();
			}

			e.Handled = true;
		}
	}
}