using Xunit.Abstractions;

namespace Microsoft.Maui.IntegrationTests
{
	/// <summary>
	/// A static context class to provide similar functionality to NUnit's TestContext for xUnit.
	/// This allows existing code to continue working with minimal changes.
	/// </summary>
	public static class TestContext
	{
		private static ITestOutputHelper? _outputHelper;

		public static void Configure(ITestOutputHelper outputHelper)
		{
			_outputHelper = outputHelper;
		}

		public static void WriteLine(string message)
		{
			_outputHelper?.WriteLine(message);
		}

		public static void WriteLine(string format, params object[] args)
		{
			_outputHelper?.WriteLine(format, args);
		}
	}
}