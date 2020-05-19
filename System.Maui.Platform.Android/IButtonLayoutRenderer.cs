using System;
#if __ANDROID_29__
using AndroidX.AppCompat.Widget;
#else
using Android.Support.V7.Widget;
#endif

namespace System.Maui.Platform.Android
{
	public interface IButtonLayoutRenderer
	{
		AppCompatButton View { get; }		
		Button Element { get; }
		event EventHandler<VisualElementChangedEventArgs> ElementChanged;
	}
}