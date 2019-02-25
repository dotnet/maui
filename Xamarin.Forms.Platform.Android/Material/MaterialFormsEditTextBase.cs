#if __ANDROID_28__
using System;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Support.Design.Widget;
using Android.Runtime;
using Android.Util;

namespace Xamarin.Forms.Platform.Android.Material
{
	public class MaterialFormsEditTextBase : TextInputEditText, IDescendantFocusToggler
	{
		DescendantFocusToggler _descendantFocusToggler;
		public MaterialFormsEditTextBase(Context context) : base(context)
		{
			MaterialFormsEditTextManager.Init(this);
		}

		protected MaterialFormsEditTextBase(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			MaterialFormsEditTextManager.Init(this);
		}

		public MaterialFormsEditTextBase(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			MaterialFormsEditTextManager.Init(this);
		}

		public MaterialFormsEditTextBase(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			MaterialFormsEditTextManager.Init(this);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				MaterialFormsEditTextManager.Dispose(this);

			base.Dispose(disposing);
		}

		bool IDescendantFocusToggler.RequestFocus(global::Android.Views.View control, Func<bool> baseRequestFocus)
		{
			_descendantFocusToggler = _descendantFocusToggler ?? new DescendantFocusToggler();
			return _descendantFocusToggler.RequestFocus(this, baseRequestFocus);
		}

		public override bool RequestFocus(FocusSearchDirection direction, Rect previouslyFocusedRect)
		{
			return (this as IDescendantFocusToggler).RequestFocus(this, () => base.RequestFocus(direction, previouslyFocusedRect));
		}
	}
}
#endif