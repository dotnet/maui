// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Pages
{
	public partial class BorderStyles : ContentPage
	{
		public BorderStyles()
		{
			InitializeComponent();

			ChangeCornerRadius(0);
		}

		void OnIncreaseCornerRadius(object sender, EventArgs e) => ChangeCornerRadius(10);

		void OnDecreaseCornerRadius(object sender, EventArgs e) => ChangeCornerRadius(-10);

		void ChangeCornerRadius(double delta)
		{
			RoundRectangle rr = (RoundRectangle)UpdateStrokeShapeBorder.StrokeShape;
			double radius = Math.Max(0, rr.CornerRadius.TopLeft + delta);
			rr.CornerRadius = new CornerRadius(radius);
			UpdateStrokeShapeInfo.Text = $"Border.StrokeShape is RoundRectangle with {radius} radius";
		}
	}
}