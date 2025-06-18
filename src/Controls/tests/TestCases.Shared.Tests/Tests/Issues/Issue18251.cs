using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18251 : _IssuesUITest
	{
		public Issue18251(TestDevice device) : base(device) { }

		public override string Issue => "IllegalArgumentException when changing number of tabbar pages";

		[Test]
		[Category(UITestCategories.Shell)]
		public void NoExceptionShouldBeThrownAddingShellTabs()
		{
			App.WaitForElement("button");

			for (int i = 0; i < 2; i++)
			{
				App.Click("button");
			}

			// The test passes if no crash is observed
		}
	}
}