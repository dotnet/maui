using System;
using AndroidX.AppCompat.Widget;

namespace Xamarin.Forms.Platform.Android
{
	public interface IButtonLayoutRenderer
	{
		AppCompatButton View { get; }
		Button Element { get; }
		event EventHandler<VisualElementChangedEventArgs> ElementChanged;
	}
}