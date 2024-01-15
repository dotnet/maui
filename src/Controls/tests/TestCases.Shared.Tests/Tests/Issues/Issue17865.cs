using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue17865 : _IssuesUITest
	{
		public Issue17865(TestDevice device) : base(device) { }

		public override string Issue => "CollectionView throws NRE when ScrollTo method is called from a handler of event Window.Created";

		[Test]
		public void Issue17865Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.iOS, TestDevice.Mac },
				"Windows Test.");

			App.WaitForElement("WaitForStubControl");

			App.Click("WaitForStubControl");

			// NOTE: Without crashes the test has passed.
		}
	}
}
