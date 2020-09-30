using System;
using System.ComponentModel;

namespace Xamarin.Forms.Platform.UWP
{
	public interface IVisualNativeElementRenderer : IVisualElementRenderer
	{
		event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;
		event EventHandler ControlChanging;
		event EventHandler ControlChanged;
	}
}
