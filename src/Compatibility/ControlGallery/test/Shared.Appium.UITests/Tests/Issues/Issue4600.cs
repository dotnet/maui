using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue4600 : IssuesUITest
	{
		public Issue4600(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] CollectionView crash with empty ObservableCollection";

		[Test]
		public void TestIssue1905RefreshShows()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForNoElement("Insert");
			App.Click("btnInsert");
			App.WaitForNoElement("Inserted");
		}
	}
}