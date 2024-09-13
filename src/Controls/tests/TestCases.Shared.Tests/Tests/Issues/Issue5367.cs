using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue5367 : _IssuesUITest
	{
		const string MaxLengthEditor = "MaxLength Editor";
		const string ForceBigStringButton = "Force Big String Button";

		public Issue5367(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Editor with MaxLength";

		[Test]
		[Category(UITestCategories.Editor)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroid]
		[FailsOnIOS]
		[FailsOnMac]
		public void Issue5367Test()
		{
			App.WaitForElement(MaxLengthEditor);
			App.Tap(ForceBigStringButton);
		}
	}
}