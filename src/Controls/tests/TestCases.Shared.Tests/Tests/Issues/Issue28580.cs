using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28580 : _IssuesUITest
{
	public Issue28580(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "CollectionView footer sizing when source is empty";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void FooterWithEmptyCVShouldHaveCorrectSize()
	{
		App.WaitForElement("labelInFooter");
		VerifyScreenshot();
	}
}