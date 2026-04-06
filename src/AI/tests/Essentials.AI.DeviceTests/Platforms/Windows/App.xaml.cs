using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Essentials.AI.DeviceTests.WinUI;

public partial class App : MauiWinUIApplication
{
	public App() => InitializeComponent();

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
