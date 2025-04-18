using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue29051 : _IssuesUITest
{
	public Issue29051(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "I8_Headers_and_Footers displays the footer 2019 in the middle of the header";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void FooterAndHeaderArePlacedCorrectlyAboveAndBelowCollectionView()
	{
		App.WaitForElement("Item1");
		VerifyScreenshot();
	}
}