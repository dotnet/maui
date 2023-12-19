using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue19465 : _IssuesUITest
	{
		public Issue19465(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Double tap gesture NullReferenceException when navigating";

		[Test]
		public void Issue19465Test()
		{
			App.WaitForElement("FirstButton");
			App.Click("FirstButton");

			App.WaitForElement("SecondLabel");
			App.DoubleClick("SecondLabel");

			App.WaitForElement("ThirdButton");
			App.DoubleClick("ThirdButton");
		}
	}
}