using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using UnitTests.HeadlessRunner;

namespace DeviceTests.iOS
{
    [Register(nameof(AppDelegate))]
    public partial class AppDelegate : Xunit.Runner.RunnerAppDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            // Invoke the headless test runner if a config was specified
            var testCfg = System.IO.File.ReadAllText("tests.cfg")?.Split(':');
            if (testCfg != null && testCfg.Length > 1)
            {
                var ip = testCfg[0];
                if (int.TryParse(testCfg[1], out var port))
                {
                    // Run the headless test runner for CI
                    Task.Run(() =>
                    {
                        return Tests.RunAsync(new TestOptions
                        {
                            Assemblies = new List<Assembly> { typeof(Battery_Tests).Assembly },
                            NetworkLogHost = ip,
                            NetworkLogPort = port,
                            Filters = Traits.GetCommonTraits(),
                            Format = TestResultsFormat.XunitV2
                        });
                    });
                }
            }

            // We need this to ensure the execution assembly is part of the app bundle
            AddExecutionAssembly(typeof(Battery_Tests).Assembly);

            // tests can be inside the main assembly
            AddTestAssembly(Assembly.GetExecutingAssembly());

            AddTestAssembly(typeof(Battery_Tests).Assembly);

            // otherwise you need to ensure that the test assemblies will
            // become part of the app bundle
            //    AddTestAssembly(typeof(PortableTests).Assembly);

#if false
            // you can use the default or set your own custom writer (e.g. save to web site and tweet it ;-)
            Writer = new TcpTextWriter("10.0.1.2", 16384);
            // start running the test suites as soon as the application is loaded
            AutoStart = true;
            // crash the application (to ensure it's ended) and return to springboard
            TerminateAfterExecution = true;
#endif
            return base.FinishedLaunching(app, options);
        }
    }
}
