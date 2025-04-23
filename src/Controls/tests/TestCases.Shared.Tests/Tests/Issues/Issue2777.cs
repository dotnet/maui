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
		public void Issue2777Test()
		{
			App.WaitForElement("The letter A");
		}
	}
}