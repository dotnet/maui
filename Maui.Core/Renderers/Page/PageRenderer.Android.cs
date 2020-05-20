using System;
using System.Collections.Generic;
using System.Maui.Platform;
using System.Text;
using Android.Views;
using AView = Android.Views.View;

namespace System.Maui.Platform
{
	public partial class PageRenderer
	{
		protected override AView CreateView()
		{
			LayoutInflater inflater = LayoutInflater.FromContext(this.Context);
			var view = inflater.Inflate(Resource.Layout.content_main, null);

			if(view is ViewGroup vg && base.VirtualView.Content is IFrameworkElement fe)
			{
				vg.AddView(fe.ToNative(Context));
			}

			return view;
		}
	}
}
