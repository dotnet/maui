using Android.Text;
using Android.Text.Method;
using Android.Views;
using Android.Widget;

namespace Microsoft.Maui.Platform;

sealed class PickerScrollingMovementMethod : ScrollingMovementMethod
{
	public override bool OnKeyDown(TextView? widget, ISpannable? buffer, Keycode keyCode, KeyEvent? e) => false;

	public override bool OnKeyOther(TextView? widget, ISpannable? buffer, KeyEvent? e) => false;
}
