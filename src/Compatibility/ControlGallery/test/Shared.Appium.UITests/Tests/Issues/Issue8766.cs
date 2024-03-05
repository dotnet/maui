using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue8766 : IssuesUITest
	{
		public Issue8766(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] CollectionView.EmptyView does not inherit parent Visual";

		[Test]
		public void VisualPropagatesToEmptyView()
		{
			App.WaitForElement("TestReady");
			App.Screenshot("CollectionViewWithEmptyView");
		}
	}
}
