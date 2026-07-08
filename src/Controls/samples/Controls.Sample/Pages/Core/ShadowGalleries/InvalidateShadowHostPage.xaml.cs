using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class InvalidateShadowHostPage
	{
		public InvalidateShadowHostPage()
		{
			InitializeComponent();
			UpdateShadowOffset();
		}

		void OnUpdateHostSizeClicked(object sender, EventArgs args)
		{
			var random = new Random();
			ShadowHost.MinimumHeightRequest = ShadowHost.MinimumWidthRequest = random.Next(100, 300);
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
			var offset = new Point(ShadowOffsetXSlider.Value, ShadowOffsetYSlider.Value);
			ShadowHost.Shadow.Offset = offset;
		}
	}
}