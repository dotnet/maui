using System;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Platform
{
	public class MauiAppCompatEditText : AppCompatEditText
	{
		public event EventHandler? SelectionChanged;

		public MauiAppCompatEditText(Context context) : base(context)
		{
		}

		protected override void OnSelectionChanged(int selStart, int selEnd)
		{
			base.OnSelectionChanged(selStart, selEnd);

			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}

		internal TouchEventFontDeferral TouchEventFontDeferral { get; } = new();

		public override bool OnTouchEvent(MotionEvent? e)
		{
			TouchEventFontDeferral.OnHandlingStarted();

			try
			{
				return base.OnTouchEvent(e);
			}
			finally
			{
				TouchEventFontDeferral.OnHandlingFinished(this);
			}
		}

		public override bool PerformLongClick()
		{
			TouchEventFontDeferral.OnHandlingStarted();

			try
			{
				return base.PerformLongClick();
			}
			finally
			{
				TouchEventFontDeferral.OnHandlingFinished(this);
			}
		}

		public override bool PerformAccessibilityAction(global::Android.Views.Accessibility.Action action, Bundle? arguments)
		{
			TouchEventFontDeferral.OnHandlingStarted();

			try
			{
				return base.PerformAccessibilityAction(action, arguments);
			}
			finally
			{
				TouchEventFontDeferral.OnHandlingFinished(this);
			}
		}
	}
}
