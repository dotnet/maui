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
				TestDevice.Windows
			});
   
			App.WaitForElement("HideSoftInputOnTappedTrue");

			if (this.Device == TestDevice.Mac)
			{
				HideSoftInputOnTappedPageTestForMac(control, hideOnTapped);
			}
			else
			{
				HideSoftInputOnTappedPageTestForAndroidiOS(control, hideOnTapped);
			}
		}

		void HideSoftInputOnTappedPageTestForMac(string control, bool hideOnTapped)
		{
			try
			{
				if (hideOnTapped)
					App.Click("HideSoftInputOnTappedTrue");
				else
					App.Click("HideSoftInputOnTappedFalse");

				App.WaitForElement(control);
				App.Click(control);

				Assert.IsTrue(App.IsFocused(control));

				App.Click("EmptySpace");
				Assert.AreEqual(!hideOnTapped, App.IsFocused(control));
			}
			finally
			{
				this.Back();
			}
		}

		void HideSoftInputOnTappedPageTestForAndroidiOS(string control, bool hideOnTapped)
		{
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
				TestDevice.Windows
			});
   
			App.WaitForElement("HideSoftInputOnTappedFalse");

			if (this.Device == TestDevice.Mac)
			{
				TogglingHideSoftInputOnTappedForMac();
			}
			else
			{
				TogglingHideSoftInputOnTappedForAndroidiOS();
			}
		}

		public void TogglingHideSoftInputOnTappedForMac()
		{
			try
			{
				App.Click("HideSoftInputOnTappedFalse");

				// Switch between enabling/disabling feature
				for (int i = 0; i < 2; i++)
				{
					App.Click("HideKeyboardWhenTappingPage");
					Assert.True(App.IsFocused("HideKeyboardWhenTappingPage"));
					App.Click("EmptySpace");
					Assert.AreEqual(false, App.IsFocused("HideKeyboardWhenTappingPage"));

					App.Click("DontHideKeyboardWhenTappingPage");
					Assert.True(App.IsFocused("DontHideKeyboardWhenTappingPage"));
					App.Click("EmptySpace");
					Assert.AreEqual(true, App.IsFocused("DontHideKeyboardWhenTappingPage"));
				}
			}
			finally
			{
				this.Back();
			}
		}

		public void TogglingHideSoftInputOnTappedForAndroidiOS()
		{
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
