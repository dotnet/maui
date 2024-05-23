using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2482 : _IssuesUITest
	{
		const string ButtonId = "SpinButton";
		const string Success = "Success";

		public Issue2482(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Animating a `View` that is currently animating will throw `System.InvalidOperationException`";

		[Test]
		[Category(UITestCategories.Animation)]
		[Category(UITestCategories.Compatibility)]
		public void AnimationCancel()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement(ButtonId);
			App.DoubleClick(ButtonId);
			App.WaitForNoElement(Success, timeout: TimeSpan.FromSeconds(25));
		}
	}
}