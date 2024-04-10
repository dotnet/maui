using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class HideSoftInputOnTappedPageTests : _IssuesUITest
	{
		public HideSoftInputOnTappedPageTests(TestDevice device) : base(device) { }

		public override string Issue => "Hide Soft Input On Tapped Page";

		public override bool ResetMainPage => false; // Requieres a NavigationPage.

		[TestCase("Entry", false)]
		[TestCase("Editor", false)]
		[TestCase("SearchBar", false)]
		[TestCase("Entry", true)]
		[TestCase("Editor", true)]
		[TestCase("SearchBar", true)]
		public void HideSoftInputOnTappedPageTest(string control, bool hideOnTapped)
		{
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.Mac, TestDevice.Windows
			});

			try
			{
				if (App.IsKeyboardShown())
					App.DismissKeyboard();

				if (hideOnTapped)
					App.Click("HideSoftInputOnTappedTrue");
				else
					App.Click("HideSoftInputOnTappedFalse");

				App.WaitForElement(control);
				App.Click(control);

				Assert.True(App.IsKeyboardShown());

				App.Click("EmptySpace");
				Assert.AreEqual(!hideOnTapped, App.IsKeyboardShown());
			}
			finally
			{
				App.DismissKeyboard();
				this.Back();
			}
		}

		[Test]
		public void TogglingHideSoftInputOnTapped()
		{
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.Mac, TestDevice.Windows
			});

			try
			{
				if (App.IsKeyboardShown())
					App.DismissKeyboard();

				App.WaitForElement("HideSoftInputOnTappedFalse");
				App.Click("HideSoftInputOnTappedFalse");

				// Switch between enabling/disabling feature
				for (int i = 0; i < 2; i++)
				{
					App.WaitForElement("HideKeyboardWhenTappingPage");
					App.Click("HideKeyboardWhenTappingPage");
					Assert.True(App.IsKeyboardShown());
					App.WaitForElement("EmptySpace");
					App.Click("EmptySpace");
					Assert.AreEqual(false, App.IsKeyboardShown());

					App.WaitForElement("DontHideKeyboardWhenTappingPage");
					App.Click("DontHideKeyboardWhenTappingPage");
					Assert.True(App.IsKeyboardShown());
					App.WaitForElement("EmptySpace");
					App.Click("EmptySpace");
					Assert.AreEqual(true, App.IsKeyboardShown());
				}
			}
			finally
			{
				App.DismissKeyboard();
				this.Back();
			}
		}
	}
}
