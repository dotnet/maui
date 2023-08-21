// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Maui.Controls.Sample.Pages
{
	public partial class CarouselViewPage
	{
		public CarouselViewPage()
		{
			InitializeComponent();
		}

		async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
		{
			await DisplayAlert("Item", "Tapped", "Successfully");
		}
	}
}