using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	[Category(UITestCategories.Page)]
	public class ContentPageFeatureTests : UITest
	{
		public const string ContentPageFeatureMatrix = "ContentPage Feature Matrix";

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
		public void ContentPage_Content()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("ContentButton");
			App.Tap("ContentButton");

			VerifyScreenshot();
		}

		[Test]
		public void ContentPage_IsVisible_WithTitle()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("VisibilityCheckBox");
			App.Tap("VisibilityCheckBox");

			VerifyScreenshot();
		}

		[Test]
		public void ContentPage_IsVisible_WithoutTitle()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("TitleEntry");
			App.ClearText("TitleEntry");

			App.WaitForElement("VisibilityCheckBox");
			App.Tap("VisibilityCheckBox");

			VerifyScreenshot();
		}

		[Test]
		public void ContentPage_Padding_WithBackgroundColor()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("PaddingButton");
			App.Tap("PaddingButton");

			App.WaitForElement("BackgroundButton");
			App.Tap("BackgroundButton");

			VerifyScreenshot();
		}

		[Test]
		public void ContentPage_Title_WithBackgroundColor()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("TitleEntry");
			App.ClearText("TitleEntry");
			App.EnterText("TitleEntry", "New Title");

			App.WaitForElement("BackgroundButton");
			App.Tap("BackgroundButton");

			VerifyScreenshot();
		}

		[Test]
		public void ContentPage_Title_WithBackgroundColorAnPadding()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("TitleEntry");
			App.ClearText("TitleEntry");
			App.EnterText("TitleEntry", "New Title");

			App.WaitForElement("BackgroundButton");
			App.Tap("BackgroundButton");

			App.WaitForElement("PaddingButton");
			App.Tap("PaddingButton");

			VerifyScreenshot();
		}

		[Test]
		public void ContentPage_Background_WithRTL()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("BackgroundButton");
			App.Tap("BackgroundButton");

			App.WaitForElement("FlowDirectionButton");
			App.Tap("FlowDirectionButton");

			VerifyScreenshot();
		}

		[Test]
		public void ContentPage_Padding_WithRTL()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("PaddingButton");
			App.Tap("PaddingButton");

			App.WaitForElement("FlowDirectionButton");
			App.Tap("FlowDirectionButton");

			VerifyScreenshot();
		}

		[Test]
		public void ContentPage_Padding_WithTitle()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("PaddingButton");
			App.Tap("PaddingButton");

			App.WaitForElement("TitleEntry");
			App.ClearText("TitleEntry");
			App.EnterText("TitleEntry", "New Title");

			VerifyScreenshot();
		}

#if ANDROID || IOS
		[Test]
		public void ContentPage_HideSoftinput_WithRTLAndPadding()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("FlowDirectionButton");
			App.Tap("FlowDirectionButton");

			App.WaitForElement("PaddingButton");
			App.Tap("PaddingButton");

			App.WaitForElement("HideSoftInputCheckBox");
			App.Tap("HideSoftInputCheckBox");

			App.WaitForElement("TestEntry");
			App.Tap("TestEntry");

			App.WaitForElement("HideSoftInputLabel");
			App.Tap("HideSoftInputLabel");

			VerifyScreenshot();
		}

		[Test]
		public void ContentPage_HideSoftinput_WithPaddingAndBackground()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("BackgroundButton");
			App.Tap("BackgroundButton");

			App.WaitForElement("PaddingButton");
			App.Tap("PaddingButton");

			App.WaitForElement("HideSoftInputCheckBox");
			App.Tap("HideSoftInputCheckBox");

			App.WaitForElement("TestEntry");
			App.Tap("TestEntry");

			App.WaitForElement("HideSoftInputLabel");
			App.Tap("HideSoftInputLabel");

			VerifyScreenshot();
		}

		[Test]
		public void ContentPage_Title_WithPaddingAndHideSoftInput()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("TitleEntry");
			App.ClearText("TitleEntry");
			App.EnterText("TitleEntry", "New Title");

			App.WaitForElement("PaddingButton");
			App.Tap("PaddingButton");

			App.WaitForElement("HideSoftInputCheckBox");
			App.Tap("HideSoftInputCheckBox");

			App.WaitForElement("TestEntry");
			App.Tap("TestEntry");

			App.WaitForElement("HideSoftInputLabel");
			App.Tap("HideSoftInputLabel");

			VerifyScreenshot();
		}
#endif
	}
}