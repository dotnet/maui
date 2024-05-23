using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3548 : _IssuesUITest
	{
		const string SuccessMessage = "EFFECT IS ATTACHED!";

		public Issue3548(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Cannot attach effect to Frame";

		[Test]
		[Category(UITestCategories.Frame)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void CheckIsEffectAttached()
		{
			App.WaitForNoElement(SuccessMessage);
		}
	}
}