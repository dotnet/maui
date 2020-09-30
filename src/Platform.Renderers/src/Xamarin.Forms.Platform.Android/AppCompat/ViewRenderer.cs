using System;
using System.ComponentModel;
using Android.Content;

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	public abstract class ViewRenderer<TView, TControl> : Android.ViewRenderer<TView, TControl> where TView : View where TControl : global::Android.Views.View
	{
		protected ViewRenderer(Context context) : base(context)
		{
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use ViewRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected ViewRenderer()
		{
		}
	}
}