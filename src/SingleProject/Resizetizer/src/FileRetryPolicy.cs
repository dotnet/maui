using Microsoft.Build.Framework;

namespace Microsoft.Maui.Resizetizer
{
	/// <summary>
	/// How often, and how long apart, to retry a file operation while another process holds the file.
	/// </summary>
	internal sealed class FileRetryPolicy
	{
		internal const string AttemptsVariable = "DOTNET_ANDROID_FILE_WRITE_RETRY_ATTEMPTS";
		internal const string DelayVariable = "DOTNET_ANDROID_FILE_WRITE_RETRY_DELAY_MS";

		const int DefaultAttempts = 10;
		const int DefaultDelayMilliseconds = 1000;

		public static FileRetryPolicy Default { get; } = new FileRetryPolicy(DefaultAttempts, DefaultDelayMilliseconds);

		public FileRetryPolicy(int attempts, int delayMilliseconds)
		{
			Attempts = attempts;
			DelayMilliseconds = delayMilliseconds;
		}

		public int Attempts { get; }

		public int DelayMilliseconds { get; }

		/// <summary>
		/// Reads <c>DOTNET_ANDROID_FILE_WRITE_RETRY_ATTEMPTS</c> and <c>DOTNET_ANDROID_FILE_WRITE_RETRY_DELAY_MS</c>
		/// from the task's environment, falling back to the defaults when they are unset or invalid.
		/// </summary>
		public static FileRetryPolicy FromEnvironment(TaskEnvironment environment) =>
			new FileRetryPolicy(
				Read(environment, AttemptsVariable, DefaultAttempts),
				Read(environment, DelayVariable, DefaultDelayMilliseconds));

		static int Read(TaskEnvironment environment, string name, int defaultValue)
		{
			var value = environment?.GetEnvironmentVariable(name);
			return !string.IsNullOrEmpty(value) && int.TryParse(value, out var parsed) ? parsed : defaultValue;
		}
	}
}
