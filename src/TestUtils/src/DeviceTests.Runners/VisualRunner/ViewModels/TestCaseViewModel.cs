#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.DotNet.XHarness.TestRunners.Common;
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

		public string DisplayName => (_testResults.Count > 1) ? TestResult?.TestResultMessage?.Test?.DisplayName ?? TestCase.DisplayName : TestCase.DisplayName;

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

		// TestCases with strongly typed class data for some reason don't get split into different
		// test cases by the TheoryDiscoverer.
		// I've worked around it here for now so that a failed result won't get hidden when using the visual runner
		Dictionary<string, TestResultViewModel> _testResults = new Dictionary<string, TestResultViewModel>();

		internal void UpdateTestState(TestResultViewModel message)
		{
			_testResults[message.TestResultMessage!.Test.DisplayName] = message;
			TestResult = message;

			// For now we just want to surface any failing tests up to the visual runner
			foreach (var result in _testResults)
			{
				if (result.Value.TestResultMessage is ITestFailed)
				{
					TestResult = result.Value;
					break;
				}
			}

			Output = TestResult.Output;

			if (TestResult.TestResultMessage is ITestPassed)
			{
				Result = TestState.Passed;
				Message = $"✔ Success! {TestResult.Duration.TotalMilliseconds} ms";
				RunStatus = RunStatus.Ok;
			}
			else if (TestResult.TestResultMessage is ITestFailed failedMessage)
			{
				Result = TestState.Failed;
				Message = $"⛔ {ExceptionUtility.CombineMessages(failedMessage)}";
				StackTrace = ExceptionUtility.CombineStackTraces(failedMessage);
				RunStatus = RunStatus.Failed;
			}
			else if (TestResult.TestResultMessage is ITestSkipped skipped)
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