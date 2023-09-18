using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Appium;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class SoftInputExtensionsPageTests : _IssuesUITest
	{
		public SoftInputExtensionsPageTests(TestDevice device) : base(device) { }

		public override string Issue => "Soft Input Extension Methods";

		[Test]
		public void SoftInputExtensionsPageTest()
		{
			// This issue is not working on net7 for the following platforms 
			// This is not a regression it's just the test being backported from net8
			UITestContext.IgnoreIfPlatforms(new[]
			{
				TestDevice.Android, TestDevice.Windows, TestDevice.iOS, TestDevice.Mac
			}, BackportedTestMessage);

			// Make sure the buttons appear on the screen.
			Task.Delay(1000).Wait();
			App.Tap("ShowKeyboard");
			Assert.IsTrue(App.WaitForTextToBePresentInElement("Result", "True"));
			App.Tap("HideKeyboard");
			Assert.IsTrue(App.WaitForTextToBePresentInElement("Result", "False"));
		}
	}
}
