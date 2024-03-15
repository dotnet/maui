#if ANDROID
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue8801 : IssuesUITest
	{
		public Issue8801(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Attempt to read from field 'int android.view.ViewGroup$LayoutParams.width' on a null object reference";

		[Test]
		[Category(UITestCategories.Layout)]
		public void NotAddingElementsNativelyDoesntCrashAndroid()
		{
			RunningApp.WaitForElement("Success");
		}
	}
}
#endif