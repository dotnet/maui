using System;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using static Android.Views.View;
using static Android.Widget.TextView;

namespace Microsoft.Maui.Handlers
{
	// TODO: NET8 issoto - Change the TPlatformView generic type to MauiAppCompatEditText
	// This type adds support to the SelectionChanged event
	public partial class EntryHandler : ViewHandler<IEntry, AppCompatEditText>
	{
		Drawable? _clearButtonDrawable;
		bool _clearButtonVisible;
		bool _set;

		protected override AppCompatEditText CreatePlatformView()
		{
			var nativeEntry = new MauiAppCompatEditText(Context);
			return nativeEntry;
		}

		// Returns the default 'X' char drawable in the AppCompatEditText.
		protected virtual Drawable? GetClearButtonDrawable() =>
			_clearButtonDrawable ??= ContextCompat.GetDrawable(Context, Resource.Drawable.abc_ic_clear_material);

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			// TODO: NET8 issoto - Remove the casting once we can set the TPlatformView generic type as MauiAppCompatEditText
			if (!_set && PlatformView is MauiAppCompatEditText editText)
				editText.SelectionChanged += OnSelectionChanged;

			_set = true;
		}

		// TODO: NET8 issoto - Change the return type to MauiAppCompatEditText
		protected override void ConnectHandler(AppCompatEditText platformView)
		{
			platformView.ViewAttachedToWindow += OnViewAttachedToWindow;
			platformView.TextChanged += OnTextChanged;
			platformView.FocusChange += OnFocusedChange;
			platformView.Touch += OnTouch;
			platformView.EditorAction += OnEditorAction;
		}

		// TODO: NET8 issoto - Change the return type to MauiAppCompatEditText
		protected override void DisconnectHandler(AppCompatEditText platformView)
		{
			_clearButtonDrawable = null;

			platformView.ViewAttachedToWindow -= OnViewAttachedToWindow;
			platformView.TextChanged -= OnTextChanged;
			platformView.FocusChange -= OnFocusedChange;
			platformView.Touch -= OnTouch;
			platformView.EditorAction -= OnEditorAction;

			// TODO: NET8 issoto - Remove the casting once we can set the TPlatformView generic type as MauiAppCompatEditText
			if (_set && platformView is MauiAppCompatEditText editText)
				editText.SelectionChanged -= OnSelectionChanged;

			_set = false;
		}

		public static void MapBackground(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateBackground(entry);

		public static void MapText(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateText(entry);

		public static void MapTextColor(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateTextColor(entry);

		public static void MapIsPassword(IEntryHandler handler, IEntry entry)
		{
			handler.UpdateValue(nameof(IEntry.Text));

			handler.PlatformView?.UpdateIsPassword(entry);
		}

		public static void MapHorizontalTextAlignment(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(entry);

		public static void MapVerticalTextAlignment(IEntryHandler handler, IEntry entry) =>
			handler?.PlatformView?.UpdateVerticalTextAlignment(entry);

		public static void MapIsTextPredictionEnabled(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsTextPredictionEnabled(entry);

		public static void MapIsSpellCheckEnabled(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsSpellCheckEnabled(entry);

		public static void MapMaxLength(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateMaxLength(entry);

		public static void MapPlaceholder(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdatePlaceholder(entry);

		public static void MapPlaceholderColor(IEntryHandler handler, IEntry entry)
		{
			if (handler is EntryHandler platformHandler)
				handler.PlatformView?.UpdatePlaceholderColor(entry);
		}

		public static void MapFont(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateFont(entry, handler.GetRequiredService<IFontManager>());

		public static void MapIsReadOnly(IEntryHandler handler, IEntry entry)
		{
			handler.UpdateValue(nameof(IEntry.Text));

			handler.PlatformView?.UpdateIsReadOnly(entry);
		}

		public static void MapKeyboard(IEntryHandler handler, IEntry entry)
		{
			handler.UpdateValue(nameof(IEntry.Text));

			handler.PlatformView?.UpdateKeyboard(entry);
		}

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

		static void MapFocus(IEntryHandler handler, IEntry entry, object? args)
		{
			if (args is FocusRequest request)
				handler.PlatformView.Focus(request);
		}

		void OnViewAttachedToWindow(object? sender, ViewAttachedToWindowEventArgs e)
		{
			if (PlatformView is null || VirtualView is null)
				return;

			PlatformView.UpdateReturnType(VirtualView);
		}

		void OnTextChanged(object? sender, TextChangedEventArgs e)
		{
			if (VirtualView == null)
			{
				return;
			}

			// Let the mapping know that the update is coming from changes to the platform control
			DataFlowDirection = DataFlowDirection.FromPlatform;

			VirtualView.UpdateText(e);

			// Reset to the default direction
			DataFlowDirection = DataFlowDirection.ToPlatform;

			MapClearButtonVisibility(this, VirtualView);
		}

		void OnFocusedChange(object? sender, FocusChangeEventArgs e)
		{
			if (VirtualView == null)
			{
				return;
			}

			MapClearButtonVisibility(this, VirtualView);
		}

		// Check whether the touched position inbounds with clear button.
		void OnTouch(object? sender, TouchEventArgs e) =>
			e.Handled =
				_clearButtonVisible && VirtualView != null &&
				PlatformView.HandleClearButtonTouched(e, GetClearButtonDrawable);

		void OnEditorAction(object? sender, EditorActionEventArgs e)
		{
			var returnType = VirtualView?.ReturnType;

			// Inside of the android implementations that map events to listeners, the default return value for "Handled" is always true
			// This means, just by subscribing to EditorAction/KeyPressed/etc.. you change the behavior of the control
			// So, we are setting handled to false here in order to maintain default behavior
			bool handled = false;
			if (returnType != null)
			{
				var actionId = e.ActionId;
				var evt = e.Event;
				ImeAction currentInputImeFlag = PlatformView.ImeOptions;

				// On API 34 it looks like they fixed the issue where the actionId is ImeAction.ImeNull when using a keyboard
				// so I'm just setting the actionId here to the current ImeOptions so the logic can all be simplified
				if (actionId == ImeAction.ImeNull && evt?.KeyCode == Keycode.Enter)
				{
					actionId = currentInputImeFlag;
				}

				// keyboard path
				if (evt?.KeyCode == Keycode.Enter && evt?.Action == KeyEventActions.Down)
				{
					handled = true;
				}
				else if (evt?.KeyCode == Keycode.Enter && evt?.Action == KeyEventActions.Up)
				{
					VirtualView?.Completed();
				}
				// InputPaneView Path
				else if (evt?.KeyCode is null && (actionId == ImeAction.Done || actionId == currentInputImeFlag))
				{
					VirtualView?.Completed();
				}
			}

			e.Handled = handled;
		}

		private void OnSelectionChanged(object? sender, EventArgs e)
		{
			var cursorPosition = PlatformView.GetCursorPosition();
			var selectedTextLength = PlatformView.GetSelectedTextLength();

			if (VirtualView.CursorPosition != cursorPosition)
				VirtualView.CursorPosition = cursorPosition;

			if (VirtualView.SelectionLength != selectedTextLength)
				VirtualView.SelectionLength = selectedTextLength;
		}

		internal void ShowClearButton()
		{
			if (_clearButtonVisible)
			{
				return;
			}

			var drawable = GetClearButtonDrawable();

			if (VirtualView?.TextColor is not null)
				drawable?.SetColorFilter(VirtualView.TextColor.ToPlatform(), FilterMode.SrcIn);
			else
				drawable?.ClearColorFilter();

			if (PlatformView.LayoutDirection == LayoutDirection.Rtl)
				PlatformView.SetCompoundDrawablesWithIntrinsicBounds(drawable, null, null, null);
			else
				PlatformView.SetCompoundDrawablesWithIntrinsicBounds(null, null, drawable, null);

			_clearButtonVisible = true;
		}

		internal void HideClearButton()
		{
			if (!_clearButtonVisible)
			{
				return;
			}

			PlatformView.SetCompoundDrawablesWithIntrinsicBounds(null, null, null, null);
			_clearButtonVisible = false;
		}
	}
}
