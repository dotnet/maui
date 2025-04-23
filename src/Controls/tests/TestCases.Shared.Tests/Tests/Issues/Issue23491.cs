using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23491 : _IssuesUITest
	{
		public Issue23491(TestDevice device) : base(device) { }

		public override string Issue => "BindableLayout.ItemsSource no longer works in 8.0.61";

		[Test]
		[Category(UITestCategories.Label)]
		public void ChildFlexLayoutContentShouldAppear()
		{
			App.WaitForElement("HeaderLabel");
			VerifyScreenshot();
		}
	}
}