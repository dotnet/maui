using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public interface IVisualNativeElementRenderer : IVisualElementRenderer
	{
		event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;
		event EventHandler ControlChanging;
		event EventHandler ControlChanged;
	}
}
