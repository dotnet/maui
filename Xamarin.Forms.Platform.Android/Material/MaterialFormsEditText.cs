#if __ANDROID81__
#else
using System;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Android.Support.V4.Graphics.Drawable;
using Android.Support.Design.Widget;

namespace Xamarin.Forms.Platform.Android.Material
{
	public class MaterialFormsEditText : TextInputEditText, IDescendantFocusToggler
	{
		DescendantFocusToggler _descendantFocusToggler;

		public MaterialFormsEditText(Context context) : base(context)
		{
			VisualElement.VerifyVisualFlagEnabled();
			DrawableCompat.Wrap(Background);
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

			OnKeyboardBackPressed?.Invoke(this, EventArgs.Empty);
			return true;
		}

		public override bool RequestFocus(FocusSearchDirection direction, Rect previouslyFocusedRect)
		{
			return (this as IDescendantFocusToggler).RequestFocus(this, () => base.RequestFocus(direction, previouslyFocusedRect));
		}

		protected override void OnSelectionChanged(int selStart, int selEnd)
		{
			base.OnSelectionChanged(selStart, selEnd);
			SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(selStart, selEnd));
		}

		internal event EventHandler OnKeyboardBackPressed;
		internal event EventHandler<SelectionChangedEventArgs> SelectionChanged;
	}
}
#endif