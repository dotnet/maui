using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class AndroidTitleViewPage : ContentPage
	{
		public AndroidTitleViewPage()
		{
			InitializeComponent();
		}

		void OnReturnButtonClicked(object sender, EventArgs e)
		{
			Navigation.PopAsync();
		}
	}
}