using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Foundation;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.DotNet.XHarness.TestRunners.Xunit;
using UIKit;
using Xamarin.Essentials;

namespace Xamarin.Platform.Handlers.DeviceTests
{
    [Register(nameof(TestApplicationDelegate))]
    public class TestApplicationDelegate : UIApplicationDelegate
    {
        public override UIWindow Window { get; set; }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            Window = new UIWindow(UIScreen.MainScreen.Bounds)
            {
                RootViewController = new ViewController()
            };
            Window.MakeKeyAndVisible();

            return true;
        }

        class ViewController : UIViewController
        {
            public override async void ViewDidLoad()
            {
                base.ViewDidLoad();

                var entryPoint = new TestsEntryPoint();

                await entryPoint.RunAsync();
            }
        }

        class TestsEntryPoint : iOSApplicationEntryPoint
        {
            protected override bool LogExcludedTests => true;

            protected override int? MaxParallelThreads => Environment.ProcessorCount;

            protected override IDevice Device { get; } = new TestDevice();

            protected override IEnumerable<TestAssemblyInfo> GetTestAssemblies()
            {
                yield return new TestAssemblyInfo(Assembly.GetExecutingAssembly(), Assembly.GetExecutingAssembly().Location);
                yield return new TestAssemblyInfo(typeof(SliderHandlerTests).Assembly, typeof(SliderHandlerTests).Assembly.Location);
            }

            protected override void TerminateWithSuccess()
            {
                Console.WriteLine("Exiting test run with success");

                var s = new ObjCRuntime.Selector("terminateWithSuccess");
                UIApplication.SharedApplication.PerformSelector(s, UIApplication.SharedApplication, 0);
            }

            protected override TestRunner GetTestRunner(LogWriter logWriter)
            {
                var testRunner = base.GetTestRunner(logWriter);
                return testRunner;
            }
        }

        class TestDevice : IDevice
        {
            public string BundleIdentifier => AppInfo.PackageName;

            public string UniqueIdentifier => Guid.NewGuid().ToString("N");

            public string Name => DeviceInfo.Name;

            public string Model => DeviceInfo.Model;

            public string SystemName => DeviceInfo.Platform.ToString();

            public string SystemVersion => DeviceInfo.VersionString;

            public string Locale => CultureInfo.CurrentCulture.Name;
        }
    }
}
