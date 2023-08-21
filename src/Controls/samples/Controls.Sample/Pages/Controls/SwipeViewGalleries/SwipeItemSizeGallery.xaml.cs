// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	public partial class SwipeItemSizeGallery
	{
		public SwipeItemSizeGallery()
		{
			InitializeComponent();
		}

		void OnSwipeItemInvoked(object sender, EventArgs e)
		{
			DisplayAlert("SwipeItemSizeGallery", "Delete SwipeItem Invoked", "Ok");
		}
	}
}