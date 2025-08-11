using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests
{
	[Category(UITestCategories.Page)]
	public class ContentPageFeatureTests : UITest
	{
		public const string ContentPageFeatureMatrix = "Content Page Feature Matrix";

		public ContentPageFeatureTests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(ContentPageFeatureMatrix);
		}

		[Test]
		public void RTL_FlowDirection_WithIsBusy()
		{
			App.WaitForElement("Resetbutton");
			App.Tap("Resetbutton");

			App.WaitForElement("FlowDirectionButton");
			App.Tap("FlowDirectionButton");

			App.WaitForElement("IsBusyCheckBox");
			App.Tap("IsBusyCheckBox");

			VerifyScreenshot();
		}

		[Test]
		public void IsVisible_WithTitle()
		{
			App.WaitForElement("Resetbutton");
			App.Tap("Resetbutton");

			App.WaitForElement("VisibilityCheckBox");
			App.Tap("VisibilityCheckBox");

			VerifyScreenshot();
		}

		[Test]
		public void IsVisible_WithIsBusy()
		{
			App.WaitForElement("Resetbutton");
			App.Tap("Resetbutton");

			App.WaitForElement("VisibilityCheckBox");
			App.Tap("VisibilityCheckBox");

			App.WaitForElement("IsBusyCheckBox");
			App.Tap("IsBusyCheckBox");

			VerifyScreenshot();
		}

		[Test]
		public void IsVisible_WithoutTitle()
		{
			App.WaitForElement("Resetbutton");
			App.Tap("Resetbutton");

			App.WaitForElement("TitleEntry");
			App.ClearText("TitleEntry");

			App.WaitForElement("VisibilityCheckBox");
			App.Tap("VisibilityCheckBox");

			VerifyScreenshot();
		}

		[Test]
		public void Padding_WithBackgroundColor()
		{
			App.WaitForElement("Resetbutton");
			App.Tap("Resetbutton");

			App.WaitForElement("PaddingButton");
			App.Tap("PaddingButton");

			App.WaitForElement("BackgroundButton");
			App.Tap("BackgroundButton");

			VerifyScreenshot();
		}

		[Test]
		public void Title_WithBackgroundColor()
		{
			App.WaitForElement("Resetbutton");
			App.Tap("Resetbutton");

			App.WaitForElement("TitleEntry");
			App.ClearText("TitleEntry");
			App.EnterText("TitleEntry", "New Title");

			App.WaitForElement("BackgroundButton");
			App.Tap("BackgroundButton");

			VerifyScreenshot();
		}

		[Test]
		public void Title_With_IsBusy()
		{
			App.WaitForElement("Resetbutton");
			App.Tap("Resetbutton");

			App.WaitForElement("TitleEntry");
			App.ClearText("TitleEntry");
			App.EnterText("TitleEntry", "New Title");

			App.WaitForElement("IsBusyCheckBox");
			App.Tap("IsBusyCheckBox");

			VerifyScreenshot();
		}

		[Test]
		public void Background_With_RTL()
		{
			App.WaitForElement("Resetbutton");
			App.Tap("Resetbutton");

			App.WaitForElement("BackgroundButton");
			App.Tap("BackgroundButton");

			App.WaitForElement("FlowDirectionButton");
			App.Tap("FlowDirectionButton");

			VerifyScreenshot();
		}

		[Test]
		public void Padding_With_RTL()
		{
			App.WaitForElement("Resetbutton");
			App.Tap("Resetbutton");

			App.WaitForElement("PaddingButton");
			App.Tap("PaddingButton");

			App.WaitForElement("FlowDirectionButton");
			App.Tap("FlowDirectionButton");

			VerifyScreenshot();
		}

		[Test]
		public void Padding_With_IsBusy()
		{
			App.WaitForElement("Resetbutton");
			App.Tap("Resetbutton");

			App.WaitForElement("PaddingButton");
			App.Tap("PaddingButton");

			App.WaitForElement("IsBusyCheckBox");
			App.Tap("IsBusyCheckBox");

			VerifyScreenshot();
		}

		[Test]
		public void Padding_With_Title()
		{
			App.WaitForElement("Resetbutton");
			App.Tap("Resetbutton");

			App.WaitForElement("PaddingButton");
			App.Tap("PaddingButton");

			App.WaitForElement("TitleEntry");
			App.ClearText("TitleEntry");
			App.EnterText("TitleEntry", "New Title");

			VerifyScreenshot();
		}
#if ANDROID || IOS
		[Test]
		public void HideSoftinput_With_RTL()
		{
			App.WaitForElement("Resetbutton");
			App.Tap("Resetbutton");

			App.WaitForElement("FlowDirectionButton");
			App.Tap("FlowDirectionButton");

			App.WaitForElement("HideSoftInputCheckBox");
			App.Tap("HideSoftInputCheckBox");

			App.WaitForElement("TestEntry");
			App.ClearText("TestEntry");
			App.EnterText("TestEntry", "New Text");

			App.WaitForElement("HideSoftInputLabel");
			App.Tap("HideSoftInputLabel");

			VerifyScreenshot();
		}

		[Test]
		public void HideSoftinput_With_Padding()
		{
			App.WaitForElement("Resetbutton");
			App.Tap("Resetbutton");

			App.WaitForElement("PaddingButton");
			App.Tap("PaddingButton");

			App.WaitForElement("HideSoftInputCheckBox");
			App.Tap("HideSoftInputCheckBox");

			App.WaitForElement("TestEntry");
			App.ClearText("TestEntry");
			App.EnterText("TestEntry", "New Text");

			App.WaitForElement("HideSoftInputLabel");
			App.Tap("HideSoftInputLabel");

			VerifyScreenshot();
		}
#endif
	}
}