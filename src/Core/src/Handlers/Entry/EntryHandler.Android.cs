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
		readonly EntryTouchListener _touchListener = new();
		readonly EntryFocusChangeListener _focusChangeListener = new();
		readonly EditorActionListener _actionListener = new();

		Drawable? _clearButtonDrawable;
		ColorStateList? _defaultPlaceholderColors;

		protected override AppCompatEditText CreateNativeView()
		{
			var nativeEntry = new AppCompatEditText(Context);

			_defaultPlaceholderColors = nativeEntry.HintTextColors;

			return nativeEntry;
		}

		// Returns the default 'X' char drawable in the AppCompatEditText.
		protected virtual Drawable GetClearButtonDrawable() =>
			_clearButtonDrawable ??= ContextCompat.GetDrawable(Context, Resource.Drawable.abc_ic_clear_material);

		protected override void ConnectHandler(AppCompatEditText nativeView)
		{
			_touchListener.Handler = this;
			_focusChangeListener.Handler = this;
			_actionListener.Handler = this;

			nativeView.TextChanged += OnTextChanged;
			nativeView.OnFocusChangeListener = _focusChangeListener;
			nativeView.SetOnTouchListener(_touchListener);
			nativeView.SetOnEditorActionListener(_actionListener);
		}

		protected override void DisconnectHandler(AppCompatEditText nativeView)
		{
			_clearButtonDrawable = null;

			nativeView.TextChanged -= OnTextChanged;
			nativeView.SetOnTouchListener(null);
			nativeView.OnFocusChangeListener = null;
			nativeView.SetOnEditorActionListener(null);

			_focusChangeListener.Handler = null;
			_touchListener.Handler = null;
			_actionListener.Handler = null;
		}

		void OnTextChanged(object? sender, Android.Text.TextChangedEventArgs e) =>
			VirtualView.UpdateText(e);

		// This is a Android-specific mapping
		public static void MapBackground(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateBackground(entry);
		}

		public static void MapText(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateText(entry);
		}

		public static void MapTextColor(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateTextColor(entry);
		}

		public static void MapIsPassword(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateIsPassword(entry);
		}

		public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateHorizontalTextAlignment(entry);
		}

		public static void MapVerticalTextAlignment(EntryHandler handler, IEntry entry)
		{
			handler?.NativeView?.UpdateVerticalTextAlignment(entry);
		}

		public static void MapIsTextPredictionEnabled(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateIsTextPredictionEnabled(entry);
		}

		public static void MapMaxLength(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateMaxLength(entry);
		}

		public static void MapPlaceholder(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdatePlaceholder(entry);
		}

		public static void MapPlaceholderColor(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdatePlaceholderColor(entry, handler._defaultPlaceholderColors);
		}

		public static void MapFont(EntryHandler handler, IEntry entry)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(entry, fontManager);
		}

		public static void MapIsReadOnly(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateIsReadOnly(entry);
		}

		public static void MapKeyboard(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateKeyboard(entry);
		}

		public static void MapReturnType(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateReturnType(entry);
		}

		public static void MapCharacterSpacing(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateCharacterSpacing(entry);
		}

		public static void MapCursorPosition(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateCursorPosition(entry);
		}

		public static void MapSelectionLength(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateSelectionLength(entry);
		}

		public static void MapClearButtonVisibility(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateClearButtonVisibility(entry, handler.GetClearButtonDrawable);
		}

		void OnFocusedChange(bool hasFocus)
		{
			if (NativeView == null || VirtualView == null)
				return;

			// This will eliminate additional native property setting if not required.
			if (VirtualView.ClearButtonVisibility == ClearButtonVisibility.WhileEditing)
				UpdateValue(nameof(VirtualView.ClearButtonVisibility));
		}

		bool OnTouch(MotionEvent? motionEvent)
		{
			if (NativeView == null || VirtualView == null)
				return false;

			// Check whether the touched position inbounds with clear button.
			return HandleClearButtonTouched(motionEvent);
		}

		void OnTextChanged(string? text)
		{
			if (VirtualView == null || NativeView == null)
				return;

			VirtualView.UpdateText(text);

			// Text changed should trigger clear button visibility.
			UpdateValue(nameof(VirtualView.ClearButtonVisibility));
		}

		/// <summary>
		/// Checks whether the touched position on the EditText is inbounds with clear button and clears if so.
		/// This will return True to handle OnTouch to prevent re-activating keyboard after clearing the text.
		/// </summary>
		/// <returns>True if clear button is clicked and Text is cleared. False if not.</returns>
		bool HandleClearButtonTouched(MotionEvent? motionEvent)
		{
			if (motionEvent == null || NativeView == null || VirtualView == null)
				return false;

			var virtualView = VirtualView;
			if (virtualView.ClearButtonVisibility == ClearButtonVisibility.Never)
				return false;

			var rBounds = GetClearButtonDrawable()?.Bounds;
			var buttonWidth = rBounds?.Width();

			if (buttonWidth > 0)
			{
				var x = motionEvent.GetX();
				var y = motionEvent.GetY();
				var nativeView = NativeView;

				if (motionEvent.Action == MotionEventActions.Up
					&& ((x >= nativeView.Right - buttonWidth
					&& x <= nativeView.Right - nativeView.PaddingRight
					&& y >= nativeView.PaddingTop
					&& y <= nativeView.Height - nativeView.PaddingBottom
					&& virtualView.FlowDirection == FlowDirection.LeftToRight)
					|| (x >= nativeView.Left + nativeView.PaddingLeft
					&& x <= nativeView.Left + buttonWidth
					&& y >= nativeView.PaddingTop
					&& y <= nativeView.Height - nativeView.PaddingBottom
					&& virtualView.FlowDirection == FlowDirection.RightToLeft)))
				{
					nativeView.Text = null;

					return true;
				}
			}

			return false;
		}

		// TODO: Maybe better to have generic version in INativeViewHandler?
		class EntryTouchListener : Java.Lang.Object, IOnTouchListener
		{
			public EntryHandler? Handler { get; set; }

			public bool OnTouch(View? v, MotionEvent? e)
			{
				return Handler?.OnTouch(e) ?? false;
			}
		}

		// TODO: Maybe better to have generic version in INativeViewHandler?
		class EntryFocusChangeListener : Java.Lang.Object, IOnFocusChangeListener
		{
			public EntryHandler? Handler { get; set; }
			public void OnFocusChange(View? v, bool hasFocus)
			{
				Handler?.OnFocusedChange(hasFocus);
			}
		}

		class EditorActionListener : Java.Lang.Object, IOnEditorActionListener
		{
			public EntryHandler? Handler { get; set; }

			public bool OnEditorAction(TextView? v, [GeneratedEnum] ImeAction actionId, KeyEvent? e)
			{
				if (actionId == ImeAction.Done || (actionId == ImeAction.ImeNull && e?.KeyCode == Keycode.Enter && e?.Action == KeyEventActions.Up))
				{
					// TODO: Dismiss keyboard for hardware / physical keyboards

					Handler?.VirtualView?.Completed();
				}

				return true;
			}
		}
	}
}