using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
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

			ClassicAssert.IsNotEmpty(headerText);
			ClassicAssert.IsNotEmpty(footerText);
		}
	}
}
