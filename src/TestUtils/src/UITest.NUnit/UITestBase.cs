using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UITest.Core;

namespace UITest.Appium.NUnit
{
//#if ANDROID
//	[TestFixture(TestDevice.Android)]
//#elif IOSUITEST
//	[TestFixture(TestDevice.iOS)]
//#elif MACUITEST
//	[TestFixture(TestDevice.Mac)]
//#elif WINTEST
//	[TestFixture(TestDevice.Windows)]
//#else
//    [TestFixture(TestDevice.iOS)]
//    [TestFixture(TestDevice.Mac)]
//    [TestFixture(TestDevice.Windows)]
//    [TestFixture(TestDevice.Android)]
//#endif
    public abstract class UITestBase : UITestContextBase
    {
        public UITestBase(TestDevice testDevice) 
            : base(testDevice)
        {
        }

        [SetUp]
        public void RecordTestSetup()
        {
            var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
            TestContext.Progress.WriteLine($">>>>> {DateTime.Now} {name} Start");
        }

        [TearDown]
        public void RecordTestTeardown()
        {
            var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
            TestContext.Progress.WriteLine($">>>>> {DateTime.Now} {name} Stop");
        }

        protected virtual void FixtureSetup()
        {
            var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
            TestContext.Progress.WriteLine($">>>>> {DateTime.Now} {nameof(FixtureSetup)} for {name}");
        }

        protected virtual void FixtureTeardown()
        {
            var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
            TestContext.Progress.WriteLine($">>>>> {DateTime.Now} {nameof(FixtureTeardown)} for {name}");
        }

        [TearDown]
        public void UITestBaseTearDown()
        {
            
            if (App.AppState == ApplicationState.Not_Running)
            {
                // Assert.Fail will immediately exit the test which is desirable as the app is not
                // running anymore so we don't want to log diagnostic data as there is nothing to collect from
                Reset();
                FixtureSetup();
                Assert.Fail("The app was expected to be running still, investigate as possible crash");
            }

            var testOutcome = TestContext.CurrentContext.Result.Outcome;
            if (testOutcome == ResultState.Error ||
                testOutcome == ResultState.Failure)
            {
                SaveDiagnosticLogs("UITestBaseTearDown");
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            InitialSetup(UITestContextSetupFixture.ServerContext);
            try
            {
                //SaveDiagnosticLogs("BeforeFixtureSetup");
                FixtureSetup();
            }
            catch
            {
                SaveDiagnosticLogs("FixtureSetup");
                throw;
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            var outcome = TestContext.CurrentContext.Result.Outcome;

            // We only care about setup failures as regular test failures will already do logging
            if (outcome.Status == ResultState.SetUpFailure.Status &&
                outcome.Site == ResultState.SetUpFailure.Site)
            {
                SaveDiagnosticLogs("OneTimeTearDown");
            }

            FixtureTeardown();
        }

        void SaveDiagnosticLogs(string? note = null)
        {
            if (string.IsNullOrEmpty(note))
                note = "-";
            else
                note = $"-{note}-";

            var logDir = (Path.GetDirectoryName(Environment.GetEnvironmentVariable("APPIUM_LOG_FILE")) ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))!;

            // App could be null if UITestContext was not able to connect to the test process (e.g. port already in use etc...)
            if (UITestContext is not null)
            {
                string name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;

                _ = App.Screenshot(Path.Combine(logDir, $"{name}-{_testDevice}{note}ScreenShot"));
                File.WriteAllText(Path.Combine(logDir, $"{name}-{_testDevice}{note}PageSource.txt"), App.ElementTree);
            }

            foreach (var log in Directory.GetFiles(logDir))
            {
                TestContext.AddTestAttachment(log, Path.GetFileName(log));
            }
        }
    }
}