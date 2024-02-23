using NUnit.Framework;
using OpenQA.Selenium;

namespace UITests
{
	public class Issue11311 : IssuesUITest
	{
		public override string Issue => "[Regression] CollectionView NSRangeException";

		[Test]
		public void CollectionViewWithFooterShouldNotCrashOnDisplay()
		{
			// If this hasn't already crashed, the test is passing
			App.FindElement(By.XPath("//*[@text=\"Success\"]"));
		}
	}
}