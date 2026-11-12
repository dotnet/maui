using Android.Content;

namespace Microsoft.Maui.Platform
{
	// The EditText that the Entry and Editor handlers create, with a focused rect that spans its width (see SpanFocusedRectAcrossWidth). The
	// override is in internal classes so that the public API of MauiAppCompatEditText and MauiMaterialEditText doesn't change.
	internal sealed class WideFocusedRectAppCompatEditText : MauiAppCompatEditText
	{
		public WideFocusedRectAppCompatEditText(Context context) : base(context)
		{
		}

		public override void GetFocusedRect(global::Android.Graphics.Rect? r)
		{
			base.GetFocusedRect(r);

			this.SpanFocusedRectAcrossWidth(r);
		}
	}

	internal sealed class WideFocusedRectMaterialEditText : MauiMaterialEditText
	{
		public WideFocusedRectMaterialEditText(Context context) : base(context)
		{
		}

		public override void GetFocusedRect(global::Android.Graphics.Rect? r)
		{
			base.GetFocusedRect(r);

			this.SpanFocusedRectAcrossWidth(r);
		}
	}
}
