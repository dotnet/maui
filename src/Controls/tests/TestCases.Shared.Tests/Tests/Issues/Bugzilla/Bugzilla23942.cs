using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla23942 : _IssuesUITest
	{
		public Bugzilla23942(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Cannot bind properties in BindableObjects added to static resources in XAML";

		[Test]
		[Category(UITestCategories.LifeCycle)]
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla23942Test()
		{
			App.WaitForElement("success");
		}
	}
}