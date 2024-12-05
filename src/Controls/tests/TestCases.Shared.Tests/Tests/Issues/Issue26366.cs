#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26366 : _IssuesUITest
	{
		public Issue26366(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionView ScrollOffset does not reset when the ItemSource is changed";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ScrollOffsetShouldReset()
		{
			App.WaitForElement("Button");
			App.ScrollDown("CollectionView");
			App.Click("Button");
			var text = App.FindElement("Label").GetText();
			Assert.That(text, Is.EqualTo("0"));
		}
	}
}
#endif