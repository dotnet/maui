using System;
#if __ANDROID_29__
using AndroidX.AppCompat.Widget;
#else
using Android.Support.V7.Widget;
#endif

namespace Xamarin.Forms.Platform.Android
{
	public interface IButtonLayoutRenderer
	{
		AppCompatButton View { get; }		
		Button Element { get; }
		event EventHandler<VisualElementChangedEventArgs> ElementChanged;
	}
}
