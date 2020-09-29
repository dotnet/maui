using Android.Content;
using Android.Views;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
	internal class ConditionalFocusLayout : LinearLayout, global::Android.Views.View.IOnTouchListener
	{
		public ConditionalFocusLayout(System.IntPtr p, global::Android.Runtime.JniHandleOwnership o) : base(p, o)
		{
			// Added default constructor to prevent crash when accessing selected row in ListViewAdapter.Dispose
		}

		public ConditionalFocusLayout(Context context) : base(context)
		{
			SetOnTouchListener(this);
		}

		public bool OnTouch(global::Android.Views.View v, MotionEvent e)
		{
			bool allowFocus = v is EditText;
			DescendantFocusability = allowFocus ? DescendantFocusability.AfterDescendants : DescendantFocusability.BlockDescendants;
			return false;
		}

		internal void ApplyTouchListenersToSpecialCells(Cell item)
		{
			DescendantFocusability = DescendantFocusability.BlockDescendants;

			global::Android.Views.View aView = GetChildAt(0);
			(aView as EntryCellView)?.EditText.SetOnTouchListener(this);

			var viewCell = item as ViewCell;
			if (viewCell?.View == null)
				return;

			IVisualElementRenderer renderer = Platform.GetRenderer(viewCell.View);
			GetEditText(renderer)?.SetOnTouchListener(this);

			foreach (Element descendant in viewCell.View.Descendants())
			{
				var element = descendant as VisualElement;
				if (element == null)
					continue;
				renderer = Platform.GetRenderer(element);
				GetEditText(renderer)?.SetOnTouchListener(this);
			}
		}

		internal EditText GetEditText(IVisualElementRenderer renderer)
		{
			var viewGroup = renderer?.View as ViewGroup;
			return viewGroup?.GetChildAt(0) as EditText;
		}
	}
}