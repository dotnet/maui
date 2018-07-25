using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using UnitTests.HeadlessRunner;
using Windows.ApplicationModel.Activation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xunit.Runners.UI;

namespace DeviceTests.UWP
{
    public sealed partial class App : RunnerApplication
    {
        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);

            if (args.Kind == ActivationKind.Protocol)
            {
                var protocolArgs = (ProtocolActivatedEventArgs)args;
                if (!string.IsNullOrEmpty(protocolArgs?.Uri?.Host))
                {
                    var parts = protocolArgs.Uri.Host.Split('_');
                    if (parts.Length >= 2 && !string.IsNullOrEmpty(parts[0]))
                    {
                        var ip = parts[0]?.Replace('-', '.');

                        if (int.TryParse(parts[1], out var port))
                        {
                            Task.Run(() =>
                            {
                                var xunitRunner = new UnitTests.HeadlessRunner.Xunit.XUnitTestInstrumentation
                                {
                                    NetworkLogEnabled = true,
                                    NetworkLogHost = ip,
                                    NetworkLogPort = port,
                                    ResultsFormat = TestResultsFormat.XunitV2,
                                    Filters = Traits.GetCommonTraits()
                                };

                                var asm = typeof(App).GetTypeInfo().Assembly;
                                var asmFilename = asm.GetName().Name + ".exe";

                                xunitRunner.Run(new TestAssemblyInfo(asm, asmFilename));
                            });
                        }
                    }
                }
            }

            var rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        protected override void OnInitializeRunner()
        {
            AddTestAssembly(typeof(App).GetTypeInfo().Assembly);
            AddTestAssembly(typeof(Accelerometer_Tests).GetTypeInfo().Assembly);
        }
    }
}
