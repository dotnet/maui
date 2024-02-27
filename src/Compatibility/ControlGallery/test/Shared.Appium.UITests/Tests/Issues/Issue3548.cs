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
		public void CheckIsEffectAttached()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForNoElement(SuccessMessage);
		}
	}
}