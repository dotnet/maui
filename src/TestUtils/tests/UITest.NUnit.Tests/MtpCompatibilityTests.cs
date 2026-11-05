using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;
using NUnit.Framework;

namespace UITest.NUnit.Tests;

public class MtpCompatibilityTests
{
	static readonly XNamespace Trx = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
	static string ProbeName => typeof(MtpProbeFixture).FullName;
	static string FixtureFilter => string.Join("|", new[]
	{
		nameof(MtpProbeFixture.Attachment), nameof(MtpProbeFixture.PassingSibling), nameof(MtpProbeFixture.IgnoredSibling)
	}.Select(method => $"FullyQualifiedName={ProbeName}.{method}"));

	[TestCase("TestCategory=MtpProbe&FullyQualifiedName~Attachment")]
	[TestCase("FullyQualifiedName=UITest.NUnit.Tests.MtpProbeFixture.Attachment")]
	[TestCase("(TestCategory=Missing|TestCategory=MtpProbe)&FullyQualifiedName~Attachment")]
	public void CategoryAndFullyQualifiedNameFiltersPreserveAttachments(string filter)
	{
		var result = RunProbe("pass", filter);
		Assert.That(result.ExitCode, Is.Zero, result.Output);
		Assert.That(result.NUnit.Descendants("test-case").Select(test => (string)test.Attribute("methodname")),
			Is.EqualTo(new[] { nameof(MtpProbeFixture.Attachment) }));
		var attachments = result.Trx.Descendants(Trx + "ResultFile").Select(file => (string)file.Attribute("path")).ToArray();
		Assert.That(attachments, Has.Length.EqualTo(2), result.Trx.ToString());
		Assert.That(attachments.Any(path => path.EndsWith("screenshot.png")), Is.True);
		Assert.That(attachments.Any(path => path.EndsWith("appium.log")), Is.True);
		foreach (var attachment in attachments)
			Assert.That(File.Exists(attachment), Is.True);
		UITestRetry.PreserveAttachments(result.Trx, result.Directory, Path.Combine(result.Directory, "raw", "attachments"));
		var preserved = result.Trx.Descendants(Trx + "ResultFile").Select(file => (string)file.Attribute("path")).ToArray();
		Assert.That(preserved.All(path => path.StartsWith(result.Directory, StringComparison.Ordinal)), Is.True);
		foreach (var attachment in preserved)
			Assert.That(File.Exists(attachment), Is.True);
	}

	[Test]
	public void NUnitWherePreservesRetrySelectionAndRunParameters()
	{
		var result = RunProbe("pass", null, $"test == '{ProbeName}.{nameof(MtpProbeFixture.Attachment)}'");
		Assert.That(result.ExitCode, Is.Zero, result.Output);
		Assert.That(result.NUnit.Descendants("test-case").Count(), Is.EqualTo(1));
		Assert.That((string)result.NUnit.Descendants("test-case").Single().Attribute("methodname"),
			Is.EqualTo(nameof(MtpProbeFixture.Attachment)));
	}

	[TestCase("setup")]
	[TestCase("teardown")]
	[TestCase("assert")]
	public void FixtureAndBodyFailuresFailTheJobAndKeepNUnitEvidence(string mode)
	{
		var result = RunProbe(mode, FixtureFilter);
		// NUnit's bridge can return success for teardown errors; the job must reconcile XML.
		if (mode != "teardown")
			Assert.That(result.ExitCode, Is.Not.Zero, result.Output);
		Assert.That((string)result.NUnit.Root.Attribute("result"), Is.EqualTo("Failed"));
		Assert.That(result.NUnit.Descendants("failure").Any(), Is.True);
		Assert.That(UITestRetry.ApplyNUnitFailures(result.Trx, new[] { result.NUnit }), Is.True);
		Assert.That((string)result.Trx.Root.Element(Trx + "ResultSummary").Attribute("outcome"), Is.EqualTo("Failed"));
	}

	[Test]
	public void FixtureRecoveryReconcilesMtpTrx()
	{
		var first = RunProbe("setup", FixtureFilter);
		var retry = RunProbe("pass", null, $"test == '{ProbeName}'");
		Assert.That(first.ExitCode, Is.Not.Zero, first.Output);
		Assert.That(retry.ExitCode, Is.Zero, retry.Output);
		UITestRetry.ApplyNUnitFailures(first.Trx, new[] { first.NUnit });
		UITestRetry.MergeResults(first.Trx, retry.Trx, new[] { first.NUnit }, new[] { retry.NUnit });
		Assert.That((string)first.Trx.Root.Element(Trx + "ResultSummary").Attribute("outcome"), Is.Not.EqualTo("Failed"),
			first.Trx.ToString());
		Assert.That(first.Trx.Descendants(Trx + "UnitTestResult").Count(), Is.EqualTo(3));
	}

	static ProbeResult RunProbe(string mode, string filter, string where = null)
	{
		var directory = Path.Combine(AppContext.BaseDirectory, "validation", Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(directory);
		var settings = Path.Combine(directory, "probe.runsettings");
		UITestRetry.CreateRunSettings(where, directory).Save(settings);
		var document = XDocument.Load(settings);
		document.Root.Add(new XElement("TestRunParameters", new XElement("Parameter",
			new XAttribute("name", "ProbeParameter"), new XAttribute("value", "preserved"))));
		document.Save(settings);
		var startInfo = new ProcessStartInfo("dotnet")
		{
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			WorkingDirectory = directory
		};
		foreach (var argument in new[] { Assembly.GetExecutingAssembly().Location, "--results-directory", directory,
			"--settings", settings, "--report-trx", "--report-trx-filename", "probe.trx" })
			startInfo.ArgumentList.Add(argument);
		if (filter != null)
		{
			startInfo.ArgumentList.Add("--filter");
			startInfo.ArgumentList.Add(filter);
		}
		startInfo.Environment["NUNIT_MTP_PROBE"] = mode;
		startInfo.Environment.Remove("GH_TOKEN");
		startInfo.Environment.Remove("GITHUB_TOKEN");
		startInfo.Environment.Remove("COPILOT_GITHUB_TOKEN");
		using var process = Process.Start(startInfo);
		var stdout = process.StandardOutput.ReadToEndAsync();
		var stderr = process.StandardError.ReadToEndAsync();
		if (!process.WaitForExit(60_000))
		{
			process.Kill(entireProcessTree: true);
			Assert.Fail("NUnit MTP probe timed out.");
		}
		var output = stdout.GetAwaiter().GetResult() + stderr.GetAwaiter().GetResult();
		Assert.That(File.Exists(Path.Combine(directory, "probe.trx")), Is.True, output);
		var nunitFiles = Directory.GetFiles(directory, "*.xml");
		Assert.That(nunitFiles, Has.Length.EqualTo(1), output);
		return new ProbeResult(process.ExitCode, output, directory,
			XDocument.Load(Path.Combine(directory, "probe.trx")), XDocument.Load(nunitFiles.Single()));
	}

	record ProbeResult(int ExitCode, string Output, string Directory, XDocument Trx, XDocument NUnit);
}

[Explicit("Executed in a child MTP host by MtpCompatibilityTests."), Category("MtpProbe")]
public class MtpProbeFixture
{
	[OneTimeSetUp]
	public void SetUp()
	{
		if (Environment.GetEnvironmentVariable("NUNIT_MTP_PROBE") == "setup")
			throw new InvalidOperationException("Appium fixture setup failed before any test body ran.");
	}

	[OneTimeTearDown]
	public void TearDown()
	{
		if (Environment.GetEnvironmentVariable("NUNIT_MTP_PROBE") == "teardown")
			throw new InvalidOperationException("Appium fixture teardown failed after passing test bodies.");
	}

	[Test]
	public void Attachment()
	{
		Assert.That(TestContext.Parameters["ProbeParameter"], Is.EqualTo("preserved"));
		var screenshot = Path.Combine(TestContext.CurrentContext.WorkDirectory, "screenshot.png");
		File.WriteAllBytes(screenshot, Convert.FromBase64String(
			"iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+a0uUAAAAASUVORK5CYII="));
		var log = Path.Combine(TestContext.CurrentContext.WorkDirectory, "appium.log");
		File.WriteAllText(log, "Appium diagnostic evidence");
		TestContext.AddTestAttachment(screenshot, "Screenshot");
		TestContext.AddTestAttachment(log, "Appium log");
		if (Environment.GetEnvironmentVariable("NUNIT_MTP_PROBE") == "assert")
			Assert.Fail("Test assertion failed.");
	}

	[Test]
	public void PassingSibling() { }

	[Test, Ignore("Existing ignored coverage must stay ignored after fixture recovery.")]
	public void IgnoredSibling() { }
}
