using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

[assembly: IssueReplicateRecordingAction]

public sealed class IssueReplicateRecordingActionAttribute : TestActionAttribute
{
	public override ActionTargets Targets => ActionTargets.Test;

	public override void BeforeTest(ITest test)
	{
		var acknowledgement = Environment.GetEnvironmentVariable("ISSUE_REPLICATE_RECORDING_ACK");
		var nonce = Environment.GetEnvironmentVariable("ISSUE_REPLICATE_RECORDING_NONCE");
		if (string.IsNullOrEmpty(acknowledgement) || string.IsNullOrEmpty(nonce))
			throw new InvalidOperationException("The native recording handshake is not configured.");

		var operation = Guid.NewGuid().ToString("N");
		TestContext.Progress.WriteLine($"ISSUE_REPLICATE_RECORDING_READY={nonce}:{operation}");
		var timer = Stopwatch.StartNew();
		var buffer = new char[129];
		while (timer.Elapsed < TimeSpan.FromSeconds(25))
		{
			if (File.Exists(acknowledgement))
			{
				using var reader = File.OpenText(acknowledgement);
				var count = reader.ReadBlock(buffer, 0, buffer.Length);
				if (count == buffer.Length)
					throw new InvalidOperationException("The native recording acknowledgement exceeds its bound.");
				var response = new string(buffer, 0, count);
				if (response == $"{operation}:started")
					return;
				if (response == $"{operation}:failed")
					throw new InvalidOperationException("Native recording did not start; the test body was not executed.");
			}
			Thread.Sleep(20);
		}
		throw new TimeoutException("Native recording was not acknowledged before the test body.");
	}
}
