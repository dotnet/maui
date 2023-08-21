// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.ShapesGalleries
{
	public partial class ClipCornerRadiusGallery : ContentPage
	{
		public ClipCornerRadiusGallery()
		{
			InitializeComponent();
		}

		void OnCornerChanged(object sender, ValueChangedEventArgs e)
		{
			RoundRectangleGeometry.CornerRadius = new CornerRadius(
				TopLeftCorner.Value,
				TopRightCorner.Value,
				BottomLeftCorner.Value,
				BottomRightCorner.Value);
		}
	}
}