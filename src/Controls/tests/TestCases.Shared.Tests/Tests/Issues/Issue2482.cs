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
			App.WaitForElement(ButtonId);
			App.DoubleTap(ButtonId);
			App.WaitForElement(Success);
		}
	}
}