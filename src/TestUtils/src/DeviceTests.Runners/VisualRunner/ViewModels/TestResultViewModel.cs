using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
    /// <summary>
    /// 
    /// </summary>
    public class TestResultViewModel : ViewModelBase
    {
        TimeSpan duration;
        string errorMessage = string.Empty;
        string errorStackTrace = string.Empty;
        TestCaseViewModel testCase;
        ITestResultMessage testResultMessage;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testCase"></param>
        /// <param name="testResult"></param>
        public TestResultViewModel(TestCaseViewModel testCase, ITestResultMessage testResult)
        {
            TestCase = testCase ?? throw new ArgumentNullException(nameof(testCase));
            TestResultMessage = testResult;

            if (testResult != null)
                testCase.UpdateTestState(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan Duration
        {
            get { return duration; }
            set
            {
                if (Set(ref duration, value))
                {
                    testCase?.UpdateTestState(this);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ErrorMessage
        {
            get { return errorMessage; }
            set { Set(ref errorMessage, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ErrorStackTrace
        {
            get { return errorStackTrace; }
            set { Set(ref errorStackTrace, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public TestCaseViewModel TestCase
        {
            get { return testCase; }
            private set { Set(ref testCase, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public ITestResultMessage TestResultMessage
        {
            get { return testResultMessage; }
            private set { Set(ref testResultMessage, value); }
        }

        public string Output => TestResultMessage?.Output ?? string.Empty;

        public bool HasOutput => !string.IsNullOrWhiteSpace(Output);
    }
}
