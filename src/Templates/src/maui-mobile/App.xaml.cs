using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace MauiApp1
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();
		}

		public override IWindow CreateWindow(IActivationState activationState)
		{
			return new MainWindow();
		}
	}
}