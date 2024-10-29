using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue13203 : _IssuesUITest
{
	public Issue13203(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] [iOS] CollectionView does not bind to items if `IsVisible=False`";

	// [Test]
	// [Category(UITestCategories.CollectionView)]
	// [FailsOnIOS]
	// public void CollectionShouldInvalidateOnVisibilityChange()
	// {
	// 	App.WaitForElement(Success);
	// }
}