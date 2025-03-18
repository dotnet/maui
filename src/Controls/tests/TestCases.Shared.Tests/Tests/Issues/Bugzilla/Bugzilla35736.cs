#if IOS
using NUnit.Framework;
using NUnit.Framework.Legacy;
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

		[Test]
		[Category(UITestCategories.Editor)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest("Fails sometimes")]
		[FailsOnMacWhenRunningOnXamarinUITest("Fails sometimes")]
		public void Bugzilla35736Test()
		{
			App.WaitForElement("Bugzilla35736Editor");
			App.EnterText("Bugzilla35736Editor", "Testig");
			App.Tap("Bugzilla35736Button");
			ClassicAssert.AreEqual("Testing", App.FindElement("Bugzilla35736Label").GetText());
		}
	}
}
#endif