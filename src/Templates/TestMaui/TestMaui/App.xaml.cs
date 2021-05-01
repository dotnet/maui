using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace TestMaui
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();
		}

		protected override IWindow CreateWindow(IActivationState activationState)
		{
			Microsoft.Maui.Controls.Compatibility.Forms.Init(activationState);

			return new Microsoft.Maui.Controls.Window(new MainPage());
		}
	}
}
