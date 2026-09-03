using System;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public interface IButtonLayoutRenderer
	{
		AppCompatButton View { get; }
		Button Element { get; }
		event EventHandler<VisualElementChangedEventArgs> ElementChanged;
	}
}