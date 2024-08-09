using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class ResizeShadowHostPage
	{
		public ResizeShadowHostPage()
		{
			InitializeComponent();
		}

		void OnResizeShadowHostClicked(object sender, EventArgs e)
		{
			var random = new Random();
			ShadowHost.HeightRequest = ShadowHost.WidthRequest = random.Next(100, 300);
		}
    }
}