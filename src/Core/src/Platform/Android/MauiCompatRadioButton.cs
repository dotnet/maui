using Android.Content;
using Android.Views;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Platform
{
	public class MauiCompatRadioButton : AppCompatRadioButton
	{
		IView? _radioButton;

		public MauiCompatRadioButton(Context context) : base(context)
		{
		}

		public void SetVirtualView(IView radioButton)
		{
			_radioButton = radioButton;
		}

		public override bool OnTouchEvent(MotionEvent? e)
		{
			bool inputTransparent = _radioButton != null && _radioButton.InputTransparent;

			if (!Enabled || (inputTransparent && Enabled))
				return false;

			return base.OnTouchEvent(e);
		}
	}
}