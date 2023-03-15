using System;

namespace Maui.Controls.Sample.Pages
{
	public partial class InvalidateShadowHostPage
	{
		public InvalidateShadowHostPage()
		{
			InitializeComponent();
		}

		void OnUpdateHostSizeClicked(object sender, EventArgs args)
		{
			var random = new Random();
			ShadowHost.MinimumHeightRequest = ShadowHost.MinimumWidthRequest = random.Next(100, 300);
		}
	}
}