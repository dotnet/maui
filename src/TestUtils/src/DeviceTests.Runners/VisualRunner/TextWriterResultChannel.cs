using System;
using System.Diagnostics;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	public class TextWriterResultChannel
	{
		int failed;
		int passed;
		int skipped;

		readonly object lockOjb = new object();

		public void RecordResult(TestResultViewModel result)
		{
			lock (lockOjb)
			{
				if (result.TestCase.Result == TestState.Passed)
				{
					Debug.Write("\t[PASS] ");
					passed++;
				}
				else if (result.TestCase.Result == TestState.Skipped)
				{
					Debug.Write("\t[SKIPPED] ");
					skipped++;
				}
				else if (result.TestCase.Result == TestState.Failed)
				{
					Debug.Write("\t[FAIL] ");
					failed++;
				}
				else
				{
					Debug.Write("\t[INFO] ");
				}
				Debug.Write(result.TestCase.DisplayName);

				var message = result.ErrorMessage;
				if (!string.IsNullOrEmpty(message))
				{
					Debug.Write(" : {0}", message.Replace("\r\n", "\\r\\n"));
				}
				Debug.WriteLine("");

				var stacktrace = result.ErrorStackTrace;
				if (!string.IsNullOrEmpty(result.ErrorStackTrace))
				{
					var lines = stacktrace.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					foreach (var line in lines)
						Debug.WriteLine("\t\t{0}", line);
				}
			}
		}

		public void OpenChannel(string message = null)
		{
			lock (lockOjb)
			{
				failed = passed = skipped = 0;
				Debug.WriteLine("[Runner executing:\t{0}]", message);
			}
		}

		public void CloseChannel()
		{
			lock (lockOjb)
			{
				var total = passed + failed; // ignored are *not* run
				Debug.WriteLine("Tests run: {0} Passed: {1} Failed: {2} Skipped: {3}", total, passed, failed, skipped);
			}
		}
	}
}
