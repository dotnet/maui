using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue10454 : _IssuesUITest
	{
		const string Success = "Success";

		public Issue10454(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionView ChildAdded";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ChildAddedShouldFire()
		{
			App.WaitForElement(Success);
		}
	}
}