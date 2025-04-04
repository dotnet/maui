using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25606 : _IssuesUITest
	{
		public Issue25606(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Empty View Overlapping Header in CollectionView with CollectionViewHandler2";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void HeaderShouldNotBeCoveredByEmptyView()
		{
			App.WaitForElement("Header");
		}
	}
}