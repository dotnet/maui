// Uncomment the code below to and sprinkle Trace.WriteLines around to help troubleshoot.

/*
using System.Diagnostics;
using NUnit.Framework;

[SetUpFixture]
public class TraceSetup
{
	[OneTimeSetUp]
	public void SetUp()
	{
		Trace.Listeners.Add(new TextWriterTraceListener(File.Create("trace.log")));
		Trace.AutoFlush = true;
		Trace.WriteLine("Starting trace...");
	}
}
*/