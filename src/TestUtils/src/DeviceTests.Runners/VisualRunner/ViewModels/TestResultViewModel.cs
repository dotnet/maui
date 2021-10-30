#nullable enable
using System;
using Xunit.Abstractions;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	public class TestResultViewModel : ViewModelBase
	{
		TimeSpan _duration;
		string? _errorMessage;
		string? _errorStackTrace;

		public TestResultViewModel(TestCaseViewModel testCase, ITestResultMessage? testResult)
		{
			TestCase = testCase ?? throw new ArgumentNullException(nameof(testCase));
			TestResultMessage = testResult;

			if (testResult != null)
				testCase.UpdateTestState(this);
		}

		public TestCaseViewModel TestCase { get; }

		public ITestResultMessage? TestResultMessage { get; }

		public TimeSpan Duration
		{
			get => _duration;
			set => Set(ref _duration, value, () => TestCase?.UpdateTestState(this));
		}

		public string? ErrorMessage
		{
			get => _errorMessage;
			set => Set(ref _errorMessage, value);
		}

		public string? ErrorStackTrace
		{
			get => _errorStackTrace;
			set => Set(ref _errorStackTrace, value);
		}

		public string? Output => TestResultMessage?.Output;

		public bool HasOutput => !string.IsNullOrWhiteSpace(Output);
	}
}