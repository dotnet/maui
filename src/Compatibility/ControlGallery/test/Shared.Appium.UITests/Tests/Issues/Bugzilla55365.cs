using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla55365 : IssuesUITest
	{
		public Bugzilla55365(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "~VisualElement crashes with System.Runtime.InteropServices.COMException";

		[Test]
		[Category(UITestCategories.ViewBaseTests)]
		public void ForcingGCDoesNotCrash()
		{
			RunningApp.WaitForElement("Clear");
			RunningApp.Tap("Clear");
			RunningApp.Tap("Garbage");
			RunningApp.WaitForNoElement("Success");
		}
	}
}