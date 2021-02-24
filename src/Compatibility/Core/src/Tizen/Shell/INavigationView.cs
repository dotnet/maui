using System;
using System.Collections.Generic;
using ElmSharp;
using EColor = ElmSharp.Color;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public interface INavigationView
	{

		EvasObject NativeView { get; }

		FlyoutHeaderBehavior HeaderBehavior { get; set; }

		View Header { get; set; }

		EColor BackgroundColor { get; set; }

		ImageSource BackgroundImageSource { get; set; }

		Aspect BackgroundImageAspect { get; set; }

		void BuildMenu(List<List<Element>> flyout);

		event EventHandler<SelectedItemChangedEventArgs> SelectedItemChanged;
	}
}
