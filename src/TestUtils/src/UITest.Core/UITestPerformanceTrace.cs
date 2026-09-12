using System.Diagnostics;
using System.Text.Json;

namespace UITest.Core
{
	public static class UITestPerformanceTrace
	{
		const string TraceFileEnvironmentVariable = "UITEST_PERFORMANCE_TRACE_FILE";
		static readonly object TraceLock = new();
		static readonly AsyncLocal<string?> CurrentTest = new();
		static StreamWriter? s_writer;
		static int s_eventsSinceFlush;

		public static bool IsEnabled =>
			!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(TraceFileEnvironmentVariable));

		public static IDisposable Measure(string operation, string? detail = null)
		{
			if (!IsEnabled)
				return DisabledScope.Instance;

			return new TraceScope(operation, detail);
		}

		public static void StartTest(string testName)
		{
			CurrentTest.Value = testName;
			WriteEvent("test_start", testName);
		}

		public static void StopTest(string testName)
		{
			WriteEvent("test_stop", testName);
			CurrentTest.Value = null;
		}

		static void WriteEvent(string eventName, string operation, string? detail = null, double? elapsedMilliseconds = null)
		{
			var traceFile = Environment.GetEnvironmentVariable(TraceFileEnvironmentVariable);
			if (string.IsNullOrWhiteSpace(traceFile))
				return;

			var traceEvent = new
			{
				timestamp = DateTimeOffset.UtcNow,
				eventName,
				operation,
				detail,
				elapsedMilliseconds,
				test = CurrentTest.Value,
				processId = Environment.ProcessId,
				threadId = Environment.CurrentManagedThreadId,
			};

			lock (TraceLock)
			{
				s_writer ??= CreateWriter(traceFile);
				s_writer.WriteLine(JsonSerializer.Serialize(traceEvent));
				s_eventsSinceFlush++;

				if (eventName != "operation" || s_eventsSinceFlush >= 100)
				{
					s_writer.Flush();
					s_eventsSinceFlush = 0;
				}
			}
		}

		static StreamWriter CreateWriter(string traceFile)
		{
			var directory = Path.GetDirectoryName(traceFile);
			if (!string.IsNullOrEmpty(directory))
				Directory.CreateDirectory(directory);

			var extension = Path.GetExtension(traceFile);
			var fileName = Path.GetFileNameWithoutExtension(traceFile);
			var processTraceFile = Path.Combine(directory ?? string.Empty, $"{fileName}_pid{Environment.ProcessId}{extension}");
			Console.WriteLine($"UI test performance trace process file: {processTraceFile}");

			return new StreamWriter(new FileStream(processTraceFile, FileMode.Create, FileAccess.Write, FileShare.Read));
		}

		sealed class TraceScope : IDisposable
		{
			readonly string _operation;
			readonly string? _detail;
			readonly long _startTimestamp = Stopwatch.GetTimestamp();
			bool _disposed;

			public TraceScope(string operation, string? detail)
			{
				_operation = operation;
				_detail = detail;
			}

			public void Dispose()
			{
				if (_disposed)
					return;

				_disposed = true;
				WriteEvent(
					"operation",
					_operation,
					_detail,
					Stopwatch.GetElapsedTime(_startTimestamp).TotalMilliseconds);
			}
		}

		sealed class DisabledScope : IDisposable
		{
			public static readonly DisabledScope Instance = new();

			public void Dispose()
			{
			}
		}
	}
}
