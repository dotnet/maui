using System;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using Microsoft.Extensions.DependencyInjection;
using static Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : AbstractViewHandler<IEntry, AppCompatEditText>
	{
		TextWatcher Watcher { get; } = new TextWatcher();
		EntryTouchListener TouchListener { get; } = new EntryTouchListener();
		EntryFocusChangeListener FocusChangeListener { get; } = new EntryFocusChangeListener();

		static ColorStateList? DefaultTextColors { get; set; }
		static Drawable? ClearButtonDrawable { get; set; }

		protected override AppCompatEditText CreateNativeView()
		{
			return new AppCompatEditText(Context);
		}

		// Returns the default 'X' char drawable in the AppCompatEditText.
		protected virtual Drawable GetClearButtonDrawable()
			=> ContextCompat.GetDrawable(Context, Resource.Drawable.abc_ic_clear_material);

		protected override void ConnectHandler(AppCompatEditText nativeView)
		{
			Watcher.Handler = this;
			TouchListener.Handler = this;
			FocusChangeListener.Handler = this;

			nativeView.OnFocusChangeListener = FocusChangeListener;
			nativeView.AddTextChangedListener(Watcher);
			nativeView.SetOnTouchListener(TouchListener);
		}

		protected override void DisconnectHandler(AppCompatEditText nativeView)
		{
			nativeView.RemoveTextChangedListener(Watcher);
			nativeView.SetOnTouchListener(null);
			nativeView.OnFocusChangeListener = null;

			FocusChangeListener.Handler = null;
			Watcher.Handler = null;
			TouchListener.Handler = null;
		}

		protected override void SetupDefaults(AppCompatEditText nativeView)
		{
			base.SetupDefaults(nativeView);

			ClearButtonDrawable = GetClearButtonDrawable();
			DefaultTextColors = nativeView.TextColors;
		}

		public static void MapText(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateText(entry);
		}

		public static void MapTextColor(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateTextColor(entry, DefaultTextColors);
		}

		public static void MapIsPassword(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateIsPassword(entry);
		}

		public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateHorizontalTextAlignment(entry);
		}

		public static void MapIsTextPredictionEnabled(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateIsTextPredictionEnabled(entry);
		}

		public static void MapMaxLength(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateMaxLength(entry);
		}

		public static void MapPlaceholder(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdatePlaceholder(entry);
		}

		public static void MapFont(EntryHandler handler, IEntry entry)
		{
			_ = handler.Services ?? throw new InvalidOperationException($"{nameof(Services)} should have been set by base class.");

			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			handler.TypedNativeView?.UpdateFont(entry, fontManager);
		}

		public static void MapIsReadOnly(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateIsReadOnly(entry);
		}

		public static void MapReturnType(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateReturnType(entry);
		}

		public static void MapCharacterSpacing(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateCharacterSpacing(entry);
		}

		public static void MapClearButtonVisibility(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateClearButtonVisibility(entry, ClearButtonDrawable);
		}

		void OnFocusedChange(bool hasFocus)
		{
			if (TypedNativeView == null || VirtualView == null)
				return;

			// This will eliminate additional native property setting if not required.
			if (VirtualView.ClearButtonVisibility == ClearButtonVisibility.WhileEditing)
				TypedNativeView?.UpdateClearButtonVisibility(VirtualView, ClearButtonDrawable);
		}

		bool OnTouch(MotionEvent? motionEvent)
		{
			if (TypedNativeView == null || VirtualView == null)
				return false;

			// Check whether the touched position inbounds with clear button.
			return HandleClearButtonTouched(motionEvent);
		}

		void OnTextChanged(string? text)
		{
			if (VirtualView == null || TypedNativeView == null)
				return;

			// Even though <null> is technically different to "", it has no
			// functional difference to apps. Thus, hide it.
			var mauiText = VirtualView.Text ?? string.Empty;
			var nativeText = text ?? string.Empty;
			if (mauiText != nativeText)
				VirtualView.Text = nativeText;

			// Text changed should trigger clear button visibility.
			TypedNativeView.UpdateClearButtonVisibility(VirtualView, ClearButtonDrawable);
		}

		/// <summary>
		/// Checks whether the touched position on the EditText is inbounds with clear button and clears if so.
		/// This will return True to handle OnTouch to prevent re-activating keyboard after clearing the text.
		/// </summary>
		/// <returns>True if clear button is clicked and Text is cleared. False if not.</returns>
		bool HandleClearButtonTouched(MotionEvent? motionEvent)
		{
			if (motionEvent == null || TypedNativeView == null || VirtualView == null)
				return false;

			var rBounds = ClearButtonDrawable?.Bounds;

			if (rBounds != null)
			{
				var x = motionEvent.GetX();
				var y = motionEvent.GetY();

				if (motionEvent.Action == MotionEventActions.Up
					&& ((x >= (TypedNativeView.Right - rBounds.Width())
					&& x <= (TypedNativeView.Right - TypedNativeView.PaddingRight)
					&& y >= TypedNativeView.PaddingTop
					&& y <= (TypedNativeView.Height - TypedNativeView.PaddingBottom)
					&& (VirtualView.FlowDirection == FlowDirection.LeftToRight))
					|| (x >= (TypedNativeView.Left + TypedNativeView.PaddingLeft)
					&& x <= (TypedNativeView.Left + rBounds.Width())
					&& y >= TypedNativeView.PaddingTop
					&& y <= (TypedNativeView.Height - TypedNativeView.PaddingBottom)
					&& VirtualView.FlowDirection == FlowDirection.RightToLeft)))
				{
					TypedNativeView.Text = null;

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
				Handler?.OnTextChanged(s?.ToString());
			}
		}

		// TODO: Maybe better to have generic version in IAndroidViewHandler?
		class EntryTouchListener : Java.Lang.Object, IOnTouchListener
		{
			public EntryHandler? Handler { get; set; }

			public bool OnTouch(View? v, MotionEvent? e)
			{
				return Handler?.OnTouch(e) ?? false;
			}
		}

		// TODO: Maybe better to have generic version in IAndroidViewHandler?
		class EntryFocusChangeListener : Java.Lang.Object, IOnFocusChangeListener
		{
			public EntryHandler? Handler { get; set; }
			public void OnFocusChange(View? v, bool hasFocus)
			{
				Handler?.OnFocusedChange(hasFocus);
			}
		}
	}
}