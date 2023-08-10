using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Appium;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

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
			UITestContext.IgnoreIfPlatforms(new[]
			{
				TestDevice.Mac, TestDevice.Windows
			});

			try
			{
				if (App.IsKeyboardShown())
					App.DismissKeyboard();

				if (hideOnTapped)
					App.Tap("HideSoftInputOnTappedTrue");
				else
					App.Tap("HideSoftInputOnTappedFalse");

				App.Tap(control);

				Assert.True(App.IsKeyboardShown());

				App.Tap("EmptySpace");
				Assert.AreEqual(!hideOnTapped, App.IsKeyboardShown());
			}
			finally
			{
				App.DismissKeyboard();
				App.NavigateBack();
			}
		}

		[Test]
		public void TogglingHideSoftInputOnTapped()
		{
			UITestContext.IgnoreIfPlatforms(new[]
			{
				TestDevice.Mac, TestDevice.Windows
			});

			try
			{
				if (App.IsKeyboardShown())
					App.DismissKeyboard();

				App.Tap("HideSoftInputOnTappedFalse");

				// Switch between enabling/disabling feature
				for (int i = 0; i < 2; i++)
				{
					App.Tap("HideKeyboardWhenTappingPage");
					Assert.True(App.IsKeyboardShown());
					App.Tap("EmptySpace");
					Assert.AreEqual(false, App.IsKeyboardShown());

					App.Tap("DontHideKeyboardWhenTappingPage");
					Assert.True(App.IsKeyboardShown());
					App.Tap("EmptySpace");
					Assert.AreEqual(true, App.IsKeyboardShown());
				}
			}
			finally
			{
				App.DismissKeyboard();
				App.NavigateBack();
			}
		}
	}
}
