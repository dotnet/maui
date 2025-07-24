using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Layout)]
public class MapFeatureTests : UITest
{
	public const string MapFeatureMatrix = "Map Feature Matrix";

	public MapFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(MapFeatureMatrix);
	}

	[Test]
	public void Map_IsVisible()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		App.WaitForElement("IsVisibleCheckBox");
		App.Tap("IsVisibleCheckBox");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void Map_IsShowingUser_WithSatellite()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		App.WaitForElement("IsShowingUserCheckBox");
		App.Tap("IsShowingUserCheckBox");

		App.WaitForElement("SatelliteRadioButton");
		App.Tap("SatelliteRadioButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void Map_IsShowingUser_WithElements()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		App.WaitForElement("IsShowingUserCheckBox");
		App.Tap("IsShowingUserCheckBox");

		for (int i = 0; i < 5; i++)
		{
			App.WaitForElement("AddElementButton");
			App.Tap("AddElementButton");
		}

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void Map_IsShowingUser_WithPins()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		for (int i = 0; i < 5; i++)
		{
			App.WaitForElement("AddPinButton");
			App.Tap("AddPinButton");
		}

		App.WaitForElement("IsShowingUserCheckBox");
		App.Tap("IsShowingUserCheckBox");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void Map_IsShowingUser_WithZoomIn()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		App.WaitForElement("IsShowingUserCheckBox");
		App.Tap("IsShowingUserCheckBox");

		App.WaitForElement("IsZoomEnabledCheckBox");
		App.Tap("IsZoomEnabledCheckBox");

		App.WaitForElement("ZoomInButton");
		App.Tap("ZoomInButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		// Thread.Sleep(2000);

		VerifyScreenshot();
	}

	[Test]
	public void Map_IsEnabledScroll_WithZoomOut()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		App.WaitForElement("IsScrollEnabledCheckBox");
		App.Tap("IsScrollEnabledCheckBox");

		App.WaitForElement("IsZoomEnabledCheckBox");
		App.Tap("IsZoomEnabledCheckBox");

		App.WaitForElement("ZoomOutButton");
		App.Tap("ZoomOutButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		App.WaitForElement("MapView");
		App.ScrollUp("MapView");

		VerifyScreenshot();
	}

	[Test]
	public void Map_IsEnabledScroll_Hybrid_AndPins()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		App.WaitForElement("IsScrollEnabledCheckBox");
		App.Tap("IsScrollEnabledCheckBox");

		App.WaitForElement("HybridRadioButton");
		App.Tap("HybridRadioButton");

		for (int i = 0; i < 7; i++)
		{
			App.WaitForElement("AddPinButton");
			App.Tap("AddPinButton");
		}

		App.WaitForElement("Apply");
		App.Tap("Apply");

		App.WaitForElement("MapView");
		App.ScrollLeft("MapView");

		VerifyScreenshot();
	}

	[Test]
	public void Map_IsEnabledScroll_Street_AndElements()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		App.WaitForElement("IsScrollEnabledCheckBox");
		App.Tap("IsScrollEnabledCheckBox");

		for (int i = 0; i < 7; i++)
		{
			App.WaitForElement("AddElementButton");
			App.Tap("AddElementButton");
		}

		App.WaitForElement("Apply");
		App.Tap("Apply");

		App.WaitForElement("MapView");
		App.ScrollUp("MapView");

		VerifyScreenshot();
	}

	[Test]
	public void Map_IsZoomEnabled_Satellite_AndPins()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		App.WaitForElement("IsZoomEnabledCheckBox");
		App.Tap("IsZoomEnabledCheckBox");

		App.WaitForElement("ZoomOutButton");
		App.Tap("ZoomOutButton");

		App.WaitForElement("SatelliteRadioButton");
		App.Tap("SatelliteRadioButton");

		for (int i = 0; i < 5; i++)
		{
			App.WaitForElement("AddPinButton");
			App.Tap("AddPinButton");
		}

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void Map_IsZoomEnabled_Street_AndElements()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		App.WaitForElement("IsZoomEnabledCheckBox");
		App.Tap("IsZoomEnabledCheckBox");

		App.WaitForElement("ZoomInButton");
		App.Tap("ZoomInButton");

		for (int i = 0; i < 5; i++)
		{
			App.WaitForElement("AddElementButton");
			App.Tap("AddElementButton");
		}

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void Map_Elements_WithPins_AndZoomOut()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		App.WaitForElement("IsZoomEnabledCheckBox");
		App.Tap("IsZoomEnabledCheckBox");

		App.WaitForElement("ZoomOutButton");
		App.Tap("ZoomOutButton");

		for (int i = 0; i < 10; i++)
		{
			App.WaitForElement("AddPinButton");
			App.Tap("AddPinButton");
		}

		for (int i = 0; i < 9; i++)
		{
			App.WaitForElement("AddElementButton");
			App.Tap("AddElementButton");
		}

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void Map_Elements_WithPins_HybridMapType()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		App.WaitForElement("HybridRadioButton");
		App.Tap("HybridRadioButton");

		for (int i = 0; i < 10; i++)
		{
			App.WaitForElement("AddPinButton");
			App.Tap("AddPinButton");
		}

		for (int i = 0; i < 9; i++)
		{
			App.WaitForElement("AddElementButton");
			App.Tap("AddElementButton");
		}

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void Map_Pins_WithSatelliteMapType()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		App.WaitForElement("SatelliteRadioButton");
		App.Tap("SatelliteRadioButton");

		for (int i = 0; i < 6; i++)
		{
			App.WaitForElement("AddPinButton");
			App.Tap("AddPinButton");
		}

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void Map_SatelliteMapType_WithZoomOut()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		App.WaitForElement("IsZoomEnabledCheckBox");
		App.Tap("IsZoomEnabledCheckBox");

		for (int i = 0; i < 5; i++)
		{
			App.WaitForElement("ZoomOutButton");
			App.Tap("ZoomOutButton");
		}

		App.WaitForElement("SatelliteRadioButton");
		App.Tap("SatelliteRadioButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void Map_Pins_WithVisibleRegion()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		for (int i = 0; i < 10; i++)
		{
			App.WaitForElement("AddPinButton");
			App.Tap("AddPinButton");
		}

		App.WaitForElement("ShowAllPinsButton");
		App.Tap("ShowAllPinsButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}
}