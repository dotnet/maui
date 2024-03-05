using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue6705 : IssuesUITest
	{
		public Issue6705(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "InvokeOnMainThreadAsync throws NullReferenceException"; 
		
		[Test]
		[Category(UITestCategories.Button)]
		public void Issue6705Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			for (var i = 1; i < 6; i++)
			{
				RunningApp.WaitForElement($"Button{i}");
				RunningApp.Tap($"Button{i}");
				RunningApp.WaitForElement($"{i}");
			}
		}
	}
}