using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue17531 : _IssuesUITest
	{
		public override string Issue => "Make senders be the same for different gesture EventArgs";

		public Issue17531(TestDevice testDevice) : base(testDevice) { }

		[Test]
		[Category(UITestCategories.Gestures)]
		public void GesturesSenderIsView()
		{
			App.WaitForElement("TapLabel");
			App.Tap("TapLabel");
			var tapResultLabel = App.FindElement("TapResultLabel").GetText();
			Assert.That(tapResultLabel, Is.EqualTo("Success"));

			App.WaitForElement("Red");
			App.WaitForElement("Green");
			App.DragAndDrop("Red", "Green");

#if MACCATALYST // In CI DragAndDrop does not effective sometimes so retry once before failing to resolve the flakiness.

			try
			{
				App.WaitForElement("DropResultLabel");
			}
			catch (TimeoutException)
			{
				App.DragAndDrop("Red", "Green");
			}
#endif
			App.WaitForElement("DropResultLabel");
			var dropResultLabel = App.FindElement("DropResultLabel").GetText();
			Assert.That(dropResultLabel, Is.EqualTo("Success"));
		}
	}
}