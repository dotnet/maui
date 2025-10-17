#nullable enable
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.Pages;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	partial class MauiVisualRunnerApp : Application
	{
		readonly TestOptions _options;
		readonly ILogger _logger;

		public MauiVisualRunnerApp(TestOptions options, ILogger logger)
		{
			_options = options;
			_logger = logger;

			InitializeComponent();
		}

		protected override Window CreateWindow(IActivationState? activationState)
		{
			var hp = new HomePage();

			hp.Loaded += async (_, _) =>
			{
#if ANDROID
				string cacheDir = global::Android.App.Application.Context.CacheDir!.AbsolutePath;

				var builder = await Microsoft.Testing.Platform.Builder.TestApplication.CreateServerModeBuilderAsync(new[] {
					"--results-directory", cacheDir,
				});

				//builder.OutputDisplay.RegisterWriteCallback(log => {
				//	myData.Add(log);
				//});

				//buider.ServerMode.ConnectToTcpClient(clientHostName: "localhost", clientPort: 6000);

				//_ = Task.Run(() => {
				//    Thread.Sleep(5000);
				//    var tcpClient = new TcpClient();
				//    // Note: Specify the port of the test runner process here.
				//    tcpClient.Connect(new IPEndPoint(IPAddress.Loopback, 6000));
				//});

				//buider.AddTestAnywhereTestFramework(new TestTemplate.SourceGeneratedTestNodesBuilder());
#endif
			};

			var nav = new TestNavigator(hp.Navigation);

			var runner = new DeviceRunner(_options.Assemblies, nav, _logger);

			var vm = new HomeViewModel(nav, runner);

			hp.BindingContext = vm;

			var navPage = new NavigationPage(hp);

			return new Window(navPage);
		}
	}
}