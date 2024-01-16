using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue19689 : _IssuesUITest
	{
		public Issue19689(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Java.Lang.IndexOutOfBoundsException: setSpan ( ... ) ends beyond length";

		[Test]
		public void Issue19689Test()
		{
			var entry = App.FindElement("TestEntry");
			Assert.IsNotNull(entry);
		}
	}
}
