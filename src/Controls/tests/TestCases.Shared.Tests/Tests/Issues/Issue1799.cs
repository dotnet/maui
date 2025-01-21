#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1799 : _IssuesUITest
	{
		const string ListView = "ListView1799";
		const string Success = "Success";

		public Issue1799(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] listView without data crash on ipad.";

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		public void ListViewWithoutDataDoesNotCrash()
		{
			var result = App.WaitForElement(ListView);
			var listViewRect = result.GetRect();

			App.DragCoordinates(listViewRect.CenterX(), listViewRect.Y, listViewRect.CenterX(), listViewRect.Y + 50);

			App.WaitForElement(Success);
		}
	}
}
#endif