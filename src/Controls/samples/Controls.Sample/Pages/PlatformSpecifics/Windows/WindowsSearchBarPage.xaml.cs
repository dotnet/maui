// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class WindowsSearchBarPage : ContentPage
	{
		public WindowsSearchBarPage()
		{
			InitializeComponent();
		}

		void OnToggleButtonClicked(object sender, EventArgs e)
		{
			_searchBar.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().SetIsSpellCheckEnabled(!_searchBar.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().GetIsSpellCheckEnabled());
		}
	}
}
