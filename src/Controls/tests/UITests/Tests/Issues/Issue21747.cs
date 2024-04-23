using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue21747 : _IssuesUITest
	{
		public Issue21747(TestDevice device) : base(device)
		{
		}

		public override string Issue => "TemplateBinding with CornerRadius on Border does not work anymore";

		[Test]
		[Category(UITestCategories.Border)]
		public void BorderTemplateBindingWorks()
		{
			App.WaitForElement("WaitForStubControl");
		}
	}
}