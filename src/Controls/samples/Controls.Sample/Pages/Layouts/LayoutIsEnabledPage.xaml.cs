using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class LayoutIsEnabledPage : ContentPage
	{
		public LayoutIsEnabledPage()
		{
			InitializeComponent();
		}

		void OnDisableLayoutBtnClicked(object sender, EventArgs e)
		{
			if(MainLayout.IsEnabled)
			{
				MainLayout.IsEnabled = false;
				DisableLayoutBtn.Text = "Enable Layout";
			}
			else
			{
				MainLayout.IsEnabled = true;
				DisableLayoutBtn.Text = "Disable Layout";
			}
		}

		void OnDisableButtonBtnClicked(object sender, EventArgs e)
		{
			if (DisabledButton.IsEnabled)
			{
				DisabledButton.IsEnabled = false;
				DisabledButton.Text = "Disabled";
				DisableButtonBtn.Text = "Enable Button";
			}
			else
			{
				DisabledButton.IsEnabled = true;
				DisabledButton.Text = "Enabled";
				DisableButtonBtn.Text = "Disable Button";
			}
		}
	}
}