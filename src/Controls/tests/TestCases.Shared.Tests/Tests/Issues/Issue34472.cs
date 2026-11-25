#if ANDROID // Android-only: edge-to-edge blank gap when HasNavigationBar=false is an Android-specific behavior.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue34472 : _IssuesUITest
	{
		public Issue34472(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Top SafeAreaEdge not respected when page pushed to NavigationPage";

		[Test]
		[Category(UITestCategories.SafeAreaEdges)]
		public void SafeAreaEdgesNoneRespectedInNavigationPage()
		{
			App.WaitForElement("PushToStackButton");
			App.Tap("PushToStackButton");
			App.WaitForElement("ContentBorder");

			var pushedBorderRect = App.WaitForElement("ContentBorder").GetRect();
			var pushedLabelRect = App.WaitForElement("SafeAreaEdgesLabel").GetRect();

			App.WaitForElement("BackButton");
			App.Tap("BackButton");

			App.WaitForElement("SetMainPageButton");
			App.Tap("SetMainPageButton");
			App.WaitForElement("ContentBorder");

			var mainPageBorderRect = App.WaitForElement("ContentBorder").GetRect();
			var mainPageLabelRect = App.WaitForElement("SafeAreaEdgesLabel").GetRect();

			Assert.That(pushedBorderRect.Y, Is.EqualTo(mainPageBorderRect.Y).Within(5),
				$"ContentBorder Y should be the same whether pushed ({pushedBorderRect.Y}) " +
				$"or set as main page ({mainPageBorderRect.Y}).");

			Assert.That(pushedLabelRect.Y, Is.EqualTo(mainPageLabelRect.Y).Within(5),
				$"SafeAreaEdgesLabel Y should be the same whether pushed ({pushedLabelRect.Y}) " +
				$"or set as main page ({mainPageLabelRect.Y}).");
		}
	}
}
#endif
