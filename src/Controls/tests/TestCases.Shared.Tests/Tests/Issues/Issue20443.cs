using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue20443 : _IssuesUITest
	{
		public override string Issue => "CollectionView item sizing wrong after refresh";

		public Issue20443(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void RefreshShouldNotChangeSize()
		{
			_ = App.WaitForElement("lblSmall");
			_ = App.WaitForElement("lblBig");

			App.DragCoordinates(100, 100, 100, 500);

			Task.Delay(5000).Wait();
			VerifyScreenshot();

		}
	}
}