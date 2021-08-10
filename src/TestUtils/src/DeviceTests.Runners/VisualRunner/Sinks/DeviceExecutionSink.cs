using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.Sinks
{
	class DeviceExecutionSink : TestMessageSink
	{
		readonly SynchronizationContext context;
		readonly ITestListener listener;
		readonly Dictionary<ITestCase, TestCaseViewModel> testCases;

		public DeviceExecutionSink(Dictionary<ITestCase, TestCaseViewModel> testCases,
								   ITestListener listener,
								   SynchronizationContext context
		)
		{
			this.testCases = testCases ?? throw new ArgumentNullException(nameof(testCases));
			this.listener = listener ?? throw new ArgumentNullException(nameof(listener));
			this.context = context ?? throw new ArgumentNullException(nameof(context));

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

			if (!testCases.TryGetValue(testResult.TestCase, out TestCaseViewModel testCase))
			{
				// no matching reference, search by Unique ID as a fallback
				testCase = testCases.FirstOrDefault(kvp => kvp.Key.UniqueID?.Equals(testResult.TestCase.UniqueID) ?? false).Value;
				if (testCase == null)
					return;
			}

			// Create the result VM on the UI thread as it updates properties
			context.Post(_ =>
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

			listener.RecordResult(r); // bring it back to the threadpool thread
		}

	}
}
