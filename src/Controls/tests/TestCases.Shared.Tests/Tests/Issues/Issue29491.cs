using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29491 : _IssuesUITest
{
	public Issue29491(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[CV2][CollectionView] Changing CollectionView's ItemsSource in runtime removes elements' parent seemingly random";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyDataTemplateParentIsNotNull()
	{
		App.WaitForElement("Button");
		App.Tap("Button");
		VerifyScreenshot();
	}
}