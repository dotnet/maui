using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class ShadowPage
	{
		public ShadowPage()
		{
			InitializeComponent();
			UpdateShadowOffset();
		}

		void RemoveShadowClicked(object sender, EventArgs e)
		{
			ClippedShadowView.Shadow = ShadowView.Shadow = LabelShadowView.Shadow = null;
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
			ShadowView.Shadow.Offset = new Size((float)ShadowOffsetXSlider.Value, (float)ShadowOffsetYSlider.Value);
			ClippedShadowView.Shadow.Offset = new Size((float)ShadowOffsetXSlider.Value, (float)ShadowOffsetYSlider.Value);
			LabelShadowView.Shadow.Offset = new Size((float)ShadowOffsetXSlider.Value, (float)ShadowOffsetYSlider.Value);
		}
	}
}