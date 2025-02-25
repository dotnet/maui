using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue16386 : _IssuesUITest
	{
		public Issue16386(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Process the hardware enter key as Done";

		[Test]
		[Category(UITestCategories.Entry)]
		public void HittingEnterKeySendsDone()
		{
			App.WaitForElement("HardwareEnterKeyEntry");
			App.Tap("HardwareEnterKeyEntry");
			App.PressEnter();
			App.WaitForElement("Success");
		}
	}
}
