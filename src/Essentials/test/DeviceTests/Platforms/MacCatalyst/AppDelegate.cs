using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Foundation;

namespace Microsoft.Maui.Essentials.DeviceTests;

[Register(nameof(AppDelegate))]
partial class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
