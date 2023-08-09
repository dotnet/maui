using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class ScrollViewButtonInteractionPageTests : _IssuesUITest
	{
		public ScrollViewButtonInteractionPageTests(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "ScrollView Button Interaction Tests";

		[Test]
		public void ButtonsRemainsInteractiveWhenContentSizeChanges()
		{
			App.Tap("RedButton");
			Assert.IsTrue(App.WaitForTextToBePresentInElement("HeaderLabel", "RedBox IsVisible: True"));

			App.Tap("BlueButton");
			Assert.IsTrue(App.WaitForTextToBePresentInElement("HeaderLabel", "BlueBox IsVisible: True"));

			//This tests confirms that buttons remain interactive when the size of the ScrollViews container is 
			//changed when showing / hiding elements.
		}
	}
}
