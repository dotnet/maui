using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue10222 : _IssuesUITest
	{
		public Issue10222(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[CollectionView] ObjectDisposedException if the page is closed during scrolling";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void Issue10222Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac },
				"The CollectionView on the second page is not rendering.");
			try
			{
				App.WaitForElement("goTo");
				App.Click("goTo");
				App.WaitForElement("collectionView");
				App.WaitForElement("goTo");
			}
			finally
			{
				App.Back();
			}
		}
	}
}
