using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class LayoutFeatureTests : UITest
{
	public const string LayoutFeatureMatrix = "Layout Feature Matrix";
	public const string Options = "Options";
	public const string Apply = "Apply";
	public const string HorizontalStart = "HorizontalStart";
	public const string HorizontalCenter = "HorizontalCenter";
	public const string HorizontalEnd = "HorizontalEnd";
	public const string HorizontalFill = "HorizontalFill";
	public const string VerticalFill = "VerticalFill";
	public const string VerticalStart = "VerticalStart";
	public const string VerticalCenter = "VerticalCenter";
	public const string VerticalEnd = "VerticalEnd";
	public const string WidthNone = "WidthNone";
	public const string Width300 = "Width300";
	public const string HeightNone = "HeightNone";
	public const string Height300 = "Height300";
	public const string VerticalStackLayoutButton = "VerticalStackLayoutButton";
	public const string HorizontalStackLayoutButton = "HorizontalStackLayoutButton";
	public const string GridWithContentViewButton = "GridWithContentViewButton";
	public const string AbsoluteLayoutButton = "AbsoluteLayoutButton";
	public const string GridButton = "GridButton";
	public const string LayoutInsideContentViewButton = "LayoutInsideContentViewButton";
	public const string LayoutInsideGridButton = "LayoutInsideGridButton";
	public const string NestedLayoutButton = "NestedLayoutButton";


	public LayoutFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(LayoutFeatureMatrix);
	}

	[Test, Order(1)]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideContentView_HorizontalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideContentViewButton);
		App.Tap(LayoutInsideContentViewButton);
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideContentView_HorizontalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideContentViewButton);
		App.Tap(LayoutInsideContentViewButton);
		VerifyScreenshot();
	}

	[Test, Order(3)]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideContentView_HorizontalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideContentViewButton);
		App.Tap(LayoutInsideContentViewButton);
		VerifyScreenshot();
	}

	[Test, Order(4)]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideContentView_HorizontalOptionsFill()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideContentViewButton);
		App.Tap(LayoutInsideContentViewButton);
		VerifyScreenshot();
	}
	
	[Test, Order(5)]
	[Category(UITestCategories.Layout)]
	public void VerifyGridWithContentViewLayout_HorizontalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridWithContentViewButton);
		App.Tap(GridWithContentViewButton);
		VerifyScreenshot();
	}

	[Test, Order(6)]
	[Category(UITestCategories.Layout)]
	public void VerifyGridWithContentViewLayout_HorizontalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridWithContentViewButton);
		App.Tap(GridWithContentViewButton);
		VerifyScreenshot();
	}

	[Test, Order(7)]
	[Category(UITestCategories.Layout)]
	public void VerifyGridWithContentViewLayout_HorizontalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridWithContentViewButton);
		App.Tap(GridWithContentViewButton);
		VerifyScreenshot();
	}

	[Test, Order(8)]
	[Category(UITestCategories.Layout)]
	public void VerifyGridWithContentViewLayout_HorizontalOptionsFill()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridWithContentViewButton);
		App.Tap(GridWithContentViewButton);
		VerifyScreenshot();
	}

	[Test, Order(9)]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideGrid_HorizontalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideGridButton);
		App.Tap(LayoutInsideGridButton);
		VerifyScreenshot();
	}

	[Test, Order(10)]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideGrid_HorizontalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideGridButton);
		App.Tap(LayoutInsideGridButton);
		VerifyScreenshot();
	}

	[Test, Order(11)]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideGrid_HorizontalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideGridButton);
		App.Tap(LayoutInsideGridButton);
		VerifyScreenshot();
	}

	[Test, Order(12)]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideGrid_HorizontalOptionsFill()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideGridButton);
		App.Tap(LayoutInsideGridButton);
		VerifyScreenshot();
	}

	[Test, Order(13)]
	[Category(UITestCategories.Layout)]
	public void VerifyNestedLayout_HorizontalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(NestedLayoutButton);
		App.Tap(NestedLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(14)]
	[Category(UITestCategories.Layout)]
	public void VerifyNestedLayout_HorizontalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(NestedLayoutButton);
		App.Tap(NestedLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(15)]
	[Category(UITestCategories.Layout)]
	public void VerifyNestedLayout_HorizontalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(NestedLayoutButton);
		App.Tap(NestedLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(16)]
	[Category(UITestCategories.Layout)]
	public void VerifyNestedLayout_HorizontalOptionsFill()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(NestedLayoutButton);
		App.Tap(NestedLayoutButton);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_ANDROID
	[Test, Order(18)]
	[Category(UITestCategories.Layout)]
	public void VerifyVerticalStackLayout_HorizontalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(VerticalStackLayoutButton);
		App.Tap(VerticalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(19)]
	[Category(UITestCategories.Layout)]
	public void VerifyVerticalStackLayout_HorizontalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(VerticalStackLayoutButton);
		App.Tap(VerticalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(22)]
	[Category(UITestCategories.Layout)]
	public void VerifyHorizontalStackLayout_HorizontalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(HorizontalStackLayoutButton);
		App.Tap(HorizontalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(23)]
	[Category(UITestCategories.Layout)]
	public void VerifyHorizontalStackLayout_HorizontalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(HorizontalStackLayoutButton);
		App.Tap(HorizontalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(26)]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_HorizontalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AbsoluteLayoutButton);
		App.Tap(AbsoluteLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(27)]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_HorizontalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AbsoluteLayoutButton);
		App.Tap(AbsoluteLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(30)]
	[Category(UITestCategories.Layout)]
	public void VerifyGridLayout_HorizontalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridButton);
		App.Tap(GridButton);
		VerifyScreenshot();
	}

	[Test, Order(31)]
	[Category(UITestCategories.Layout)]
	public void VerifyGridLayout_HorizontalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridButton);
		App.Tap(GridButton);
		VerifyScreenshot();
	}
#endif

    [Test, Order(17)]
	[Category(UITestCategories.Layout)]
	public void VerifyVerticalStackLayout_HorizontalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(VerticalStackLayoutButton);
		App.Tap(VerticalStackLayoutButton);
		VerifyScreenshot();
	}
	[Test, Order(20)]
	[Category(UITestCategories.Layout)]
	public void VerifyVerticalStackLayout_HorizontalOptionsFill()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(VerticalStackLayoutButton);
		App.Tap(VerticalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(21)]
	[Category(UITestCategories.Layout)]
	public void VerifyHorizontalStackLayout_HorizontalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(HorizontalStackLayoutButton);
		App.Tap(HorizontalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(24)]
	[Category(UITestCategories.Layout)]
	public void VerifyHorizontalStackLayout_HorizontalOptionsFill()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(HorizontalStackLayoutButton);
		App.Tap(HorizontalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(25)]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_HorizontalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AbsoluteLayoutButton);
		App.Tap(AbsoluteLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(28)]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_HorizontalOptionsFill()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AbsoluteLayoutButton);
		App.Tap(AbsoluteLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(29)]
	[Category(UITestCategories.Layout)]
	public void VerifyGridLayout_HorizontalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridButton);
		App.Tap(GridButton);
		VerifyScreenshot();
	}

	[Test, Order(32)]
	[Category(UITestCategories.Layout)]
	public void VerifyGridLayout_HorizontalOptionsFill()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridButton);
		App.Tap(GridButton);
		VerifyScreenshot();
	}

	[Test, Order(33)]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideContentView_VerticalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalStart);
		App.Tap(VerticalStart);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideContentViewButton);
		App.Tap(LayoutInsideContentViewButton);
		VerifyScreenshot();
	}

	[Test, Order(34)]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideContentView_VerticalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalCenter);
		App.Tap(VerticalCenter);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideContentViewButton);
		App.Tap(LayoutInsideContentViewButton);
		VerifyScreenshot();
	}

	[Test, Order(35)]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideContentView_VerticalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalEnd);
		App.Tap(VerticalEnd);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideContentViewButton);
		App.Tap(LayoutInsideContentViewButton);
		VerifyScreenshot();
	}

	[Test, Order(36)]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideContentView_VerticalOptionsFill()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalFill);
		App.Tap(VerticalFill);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideContentViewButton);
		App.Tap(LayoutInsideContentViewButton);
		VerifyScreenshot();
	}

	[Test, Order(37)]
	[Category(UITestCategories.Layout)]
	public void VerifyGridWithContentViewLayout_VerticalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalStart);
		App.Tap(VerticalStart);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridWithContentViewButton);
		App.Tap(GridWithContentViewButton);
		VerifyScreenshot();
	}

	[Test, Order(38)]
	[Category(UITestCategories.Layout)]
	public void VerifyGridWithContentViewLayout_VerticalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalCenter);
		App.Tap(VerticalCenter);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridWithContentViewButton);
		App.Tap(GridWithContentViewButton);
		VerifyScreenshot();
	}

	[Test, Order(39)]
	[Category(UITestCategories.Layout)]
	public void VerifyGridWithContentViewLayout_VerticalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalEnd);
		App.Tap(VerticalEnd);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridWithContentViewButton);
		App.Tap(GridWithContentViewButton);
		VerifyScreenshot();
	}

	[Test, Order(40)]
	[Category(UITestCategories.Layout)]
	public void VerifyGridWithContentViewLayout_VerticalOptionsFill()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalFill);
		App.Tap(VerticalFill);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridWithContentViewButton);
		App.Tap(GridWithContentViewButton);
		VerifyScreenshot();
	}

	[Test, Order(41)]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideGrid_VerticalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalStart);
		App.Tap(VerticalStart);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideGridButton);
		App.Tap(LayoutInsideGridButton);
		VerifyScreenshot();
	}

	[Test, Order(42)]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideGrid_VerticalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalCenter);
		App.Tap(VerticalCenter);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideGridButton);
		App.Tap(LayoutInsideGridButton);
		VerifyScreenshot();
	}

	[Test, Order(43)]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideGrid_VerticalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalEnd);
		App.Tap(VerticalEnd);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideGridButton);
		App.Tap(LayoutInsideGridButton);
		VerifyScreenshot();
	}

	[Test, Order(44)]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideGrid_VerticalOptionsFill()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalFill);
		App.Tap(VerticalFill);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideGridButton);
		App.Tap(LayoutInsideGridButton);
		VerifyScreenshot();
	}

	[Test, Order(45)]
	[Category(UITestCategories.Layout)]
	public void VerifyNestedLayout_VerticalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalStart);
		App.Tap(VerticalStart);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(NestedLayoutButton);
		App.Tap(NestedLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(46)]
	[Category(UITestCategories.Layout)]
	public void VerifyNestedLayout_VerticalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalCenter);
		App.Tap(VerticalCenter);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(NestedLayoutButton);
		App.Tap(NestedLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(47)]
	[Category(UITestCategories.Layout)]
	public void VerifyNestedLayout_VerticalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalEnd);
		App.Tap(VerticalEnd);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(NestedLayoutButton);
		App.Tap(NestedLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(48)]
	[Category(UITestCategories.Layout)]
	public void VerifyNestedLayout_VerticalOptionsFill()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalFill);
		App.Tap(VerticalFill);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(NestedLayoutButton);
		App.Tap(NestedLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(49)]
	[Category(UITestCategories.Layout)]
	public void VerifyVerticalStackLayout_VerticalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalStart);
		App.Tap(VerticalStart);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(VerticalStackLayoutButton);
		App.Tap(VerticalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(50)]
	[Category(UITestCategories.Layout)]
	public void VerifyVerticalStackLayout_VerticalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalCenter);
		App.Tap(VerticalCenter);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(VerticalStackLayoutButton);
		App.Tap(VerticalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(51)]
	[Category(UITestCategories.Layout)]
	public void VerifyVerticalStackLayout_VerticalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalEnd);
		App.Tap(VerticalEnd);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(VerticalStackLayoutButton);
		App.Tap(VerticalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(52)]
	[Category(UITestCategories.Layout)]
	public void VerifyVerticalStackLayout_VerticalOptionsFill()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalFill);
		App.Tap(VerticalFill);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(VerticalStackLayoutButton);
		App.Tap(VerticalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(53)]
	[Category(UITestCategories.Layout)]
	public void VerifyHorizontalStackLayout_VerticalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalStart);
		App.Tap(VerticalStart);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(HorizontalStackLayoutButton);
		App.Tap(HorizontalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(54)]
	[Category(UITestCategories.Layout)]
	public void VerifyHorizontalStackLayout_VerticalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalCenter);
		App.Tap(VerticalCenter);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(HorizontalStackLayoutButton);
		App.Tap(HorizontalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(55)]
	[Category(UITestCategories.Layout)]
	public void VerifyHorizontalStackLayout_VerticalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalEnd);
		App.Tap(VerticalEnd);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(HorizontalStackLayoutButton);
		App.Tap(HorizontalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(56)]
	[Category(UITestCategories.Layout)]
	public void VerifyHorizontalStackLayout_VerticalOptionsFill()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalFill);
		App.Tap(VerticalFill);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(HorizontalStackLayoutButton);
		App.Tap(HorizontalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(57)]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_VerticalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalStart);
		App.Tap(VerticalStart);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AbsoluteLayoutButton);
		App.Tap(AbsoluteLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(58)]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_VerticalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalCenter);
		App.Tap(VerticalCenter);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AbsoluteLayoutButton);
		App.Tap(AbsoluteLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(59)]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_VerticalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalEnd);
		App.Tap(VerticalEnd);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AbsoluteLayoutButton);
		App.Tap(AbsoluteLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(60)]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_VerticalOptionsFill()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalFill);
		App.Tap(VerticalFill);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AbsoluteLayoutButton);
		App.Tap(AbsoluteLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(61)]
	[Category(UITestCategories.Layout)]
	public void VerifyGridLayout_VerticalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalStart);
		App.Tap(VerticalStart);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridButton);
		App.Tap(GridButton);
		VerifyScreenshot();
	}

	[Test, Order(62)]
	[Category(UITestCategories.Layout)]
	public void VerifyGridLayout_VerticalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalCenter);
		App.Tap(VerticalCenter);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridButton);
		App.Tap(GridButton);
		VerifyScreenshot();
	}

	[Test, Order(63)]
	[Category(UITestCategories.Layout)]
	public void VerifyGridLayout_VerticalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalEnd);
		App.Tap(VerticalEnd);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridButton);
		App.Tap(GridButton);
		VerifyScreenshot();
	}

	[Test, Order(64)]
	[Category(UITestCategories.Layout)]
	public void VerifyGridLayout_VerticalOptionsFill()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalFill);
		App.Tap(VerticalFill);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridButton);
		App.Tap(GridButton);
		VerifyScreenshot();
	}
	
	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideContentView_HorizontalOptionsStartWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideContentViewButton);
		App.Tap(LayoutInsideContentViewButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideContentView_HorizontalOptionsCenterWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideContentViewButton);
		App.Tap(LayoutInsideContentViewButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideContentView_HorizontalOptionsEndWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideContentViewButton);
		App.Tap(LayoutInsideContentViewButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideContentView_HorizontalOptionsFillWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideContentViewButton);
		App.Tap(LayoutInsideContentViewButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGridWithContentViewLayout_HorizontalOptionsStartWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridWithContentViewButton);
		App.Tap(GridWithContentViewButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGridWithContentViewLayout_HorizontalOptionsCenterWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridWithContentViewButton);
		App.Tap(GridWithContentViewButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGridWithContentViewLayout_HorizontalOptionsEndWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridWithContentViewButton);
		App.Tap(GridWithContentViewButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGridWithContentViewLayout_HorizontalOptionsFillWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridWithContentViewButton);
		App.Tap(GridWithContentViewButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideGrid_HorizontalOptionsStartWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideGridButton);
		App.Tap(LayoutInsideGridButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideGrid_HorizontalOptionsCenterWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideGridButton);
		App.Tap(LayoutInsideGridButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideGrid_HorizontalOptionsEndWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideGridButton);
		App.Tap(LayoutInsideGridButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideGrid_HorizontalOptionsFillWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideGridButton);
		App.Tap(LayoutInsideGridButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyNestedLayout_HorizontalOptionsStartWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(NestedLayoutButton);
		App.Tap(NestedLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyNestedLayout_HorizontalOptionsCenterWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(NestedLayoutButton);
		App.Tap(NestedLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyNestedLayout_HorizontalOptionsEndWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(NestedLayoutButton);
		App.Tap(NestedLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyNestedLayout_HorizontalOptionsFillWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(NestedLayoutButton);
		App.Tap(NestedLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyVerticalStackLayout_HorizontalOptionsStartWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(VerticalStackLayoutButton);
		App.Tap(VerticalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyVerticalStackLayout_HorizontalOptionsCenterWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(VerticalStackLayoutButton);
		App.Tap(VerticalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyVerticalStackLayout_HorizontalOptionsEndWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(VerticalStackLayoutButton);
		App.Tap(VerticalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyVerticalStackLayout_HorizontalOptionsFillWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(VerticalStackLayoutButton);
		App.Tap(VerticalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyHorizontalStackLayout_HorizontalOptionsStartWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(HorizontalStackLayoutButton);
		App.Tap(HorizontalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyHorizontalStackLayout_HorizontalOptionsCenterWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(HorizontalStackLayoutButton);
		App.Tap(HorizontalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyHorizontalStackLayout_HorizontalOptionsEndWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(HorizontalStackLayoutButton);
		App.Tap(HorizontalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyHorizontalStackLayout_HorizontalOptionsFillWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(HorizontalStackLayoutButton);
		App.Tap(HorizontalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_HorizontalOptionsStartWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AbsoluteLayoutButton);
		App.Tap(AbsoluteLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_HorizontalOptionsCenterWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AbsoluteLayoutButton);
		App.Tap(AbsoluteLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_HorizontalOptionsEndWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AbsoluteLayoutButton);
		App.Tap(AbsoluteLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_HorizontalOptionsFillWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AbsoluteLayoutButton);
		App.Tap(AbsoluteLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGridLayout_HorizontalOptionsStartWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridButton);
		App.Tap(GridButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGridLayout_HorizontalOptionsCenterWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridButton);
		App.Tap(GridButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGridLayout_HorizontalOptionsEndWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridButton);
		App.Tap(GridButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGridLayout_HorizontalOptionsFillWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(Width300);
		App.Tap(Width300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridButton);
		App.Tap(GridButton);
		VerifyScreenshot();
	}

    [Test]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideContentView_VerticalOptionsStartWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalStart);
		App.Tap(VerticalStart);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideContentViewButton);
		App.Tap(LayoutInsideContentViewButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideContentView_VerticalOptionsCenterWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalCenter);
		App.Tap(VerticalCenter);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideContentViewButton);
		App.Tap(LayoutInsideContentViewButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideContentView_VerticalOptionsEndWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalEnd);
		App.Tap(VerticalEnd);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideContentViewButton);
		App.Tap(LayoutInsideContentViewButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideContentView_VerticalOptionsFillWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalFill);
		App.Tap(VerticalFill);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideContentViewButton);
		App.Tap(LayoutInsideContentViewButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGridWithContentViewLayout_VerticalOptionsStartWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalStart);
		App.Tap(VerticalStart);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridWithContentViewButton);
		App.Tap(GridWithContentViewButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGridWithContentViewLayout_VerticalOptionsCenterWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalCenter);
		App.Tap(VerticalCenter);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridWithContentViewButton);
		App.Tap(GridWithContentViewButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGridWithContentViewLayout_VerticalOptionsEndWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalEnd);
		App.Tap(VerticalEnd);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridWithContentViewButton);
		App.Tap(GridWithContentViewButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGridWithContentViewLayout_VerticalOptionsFillWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalFill);
		App.Tap(VerticalFill);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridWithContentViewButton);
		App.Tap(GridWithContentViewButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideGrid_VerticalOptionsStartWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalStart);
		App.Tap(VerticalStart);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideGridButton);
		App.Tap(LayoutInsideGridButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideGrid_VerticalOptionsCenterWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalCenter);
		App.Tap(VerticalCenter);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideGridButton);
		App.Tap(LayoutInsideGridButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideGrid_VerticalOptionsEndWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalEnd);
		App.Tap(VerticalEnd);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideGridButton);
		App.Tap(LayoutInsideGridButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyLayoutInsideGrid_VerticalOptionsFillWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalFill);
		App.Tap(VerticalFill);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(LayoutInsideGridButton);
		App.Tap(LayoutInsideGridButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyNestedLayout_VerticalOptionsStartWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalStart);
		App.Tap(VerticalStart);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(NestedLayoutButton);
		App.Tap(NestedLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyNestedLayout_VerticalOptionsCenterWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalCenter);
		App.Tap(VerticalCenter);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(NestedLayoutButton);
		App.Tap(NestedLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyNestedLayout_VerticalOptionsEndWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalEnd);
		App.Tap(VerticalEnd);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(NestedLayoutButton);
		App.Tap(NestedLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyNestedLayout_VerticalOptionsFillWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalFill);
		App.Tap(VerticalFill);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(NestedLayoutButton);
		App.Tap(NestedLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyVerticalStackLayout_VerticalOptionsStartWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalStart);
		App.Tap(VerticalStart);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(VerticalStackLayoutButton);
		App.Tap(VerticalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyVerticalStackLayout_VerticalOptionsCenterWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalCenter);
		App.Tap(VerticalCenter);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(VerticalStackLayoutButton);
		App.Tap(VerticalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyVerticalStackLayout_VerticalOptionsEndWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalEnd);
		App.Tap(VerticalEnd);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(VerticalStackLayoutButton);
		App.Tap(VerticalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyVerticalStackLayout_VerticalOptionsFillWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalFill);
		App.Tap(VerticalFill);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(VerticalStackLayoutButton);
		App.Tap(VerticalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyHorizontalStackLayout_VerticalOptionsStartWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalStart);
		App.Tap(VerticalStart);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(HorizontalStackLayoutButton);
		App.Tap(HorizontalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyHorizontalStackLayout_VerticalOptionsCenterWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalCenter);
		App.Tap(VerticalCenter);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(HorizontalStackLayoutButton);
		App.Tap(HorizontalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyHorizontalStackLayout_VerticalOptionsEndWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalEnd);
		App.Tap(VerticalEnd);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(HorizontalStackLayoutButton);
		App.Tap(HorizontalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyHorizontalStackLayout_VerticalOptionsFillWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalFill);
		App.Tap(VerticalFill);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(HorizontalStackLayoutButton);
		App.Tap(HorizontalStackLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_VerticalOptionsStartWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalStart);
		App.Tap(VerticalStart);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AbsoluteLayoutButton);
		App.Tap(AbsoluteLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_VerticalOptionsCenterWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalCenter);
		App.Tap(VerticalCenter);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AbsoluteLayoutButton);
		App.Tap(AbsoluteLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_VerticalOptionsEndWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalEnd);
		App.Tap(VerticalEnd);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AbsoluteLayoutButton);
		App.Tap(AbsoluteLayoutButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_VerticalOptionsFillWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalFill);
		App.Tap(VerticalFill);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AbsoluteLayoutButton);
		App.Tap(AbsoluteLayoutButton);
		VerifyScreenshot();
	}
	
	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGridLayout_VerticalOptionsStartWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalStart);
		App.Tap(VerticalStart);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridButton);
		App.Tap(GridButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGridLayout_VerticalOptionsCenterWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalCenter);
		App.Tap(VerticalCenter);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridButton);
		App.Tap(GridButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGridLayout_VerticalOptionsEndWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalEnd);
		App.Tap(VerticalEnd);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridButton);
		App.Tap(GridButton);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyGridLayout_VerticalOptionsFillWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalFill);
		App.Tap(VerticalFill);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridButton);
		App.Tap(GridButton);
		VerifyScreenshot();
	}
  
}

	 