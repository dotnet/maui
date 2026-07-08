using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
namespace Maui.Controls.Sample.Pages
{
	public partial class iOSTranslucentTabbedPage : Microsoft.Maui.Controls.TabbedPage
	{
		public iOSTranslucentTabbedPage()
		{
			InitializeComponent();
		}

		void OnToggleButtonClicked(object sender, EventArgs e)
		{
			switch (On<iOS>().GetTranslucencyMode())
			{
				case TranslucencyMode.Default:
					On<iOS>().SetTranslucencyMode(TranslucencyMode.Translucent);
					break;
				case TranslucencyMode.Translucent:
					On<iOS>().SetTranslucencyMode(TranslucencyMode.Opaque);
					break;
				case TranslucencyMode.Opaque:
					On<iOS>().SetTranslucencyMode(TranslucencyMode.Default);
					break;
			}
		}

		void OnReturnButtonClicked(object sender, EventArgs e)
		{
			Navigation.PopAsync();
		}
	}
}
