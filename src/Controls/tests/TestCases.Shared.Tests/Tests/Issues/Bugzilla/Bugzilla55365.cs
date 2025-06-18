using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla55365 : _IssuesUITest
	{
		public Bugzilla55365(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "~VisualElement crashes with System.Runtime.InteropServices.COMException";

		[Test]
		[Category(UITestCategories.ViewBaseTests)]
		public void ForcingGCDoesNotCrash()
		{
			App.WaitForElement("Clear");
			App.Tap("Clear");
			App.Tap("Garbage");
			App.WaitForElement("Success");
		}
	}
}