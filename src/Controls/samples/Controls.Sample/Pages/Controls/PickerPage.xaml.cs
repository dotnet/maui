using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class PickerPage
	{
		public PickerPage()
		{
			InitializeComponent();
		}

		void OnSelectedIndexChanged(object sender, EventArgs e)
		{
			string selectedCountry = (string)((Picker)sender).SelectedItem;
			DisplayAlert("SelectedIndexChanged", selectedCountry, "Ok");
		}
	}
}