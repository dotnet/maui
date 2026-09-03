using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.Xaml;
using Samples.View;
using Device = Microsoft.Maui.Controls.Device;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Samples
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();
		}

		protected override Window CreateWindow(IActivationState activationState)
		{
			return new Window(new NavigationPage(new HomePage()));
		}

		public static void HandleAppActions(AppAction appAction)
		{
			App.Current.Dispatcher.Dispatch(async () =>
			{
				var page = appAction.Id switch
				{
					"battery_info" => new BatteryPage(),
					"app_info" => new AppInfoPage(),
					_ => default(Page)
				};

				if (page != null)
				{
					await Application.Current.Windows[0].Page.Navigation.PopToRootAsync();
					await Application.Current.Windows[0].Page.Navigation.PushAsync(page);
				}
			});
		}
	}
}
