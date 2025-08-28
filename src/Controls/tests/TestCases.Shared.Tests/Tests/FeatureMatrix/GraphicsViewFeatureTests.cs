using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class GraphicsViewFeatureTests : UITest
{
	public const string GraphicsViewFeatureMatrix = "GraphicsView Feature Matrix";

	public GraphicsViewFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(GraphicsViewFeatureMatrix);
	}

	public void VerifyShapeScreenshot()
	{
#if WINDOWS
		VerifyScreenshot(cropTop: 100, tolerance: 0.10);
#else
		VerifyScreenshot(tolerance: 0.10);
#endif
	}

	#region Default Values and Initial State Tests

	[Test, Order(1)]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_ValidateDefaultValues_VerifyInitialState()
	{
		App.WaitForElement("Options");

		// Verify default drawable type is Square
		Assert.That(App.FindElement("DrawableTypeLabel").GetText(), Is.EqualTo("Square"));

		// Verify default dimensions are displayed
		var dimensionsText = App.FindElement("DrawableDimensionsLabel").GetText();
		Assert.That(dimensionsText, Does.Contain("Height: 100"));
		Assert.That(dimensionsText, Does.Contain("Width: 100"));

		var interactionLabel = App.FindElement("InteractionEventLabel");
		Assert.That(interactionLabel.GetText(), Is.EqualTo("No interactions yet"));
	}

	#endregion

	#region Drawable Type Tests

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_SquareDrawable_VerifyTypeAndRendering()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Square");
		App.Tap("Square");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");

		Assert.That(App.FindElement("DrawableTypeLabel").GetText(), Is.EqualTo("Square"));
		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_TriangleDrawable_VerifyTypeAndRendering()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Triangle");
		App.Tap("Triangle");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");

		Assert.That(App.FindElement("DrawableTypeLabel").GetText(), Is.EqualTo("Triangle"));
		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_EllipseDrawable_VerifyTypeAndRendering()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Ellipse");
		App.Tap("Ellipse");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");

		Assert.That(App.FindElement("DrawableTypeLabel").GetText(), Is.EqualTo("Ellipse"));
		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_LineDrawable_VerifyTypeAndRendering()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Line");
		App.Tap("Line");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");

		Assert.That(App.FindElement("DrawableTypeLabel").GetText(), Is.EqualTo("Line"));
		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_StringDrawable_VerifyTypeAndRendering()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("String");
		App.Tap("String");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");

		Assert.That(App.FindElement("DrawableTypeLabel").GetText(), Is.EqualTo("String"));
		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_ImageDrawable_VerifyTypeAndRendering()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Image");
		App.Tap("Image");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");

		Assert.That(App.FindElement("DrawableTypeLabel").GetText(), Is.EqualTo("Image"));
		VerifyShapeScreenshot();
	}

#if TEST_FAILS_ON_ANDROID  //See issue : https://github.com/dotnet/maui/issues/29394                                                            
	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_TransparentEllipseDrawable_VerifyTypeAndRendering()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TransparentEllipse");
		App.Tap("TransparentEllipse");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");

		Assert.That(App.FindElement("DrawableTypeLabel").GetText(), Is.EqualTo("TransparentEllipse"));
		
		VerifyShapeScreenshot();
	}
#endif

	#endregion

	#region IsVisible Tests

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_SetVisibilityToTrue_VerifyVisibleState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleTrueRadio");
		App.Tap("IsVisibleTrueRadio");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");
		// When IsVisible is true, the element should be visible
		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_SetVisibilityToFalse_VerifyHiddenState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleFalseRadio");
		App.Tap("IsVisibleFalseRadio");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");
		// When IsVisible is false, the element should not be visible
		App.WaitForNoElement("GraphicsViewControl");
	}

	#endregion

	#region Dimensions Tests

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_ChangeHeightRequest_VerifyDimensionsUpdate()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HeightRequestEntry");
		App.ClearText("HeightRequestEntry");
		App.EnterText("HeightRequestEntry", "150");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("DrawableTypeLabel");
		App.Tap("DrawableTypeLabel");

		var dimensionsText = App.FindElement("DrawableDimensionsLabel").GetText();
		Assert.That(dimensionsText, Does.Contain("Height: 150"));
	}

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_ChangeWidthRequest_VerifyDimensionsUpdate()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("WidthRequestEntry");
		App.ClearText("WidthRequestEntry");
		App.EnterText("WidthRequestEntry", "200");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("DrawableTypeLabel");
		App.Tap("DrawableTypeLabel");

		var dimensionsText = App.FindElement("DrawableDimensionsLabel").GetText();
		Assert.That(dimensionsText, Does.Contain("Width: 200"));
	}

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_ChangeBothDimensions_VerifyDimensionsUpdate()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HeightRequestEntry");
		App.ClearText("HeightRequestEntry");
		App.EnterText("HeightRequestEntry", "180");
		App.WaitForElement("WidthRequestEntry");
		App.ClearText("WidthRequestEntry");
		App.EnterText("WidthRequestEntry", "250");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("DrawableTypeLabel");
		App.Tap("DrawableTypeLabel");

		var dimensionsText = App.FindElement("DrawableDimensionsLabel").GetText();
		Assert.That(dimensionsText, Does.Contain("Height: 180"));
		Assert.That(dimensionsText, Does.Contain("Width: 250"));
	}

	#endregion

	#region Shadow Tests
#if TEST_FAILS_ON_WINDOWS
	// Note: Shadow tests are currently disabled on Windows due to known issues with GraphicsView                                                                                                    
	//See Issue : https://github.com/dotnet/maui/issues/30778	
	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_SetShadowProperties_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Triangle");
		App.Tap("Triangle");
		App.WaitForElement("ShadowInputEntry");
		App.EnterText("ShadowInputEntry", "5,5,10,0.5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("DrawableTypeLabel");
		App.Tap("DrawableTypeLabel");
		VerifyShapeScreenshot();
	}

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_SetInvalidShadowProperties_VerifyGracefulHandling()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Triangle");
		App.Tap("Triangle");
		App.WaitForElement("ShadowInputEntry");
		App.EnterText("ShadowInputEntry", "invalid,input");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("DrawableTypeLabel");
		App.Tap("DrawableTypeLabel");
		VerifyShapeScreenshot();
	}
#endif
	#endregion


#if TEST_FAILS_ON_WINDOWS       //Note:These tests are currently disabled on Windows due to a Graphicsview automationid doesn't work.                                                                              

	#region Interaction Event Tests

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_StartInteraction_VerifyEventTriggered()
	{
		App.WaitForElement("ClearEventsButton");
		App.Tap("ClearEventsButton");
		App.WaitForElement("GraphicsViewControl");
		App.Tap("GraphicsViewControl");

		var interactionLabel = App.FindElement("InteractionEventLabel");
		Assert.That(interactionLabel.GetText(), Does.Contain("StartInteraction"));
	}

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_EndInteraction_VerifyEventTriggered()
	{
		App.WaitForElement("ClearEventsButton");
		App.Tap("ClearEventsButton");
		App.WaitForElement("GraphicsViewControl");
		App.Tap("GraphicsViewControl");

		var interactionLabel = App.FindElement("InteractionEventLabel");
		Assert.That(interactionLabel.GetText(), Does.Contain("EndInteraction"));
	}

	[Test]
	[Category(UITestCategories.GraphicsView)]
	[Category(UITestCategories.Gestures)]
	public void GraphicsView_DragInteraction_VerifyEventTriggered()
	{
		App.WaitForElement("ClearEventsButton");
		App.Tap("ClearEventsButton");
		App.WaitForElement("GraphicsViewControl");

		// Perform a longer drag gesture with multiple points to ensure DragInteraction is triggered
		var graphicsView = App.FindElement("GraphicsViewControl");
		var bounds = graphicsView.GetRect();

		var startX = (float)(bounds.X + (bounds.Width * 0.3));
		var startY = (float)(bounds.Y + (bounds.Height * 0.3));
		var endX = (float)(bounds.X + (bounds.Width * 0.7));
		var endY = (float)(bounds.Y + (bounds.Height * 0.7));

		App.DragCoordinates(startX, startY, endX, endY);

		var interactionLabel = App.FindElement("InteractionEventLabel");
		var eventText = interactionLabel.GetText();

		Assert.That(eventText, Does.Contain("DragInteraction"));
	}
	#endregion

	#region IsEnabled Tests

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_SetEnabledToTrue_VerifyEnabledState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsEnabledTrueRadio");
		App.Tap("IsEnabledTrueRadio");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");

		var graphicsView = App.FindElement("GraphicsViewControl");
		Assert.That(graphicsView.IsEnabled(), Is.True);
	}

#if TEST_FAILS_ON_MACCATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_ANDROID
//See Issue : https://github.com/dotnet/maui/issues/30649
		[Test]
		[Category(UITestCategories.GraphicsView)]
		public void GraphicsView_SetEnabledToFalse_VerifyDisabledState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsEnabledFalseRadio");
			App.Tap("IsEnabledFalseRadio");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Options");

			var graphicsView = App.FindElement("GraphicsViewControl");
			Assert.That(graphicsView.IsEnabled(), Is.False);
		}
#endif
	#endregion
#endif

	#region Combined Feature Tests

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_TriangleWithCustomDimensions_VerifyState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Triangle");
		App.Tap("Triangle");
		App.WaitForElement("HeightRequestEntry");
		App.ClearText("HeightRequestEntry");
		App.EnterText("HeightRequestEntry", "120");
		App.WaitForElement("WidthRequestEntry");
		App.ClearText("WidthRequestEntry");
		App.EnterText("WidthRequestEntry", "160");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("DrawableTypeLabel");
		App.Tap("DrawableTypeLabel");

		Assert.That(App.FindElement("DrawableTypeLabel").GetText(), Is.EqualTo("Triangle"));
		var dimensionsText = App.FindElement("DrawableDimensionsLabel").GetText();
		Assert.That(dimensionsText, Does.Contain("Height: 120"));
		Assert.That(dimensionsText, Does.Contain("Width: 160"));
		VerifyShapeScreenshot();
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS
// This test fails on Android, iOS, and MacCatalyst. See issue: https://github.com/dotnet/maui/issues/30649
// Note: The test is also disabled on Windows due to the GraphicsView AutomationId not functioning correctly.

		[Test]
		[Category(UITestCategories.GraphicsView)]
		public void GraphicsView_DisabledSquareWithInteraction_VerifyNoInteraction()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("Square");
			App.Tap("Square");
			App.WaitForElement("IsEnabledFalseRadio");
			App.Tap("IsEnabledFalseRadio");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Options");
			
			var graphicsView = App.FindElement("GraphicsViewControl");
			Assert.That(graphicsView.IsEnabled(), Is.False);
			
			// Try to interact with disabled GraphicsView
			App.Tap("GraphicsViewControl");
			
			// Interaction events should not be triggered when disabled
			var interactionLabel = App.FindElement("InteractionEventLabel");
			Assert.That(interactionLabel.GetText(), Is.EqualTo("No interactions yet"));
		}
#endif
	#endregion

	#region Edge Cases and Error Handling

	[TestCase("0", "0", false, TestName = "GraphicsView_SetZeroDimensions_VerifyHandling")]
	[TestCase("-10", "-20", false, TestName = "GraphicsView_SetNegativeDimensions_VerifyHandling")]
	[TestCase("0.5", "0.5", true, TestName = "GraphicsView_SetDecimalDimensions_VerifyHandling")]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_SetDimensionsEdgeCases_VerifyHandling(string height, string width, bool shouldBeVisible)
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HeightRequestEntry");
		App.ClearText("HeightRequestEntry");
		App.EnterText("HeightRequestEntry", height);
		App.WaitForElement("WidthRequestEntry");
		App.ClearText("WidthRequestEntry");
		App.EnterText("WidthRequestEntry", width);
		App.WaitForElement("Apply");
		App.Tap("Apply");

		if (shouldBeVisible)
		{
			App.WaitForElement("DrawableTypeLabel");
			App.Tap("DrawableTypeLabel");
#if WINDOWS
			VerifyShapeScreenshot();
#else
			var graphicsView = App.FindElement("GraphicsViewControl");
			Assert.That(graphicsView, Is.Not.Null);
#endif
		}
		else
		{
			App.WaitForElement("Options");
			App.WaitForNoElement("GraphicsViewControl");
		}
	}

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsView_InvalidateButton_ChangesColorAndLogsEvent()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Square");
		App.Tap("Square");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		// Clear any existing events to start fresh
		App.WaitForElement("ClearEventsButton");
		App.Tap("ClearEventsButton");

		var interactionEventLabel = App.FindElement("InteractionEventLabel");
		var initialEventsText = interactionEventLabel.GetText();

		App.WaitForElement("InvalidateButton");
		App.Tap("InvalidateButton");

		var updatedEventsText = interactionEventLabel.GetText();

		Assert.That(updatedEventsText, Is.Not.EqualTo(initialEventsText),
			"Interaction events should be updated after clicking Invalidate button");

		Assert.That(updatedEventsText, Does.Contain("Invalidate() called"),
			"Events should indicate that Invalidate() was called on the GraphicsView");

		VerifyShapeScreenshot();
	}

	#endregion
}
