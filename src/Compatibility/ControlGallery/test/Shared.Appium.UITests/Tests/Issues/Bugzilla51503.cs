using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla51503 : IssuesUITest
	{
		public Bugzilla51503(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NullReferenceException on VisualElement Finalize";

		[Test]
		[Category(UITestCategories.Navigation)]
		[FailsOnAndroid]
		public void Issue51503Test()
		{
			for (int i = 0; i < 3; i++)
			{
				RunningApp.WaitForElement("Button");

				RunningApp.Tap("Button");

				RunningApp.WaitForElement("VisualElement");

				RunningApp.Back();
			}
		}
	}
}