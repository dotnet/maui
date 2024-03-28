using NUnit.Framework;
using UITest.Core;

namespace UITests
{
	public class Issue3342 : IssuesUITest
	{
		public Issue3342(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] BoxView BackgroundColor not working on 3.2.0-pre1";
		
		[Test]
		[Category(UITestCategories.BoxView)]
		public void Issue3342Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.Screenshot("I am at Issue 3342");
			RunningApp.Screenshot("I see the green box");
		}
	}
}
