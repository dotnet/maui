// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class BasicSwipeGallery
	{
		public BasicSwipeGallery()
		{
			InitializeComponent();
		}

		async void OnInvoked(object sender, EventArgs e)
		{
			await DisplayAlert("SwipeView", "Delete Invoked", "OK");
		}
	}
}