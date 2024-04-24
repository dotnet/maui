#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue17400 : _IssuesUITest
	{
		public Issue17400(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "CollectionView wrong Layout";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void Issue17400Test()
		{
			App.WaitForElement("UpdateBtn");
			App.Click("UpdateBtn");

			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}
#endif