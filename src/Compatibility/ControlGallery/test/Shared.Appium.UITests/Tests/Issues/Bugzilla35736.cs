#if IOS
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla35736 : IssuesUITest
	{
		public Bugzilla35736(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Editor does not update Text value from autocorrect when losing focus";

		[Test]
		[Category(UITestCategories.Editor)]
		[FailsOnIOS("Fails sometimes")]
		public void Bugzilla35736Test()
		{
			RunningApp.WaitForElement("Bugzilla35736Editor");
			RunningApp.EnterText("Bugzilla35736Editor", "Testig");
			RunningApp.Tap("Bugzilla35736Button");
			ClassicAssert.AreEqual("Testing", RunningApp.FindElement("Bugzilla35736Label").GetText());
		}
	}
}
#endif