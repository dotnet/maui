// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class WindowsVisualElementAccessKeysPage : TabbedPage
	{
		public WindowsVisualElementAccessKeysPage()
		{
			InitializeComponent();
		}

		async void OnButtonClicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			await DisplayAlert("Button clicked", $"Clicked {button?.Text}", "OK");
		}
	}
}
