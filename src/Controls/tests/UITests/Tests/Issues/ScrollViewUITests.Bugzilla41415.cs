using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(UITestCategories.ScrollView)]
	public class Bugzilla41415UITests : _IssuesUITest
	{
		const string ButtonId = "ClickId";

		public Bugzilla41415UITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "ScrollX and ScrollY values are not consistent with iOS";

		// Bugzilla41415 (src\Compatibility\ControlGallery\src\Issues.Shared\Bugzilla41415.cs)
		[Test]
		public void Bugzilla41415Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS },
				"This test is failing, likely due to product issue");

			App.WaitForElement(ButtonId);
			App.Click(ButtonId);
			App.WaitForNoElement("x: 100");
			App.WaitForNoElement("y: 100");
			App.WaitForNoElement("z: True", timeout: TimeSpan.FromSeconds(25));
			App.WaitForNoElement("a: True");
			App.Click(ButtonId);
			App.WaitForNoElement("x: 200");
			App.WaitForNoElement("y: 100");
			App.WaitForNoElement("z: True", timeout: TimeSpan.FromSeconds(25));
			App.WaitForNoElement("a: False");
		}
	}
}