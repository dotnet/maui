using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla37625 : _IssuesUITest
	{
		public Bugzilla37625(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "App crashes when quickly adding/removing Image views (Windows UWP)";

		[Fact]
		[Category(UITestCategories.Image)]
		public void Bugzilla37625Test()
		{
			App.WaitForElement("success");
		}
	}
}