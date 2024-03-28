using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue1900 : IssuesUITest
	{
		public Issue1900(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Xamarin ios ListView ObservableCollection<myClass>. Collection.Add() throwing 'Index # is greater than the number of rows #' exception";

		[Test]
		[Category(UITestCategories.ListView)]
		[FailsOnIOS]
		public void Issue1900Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("ListView");
		}
	}
}