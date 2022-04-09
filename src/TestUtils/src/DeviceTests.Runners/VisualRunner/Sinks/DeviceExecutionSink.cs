#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	class DeviceExecutionSink : TestMessageSink
	{
		readonly SynchronizationContext _context;
		readonly ITestListener _listener;
		readonly Dictionary<ITestCase, TestCaseViewModel> _testCases;

		public DeviceExecutionSink(
			Dictionary<ITestCase, TestCaseViewModel> testCases,
			ITestListener listener,
			SynchronizationContext context)
		{
			_testCases = testCases ?? throw new ArgumentNullException(nameof(testCases));
			_listener = listener ?? throw new ArgumentNullException(nameof(listener));
			_context = context ?? throw new ArgumentNullException(nameof(context));

			Execution.TestFailedEvent += HandleTestFailed;
			Execution.TestPassedEvent += HandleTestPassed;
			Execution.TestSkippedEvent += HandleTestSkipped;
		}

		void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
		{
			MakeTestResultViewModel(args.Message, TestState.Failed);
		}

		void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
		{
			MakeTestResultViewModel(args.Message, TestState.Passed);
		}

		void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
		{
			MakeTestResultViewModel(args.Message, TestState.Skipped);
		}

		async void MakeTestResultViewModel(ITestResultMessage testResult, TestState outcome)
		{
			var tcs = new TaskCompletionSource<TestResultViewModel>(TaskCreationOptions.RunContinuationsAsynchronously);

			if (!_testCases.TryGetValue(testResult.TestCase, out TestCaseViewModel? testCase))
			{
				// no matching reference, search by Unique ID as a fallback
				testCase = _testCases.FirstOrDefault(kvp => kvp.Key.UniqueID?.Equals(testResult.TestCase.UniqueID, StringComparison.Ordinal) ?? false).Value;

				if (testCase == null)
					return;
			}

			// Create the result VM on the UI thread as it updates properties
			_context.Post(_ =>
			{
				var result = new TestResultViewModel(testCase, testResult)
				{
					Duration = TimeSpan.FromSeconds((double)testResult.ExecutionTime)
				};

				if (outcome == TestState.Failed)
				{
					result.ErrorMessage = ExceptionUtility.CombineMessages((ITestFailed)testResult);
					result.ErrorStackTrace = ExceptionUtility.CombineStackTraces((ITestFailed)testResult);
				}

				tcs.TrySetResult(result);
			}, null);

			var r = await tcs.Task;

			_listener.RecordResult(r); // bring it back to the threadpool thread
		}
	}
}