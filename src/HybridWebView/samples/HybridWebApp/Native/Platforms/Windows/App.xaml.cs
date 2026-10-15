using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample.HybridWebApp.WinUI;

public partial class App : MauiWinUIApplication
{
	public App() => InitializeComponent();

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
