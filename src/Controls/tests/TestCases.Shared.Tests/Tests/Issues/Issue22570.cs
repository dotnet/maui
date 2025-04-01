using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22570 : _IssuesUITest
	{
		public Issue22570(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "[iOS] Cross TabBar navigation broken";

		[Test]
		[Category(UITestCategories.Shell)]
		public void Issue22570Test()
		{
			App.WaitForElement("button");
			App.Click("button");
			App.WaitForElement("label");
		}
	}
}
