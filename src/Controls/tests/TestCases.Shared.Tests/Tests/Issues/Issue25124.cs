using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25124 : _IssuesUITest
{
	public Issue25124(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "OnPlatform does not work in the Header and Footer of CollectionView";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ShouldDisplayHeaderBasedOnOnPlatform()
	{
		App.WaitForElement("CollectionView");
		VerifyScreenshot();
	}
}