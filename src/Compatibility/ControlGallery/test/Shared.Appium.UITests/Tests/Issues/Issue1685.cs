using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue1685 : IssuesUITest
	{
		const string ButtonId = "Button1685";
		const string Success = "Success";

		public Issue1685(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Entry clears when upadting text from native with one-way binding"; 
		
		[Test]
		[Category(UITestCategories.Entry)]
		public void EntryOneWayBindingShouldUpdate()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement(ButtonId);
			App.Click(ButtonId);
			App.WaitForNoElement(Success);
		}
	}
}
