using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.SoftInput)]
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
		[FailsOnWindowsWhenRunningOnXamarinUITest("Test ignored on Windows")]
		public void HideSoftInputOnTappedPageTest(string control, bool hideOnTapped)
		{
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

				ClassicAssert.IsTrue(App.IsFocused(control));

				App.Click("EmptySpace");
				ClassicAssert.AreEqual(!hideOnTapped, App.IsFocused(control));
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
					App.Tap("HideSoftInputOnTappedTrue");
				else
					App.Tap("HideSoftInputOnTappedFalse");

				App.WaitForElement(control);
				App.Tap(control);

				ClassicAssert.True(App.IsKeyboardShown());

				App.Tap("EmptySpace");
				ClassicAssert.AreEqual(!hideOnTapped, App.IsKeyboardShown());
			}
			finally
			{
				App.DismissKeyboard();
				this.Back();
			}
		}

		[Test]
		[FailsOnWindowsWhenRunningOnXamarinUITest("Test ignored on Windows")]
		public void TogglingHideSoftInputOnTapped()
		{
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
					ClassicAssert.True(App.IsFocused("HideKeyboardWhenTappingPage"));
					App.Click("EmptySpace");
					ClassicAssert.AreEqual(false, App.IsFocused("HideKeyboardWhenTappingPage"));

					App.Click("DontHideKeyboardWhenTappingPage");
					ClassicAssert.True(App.IsFocused("DontHideKeyboardWhenTappingPage"));
					App.Click("EmptySpace");
					ClassicAssert.AreEqual(true, App.IsFocused("DontHideKeyboardWhenTappingPage"));
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

				App.Tap("HideSoftInputOnTappedFalse");

				// Switch between enabling/disabling feature
				for (int i = 0; i < 2; i++)
				{
					App.WaitForElement("HideKeyboardWhenTappingPage");
					App.Tap("HideKeyboardWhenTappingPage");
					ClassicAssert.True(App.IsKeyboardShown());
					App.Tap("EmptySpace");
					ClassicAssert.AreEqual(false, App.IsKeyboardShown());

					App.Tap("DontHideKeyboardWhenTappingPage");
					ClassicAssert.True(App.IsKeyboardShown());
					App.Tap("EmptySpace");
					ClassicAssert.AreEqual(true, App.IsKeyboardShown());
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