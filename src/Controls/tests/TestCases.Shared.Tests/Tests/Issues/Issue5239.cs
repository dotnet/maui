#if IOS ////This test case verifies that the sample is working exclusively on iOS platforms "due to navigationBehavior: NavigationBehavior.SetApplicationRoot".
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue5239 : _IssuesUITest
	{
		public Issue5239(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Top Padding not working on iOS when it is set alone";

		[Test]
		[Category(UITestCategories.Layout)]
		public void PaddingEqualToSafeAreaWorks()
		{
			var somePadding = App.WaitForElement("Hello").GetRect();
			Assert.That(20f, Is.EqualTo(somePadding.Y));
		}
	}
}
#endif