using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.WinUI
{
	public partial class App : MauiWinUIApplication
	{
		public App()
		{
			WindowsTestDiagnostics.Initialize();
			WindowsTestDiagnostics.WriteStage("App.InitializeComponent.start");
			InitializeComponent();
			WindowsTestDiagnostics.WriteStage("App.InitializeComponent.complete");
			UnhandledException += (_, args) =>
				WindowsTestDiagnostics.WriteStage($"WinUI.unhandled-exception exception={args.Exception}");
		}

		protected override MauiApp CreateMauiApp()
		{
			WindowsTestDiagnostics.WriteStage("MauiProgram.CreateMauiApp.start");
			var app = MauiProgram.CreateMauiApp();
			WindowsTestDiagnostics.WriteStage("MauiProgram.CreateMauiApp.complete");
			return app;
		}

		protected override void OnLaunched(global::Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
		{
			// The full-suite activation supplies only the quoted result path; no caller environment is required.
			WindowsTestDiagnostics.Initialize(args.Arguments?.Trim().Trim('"'));
			WindowsTestDiagnostics.WriteStage("App.OnLaunched.start");
			base.OnLaunched(args);
			WindowsTestDiagnostics.WriteStage("App.OnLaunched.complete");
		}
	}
}
