using System;
using System.Linq;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class PickerPage
	{
		public PickerPage()
		{
			InitializeComponent();
			this.BindingContext = this;
		}

		void OnSelectedIndexChanged(object sender, EventArgs e)
		{
			string selectedCountry = (string)((Picker)sender).SelectedItem;
			DisplayAlert("SelectedIndexChanged", selectedCountry, "Ok");
		}

		public string[] PickerItems { get; } =
		{
			"Item 1",
			"Item 2",
			"Item 3"
		};

		public string[] MorePickerItems { get; } = Enumerable.Range(1, 20).Select(i => $"Item {i}").ToArray();
	}
}