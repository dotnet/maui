using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
    public class TestCaseViewModel : ViewModelBase
    {
        readonly INavigation navigation;
        readonly ITestRunner runner;
        string message;
        string output;

        TestState result;
        RunStatus runStatus;
        string stackTrace;

        ITestCase testCase;
        TestResultViewModel testResult;


        internal TestCaseViewModel(string assemblyFileName, ITestCase testCase, INavigation navigation, ITestRunner runner)
        {
            this.navigation = navigation;
            this.runner = runner;

            AssemblyFileName = assemblyFileName ?? throw new ArgumentNullException(nameof(assemblyFileName));
            TestCase = testCase ?? throw new ArgumentNullException(nameof(testCase));


            Result = TestState.NotRun;
            RunStatus = RunStatus.NotRun;
            Message = "🔷 not run";

            // Create an initial result representing not run
            TestResult = new TestResultViewModel(this, null);

            NavigateToResultCommand = new DelegateCommand(NavigateToResultsPage);
        }


        public string AssemblyFileName { get; }
        public string DisplayName => TestCase.DisplayName;

        // This should be raised on a UI thread as listeners will likely be
        // UI elements


        public string Message
        {
            get { return message; }
            private set { Set(ref message, value); }
        }

        public ICommand NavigateToResultCommand { get; private set; }

        public string Output
        {
            get { return output; }
            private set { Set(ref output, value); }
        }

        public TestState Result
        {
            get { return result; }
            private set { Set(ref result, value); }
        }

        public RunStatus RunStatus
        {
            get { return runStatus; }
            set { Set(ref runStatus, value); }
        }

        public string StackTrace
        {
            get { return stackTrace; }
            private set { Set(ref stackTrace, value); }
        }

        public ITestCase TestCase
        {
            get { return testCase; }
            private set
            {
                if (Set(ref testCase, value))
                {
                    RaisePropertyChanged("DisplayName");
                }
            }
        }

        public TestResultViewModel TestResult
        {
            get { return testResult; }
            private set { Set(ref testResult, value); }
        }


        internal void UpdateTestState(TestResultViewModel message)
        {
            TestResult = message;

            Output = message.TestResultMessage.Output ?? string.Empty;

            var msg = string.Empty;
            var stackTrace = string.Empty;
            var rs = RunStatus.NotRun;

            if (message.TestResultMessage is ITestPassed)
            {
                Result = TestState.Passed;
                msg = $"✔ Success! {TestResult.Duration.TotalMilliseconds} ms";
                rs = RunStatus.Ok;
            }
            if (message.TestResultMessage is ITestFailed)
            {
                Result = TestState.Failed;
                var failedMessage = (ITestFailed)(message.TestResultMessage);
                msg = $"⛔ {ExceptionUtility.CombineMessages(failedMessage)}";
                stackTrace = ExceptionUtility.CombineStackTraces(failedMessage);
                rs = RunStatus.Failed;
            }
            if (message.TestResultMessage is ITestSkipped)
            {
                Result = TestState.Skipped;

                var skipped = (ITestSkipped)(message.TestResultMessage);
                msg = $"⚠ {skipped.Reason}";
                rs = RunStatus.Skipped;
            }

            Message = msg;
            StackTrace = stackTrace;
            RunStatus = rs;
        }

        async void NavigateToResultsPage()
        {
            // run again
            await runner.Run(this);

            await navigation.NavigateTo(PageType.TestResult, TestResult);
        }
    }
}
