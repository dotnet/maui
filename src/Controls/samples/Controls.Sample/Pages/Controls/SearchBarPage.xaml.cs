// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class SearchBarPage
	{
		public SearchBarPage()
		{
			InitializeComponent();
		}

		void OnSearchBarFocused(object sender, FocusEventArgs e)
		{
			var text = ((SearchBar)sender).Text;
			DisplayAlert("Focused", text, "Ok");
		}

		void OnSearchBarUnfocused(object sender, FocusEventArgs e)
		{
			var text = ((SearchBar)sender).Text;
			DisplayAlert("Unfocused", text, "Ok");
		}

		void OnSearchBarTextChanged(object sender, TextChangedEventArgs args)
		{
			var text = ((SearchBar)sender).Text;
			Debug.WriteLine($"SearchBar Text changed: {text}");
		}
	}
}