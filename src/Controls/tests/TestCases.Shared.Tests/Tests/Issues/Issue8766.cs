using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8766 : _IssuesUITest
	{
		public Issue8766(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] CollectionView.EmptyView does not inherit parent Visual";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void VisualPropagatesToEmptyView()
		{
			App.WaitForElement("TestReady");
			App.Screenshot("CollectionViewWithEmptyView");
		}
	}
}
