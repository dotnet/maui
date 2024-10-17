using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla60524 : _IssuesUITest
{
	public Bugzilla60524(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "NRE when rendering ListView with grouping enabled and HasUnevenRows set to true";

	// [Test]
	// [Category(UITestCategories.ListView)]
	// [FailsOnIOS]
	// public void Bugzilla60524Test()
	// {
	// 	App.WaitForElement(q => q.Marked("Group 1"));
	// }
}
