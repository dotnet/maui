using System;
using System.Collections.Generic;
using ElmSharp;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen
{
	public interface INavigationView
	{
		EvasObject Header { get; set; }

		EColor BackgroundColor { get; set; }

		ImageSource BackgroundImageSource { get; set; }

		Aspect BackgroundImageAspect { get; set; }

		void BuildMenu(List<List<Element>> flyout);

		event EventHandler<SelectedItemChangedEventArgs> SelectedItemChanged;
	}
}
