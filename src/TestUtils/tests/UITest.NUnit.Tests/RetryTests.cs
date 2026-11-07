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
