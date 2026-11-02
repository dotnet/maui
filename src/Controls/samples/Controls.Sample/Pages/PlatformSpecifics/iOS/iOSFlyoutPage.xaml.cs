using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSFlyoutPage : Microsoft.Maui.Controls.FlyoutPage
	{
		ICommand returnToPlatformSpecificsPage;

		public iOSFlyoutPage(ICommand restore)
		{
			InitializeComponent();
			returnToPlatformSpecificsPage = restore;
		}

		void OnShadowButtonClicked(object? sender, EventArgs e)
		{
			On<iOS>().SetApplyShadow(!On<iOS>().GetApplyShadow());
		}

		void OnReturnButtonClicked(object? sender, EventArgs e)
		{
			returnToPlatformSpecificsPage.Execute(null);
		}
	}
}