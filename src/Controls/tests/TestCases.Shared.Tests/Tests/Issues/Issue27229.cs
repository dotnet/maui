using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue27229 : _IssuesUITest
	{
		public Issue27229(TestDevice device) : base(device) { }

		public override string Issue => "CollectionView, EmptyView Fills Available Space By Default";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewEmptyViewFillsAvailableSpaceByDefault()
		{
			App.WaitForElement("ReadyToTest");
			VerifyScreenshot();
		}
	}
}