using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2777 : _IssuesUITest
	{
		public Issue2777(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "When add GroupHeaderTemplate in XAML the group header does not show up";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[FailsOnIOS]
		[FailsOnMac]
		public void Issue2777Test()
		{
			App.Screenshot("I am at Issue 2965");
			App.WaitForNoElement("The letter A");
		}
	}
}