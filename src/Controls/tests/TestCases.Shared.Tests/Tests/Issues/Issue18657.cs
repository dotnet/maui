using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18657 : _IssuesUITest
{
	public Issue18657(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "CollectionView.EmptyView can not be removed by setting it to Null";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void RemoveEmptyViewAtRuntime()
	{
		App.WaitForElement("Button");
		App.Tap("Button");
		VerifyScreenshot();
	}
}