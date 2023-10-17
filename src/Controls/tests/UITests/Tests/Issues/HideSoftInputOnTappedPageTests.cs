using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class HideSoftInputOnTappedPageTests : _IssuesUITest
	{
		public HideSoftInputOnTappedPageTests(TestDevice device) : base(device) { }

		public override string Issue => "Hide Soft Input On Tapped Page";

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

				App.Click("HideSoftInputOnTappedFalse");

				// Switch between enabling/disabling feature
				for (int i = 0; i < 2; i++)
				{
					App.Click("HideKeyboardWhenTappingPage");
					Assert.True(App.IsKeyboardShown());
					App.Click("EmptySpace");
					Assert.AreEqual(false, App.IsKeyboardShown());

					App.Click("DontHideKeyboardWhenTappingPage");
					Assert.True(App.IsKeyboardShown());
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
