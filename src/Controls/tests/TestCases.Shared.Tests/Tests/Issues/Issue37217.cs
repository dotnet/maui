using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37217 : _IssuesUITest
{
	public override string Issue => "ShellContent.Content — shared Page.PropertyChanged retains prior ShellContent instances";

	public Issue37217(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Shell)]
	public void SharedContentPage_TransientShellContentsAreCollectedAfterForceGC()
	{
		App.WaitForElement("CreateShellContentsButton");
		App.Tap("CreateShellContentsButton");
		App.WaitForTextToBePresentInElement("CreatedCountLabel", "ShellContents created: 30");

		App.Tap("ForceGCButton");
		bool gcCompleted = App.WaitForTextToBePresentInElement("SummaryLabel", "Alive count: 0/30");

		Assert.That(gcCompleted, Is.True, "Expected all transient ShellContent instances sharing the same Page to be collectable after forcing GC.");
	}
}
