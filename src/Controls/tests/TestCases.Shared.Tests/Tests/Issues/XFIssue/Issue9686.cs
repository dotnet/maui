using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue9686 : _IssuesUITest
{
	public Issue9686(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug, CollectionView,iOS] Foundation.Monotouch Exception in Grouped CollectionView";

	//[Test]
	//[Category(UITestCategories.CollectionView)]
	//[FailsOnAndroid]
	//[FailsOnIOS]
	//public void AddRemoveEmptyGroupsShouldNotCrashOnInsert()
	//{
	//	App.WaitForElement(Run);
	//	App.Tap(Run);
	//	App.WaitForElement("Item 1");
	//	App.Tap(Run);
	//	App.WaitForElement(Success);
	//}
}