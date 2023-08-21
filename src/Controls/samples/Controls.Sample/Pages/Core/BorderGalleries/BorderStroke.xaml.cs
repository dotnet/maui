// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class BorderStroke : ContentPage
	{
		public BorderStroke()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			ContentHeightSlider.Value = 60;
		}
	}
}