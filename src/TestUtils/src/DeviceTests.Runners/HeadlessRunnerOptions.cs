#nullable enable
#if ANDROID
using System;
using System.IO;
#endif

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners
{
	public class HeadlessRunnerOptions
	{
		string _testResultsFilename = "testResults.xml";

		public string TestResultsFilename
		{
			get => _testResultsFilename;
			set
			{
#if ANDROID
				if (string.IsNullOrWhiteSpace(value) || value is "." or ".." ||
					value.Contains('/', StringComparison.Ordinal) || value.Contains('\\', StringComparison.Ordinal) ||
					value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
				{
					throw new ArgumentException("The Android result filename must be a valid basename within application FilesDir.", nameof(value));
				}
#endif
				_testResultsFilename = value;
			}
		}

		public bool RequiresUIContext { get; set; } = true;
	}
}
