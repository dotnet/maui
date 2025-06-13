#if IOS
using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla35736 : _IssuesUITest
	{
		public Bugzilla35736(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Editor does not update Text value from autocorrect when losing focus";

		[Fact]
		[Category(UITestCategories.Editor)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest("Fails sometimes")]
		[FailsOnMacWhenRunningOnXamarinUITest("Fails sometimes")]
		public void Bugzilla35736Test()
		{
			App.WaitForElement("Bugzilla35736Editor");
			App.EnterText("Bugzilla35736Editor", "Testig");
			App.Tap("Bugzilla35736Button");
			Assert.Equal("Testing", App.FindElement("Bugzilla35736Label").GetText());
		}
	}
}
#endif