using System;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Devices;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 18161, "Toggling FlyoutLayoutBehavior on Android causes the app to crash", PlatformAffected.Android)]
	public partial class Issue18161 : FlyoutPage, IFlyoutPageController, IFlyoutView
	{
		public Issue18161()
		{
			InitializeComponent();
			this.Loaded += Issue18161_Loaded;
		}

		async void Issue18161_Loaded(object sender, EventArgs e)
		{
			// https://github.com/dotnet/maui/issues/13496
			await Task.Yield();
			this.IsPresented = true;
		}

		public void ToggleBehaviour_Clicked(object sender, EventArgs e)
		{
			FlyoutLayoutBehavior = FlyoutLayoutBehavior == FlyoutLayoutBehavior.Split 
				? FlyoutLayoutBehavior.Popover 
				: FlyoutLayoutBehavior.Split;
		}

		bool IFlyoutPageController.ShouldShowSplitMode => FlyoutLayoutBehavior == FlyoutLayoutBehavior.Split;

		double IFlyoutView.FlyoutWidth => 100;
	}
}