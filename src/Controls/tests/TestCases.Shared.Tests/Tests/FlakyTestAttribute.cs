using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace Microsoft.Maui.TestCases.Tests
{
	/// <summary>
	/// Custom NUnit attribute to mark a test as flaky, allowing retries (by default 2).
	/// If after the retries the test fails, can ignore it.
	/// Note: This attribute should be used temporarily until the test is changed.
	/// </summary>  
	/// <example>
	/// <code>
	/// [FlakyTest("Description with details of the test that sometimes fails.", retryCount: 2, ignore: true)]
	/// </code>
	/// </example>
	internal class FlakyTestAttribute : Attribute, IWrapTestMethod, IWrapSetUpTearDown
	{
		readonly string _ignoreMessage;
		readonly int _retryCount;
		readonly bool _ignore;

		public FlakyTestAttribute(string message, int retryCount = 2, bool ignore = true)
		{
			_ignoreMessage = message;
			_retryCount = retryCount;
			_ignore = ignore;
		}

		public ActionTargets Targets => ActionTargets.Suite | ActionTargets.Test;

		public TestCommand Wrap(TestCommand command)
		{
			return new CustomRetryCommand(command, _ignoreMessage, _retryCount, _ignore);
		}

		public class CustomRetryCommand : DelegatingTestCommand
		{
			readonly string _ignoreMessage;
			readonly int _retryCount;
			readonly bool _ignore;

			int _failedAttempts = 0;

			public CustomRetryCommand(TestCommand innerCommand, string ignoreMessage, int retryCount, bool ignore)
				: base(innerCommand)
			{
				_ignoreMessage = ignoreMessage;
				_retryCount = retryCount;
				_ignore = ignore;
			}

			public override TestResult Execute(TestExecutionContext context)
			{
				int count = _retryCount;
				while (count-- > 0)
				{
					context.CurrentResult = innerCommand.Execute(context);
					var results = context.CurrentResult.ResultState;

					if (results.Equals(ResultState.Error)
						|| results.Equals(ResultState.Failure)
						|| results.Equals(ResultState.SetUpError)
						|| results.Equals(ResultState.SetUpFailure)
						|| results.Equals(ResultState.TearDownError)
						|| results.Equals(ResultState.ChildFailure)
						|| results.Equals(ResultState.Cancelled))
					{
						_failedAttempts++;
						TestExecutionContext.CurrentContext.OutWriter.WriteLine("Test Failed on attempt #" + _failedAttempts);
					}
					else
					{
						TestExecutionContext.CurrentContext.OutWriter.WriteLine("Test Passed on attempt #" + (_failedAttempts + 1));
						break;
					}
				}

				// If want to ignore and all retry attempts fail, ignore the test with the provided message.
				if (_ignore && _failedAttempts == _retryCount)
				{
					context.CurrentResult.SetResult(ResultState.Ignored, _ignoreMessage);
				}

				return context.CurrentResult;
			}
		}
	}
}