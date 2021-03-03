using System;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public interface IButtonLayoutRenderer
	{
		AppCompatButton View { get; }
		Button Element { get; }
		event EventHandler<VisualElementChangedEventArgs> ElementChanged;
	}
}