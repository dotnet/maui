using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	class Issue14557 : _IssuesUITest
	{
		public Issue14557(TestDevice device) : base(device) { }
		public override string Issue => "CollectionView header and footer not displaying on Windows";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void HeaderAndFooterRender()
		{
			App.WaitForElement("collectionView");

			var headerText = App.FindElement("headerLabel").GetText();
			var footerText = App.FindElement("footerLabel").GetText();

			Assert.IsNotEmpty(headerText);
			Assert.IsNotEmpty(footerText);
		}
	}
}
