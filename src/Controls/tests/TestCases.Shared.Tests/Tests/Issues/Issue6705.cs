using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue6705 : _IssuesUITest
	{
		public Issue6705(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "InvokeOnMainThreadAsync throws NullReferenceException";

		[Test]
		[Category(UITestCategories.Button)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void Issue6705Test()
		{
			for (var i = 1; i < 6; i++)
			{
				App.WaitForElement($"Button{i}");
				App.Tap($"Button{i}");
				App.WaitForNoElement($"{i}");
			}
		}
	}
}