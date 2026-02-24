using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue31372 : _IssuesUITest
	{
		public override string Issue => "IsPresented=true Not Working on Initial Value in FlyoutPage";

		public Issue31372(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.FlyoutPage)]
		public void VerifyIsPresentedInitialValue()
		{
			App.WaitForElement("FlyoutLabel");
		}
	}
}