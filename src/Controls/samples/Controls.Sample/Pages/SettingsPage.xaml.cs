using System;
using Maui.Controls.Sample.Pages.Base;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class SettingsPage : BasePage
	{
		public SettingsPage()
		{
			InitializeComponent();
		}

		void OnTapGestureRecognizerTapped(object sender, EventArgs args)
		{
			Navigation.PopModalAsync();
		}

		void OnRTLToggled(object sender, ToggledEventArgs e)
		{
			var mainPage = Application.Current!.MainPage;

			if (mainPage == null)
				return;

			if (mainPage.FlowDirection != Microsoft.Maui.FlowDirection.RightToLeft)
				mainPage.FlowDirection = FlowDirection = Microsoft.Maui.FlowDirection.RightToLeft;
			else
				mainPage.FlowDirection = FlowDirection = Microsoft.Maui.FlowDirection.LeftToRight;
		}
	}
}