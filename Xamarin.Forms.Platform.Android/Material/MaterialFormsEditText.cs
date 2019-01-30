#if __ANDROID_28__
using System;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Android.Support.V4.Graphics.Drawable;
using Android.Support.Design.Widget;
using Android.Runtime;
using Android.Util;


namespace Xamarin.Forms.Platform.Android.Material
{

	public class MaterialFormsTextInputLayout : TextInputLayout
	{
		public MaterialFormsTextInputLayout(Context context) : base(context)
		{
			Init();
		}

		public MaterialFormsTextInputLayout(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			Init();
		}

		public MaterialFormsTextInputLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			Init();
		}

		protected MaterialFormsTextInputLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			Init();
		}

		void Init()
		{
			VisualElement.VerifyVisualFlagEnabled();
		}

	}

	public class MaterialFormsEditText : TextInputEditText, IDescendantFocusToggler, IFormsEditText
	{
		DescendantFocusToggler _descendantFocusToggler;

		// These paddings are a hack to center the hint
		// once this issue is resolved we can get rid of these paddings
		// https://github.com/material-components/material-components-android/issues/120
		// https://stackoverflow.com/questions/50487871/how-to-make-the-hint-text-of-textinputlayout-vertically-center

		static Thickness _centeredText = new Thickness(16, 8, 12, 27);
		static Thickness _alignedWithUnderlineText = new Thickness(16, 20, 12, 16);

		public MaterialFormsEditText(Context context) : base(context)
		{
			Init();
		}

		protected MaterialFormsEditText(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			Init();
		}

		public MaterialFormsEditText(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			Init();
		}

		public MaterialFormsEditText(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			Init();
		}

		void Init()
		{
			VisualElement.VerifyVisualFlagEnabled();
			UpdatePadding();
		}

		void UpdatePadding()
		{
			Thickness rect = _centeredText;

			if (!String.IsNullOrWhiteSpace(Text) || HasFocus)
			{
				rect = _alignedWithUnderlineText;
			}

			SetPadding((int)Context.ToPixels(rect.Left), (int)Context.ToPixels(rect.Top), (int)Context.ToPixels(rect.Right), (int)Context.ToPixels(rect.Bottom));
		}

		protected override void OnTextChanged(Java.Lang.ICharSequence text, int start, int lengthBefore, int lengthAfter)
		{
			base.OnTextChanged(text, start, lengthBefore, lengthAfter);
			if (lengthBefore == 0 || lengthAfter == 0)
				UpdatePadding();
		}

		protected override void OnFocusChanged(bool gainFocus, [GeneratedEnum] FocusSearchDirection direction, Rect previouslyFocusedRect)
		{
			base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);

			// Delay padding update until after the keyboard has showed up otherwise updating the padding
			// stops the keyboard from showing up
			if (gainFocus)
				Device.BeginInvokeOnMainThread(() => UpdatePadding());
			else
				UpdatePadding();
		}

		bool IDescendantFocusToggler.RequestFocus(global::Android.Views.View control, Func<bool> baseRequestFocus)
		{
			_descendantFocusToggler = _descendantFocusToggler ?? new DescendantFocusToggler();

			return _descendantFocusToggler.RequestFocus(control, baseRequestFocus);
		}

		public override bool OnKeyPreIme(Keycode keyCode, KeyEvent e)
		{
			if (keyCode != Keycode.Back || e.Action != KeyEventActions.Down)
			{
				return base.OnKeyPreIme(keyCode, e);
			}

			this.HideKeyboard();

			_onKeyboardBackPressed?.Invoke(this, EventArgs.Empty);
			return true;
		}

		public override bool RequestFocus(FocusSearchDirection direction, Rect previouslyFocusedRect)
		{
			return (this as IDescendantFocusToggler).RequestFocus(this, () => base.RequestFocus(direction, previouslyFocusedRect));
		}

		protected override void OnSelectionChanged(int selStart, int selEnd)
		{
			base.OnSelectionChanged(selStart, selEnd);
			_selectionChanged?.Invoke(this, new SelectionChangedEventArgs(selStart, selEnd));
		}

		event EventHandler _onKeyboardBackPressed;
		event EventHandler IFormsEditText.OnKeyboardBackPressed
		{
			add => _onKeyboardBackPressed += value;
			remove => _onKeyboardBackPressed -= value;
		}

		event EventHandler<SelectionChangedEventArgs> _selectionChanged;
		event EventHandler<SelectionChangedEventArgs> IFormsEditText.SelectionChanged
		{
			add => _selectionChanged += value;
			remove => _selectionChanged -= value;
		}
	}
}
#endif