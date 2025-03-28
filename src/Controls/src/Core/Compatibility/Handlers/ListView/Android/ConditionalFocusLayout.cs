#nullable disable
using Android.Content;
using Android.Views;
using Android.Widget;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	internal sealed class ConditionalFocusLayout : LinearLayout, global::Android.Views.View.IOnTouchListener
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

#pragma warning disable CS0618 // Type or member is obsolete
		internal void ApplyTouchListenersToSpecialCells(Cell item)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			DescendantFocusability = DescendantFocusability.BlockDescendants;

			global::Android.Views.View aView = GetChildAt(0);
			(aView as EntryCellView)?.EditText.SetOnTouchListener(this);

#pragma warning disable CS0618 // Type or member is obsolete
			var viewCell = item as ViewCell;
#pragma warning restore CS0618 // Type or member is obsolete
			if (viewCell?.View == null)
				return;

			var renderer = viewCell.View.ToPlatform(item.Handler.MauiContext);
			GetEditText(renderer)?.SetOnTouchListener(this);

			foreach (Element descendant in viewCell.View.Descendants())
			{
				var element = descendant as VisualElement;
				if (element == null)
					continue;
				renderer = element.ToPlatform(item.Handler.MauiContext);
				GetEditText(renderer)?.SetOnTouchListener(this);
			}
		}

		internal EditText GetEditText(global::Android.Views.View v)
		{
			if (v is EditText editText)
				return editText;

			if (v is ViewGroup vg)
				return vg.GetFirstChildOfType<EditText>();

			return null;
		}
	}
}
