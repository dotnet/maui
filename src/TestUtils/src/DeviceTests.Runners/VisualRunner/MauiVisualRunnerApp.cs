using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.Pages;
using Application = Microsoft.Maui.Controls.Application;
using WindowsPlatform = Microsoft.Maui.Controls.PlatformConfiguration.Windows;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	class MauiVisualRunnerApp : Application
	{
		readonly TestOptions options;

		public MauiVisualRunnerApp(TestOptions options)
		{
			this.options = options;

			On<WindowsPlatform>().SetImageDirectory("Assets");
		}

		protected override Window CreateWindow(IActivationState activationState)
		{
			var hp = new HomePage();

			var nav = new Navigator(hp.Navigation);

			var runner = new DeviceRunner(options.Assemblies, nav);

			var vm = new HomeViewModel(nav, runner);

			hp.BindingContext = vm;

			var navPage = new NavigationPage(hp);

			return new Window(navPage);
		}
	}
}