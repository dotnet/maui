// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class AndroidEntryPage : ContentPage
	{
		public AndroidEntryPage()
		{
			InitializeComponent();
		}

		void OnSelectedIndexChanged(object sender, EventArgs e)
		{
			ImeFlags flag = (ImeFlags)Enum.Parse(typeof(ImeFlags), _picker.SelectedItem.ToString());
			_entry.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetImeOptions(flag);
			_label.Text = $"ImeOptions: {_entry.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().ImeOptions()}";
		}
	}
}
