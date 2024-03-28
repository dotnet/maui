using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18740 : _IssuesUITest
	{
		public Issue18740(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Virtual keyboard appears with focus on Entry";

		[Test]
		[Category(UITestCategories.Entry)]
		[TestCase("Entry")]
		[TestCase("Editor")]
		[TestCase("SearchBar")]
		public void Issue18740Test(string view)
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac, TestDevice.Windows });

			try
			{
				// Navigate to the specific View test
				App.WaitForElement("WaitForStubControl");
				App.Click($"{view}Button");

				// 1.Make sure keyboard starts out closed.	
				App.WaitForElement("WaitForStubControl");
				App.DismissKeyboard();

				// 2. Focus the Entry.
				App.EnterText($"Test{view}", "test");
				App.Click($"Test{view}");

				// 3. Verify that the virtual keyboard appears.
				Assert.IsTrue(App.IsKeyboardShown());

				// 4. Extra step, get the performance data.
				var batteryInfo = App.GetPerformanceData("batteryinfo");
				Console.WriteLine(batteryInfo.ToString());
			}
			finally
			{
				this.Back();
			}
		}
	}
}