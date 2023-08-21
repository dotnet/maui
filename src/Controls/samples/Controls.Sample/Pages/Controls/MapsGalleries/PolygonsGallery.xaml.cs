// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.MapsGalleries
{
	public partial class PolygonsGallery : ContentPage
	{
		public PolygonsGallery()
		{
			InitializeComponent();
		}

		void OnClearMapElementsClicked(object sender, EventArgs args)
		{
			MapElementsMap.MapElements.Clear();
		}
	}
}