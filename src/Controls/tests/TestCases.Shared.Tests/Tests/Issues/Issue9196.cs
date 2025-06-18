using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue9196 : _IssuesUITest
	{
		const string Success = "Success";

		public Issue9196(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] [iOS] CollectionView EmptyView causes the application to crash";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void EmptyViewShouldNotCrash()
		{
			App.WaitForElement(Success);
		}
	}
}