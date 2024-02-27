using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests 
{ 
    internal class Issue2728 : IssuesUITest
	{
		const string LabelHome = "Hello Label";

		public Issue2728(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[macOS] Label FontAttributes Italic is not working";

		[Test]
		public void Issue2728TestsItalicLabel()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Windows]);

			App.WaitForNoElement(LabelHome);
			App.Screenshot("Label rendered with italic font");
		}
	}
}