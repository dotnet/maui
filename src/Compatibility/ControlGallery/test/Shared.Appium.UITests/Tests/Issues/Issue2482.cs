using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue2482 : IssuesUITest
	{
		const string ButtonId = "SpinButton";
		const string Success = "Success";

		public Issue2482(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Animating a `View` that is currently animating will throw `System.InvalidOperationException`";

		[Test]
		[Category(UITestCategories.Animation)]
		public void AnimationCancel()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(ButtonId);
			App.DoubleClick(ButtonId);
			RunningApp.WaitForNoElement(Success, timeout: TimeSpan.FromSeconds(25));
		}
	}
}