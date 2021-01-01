using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	sealed class FlyoutPageRendererNonAppCompat : PageRenderer
	{
		public FlyoutPageRendererNonAppCompat(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
		{
			throw new Exception("FlyoutPage only works with Theme.AppCompat theme (or descendant)");
		}
	}
}
