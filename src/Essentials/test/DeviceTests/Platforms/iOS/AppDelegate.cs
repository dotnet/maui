using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Essentials.DeviceTests;

[Register(nameof(AppDelegate))]
partial class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
