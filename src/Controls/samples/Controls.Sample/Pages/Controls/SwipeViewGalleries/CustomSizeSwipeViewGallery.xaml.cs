// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class CustomSizeSwipeViewGallery
	{
		public CustomSizeSwipeViewGallery()
		{
			InitializeComponent();
		}

		void OnContentClicked(object sender, EventArgs args)
		{
			DisplayAlert("OnClicked", "The Content Button has been clicked.", "Ok");
		}

		void OnRightItemsClicked(object sender, EventArgs args)
		{
			DisplayAlert("OnClicked", "The RightItems Button has been clicked.", "Ok");
		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			DisplayAlert("Custom SwipeItem", "Button Clicked!", "Ok");
		}
	}

}