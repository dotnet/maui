using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSLargeTitlePage : ContentPage
	{
		public iOSLargeTitlePage()
		{
			InitializeComponent();
		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			switch (On<iOS>().LargeTitleDisplay())
			{
				case LargeTitleDisplayMode.Always:
					On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Automatic);
					break;
				case LargeTitleDisplayMode.Automatic:
					On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Never);
					break;
				case LargeTitleDisplayMode.Never:
					On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Always);
					break;
			}
		}

		void OnReturnButtonClicked(object sender, EventArgs e)
		{
			Navigation.PopAsync();
		}
	}
}
