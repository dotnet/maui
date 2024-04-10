using CollectionViewPerformanceMaui.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace CollectionViewPerformanceMaui
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			MainPage = new AppShell();
		}

		//protected override Window CreateWindow(IActivationState? activationState)
		//{
		//	return new Window(activationState!.Context.Services.GetRequiredService<DataView>());
		//}
	}
}
