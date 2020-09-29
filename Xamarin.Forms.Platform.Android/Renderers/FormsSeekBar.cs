using System;
using Android.Content;
using Android.Views;
using Android.Widget;


namespace Xamarin.Forms.Platform.Android
{
	internal class FormsSeekBar : SeekBar
	{
		public FormsSeekBar(Context context) : base(context)
		{
			//this should work, but it doesn't.
			DuplicateParentStateEnabled = false;
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			switch (e.Action)
			{
				case MotionEventActions.Down:
					isTouching = true;
					break;
				case MotionEventActions.Up:
					Pressed = false;
					break;
			}

			return base.OnTouchEvent(e);
		}

		public override bool Pressed
		{
			get
			{
				return base.Pressed;
			}
			set
			{
				if (isTouching)
				{
					base.Pressed = value;
					isTouching = value;
				}

			}
		}

		bool isTouching = false;
	}
}