using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla32898 : _IssuesUITest
	{
		const string Success = "Success";
		const int Timeout = 20000;

		public Bugzilla32898(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Memory leak when TabbedPage is popped out ";

		[Test]
		[Category(UITestCategories.LifeCycle)]
		public void Issu32898Test()
		{
			var timeout = Timeout; // Give this a little slop to set the result text
			App.WaitForElement(Success, timeout: TimeSpan.FromMilliseconds(timeout));
		}
	}
}