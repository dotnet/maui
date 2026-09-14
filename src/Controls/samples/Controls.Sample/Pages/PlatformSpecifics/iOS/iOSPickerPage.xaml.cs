using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSPickerPage : ContentPage
	{
		public iOSPickerPage()
		{
			InitializeComponent();
		}

		void OnButtonClicked(object? sender, EventArgs e)
		{
			switch (picker.On<iOS>().UpdateMode())
			{
				case UpdateMode.Immediately:
					picker.On<iOS>().SetUpdateMode(UpdateMode.WhenFinished);
					break;
				case UpdateMode.WhenFinished:
					picker.On<iOS>().SetUpdateMode(UpdateMode.Immediately);
					break;
			}
		}
	}
}
