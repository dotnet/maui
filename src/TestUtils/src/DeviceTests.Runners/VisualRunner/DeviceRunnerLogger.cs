using Xunit;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	class DeviceRunnerLogger : IRunnerLogger
	{
		public object LockObject { get; } = new object();

		public void LogError(StackFrameInfo stackFrame, string message)
		{

		}

		public void LogImportantMessage(StackFrameInfo stackFrame, string message)
		{

		}

		public void LogMessage(StackFrameInfo stackFrame, string message)
		{

		}

		public void LogWarning(StackFrameInfo stackFrame, string message)
		{

		}
	}
}
