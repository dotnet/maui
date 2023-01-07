using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class LayoutIsEnabledPage : ContentPage
	{
		bool isLayoutEnabled;
		bool isButtonEnabled;

		public LayoutIsEnabledPage()
		{
			InitializeComponent();

			BindingContext = this;
		}

		public bool IsLayoutEnabled
		{
			get => isLayoutEnabled;
			set
			{
				isLayoutEnabled = value;
				OnPropertyChanged();
			}
		}

		public bool IsButtonEnabled
		{
			get => isButtonEnabled;
			set
			{
				isButtonEnabled = value;
				OnPropertyChanged();
			}
		}

		void OnDisableLayoutBtnClicked(object sender, EventArgs e)
		{
			MainLayout.IsEnabled = !MainLayout.IsEnabled;

			((Button)sender).Text = MainLayout.IsEnabled ? "Disable Layout" : "Enable Layout";
		}

		void OnDisableButtonBtnClicked(object sender, EventArgs e)
		{
			DisabledButton.IsEnabled = !DisabledButton.IsEnabled;

			DisabledButton.Text = DisabledButton.IsEnabled ? "Enabled" : "Disabled";

			((Button)sender).Text = DisabledButton.IsEnabled ? "Disable Button" : "Enable Button";
		}
	}
}