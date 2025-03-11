#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27622 : _IssuesUITest
	{
		const string CollectionView = "CollectionView27622";
		const string NoneRadioButton = "NoneRadioButton";
		const string SingleRadioButton = "SingleRadioButton";
		const string MultipleRadioButton = "MultipleRadioButton";

		public Issue27622(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Windows CollectionView Keyboard Navigation";

		[Test]
		[Category(UITestCategories.Accessibility)]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewKeyboardNavigation()
		{
			App.WaitForElement(CollectionView);

			// Test keyboard navigation by sending Down and Up arrow keys with a 
			// None CollectionView SelectionMode.
			App.WaitForElement(NoneRadioButton);
			App.Tap(NoneRadioButton);
			SendTabKey(); // Move the focus to the Single Selection RadioButton
			SendTabKey(); // Move the focus to the Multiple Selection RadioButton
			SendTabKey(); // Move the focus to CollectionView
			SendDownArrowKey(); // Item 2
			SendDownArrowKey(); // Item 3
			SendDownArrowKey(); // Item 4
			SendUpArrowKey(); // Item 3
			VerifyScreenshot("NoneCollectionViewKeyboardNavigation");

			// Test keyboard navigation by sending Down and Up arrow keys with a 
			// Single CollectionView SelectionMode.
			App.WaitForElement(SingleRadioButton);
			App.Tap(SingleRadioButton);
			SendTabKey(); // Move the focus to the Multiple Selection RadioButton
			SendTabKey(); // Move the focus to CollectionView
			SendDownArrowKey(); // Item 4
			SendDownArrowKey(); // Item 5
			VerifyScreenshot("SingleCollectionViewKeyboardNavigation");

			// Test keyboard navigation by sending Down and Up arrow keys with a 
			// Multiple CollectionView SelectionMode.
			App.WaitForElement(MultipleRadioButton);
			App.Tap(MultipleRadioButton);
			SendTabKey(); // Move the focus to CollectionView		  
			SendDownArrowKey(); // Item 6
			SendSpaceBarKey(); // Item 6 is selected
			SendDownArrowKey(); // Item 7
			SendDownArrowKey(); // Item 8
			SendSpaceBarKey(); // Item 8 is selected
			VerifyScreenshot("MultipleCollectionViewKeyboardNavigation");
		}

		void SendTabKey()
		{
			if (App is not AppiumWindowsApp appiumWindowsApp)
				throw new InvalidOperationException("SendTabKey is only supported on AppiumWindowsApp");

			appiumWindowsApp.Driver.ExecuteScript("windows: keys", new Dictionary<string, object>
			{
				["actions"] = new[]
				{
					new Dictionary<string, object> { ["virtualKeyCode"] = 0x09, ["down"]=true }, // Tab
				}
			});
		}

		void SendSpaceBarKey()
		{
			if (App is not AppiumWindowsApp appiumWindowsApp)
				throw new InvalidOperationException("SendSpaceBarKey is only supported on AppiumWindowsApp");

			appiumWindowsApp.Driver.ExecuteScript("windows: keys", new Dictionary<string, object>
			{
				["actions"] = new[]
				{
					new Dictionary<string, object> { ["virtualKeyCode"] = 0x20, ["down"]=true }, // Space
				}
			});
		}

		void SendDownArrowKey()
		{
			if (App is not AppiumWindowsApp appiumWindowsApp)
				throw new InvalidOperationException("SendDownArrowKey is only supported on AppiumWindowsApp");

			appiumWindowsApp.Driver.ExecuteScript("windows: keys", new Dictionary<string, object>
			{
				["actions"] = new[]
				{
					new Dictionary<string, object> { ["virtualKeyCode"] = 0x28, ["down"]=true }, // Down
				}
			});
		}

		void SendUpArrowKey()
		{
			if (App is not AppiumWindowsApp appiumWindowsApp)
				throw new InvalidOperationException("SendUpArrowKey is only supported on AppiumWindowsApp");

			appiumWindowsApp.Driver.ExecuteScript("windows: keys", new Dictionary<string, object>
			{
				["actions"] = new[]
				{
					new Dictionary<string, object> { ["virtualKeyCode"] = 0x26, ["down"]=true }, // Up
				}
			});
		}
	}
}
#endif