using System;
using System.Collections.Generic;
using System.Text;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : ViewHandler<IImageButton, AppCompatImageButton>
	{
		protected override AppCompatImageButton CreateNativeView()
		{
			return new AppCompatImageButton(Context);
		}

		void OnSetImageSource(Drawable? obj)
		{
			NativeView.SetImageDrawable(obj);
		}

		protected override void DisconnectHandler(AppCompatImageButton nativeView)
		{
			base.DisconnectHandler(nativeView);
			SourceLoader.Reset();
		}
	}
}
