using Android.Content;
using Android.OS;
using Android.Views;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// The <see cref="MauiAppCompatEditText"/> created by the Entry and Editor handlers. It postpones font changes that are
	/// requested while it handles a touch event, a long click or an accessibility action (see <see cref="TouchEventFontDeferral"/>).
	/// </summary>
	internal sealed class FontDeferringAppCompatEditText : MauiAppCompatEditText
	{
		public FontDeferringAppCompatEditText(Context context) : base(context)
		{
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
