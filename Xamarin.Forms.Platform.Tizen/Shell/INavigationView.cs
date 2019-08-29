using System;
using System.Collections;
using System.Collections.Generic;
using ElmSharp;
using EColor = ElmSharp.Color;
using Xamarin.Forms;

namespace Xamarin.Forms.Platform.Tizen
{
	public interface INavigationView
	{
		EvasObject Header { get; set; }

		EColor BackgroundColor { get; set; }

		void BuildMenu(List<List<Element>> flyout);

		event EventHandler<SelectedItemChangedEventArgs> SelectedItemChanged;
	}
}
