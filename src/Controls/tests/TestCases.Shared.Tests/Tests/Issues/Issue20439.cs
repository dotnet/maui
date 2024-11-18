#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue20439 : _IssuesUITest
	{
		public override string Issue => "[iOS] Double dash in Entry or Editor crashes the app";

		public Issue20439(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void ErrorShouldNotBeThrown()
		{
			try
			{
				_ = App.WaitForElement("GoToTest");
				App.Tap("GoToTest");
				
				_ = App.WaitForElement("entry");
				App.Tap("entry");
				App.Tap("button");

				// The test passes if no crash is observed
				App.FindElement("editor");
			}
			finally
			{
				Reset();
			}
		}
	}
}
#endif