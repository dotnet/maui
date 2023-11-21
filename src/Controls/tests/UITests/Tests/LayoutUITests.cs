using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	public class LayoutUITests : UITest
	{
		const string LayoutGallery = "Layout Gallery";

		public LayoutUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(LayoutGallery);
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			this.Back();
		}

		[Test]
		[Description("The AbsoluteLayout is able of positioning its child elements with proportional positions")]
		public void PositionProportional()
		{
			App.Click("AbsoluteLayoutPositionProportional");
			App.WaitForElement("TestAbsoluteLayout");

			// 1. With a snapshot we verify that The AbsoluteLayout is able
			// of positioning its child elements with proportional positions.
			VerifyScreenshot();

			this.Back();
		}

		[Test]
		[Description("Organizes child views in a vertical one-dimensional stack")]
		public void VerticalStackLayout()
		{
			App.Click("VerticalStackLayout");
			App.WaitForElement("TestStackLayout");

			// 1. With a snapshot we verify that The StackLayout
			// organizes child views in a vertical one-dimensional stack.
			VerifyScreenshot();

			this.Back();
		}

		[Test]
		[Description("Organizes child views in a horizontal one-dimensional stack")]
		public void HorizontalStackLayout()
		{
			App.Click("HorizontalStackLayout");
			App.WaitForElement("TestStackLayout");

			// 1. With a snapshot we verify that The StackLayout
			// organizes child views in a horizontal one-dimensional stack.
			VerifyScreenshot();

			this.Back();
		}

		[Test]
		[Description("Align child elements in the X axis works")]
		public void StackLayoutAlignment()
		{
			App.Click("StackLayoutAlignment");
			App.WaitForElement("TestStackLayout");

			// 1. With a snapshot we verify that align child elements in
			// the X axis works.
			VerifyScreenshot();

			this.Back();
		}

		[Test]
		[Description("Align child elements in the Y axis works")]
		public void StackLayoutExpansion()
		{
			App.Click("StackLayoutExpansion");
			App.WaitForElement("TestStackLayout");

			// 1. With a snapshot we verify that align child elements in
			// the Y axis works.
			VerifyScreenshot();

			this.Back();
		}

		[Test]
		[Description("Can apply space between each child")]
		public void StackLayoutSpacing()
		{
			App.Click("StackLayoutSpacing");
			App.WaitForElement("TestStackLayout");

			// 1. With a snapshot we verify that child elements has not
			// space between them.
			VerifyScreenshot("StackLayoutNoSpacing");

			// 2. Apply spacing (40) between items.
			App.Click("SpacingButton");

			// 3. With a snapshot we verify that can apply space
			// between each child.
			VerifyScreenshot("StackLayoutSpacing");

			// 4. Remove the spacing.
			App.Click("NoSpacingButton");
			VerifyScreenshot("StackLayoutNoSpacing");

			this.Back();
		}

		[Test]
		[Description("Modify the visibility of the ScrollBars")]
		public void ScrollBarVisibility()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android },
				"Currently fails on Android; see https://github.com/dotnet/maui/issues/12028");

			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac },
				"Currently fails on iOS; see https://github.com/dotnet/maui/issues/7767");

			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Windows },
				"Currently fails on Windows; see https://github.com/dotnet/maui/issues/7766");

			App.Click("ScrollViewScrollBarVisibility");
			App.WaitForElement("TestScrollView");

			// 1. The ScrollView use ScrollBarVisibility.Never and
			// the scrollbars should not appear.
			VerifyScreenshot("ScrollBarVisibilityNever");

			// 2. Tap the Button to scroll to the end.
			App.Click("ChangeVisibility");

			// 3. The ScrollView use ScrollBarVisibility.Always and
			// the scrollbars should always appear.
			VerifyScreenshot("ScrollBarVisibilityAlways");

			this.Back();
		}
		
		[Test]
		[Description("Scroll to the end using ScrollToAsync method")]
		public async Task ScrollViewScrollTo()
		{
			App.Click("ScrollViewScrollTo");
			App.WaitForElement("TestScrollView");

			// 1. Tap the Button to scroll to the end.
			App.Click("ScrollToEndButton");

			// Wait for the ScrollView to complete the scroll operation.
			await Task.Delay(1000);

			// 2.With a snapshot we verify that the ScrollView scrolled to the end.
			VerifyScreenshot();

			this.Back();
		}
	}
}