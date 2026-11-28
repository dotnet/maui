using System.Xml;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;
using Xunit;

namespace Microsoft.Maui.TestUtils.UnitTests;

public class NativeTestResultTests
{
	[Fact]
	public void PreservesTheoryRowsSharingAnIdentityAndNonzeroExit()
	{
		WithReport("""
			<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
			  <Results>
			    <UnitTestResult testId="theory" executionId="one" outcome="Passed" />
			    <UnitTestResult testId="theory" executionId="two" outcome="Passed" />
			    <UnitTestResult testId="theory" executionId="three" outcome="Failed" />
			    <UnitTestResult testId="skipped" executionId="four" outcome="NotExecuted" />
			  </Results>
			  <ResultSummary>
			    <Counters total="4" passed="2" failed="1" notExecuted="1" />
			  </ResultSummary>
			</TestRun>
			""", path => Assert.Equal(new NativeTestResult(2, path, 2, 1, 1), NativeTestResult.ReadTrx(2, path)));
	}

	[Theory]
	[InlineData("-1")]
	[InlineData("invalid")]
	[InlineData("")]
	public void RejectsInvalidCounts(string passed)
	{
		WithReport($"""
			<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
			  <ResultSummary>
			    <Counters passed="{passed}" failed="0" notExecuted="0" />
			  </ResultSummary>
			</TestRun>
			""", path => Assert.Throws<FormatException>(() => NativeTestResult.ReadTrx(0, path)));
	}

	[Theory]
	[InlineData("<TestRun />")]
	[InlineData("""
		<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
		  <ResultSummary><Counters passed="1" failed="0" /></ResultSummary>
		</TestRun>
		""")]
	public void RejectsMissingCounters(string report) =>
		WithReport(report, path => Assert.Throws<InvalidDataException>(() => NativeTestResult.ReadTrx(0, path)));

	[Fact]
	public void RejectsDocumentTypeDeclarations() =>
		WithReport("<!DOCTYPE TestRun><TestRun />",
			path => Assert.Throws<XmlException>(() => NativeTestResult.ReadTrx(0, path)));

	static void WithReport(string report, Action<string> assert)
	{
		var path = Path.Combine(Path.GetTempPath(), $"native-results-{Guid.NewGuid():N}.trx");
		try
		{
			File.WriteAllText(path, report);
			assert(path);
		}
		finally
		{
			File.Delete(path);
		}
	}
}
