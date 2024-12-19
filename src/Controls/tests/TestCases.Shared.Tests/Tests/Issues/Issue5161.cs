using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue5161:_IssuesUITest
{
public Issue5161(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ShellContent IsEnabledProperty does not work"; 

        [Test]
		[Category(UITestCategories.Shell)]
		public void CheckIsEnabled()
		{
			App.WaitForElement("ThirdPage");
            App.Tap("ThirdPage");
			App.Tap("SecondPage");
            App.WaitForElement("This is Third Page");
		}
}
