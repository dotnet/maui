using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue3548 : IssuesUITest
	{
		const string SuccessMessage = "EFFECT IS ATTACHED!";

		public Issue3548(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Cannot attach effect to Frame";

		[Test]
		[Category(UITestCategories.Frame)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void CheckIsEffectAttached()
		{
			RunningApp.WaitForNoElement(SuccessMessage);
		}
	}
}