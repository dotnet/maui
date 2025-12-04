#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // This test will fail on Windows due to lack of Map control support, and on Android unless a valid API key is configured for Maps.

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Maps)]
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

		var value = App.WaitForElement("VisibleRegionLatitudeDegrees").GetText();
		var rect = App.WaitForElement("MapView").GetRect();
		App.DragCoordinates(rect.CenterX(), rect.CenterY(), rect.CenterX(), rect.CenterY() - 100);

		Thread.Sleep(2000);
		Assert.That(App.WaitForElement("VisibleRegionLatitudeDegrees").GetText(), Is.Not.EqualTo(value));
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

		var value = App.WaitForElement("VisibleRegionLatitudeDegrees").GetText();
		var rect = App.WaitForElement("MapView").GetRect();
		App.DragCoordinates(rect.CenterX() - 100, rect.CenterY(), rect.CenterX(), rect.CenterY() - 100);

		Thread.Sleep(2000);
		Assert.That(App.WaitForElement("VisibleRegionLatitudeDegrees").GetText(), Is.Not.EqualTo(value));
	}

	[Test]
	public void Map_IsEnabledScroll_Street_AndElements_PolyLine()
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

		var value = App.WaitForElement("VisibleRegionLatitudeDegrees").GetText();
		var rect = App.WaitForElement("MapView").GetRect();
		App.DragCoordinates(rect.CenterX(), rect.CenterY(), rect.CenterX(), rect.CenterY() - 100);

		Thread.Sleep(2000);
		Assert.That(App.WaitForElement("VisibleRegionLatitudeDegrees").GetText(), Is.Not.EqualTo(value));
	}

	[Test]
	public void Map_IsEnabledScroll_Street_AndElements_Circle()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		App.WaitForElement("IsScrollEnabledCheckBox");
		App.Tap("IsScrollEnabledCheckBox");

		App.WaitForElement("CircleRadioButton");
		App.Tap("CircleRadioButton");

		for (int i = 0; i < 7; i++)
		{
			App.WaitForElement("AddElementButton");
			App.Tap("AddElementButton");
		}

		App.WaitForElement("Apply");
		App.Tap("Apply");

		var value = App.WaitForElement("VisibleRegionLatitudeDegrees").GetText();
		var rect = App.WaitForElement("MapView").GetRect();
		App.DragCoordinates(rect.CenterX(), rect.CenterY(), rect.CenterX(), rect.CenterY() - 100);

		Thread.Sleep(2000);
		Assert.That(App.WaitForElement("VisibleRegionLatitudeDegrees").GetText(), Is.Not.EqualTo(value));
	}

	[Test]
	public void Map_IsEnabledScroll_Street_AndElements_Polygon()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		App.WaitForElement("IsScrollEnabledCheckBox");
		App.Tap("IsScrollEnabledCheckBox");

		App.WaitForElement("PolygonRadioButton");
		App.Tap("PolygonRadioButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		var value = App.WaitForElement("VisibleRegionLatitudeDegrees").GetText();
		var rect = App.WaitForElement("MapView").GetRect();
		App.DragCoordinates(rect.CenterX(), rect.CenterY(), rect.CenterX(), rect.CenterY() - 100);

		Thread.Sleep(2000);
		Assert.That(App.WaitForElement("VisibleRegionLatitudeDegrees").GetText(), Is.Not.EqualTo(value));
	}

	[Test]
	public void Map_MapClicked()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		App.WaitForElement("MapView");
		App.TapCoordinates(300, 300);

		Assert.That(App.WaitForElement("MapClickedLabel").GetText(), Is.Not.EqualTo("Not Clicked"));
	}

	[Test]
	public void Map_IsEnabledScroll_Hybrid_WithItemTemplate()
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

		App.WaitForElement("SetItemsSourceButton");
		App.Tap("SetItemsSourceButton");

		App.WaitForElement("SetItemTemplateButton");
		App.Tap("SetItemTemplateButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		var value = App.WaitForElement("VisibleRegionLatitudeDegrees").GetText();
		var rect = App.WaitForElement("MapView").GetRect();
		App.DragCoordinates(rect.CenterX() - 100, rect.CenterY(), rect.CenterX(), rect.CenterY() - 100);

		Thread.Sleep(2000);
		Assert.That(App.WaitForElement("VisibleRegionLatitudeDegrees").GetText(), Is.Not.EqualTo(value));
	}

	[Test]
	public void Map_IsZoomDisabled_PreventZoom()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		// Ensure zoom is disabled (default state)
		App.WaitForElement("Apply");
		App.Tap("Apply");

		var initialValue = App.WaitForElement("VisibleRegionLatitudeDegrees").GetText();

		// Try to zoom (should not work when disabled)
		var rect = App.WaitForElement("MapView").GetRect();
		App.PinchToZoomIn("MapView");

		Thread.Sleep(1000);

		// Verify zoom level hasn't changed significantly
		var finalValue = App.WaitForElement("VisibleRegionLatitudeDegrees").GetText();
		Assert.That(finalValue, Is.EqualTo(initialValue));
	}

	[Test]
	public void Map_IsScrollDisabled_PreventScroll()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		// Leave scroll disabled (default state)
		App.WaitForElement("Apply");
		App.Tap("Apply");

		var initialValue = App.WaitForElement("VisibleRegionLatitudeDegrees").GetText();
		var rect = App.WaitForElement("MapView").GetRect();

		// Try to scroll (should not work when disabled)
		App.DragCoordinates(rect.CenterX(), rect.CenterY(), rect.CenterX(), rect.CenterY() - 100);

		Thread.Sleep(2000);

		// Verify region hasn't changed
		Assert.That(App.WaitForElement("VisibleRegionLatitudeDegrees").GetText(), Is.EqualTo(initialValue));
	}

	[Test]
	public void Map_ZoomIn_ChangesVisibleRegion()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		var initialValue = App.WaitForElement("VisibleRegionLatitudeDegrees").GetText();

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("IsZoomEnabledCheckBox");
		App.Tap("IsZoomEnabledCheckBox");

		// Zoom in
		App.WaitForElement("ZoomInButton");
		App.Tap("ZoomInButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		Thread.Sleep(1000);

		// Verify region changed after zoom in (latitude degrees should be smaller)
		var finalValue = App.WaitForElement("VisibleRegionLatitudeDegrees").GetText();
		Assert.That(finalValue, Is.Not.EqualTo(initialValue));
	}

	[Test]
	public void Map_ZoomOut_ChangesVisibleRegion()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ResetToInitialButton");
		App.Tap("ResetToInitialButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		var initialValue = App.WaitForElement("VisibleRegionLatitudeDegrees").GetText();

		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("IsZoomEnabledCheckBox");
		App.Tap("IsZoomEnabledCheckBox");

		// Zoom out
		App.WaitForElement("ZoomOutButton");
		App.Tap("ZoomOutButton");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		Thread.Sleep(1000);

		// Verify region changed after zoom out (latitude degrees should be larger)
		var finalValue = App.WaitForElement("VisibleRegionLatitudeDegrees").GetText();
		Assert.That(finalValue, Is.Not.EqualTo(initialValue));
	}
}
#endif