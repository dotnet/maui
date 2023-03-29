using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class ShadowPlaygroundPage
	{
		public ShadowPlaygroundPage()
		{
			InitializeComponent();
			UpdateShadowOffset();
		}

		void RemoveShadowClicked(object sender, EventArgs e)
		{
			ClippedShadowView.Shadow = ShadowView.Shadow = ShadowViewGradient.Shadow = LabelShadowView.Shadow = null;
		}

		void OnShadowOffsetXChanged(object sender, ValueChangedEventArgs e)
		{
			UpdateShadowOffset();
		}

		void OnShadowOffsetYChanged(object sender, ValueChangedEventArgs e)
		{
			UpdateShadowOffset();
		}

		void UpdateShadowOffset()
		{
			if (ShadowViewGradient.Shadow == null)
				return;

			var offset = new Point(ShadowOffsetXSlider.Value, ShadowOffsetYSlider.Value);
			ShadowViewGradient.Shadow.Offset = offset;
			ShadowView.Shadow.Offset = offset;
			ClippedShadowView.Shadow.Offset = offset;
			LabelShadowView.Shadow.Offset = offset;
		}
	}
}