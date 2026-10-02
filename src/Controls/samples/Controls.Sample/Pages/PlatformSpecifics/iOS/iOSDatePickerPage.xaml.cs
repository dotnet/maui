using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSDatePickerPage : ContentPage
	{
		public iOSDatePickerPage()
		{
			InitializeComponent();
		}

		void OnButtonClicked(object? sender, EventArgs e)
		{
			switch (datePicker.On<iOS>().UpdateMode())
			{
				case UpdateMode.Immediately:
					datePicker.On<iOS>().SetUpdateMode(UpdateMode.WhenFinished);
					break;
				case UpdateMode.WhenFinished:
					datePicker.On<iOS>().SetUpdateMode(UpdateMode.Immediately);
					break;
			}
		}
	}
}

