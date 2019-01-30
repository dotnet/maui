using System;
using Android.Support.V7.Widget;

namespace Xamarin.Forms.Platform.Android
{
	public interface IButtonLayoutRenderer
	{
		AppCompatButton View { get; }
		Button Element { get; }
		event EventHandler<VisualElementChangedEventArgs> ElementChanged;
	}
}