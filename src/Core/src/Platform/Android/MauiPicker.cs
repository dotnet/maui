using Android.Content;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Graphics.Drawable;
using ARect = Android.Graphics.Rect;

namespace Microsoft.Maui.Platform
{
	public class MauiPicker : MauiPickerBase
	{

		public MauiPicker(Context context) : base(context)
		{
			PickerManager.Init(this);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				PickerManager.Dispose(this);

			base.Dispose(disposing);
		}
	}

	public class MauiPickerBase : AppCompatEditText
	{
		public MauiPickerBase(Context context) : base(context)
		{
			if (Background != null)
				DrawableCompat.Wrap(Background);
		}
	}
}