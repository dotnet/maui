using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue5830 : IssuesUITest
	{
		public Issue5830(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Enhancement] EntryCellTableViewCell should be public";

		[Test]
		public void Issue5830Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("TestReady");
			App.Screenshot("EntryTableViewCell Test with custom Text and TextColor");
		}
	}
}