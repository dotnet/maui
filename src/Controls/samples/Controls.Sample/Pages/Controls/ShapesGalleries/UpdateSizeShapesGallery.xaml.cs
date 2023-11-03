using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.ShapesGalleries
{
	public partial class UpdateSizeShapesGallery : ContentPage
	{
		public UpdateSizeShapesGallery()
		{
			InitializeComponent();
		}

		void OnIncreaseButtonClicked(object sender, EventArgs e)
		{
			TestLine.Y2 = Math.Min(400.0, TestLine.Y2 + 50.0);
		}

		void OnDecreaseButtonClicked(object sender, EventArgs e)
		{
			TestLine.Y2 = Math.Max(0.0, TestLine.Y2 - 50.0);
		}
	}
}