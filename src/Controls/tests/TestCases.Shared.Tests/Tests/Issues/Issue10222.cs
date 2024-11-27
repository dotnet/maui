using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue10222 : _IssuesUITest
	{
		public Issue10222(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Exception Ancestor must be provided for all pushes except first";

		// Crash after navigation
		/*
		[Test]
		[Category(UITestCategories.LifeCycle)]
		[Category(UITestCategories.Compatibility)]
		public void Issue10222Test()
		{
			App.WaitForElement("goTo");
			App.Tap("goTo");
			App.WaitForElement("collectionView");
			App.WaitForElement("goTo");
		}
		*/
	}
}