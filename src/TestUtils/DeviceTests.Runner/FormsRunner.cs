using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Xunit.Runners.Pages;
using Application = Microsoft.Maui.Controls.Application;
using WindowsPlatform = Microsoft.Maui.Controls.PlatformConfiguration.Windows;

namespace Xunit.Runners
{
	class FormsRunner : Application
	{
		readonly TestRunnerOptions options;

		public FormsRunner(TestRunnerOptions options)
		{
			this.options = options;

			On<WindowsPlatform>().SetImageDirectory("Assets");
		}

		protected override IWindow CreateWindow(IActivationState activationState)
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