using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class GridFeatureTests : UITest
{
	public const string GridFeatureMatrix = "Grid Feature Matrix";
	public const string Options = "Options";
	public const string Apply = "Apply";
	public const string RowEntry = "RowEntry";
	public const string ColumnEntry = "ColumnEntry";
	public const string RowDefinitionValueEntry = "RowDefinitionValueEntry";
	public const string ColumnDefinitionValueEntry = "ColumnDefinitionValueEntry";
	public const string MainContentRowSpanEntry = "MainContentRowSpanEntry";
	public const string MainContentColumnSpanEntry = "MainContentColumnSpanEntry";
	public const string RowSpacingEntry = "RowSpacingEntry";
	public const string ColumnSpacingEntry = "ColumnSpacingEntry";
	public const string PaddingEntry = "PaddingEntry";


	public GridFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(GridFeatureMatrix);
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_SetRow()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(RowEntry);
		App.ClearText(RowEntry);
		App.EnterText(RowEntry, "3");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_SetColumn()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ColumnEntry);
		App.ClearText(ColumnEntry);
		App.EnterText(ColumnEntry, "4");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_IsVisible()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IsVisibleFalseRadio");
		App.Tap("IsVisibleFalseRadio");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void Grid_FlowDirection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void Grid_BackgroundColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ColorGray");
		App.Tap("ColorGray");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_Padding()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(PaddingEntry);
		App.ClearText(PaddingEntry);
		App.EnterText(PaddingEntry, "50,50,50,50");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_RowSpacing()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(RowSpacingEntry);
		App.ClearText(RowSpacingEntry);
		App.EnterText(RowSpacingEntry, "20");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_ColumnSpacing()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ColumnSpacingEntry);
		App.ClearText(ColumnSpacingEntry);
		App.EnterText(ColumnSpacingEntry, "30");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_NestedGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("NestedGridCheckbox");
		App.Tap("NestedGridCheckbox");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}


	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_ColumnSpan()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(MainContentColumnSpanEntry);
		App.ClearText(MainContentColumnSpanEntry);
		App.EnterText(MainContentColumnSpanEntry, "2");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_RowSpan()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(MainContentRowSpanEntry);
		App.ClearText(MainContentRowSpanEntry);
		App.EnterText(MainContentRowSpanEntry, "2");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_HorizontalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("HorizontalOptionsStart");
		App.Tap("HorizontalOptionsStart");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_HorizontalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("HorizontalOptionsCenter");
		App.Tap("HorizontalOptionsCenter");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_HorizontalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("HorizontalOptionsEnd");
		App.Tap("HorizontalOptionsEnd");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_VerticalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("VerticalOptionsStart");
		App.Tap("VerticalOptionsStart");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_VerticalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("VerticalOptionsCenter");
		App.Tap("VerticalOptionsCenter");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_VerticalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("VerticalOptionsEnd");
		App.Tap("VerticalOptionsEnd");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_VerticalAndHorizontalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("VerticalOptionsStart");
		App.Tap("VerticalOptionsStart");
		App.WaitForElement("HorizontalOptionsStart");
		App.Tap("HorizontalOptionsStart");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_VerticalAndHorizontalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("VerticalOptionsCenter");
		App.Tap("VerticalOptionsCenter");
		App.WaitForElement("HorizontalOptionsCenter");
		App.Tap("HorizontalOptionsCenter");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_VerticalAndHorizontalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("VerticalOptionsEnd");
		App.Tap("VerticalOptionsEnd");
		App.WaitForElement("HorizontalOptionsEnd");
		App.Tap("HorizontalOptionsEnd");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_RowAndColumnTypeAbsolute()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(RowDefinitionValueEntry);
		App.ClearText(RowDefinitionValueEntry);
		App.EnterText(RowDefinitionValueEntry, "100");
		App.WaitForElement(ColumnDefinitionValueEntry);
		App.ClearText(ColumnDefinitionValueEntry);
		App.EnterText(ColumnDefinitionValueEntry, "100");
		App.WaitForElement("ColumnTypeAbsolute");
		App.Tap("ColumnTypeAbsolute");
		App.WaitForElement("RowTypeAbsolute");
		App.Tap("RowTypeAbsolute");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_SetRowAndColumn_BackgroundColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(RowEntry);
		App.ClearText(RowEntry);
		App.EnterText(RowEntry, "4");
		App.WaitForElement(ColumnEntry);
		App.ClearText(ColumnEntry);
		App.EnterText(ColumnEntry, "5");
		App.WaitForElement("ColorGray");
		App.Tap("ColorGray");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_SetRowAndColumnSpacing_BackgroundColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(RowSpacingEntry);
		App.ClearText(RowSpacingEntry);
		App.EnterText(RowSpacingEntry, "30");
		App.WaitForElement(ColumnSpacingEntry);
		App.ClearText(ColumnSpacingEntry);
		App.EnterText(ColumnSpacingEntry, "20");
		App.WaitForElement("ColorGray");
		App.Tap("ColorGray");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_SetRow_SetRowSpacing()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(RowEntry);
		App.ClearText(RowEntry);
		App.EnterText(RowEntry, "4");
		App.WaitForElement(RowSpacingEntry);
		App.ClearText(RowSpacingEntry);
		App.EnterText(RowSpacingEntry, "20");
		App.WaitForElement("ColumnTypeStar");
		App.Tap("ColumnTypeStar");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_SetColumn_SetColumnSpacing()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ColumnEntry);
		App.ClearText(ColumnEntry);
		App.EnterText(ColumnEntry, "4");
		App.WaitForElement(ColumnSpacingEntry);
		App.ClearText(ColumnSpacingEntry);
		App.EnterText(ColumnSpacingEntry, "20");
		App.WaitForElement("ColumnTypeStar");
		App.Tap("ColumnTypeStar");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_SetRow_SetColumnSpacing()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(RowEntry);
		App.ClearText(RowEntry);
		App.EnterText(RowEntry, "4");
		App.WaitForElement(ColumnSpacingEntry);
		App.ClearText(ColumnSpacingEntry);
		App.EnterText(ColumnSpacingEntry, "20");
		App.WaitForElement("ColumnTypeStar");
		App.Tap("ColumnTypeStar");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_SetColumn_SetRowSpacing()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ColumnEntry);
		App.ClearText(ColumnEntry);
		App.EnterText(ColumnEntry, "4");
		App.WaitForElement(RowSpacingEntry);
		App.ClearText(RowSpacingEntry);
		App.EnterText(RowSpacingEntry, "20");
		App.WaitForElement("ColumnTypeStar");
		App.Tap("ColumnTypeStar");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_SetRowSpacingAndPadding()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(PaddingEntry);
		App.ClearText(PaddingEntry);
		App.EnterText(PaddingEntry, "20,20,20,20");
		App.WaitForElement(RowSpacingEntry);
		App.ClearText(RowSpacingEntry);
		App.EnterText(RowSpacingEntry, "20");
		App.WaitForElement("ColumnTypeStar");
		App.Tap("ColumnTypeStar");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_SetColumnSpacingAndPadding()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(PaddingEntry);
		App.ClearText(PaddingEntry);
		App.EnterText(PaddingEntry, "20,20,20,20");
		App.WaitForElement(ColumnSpacingEntry);
		App.ClearText(ColumnSpacingEntry);
		App.EnterText(ColumnSpacingEntry, "20");
		App.WaitForElement("ColumnTypeStar");
		App.Tap("ColumnTypeStar");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGrid_SetRowAndColumnSpacingAndPadding()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(PaddingEntry);
		App.ClearText(PaddingEntry);
		App.EnterText(PaddingEntry, "20,20,20,20");
		App.WaitForElement("ColorRed");
		App.Tap("ColorRed");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
}