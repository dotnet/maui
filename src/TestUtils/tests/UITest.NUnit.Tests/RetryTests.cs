using System.Xml.Linq;
using NUnit.Framework;

namespace UITest.NUnit.Tests;

public class RetryTests
{
	[Test]
	public void RunSettingsPreserveExistingConfiguration()
	{
		var file = Path.Combine(AppContext.BaseDirectory, "validation", Guid.NewGuid() + ".runsettings");
		Directory.CreateDirectory(Path.GetDirectoryName(file));
		new XDocument(new XElement("RunSettings",
			new XElement("RunConfiguration", new XElement("MaxCpuCount", 1)),
			new XElement("TestRunParameters", new XElement("Parameter", new XAttribute("name", "device"), new XAttribute("value", "ios"))),
			new XElement("NUnit", new XElement("Where", "cat == 'Issues'"), new XElement("NumberOfTestWorkers", 0)))).Save(file);
		try
		{
			var settings = UITestRetry.CreateRunSettings(null, Path.GetDirectoryName(file), file);
			Assert.That((string)settings.Root.Element("NUnit").Element("Where"), Is.EqualTo("cat == 'Issues'"));
			Assert.That((string)settings.Root.Element("NUnit").Element("NumberOfTestWorkers"), Is.EqualTo("0"));
			Assert.That(settings.Root.Element("RunConfiguration"), Is.Not.Null);
			Assert.That(settings.Root.Element("TestRunParameters"), Is.Not.Null);
			Assert.That((string)settings.Root.Element("NUnit").Element("TestOutputXml"), Is.EqualTo(Path.GetDirectoryName(file)));
			Assert.That(XDocument.Load(file).Root.Element("NUnit").Element("TestOutputXml"), Is.Null);
		}
		finally
		{
			File.Delete(file);
		}
	}

	[TestCase("relative")]
	[TestCase("absolute")]
	[TestCase("deployment")]
	[TestCase("result-directory")]
	public void RejectsExistingAttachmentsOutsideConfiguredRoots(string sourceKind)
	{
		var scope = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
		var results = Path.Combine(scope, "results");
		var outside = Path.Combine(scope, "outside");
		Directory.CreateDirectory(results);
		Directory.CreateDirectory(outside);
		var sentinel = Path.Combine(outside, "sentinel.txt");
		File.WriteAllText(sentinel, "must not be imported");
		var path = sourceKind == "absolute" ? sentinel :
			sourceKind == "relative" ? Path.GetRelativePath(results, sentinel) : "sentinel.txt";
		var deployment = sourceKind == "deployment" ? outside : "";
		var relativeDirectory = sourceKind == "result-directory" ? outside : "";
		var archive = Path.Combine(results, "raw");
		try
		{
			var trx = AttachmentReport(path, deployment, relativeDirectory);
			Assert.Throws<InvalidDataException>(() => UITestRetry.PreserveAttachments(trx, results, archive));
			Assert.That(File.Exists(sentinel), Is.True);
			Assert.That(Directory.Exists(archive), Is.False);
		}
		finally { Directory.Delete(scope, true); }
	}

	[Test]
	public void PreservesAbsoluteAttachmentsOnlyWhenTheirRootIsExplicitlyAllowed()
	{
		var scope = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
		var results = Path.Combine(scope, "results");
		var logs = Path.Combine(scope, "logs");
		Directory.CreateDirectory(results);
		Directory.CreateDirectory(logs);
		var screenshot = Path.Combine(logs, "screenshot.png");
		File.WriteAllText(screenshot, "legitimate screenshot");
		try
		{
			var trx = AttachmentReport(screenshot);
			UITestRetry.PreserveAttachments(trx, results, Path.Combine(results, "raw"), new[] { logs });
			var path = (string)trx.Descendants().Single(element => element.Name.LocalName == "ResultFile").Attribute("path");
			Assert.That(File.ReadAllText(path), Is.EqualTo("legitimate screenshot"));
		}
		finally { Directory.Delete(scope, true); }
	}

	static XDocument AttachmentReport(string path, string deployment = "", string relativeDirectory = "")
	{
		XNamespace ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
		return new XDocument(new XElement(ns + "TestRun",
			new XElement(ns + "TestSettings", new XElement(ns + "Deployment", new XAttribute("runDeploymentRoot", deployment))),
			new XElement(ns + "Results", new XElement(ns + "UnitTestResult", new XAttribute("relativeResultsDirectory", relativeDirectory),
				new XElement(ns + "ResultFiles", new XElement(ns + "ResultFile", new XAttribute("path", path)))))));
	}

	[TestCase("FullyQualifiedName~Issue123")]
	[TestCase("TestCategory=Issues&FullyQualifiedName~Issue123")]
	public void ArbitraryFiltersFallBackToOriginalSelection(string filter)
	{
		Assert.Throws<InvalidOperationException>(() => UITestRetry.GetFilter(filter, Array.Empty<XDocument>()));
	}

	[Test]
	public void CategoryRetryKeepsOriginalConstraintAndEscapesNames()
	{
		var report = XDocument.Parse("""
            <test-run result="Failed" total="1">
              <test-suite type="Assembly" fullname="/tests.dll" result="Failed" site="Child">
                <test-suite type="TestFixture" classname="Example.Issues" fullname="Example.Issues" result="Failed" site="Child">
                  <properties><property name="Category" value="Issues" /></properties>
                  <test-case fullname="Example.Issues.Case('quoted')" result="Failed" />
                </test-suite>
              </test-suite>
            </test-run>
            """);
		Assert.That(UITestRetry.GetFilter("TestCategory=Issues|TestCategory=Other", new[] { report }),
			Is.EqualTo("((cat == 'Issues') and test == 'Example.Issues.Case(\\'quoted\\')')"));
	}

	[Test]
	public void MissingAttachmentsCannotSilentlyBecomeBrokenLinks()
	{
		var trx = XDocument.Parse("""
            <TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
              <Results><UnitTestResult><ResultFiles><ResultFile path="missing.png" /></ResultFiles></UnitTestResult></Results>
            </TestRun>
            """);
		Assert.Throws<FileNotFoundException>(() =>
			UITestRetry.PreserveAttachments(trx, AppContext.BaseDirectory, Path.Combine(AppContext.BaseDirectory, "raw")));
	}
}
