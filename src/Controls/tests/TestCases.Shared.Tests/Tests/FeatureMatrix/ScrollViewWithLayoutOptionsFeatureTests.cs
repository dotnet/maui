using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class ScrollViewWithLayoutOptionsFeatureTests : UITest
{
	public const string ScrollViewWithLayoutOptionsFeatureMatrix = "ScrollView With LayoutOptions Feature Matrix";
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
	public const string Width350 = "Width350";
	public const string HeightNone = "HeightNone";
	public const string Height300 = "Height300";
	public const string StackLayoutButton = "StackLayoutButton";
	public const string GridButton = "GridButton";
	public const string OrientationHorizontal = "OrientationHorizontal";

	public ScrollViewWithLayoutOptionsFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(ScrollViewWithLayoutOptionsFeatureMatrix);
	}

	[Test, Order(1)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationVertical_HorizontalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationVertical_HorizontalOptionsFill()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(3)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationVertical_HorizontalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(4)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationVertical_HorizontalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(5)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationVertical_HorizontalOptionsStartWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(Width350);
		App.Tap(Width350);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(6)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationVertical_HorizontalOptionsFillWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(Width350);
		App.Tap(Width350);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(7)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationVertical_HorizontalOptionsCenterWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(Width350);
		App.Tap(Width350);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(8)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationVertical_HorizontalOptionsEndWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(Width350);
		App.Tap(Width350);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(9)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationVertical_VerticalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalStart);
		App.Tap(VerticalStart);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(10)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationVertical_VerticalOptionsFill()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalFill);
		App.Tap(VerticalFill);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(11)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationVertical_VerticalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalCenter);
		App.Tap(VerticalCenter);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(12)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationVertical_VerticalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalEnd);
		App.Tap(VerticalEnd);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(13)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationVertical_VerticalOptionsStartWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalStart);
		App.Tap(VerticalStart);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(14)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationVertical_VerticalOptionsFillWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalFill);
		App.Tap(VerticalFill);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(15)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationVertical_VerticalOptionsCenterWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalCenter);
		App.Tap(VerticalCenter);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(16)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationVertical_VerticalOptionsEndWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalEnd);
		App.Tap(VerticalEnd);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(17)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationHorizontal_HorizontalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(18)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationHorizontal_HorizontalOptionsFill()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(19)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationHorizontal_HorizontalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(20)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationHorizontal_HorizontalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(21)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationHorizontal_HorizontalOptionsStartWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Width350);
		App.Tap(Width350);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(22)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationHorizontal_HorizontalOptionsFillWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Width350);
		App.Tap(Width350);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(23)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationHorizontal_HorizontalOptionsCenterWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Width350);
		App.Tap(Width350);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(24)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationHorizontal_HorizontalOptionsEndWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Width350);
		App.Tap(Width350);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(25)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationHorizontal_VerticalOptionsStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalStart);
		App.Tap(VerticalStart);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(26)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationHorizontal_VerticalOptionsFill()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalFill);
		App.Tap(VerticalFill);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(27)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationHorizontal_VerticalOptionsCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalCenter);
		App.Tap(VerticalCenter);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(28)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationHorizontal_VerticalOptionsEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalEnd);
		App.Tap(VerticalEnd);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(29)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationHorizontal_VerticalOptionsStartWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalStart);
		App.Tap(VerticalStart);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(30)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationHorizontal_VerticalOptionsFillWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalFill);
		App.Tap(VerticalFill);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(31)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationHorizontal_VerticalOptionsCenterWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalCenter);
		App.Tap(VerticalCenter);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(32)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyLayoutWithOrientationHorizontal_VerticalOptionsEndWithHeightRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(VerticalEnd);
		App.Tap(VerticalEnd);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Height300);
		App.Tap(Height300);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(StackLayoutButton);
		App.Tap(StackLayoutButton);
		VerifyScreenshot();
	}

	[Test, Order(33)]
	[Category(UITestCategories.ScrollView)]
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

	[Test, Order(34)]
	[Category(UITestCategories.ScrollView)]
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

	[Test, Order(35)]
	[Category(UITestCategories.ScrollView)]
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

	[Test, Order(36)]
	[Category(UITestCategories.ScrollView)]
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

	[Test, Order(37)]
	[Category(UITestCategories.ScrollView)]
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

	[Test, Order(38)]
	[Category(UITestCategories.ScrollView)]
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

	[Test, Order(39)]
	[Category(UITestCategories.ScrollView)]
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

	[Test, Order(40)]
	[Category(UITestCategories.ScrollView)]
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

	[Test, Order(41)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyGridLayout_HorizontalOptionsStartWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalStart);
		App.Tap(HorizontalStart);
		App.WaitForElement(Width350);
		App.Tap(Width350);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridButton);
		App.Tap(GridButton);
		VerifyScreenshot();
	}

	[Test, Order(42)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyGridLayout_HorizontalOptionsCenterWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalCenter);
		App.Tap(HorizontalCenter);
		App.WaitForElement(Width350);
		App.Tap(Width350);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridButton);
		App.Tap(GridButton);
		VerifyScreenshot();
	}

	[Test, Order(43)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyGridLayout_HorizontalOptionsEndWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalEnd);
		App.Tap(HorizontalEnd);
		App.WaitForElement(Width350);
		App.Tap(Width350);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridButton);
		App.Tap(GridButton);
		VerifyScreenshot();
	}

	[Test, Order(44)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyGridLayout_HorizontalOptionsFillWithWidthRequest()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalFill);
		App.Tap(HorizontalFill);
		App.WaitForElement(Width350);
		App.Tap(Width350);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(GridButton);
		App.Tap(GridButton);
		VerifyScreenshot();
	}

	[Test, Order(45)]
	[Category(UITestCategories.ScrollView)]
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

	[Test, Order(46)]
	[Category(UITestCategories.ScrollView)]
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

	[Test, Order(47)]
	[Category(UITestCategories.ScrollView)]
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

	[Test, Order(48)]
	[Category(UITestCategories.ScrollView)]
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