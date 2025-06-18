using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27808 : _IssuesUITest
	{
		public Issue27808(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionView with header or footer has incorrect height";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewShouldHaveCorrectHeight()
		{
			App.WaitForElement("item3");
		}
	}
}