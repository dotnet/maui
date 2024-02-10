using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
using Microsoft.Maui.AppiumTests;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue20380 : _IssuesUITest
	{
		public Issue20380(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Pickers should not scroll and should not focus while hidden.";
		string datePicker => "datePickerVisible";
		string timePicker => "timeVisible";
		string button1 => "button1";
		string button2 => "button2";
		string button3 => "button3";
		string button4 => "button4";

		[Test]
		public void UpdateCollectionViewHeaderTest()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows], "Testing the iOS Keyboard Scrolling");

			var dateP = App.WaitForElement(datePicker);
			var initialPosition = dateP.GetRect();

			var app = App as AppiumApp;
			if (app is null)
				return;

			app.Click(button1);
			var keyboardPos = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver, true);
			Assert.IsTrue(keyboardPos is not null && keyboardPos.Value.Y > 0);
			CheckIfScrolled(initialPosition);
			KeyboardScrolling.HitDoneKey(app.Driver);

			app.Click(button2);
			keyboardPos = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver, true);
			Assert.IsNull(keyboardPos);
			CheckIfScrolled(initialPosition);

			app.Click(button3);
			keyboardPos = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver, true);
			Assert.IsTrue(keyboardPos is not null && keyboardPos.Value.Y > 0);
			CheckIfScrolled(initialPosition);
			KeyboardScrolling.HitDoneKey(app.Driver);

			app.Click(button4);
			keyboardPos = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver, true);
			Assert.IsNull(keyboardPos);
			CheckIfScrolled(initialPosition);
		}

		void CheckIfScrolled (System.Drawing.Rectangle initialPosition){
			var dateP = App.WaitForElement(datePicker);
			var datePLocation = dateP.GetRect();
			Assert.AreEqual(datePLocation.Y, initialPosition.Y);
		}
	}
}
