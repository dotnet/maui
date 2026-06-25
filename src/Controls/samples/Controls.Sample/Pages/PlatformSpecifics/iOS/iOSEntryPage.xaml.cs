using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSEntryPage : ContentPage
	{
		public iOSEntryPage()
		{
			InitializeComponent();
		}

		void OnButtonClicked(object? sender, EventArgs e)
		{
			entry.On<iOS>().SetAdjustsFontSizeToFitWidth(!entry.On<iOS>().AdjustsFontSizeToFitWidth());
		}
	}
}
