using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(UITestCategories.ScrollView)]
	public class Bugzilla49069UITests : _IssuesUITest
	{
		public Bugzilla49069UITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "Java.Lang.ArrayIndexOutOfBoundsException when rendering long Label on Android";

		// Bugzilla49069 (src\Compatibility\ControlGallery\src\Issues.Shared\Bugzilla49069.cs)
		[Test]
		public void Bugzilla49069Test()
		{
			App.WaitForElement("lblLong");
		}
	}
}