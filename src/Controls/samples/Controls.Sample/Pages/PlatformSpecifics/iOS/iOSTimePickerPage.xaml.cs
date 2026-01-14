using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSTimePickerPage : ContentPage
	{
		public iOSTimePickerPage()
		{
			InitializeComponent();
		}

		void OnButtonClicked(object? sender, EventArgs e)
		{
			switch (timePicker.On<iOS>().UpdateMode())
			{
				case UpdateMode.Immediately:
					timePicker.On<iOS>().SetUpdateMode(UpdateMode.WhenFinished);
					break;
				case UpdateMode.WhenFinished:
					timePicker.On<iOS>().SetUpdateMode(UpdateMode.Immediately);
					break;
			}
		}
	}
}
