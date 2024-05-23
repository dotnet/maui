using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue13203 : _IssuesUITest
	{
		const string Success = "Success";

		public Issue13203(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] [iOS] CollectionView does not bind to items if `IsVisible=False`";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
		public void CollectionShouldInvalidateOnVisibilityChange()
		{
			App.WaitForNoElement(Success);
		}
	}
}