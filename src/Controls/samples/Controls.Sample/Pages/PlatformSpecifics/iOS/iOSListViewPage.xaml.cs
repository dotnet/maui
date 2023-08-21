// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSListViewPage : ContentPage
	{
		public iOSListViewPage()
		{
			InitializeComponent();
			BindingContext = new ListViewViewModel();
		}
	}
}
