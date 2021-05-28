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
		TextWatcher Watcher { get; } = new TextWatcher();
		EntryTouchListener TouchListener { get; } = new EntryTouchListener();
		EntryFocusChangeListener FocusChangeListener { get; } = new EntryFocusChangeListener();
		EditorActionListener ActionListener { get; } = new EditorActionListener();

		static ColorStateList? DefaultTextColors { get; set; }
		static Drawable? ClearButtonDrawable { get; set; }
		static Drawable? DefaultBackground;

		protected override AppCompatEditText CreateNativeView()
		{
			return new AppCompatEditText(Context);
		}

		// Returns the default 'X' char drawable in the AppCompatEditText.
		protected virtual Drawable GetClearButtonDrawable() =>
			ContextCompat.GetDrawable(Context, Resource.Drawable.abc_ic_clear_material);

		protected override void ConnectHandler(AppCompatEditText nativeView)
		{
			Watcher.Handler = this;
			TouchListener.Handler = this;
			FocusChangeListener.Handler = this;
			ActionListener.Handler = this;

			nativeView.OnFocusChangeListener = FocusChangeListener;
			nativeView.AddTextChangedListener(Watcher);
			nativeView.SetOnTouchListener(TouchListener);
			nativeView.SetOnEditorActionListener(ActionListener);
		}

		protected override void DisconnectHandler(AppCompatEditText nativeView)
		{
			nativeView.RemoveTextChangedListener(Watcher);
			nativeView.SetOnTouchListener(null);
			nativeView.OnFocusChangeListener = null;
			nativeView.SetOnEditorActionListener(null);

			FocusChangeListener.Handler = null;
			Watcher.Handler = null;
			TouchListener.Handler = null;
			ActionListener.Handler = null;
		}

		protected override void SetupDefaults(AppCompatEditText nativeView)
		{
			base.SetupDefaults(nativeView);

			ClearButtonDrawable = GetClearButtonDrawable();
			DefaultTextColors = nativeView.TextColors;
			DefaultBackground = nativeView.Background;
		}

		// This is a Android-specific mapping
		public static void MapBackground(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateBackground(entry, DefaultBackground);
		}

		public static void MapText(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateText(entry);
		}

		public static void MapTextColor(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateTextColor(entry, DefaultTextColors);
		}

		public static void MapIsPassword(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateIsPassword(entry);
		}

		public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateHorizontalTextAlignment(entry);
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

		public static void MapClearButtonVisibility(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateClearButtonVisibility(entry, ClearButtonDrawable);
		}

		void OnFocusedChange(bool hasFocus)
		{
			if (NativeView == null || VirtualView == null)
				return;

			// This will eliminate additional native property setting if not required.
			if (VirtualView.ClearButtonVisibility == ClearButtonVisibility.WhileEditing)
				NativeView?.UpdateClearButtonVisibility(VirtualView, ClearButtonDrawable);
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

			// Even though <null> is technically different to "", it has no
			// functional difference to apps. Thus, hide it.
			var mauiText = VirtualView.Text ?? string.Empty;
			var nativeText = text ?? string.Empty;
			if (mauiText != nativeText)
				VirtualView.Text = nativeText;

			// Text changed should trigger clear button visibility.
			NativeView.UpdateClearButtonVisibility(VirtualView, ClearButtonDrawable);
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

			var rBounds = ClearButtonDrawable?.Bounds;

			if (rBounds != null)
			{
				var x = motionEvent.GetX();
				var y = motionEvent.GetY();

				if (motionEvent.Action == MotionEventActions.Up
					&& ((x >= (NativeView.Right - rBounds.Width())
					&& x <= (NativeView.Right - NativeView.PaddingRight)
					&& y >= NativeView.PaddingTop
					&& y <= (NativeView.Height - NativeView.PaddingBottom)
					&& (VirtualView.FlowDirection == FlowDirection.LeftToRight))
					|| (x >= (NativeView.Left + NativeView.PaddingLeft)
					&& x <= (NativeView.Left + rBounds.Width())
					&& y >= NativeView.PaddingTop
					&& y <= (NativeView.Height - NativeView.PaddingBottom)
					&& VirtualView.FlowDirection == FlowDirection.RightToLeft)))
				{
					NativeView.Text = null;

					return true;
				}
			}

			return false;
		}

		class TextWatcher : Java.Lang.Object, ITextWatcher
		{
			public EntryHandler? Handler { get; set; }

			void ITextWatcher.AfterTextChanged(IEditable? s)
			{
			}

			void ITextWatcher.BeforeTextChanged(Java.Lang.ICharSequence? s, int start, int count, int after)
			{
			}

			void ITextWatcher.OnTextChanged(Java.Lang.ICharSequence? s, int start, int before, int count)
			{
				// We are replacing 0 characters with 0 characters, so skip
				if (before == 0 && count == 0)
					return;

				Handler?.OnTextChanged(s?.ToString());
			}
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