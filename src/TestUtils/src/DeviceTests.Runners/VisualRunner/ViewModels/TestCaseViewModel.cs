#nullable enable
using System;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	public class TestCaseViewModel : ViewModelBase
	{
		string? _message;
		string? _output;
		TestState _result;
		RunStatus _runStatus;
		string? _stackTrace;

		TestResultViewModel _testResult;

		internal TestCaseViewModel(string assemblyFileName, ITestCase testCase)
		{
			AssemblyFileName = assemblyFileName ?? throw new ArgumentNullException(nameof(assemblyFileName));
			TestCase = testCase ?? throw new ArgumentNullException(nameof(testCase));

			Result = TestState.NotRun;
			RunStatus = RunStatus.NotRun;
			Message = "🔷 not run";

			// Create an initial result representing not run
			_testResult = new TestResultViewModel(this, null);
		}

		public string AssemblyFileName { get; }

		public string DisplayName => TestCase.DisplayName;

		public string? Message
		{
			get => _message;
			private set => Set(ref _message, value);
		}

		public string? Output
		{
			get => _output;
			private set => Set(ref _output, value);
		}

		public TestState Result
		{
			get => _result;
			private set => Set(ref _result, value);
		}

		public RunStatus RunStatus
		{
			get => _runStatus;
			set => Set(ref _runStatus, value);
		}

		public string? StackTrace
		{
			get => _stackTrace;
			private set => Set(ref _stackTrace, value);
		}

		public ITestCase TestCase { get; }

		public TestResultViewModel TestResult
		{
			get => _testResult;
			private set => Set(ref _testResult, value);
		}

		internal void UpdateTestState(TestResultViewModel message)
		{
			TestResult = message;

			Output = message.TestResultMessage?.Output ?? string.Empty;

			if (message.TestResultMessage is ITestPassed)
			{
				Result = TestState.Passed;
				Message = $"✔ Success! {TestResult.Duration.TotalMilliseconds} ms";
				RunStatus = RunStatus.Ok;
			}
			else if (message.TestResultMessage is ITestFailed failedMessage)
			{
				Result = TestState.Failed;
				Message = $"⛔ {ExceptionUtility.CombineMessages(failedMessage)}";
				StackTrace = ExceptionUtility.CombineStackTraces(failedMessage);
				RunStatus = RunStatus.Failed;
			}
			else if (message.TestResultMessage is ITestSkipped skipped)
			{
				Result = TestState.Skipped;
				Message = $"⚠ {skipped.Reason}";
				RunStatus = RunStatus.Skipped;
			}
			else
			{
				Message = string.Empty;
				StackTrace = string.Empty;
				RunStatus = RunStatus.NotRun;
			}
		}
	}
}