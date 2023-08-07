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

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
		}

		[TestCase("Entry", false)]
		[TestCase("Editor", false)]
		[TestCase("SearchBar", false)]
		[TestCase("Entry", true)]
		[TestCase("Editor", true)]
		[TestCase("SearchBar", true)]
		public void HideSoftInputOnTappedPageTest(string control, bool hideOnTapped)
		{
			if (hideOnTapped)
				App.Tap("ToggleHideSoftInputOnTapped");

			if (App.IsKeyboardShown())
				App.DismissKeyboard();

			App.Tap(control);

			Assert.True(App.IsKeyboardShown());

			App.Tap("EmptySpace");
			Assert.AreEqual(!hideOnTapped, App.IsKeyboardShown());
		}
	}
}
