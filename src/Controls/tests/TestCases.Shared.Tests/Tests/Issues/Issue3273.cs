using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3273 : _IssuesUITest
	{
		public Issue3273(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Drag and drop reordering not firing CollectionChanged";

		[Test]
		[Category(UITestCategories.Gestures)]
		public void Issue3273Test()
		{
			App.WaitForElement("Move items");
			App.Tap("Move items");
			App.WaitForElement("Success");
		}
	}
}