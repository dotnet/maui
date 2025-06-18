using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26187 : _IssuesUITest
	{
		public override string Issue => "[MAUI] Select items traces are preserved";

		public Issue26187(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void SelectedItemVisualIsCleared()
		{
			App.WaitForElement("lblItem");
			App.Tap("lblItem");
			App.WaitForElement("btnGoBack");
			App.Tap("btnGoBack");
			App.WaitForElement("lblItem");
			VerifyScreenshot();
		}
	}
}
