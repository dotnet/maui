using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class GraphicsViewFeatureTests : UITest
	{
		public const string GraphicsViewFeatureMatrix = "GraphicsView Feature Matrix";

		public GraphicsViewFeatureTests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			// Setup code for GraphicsView tests
			base.FixtureSetup();
			App.NavigateToGallery(GraphicsViewFeatureMatrix);
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
			VerifyScreenshot();
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
			VerifyScreenshot();
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
			VerifyScreenshot();
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
			VerifyScreenshot();
		}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST        // See Issue : https://github.com/dotnet/maui/issues/30673                                                             
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
			VerifyScreenshot();
		}

#endif

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
			VerifyScreenshot();
		}

		#endregion

		#region Flow Direction Tests

		[Test]
		[Category(UITestCategories.GraphicsView)]
		public void GraphicsView_ChangeFlowDirection_LTR_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FlowDirectionLTR");
			App.Tap("FlowDirectionLTR");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Options");

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.GraphicsView)]
		public void GraphicsView_ChangeFlowDirection_RTL_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FlowDirectionRTL");
			App.Tap("FlowDirectionRTL");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Options");

			VerifyScreenshot();
		}

		[TestCase("Square", "LTR", TestName = "GraphicsView_SquareWithLTRText_VerifyVisualState")]
		[TestCase("Triangle", "LTR", TestName = "GraphicsView_TriangleWithLTRText_VerifyVisualState")]
		[TestCase("Ellipse", "LTR", TestName = "GraphicsView_EllipseWithLTRText_VerifyVisualState")]
		[TestCase("Line", "LTR", TestName = "GraphicsView_LineWithLTRText_VerifyVisualState")]
		[TestCase("Square", "RTL", TestName = "GraphicsView_SquareWithRTLText_VerifyVisualState")]
		[TestCase("Triangle", "RTL", TestName = "GraphicsView_TriangleWithRTLText_VerifyVisualState")]
		[TestCase("Ellipse", "RTL", TestName = "GraphicsView_EllipseWithRTLText_VerifyVisualState")]
		[TestCase("Line", "RTL", TestName = "GraphicsView_LineWithRTLText_VerifyVisualState")]
		[Category(UITestCategories.GraphicsView)]
		[Category(UITestCategories.Visual)]
		public void GraphicsView_ShapeWithFlowDirectionText_VerifyVisualState(string shapeType, string flowDirection)
		{
			App.WaitForElement("Options");
			App.Tap("Options");

			// Select the shape
			App.WaitForElement(shapeType);
			App.Tap(shapeType);

			// Select the flow direction
			string flowDirectionElement = flowDirection == "LTR" ? "FlowDirectionLTR" : "FlowDirectionRTL";
			App.WaitForElement(flowDirectionElement);
			App.Tap(flowDirectionElement);

			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Options");

			// Verify the shape type is correctly displayed
			Assert.That(App.FindElement("DrawableTypeLabel").GetText(), Is.EqualTo(shapeType));

			// Verify the GraphicsView is visible (containing the shape with flow direction text)
			var graphicsView = App.FindElement("GraphicsViewControl");
			Assert.That(graphicsView.IsDisplayed(), Is.True);

			// Visual verification through screenshot - this will show the text within shapes
			VerifyScreenshot();
		}

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
			VerifyScreenshot();
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
			App.WaitForElement("Options");

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
			App.WaitForElement("Options");

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
			App.WaitForElement("Options");

			var dimensionsText = App.FindElement("DrawableDimensionsLabel").GetText();
			Assert.That(dimensionsText, Does.Contain("Height: 180"));
			Assert.That(dimensionsText, Does.Contain("Width: 250"));
		}

		#endregion

		#region Shadow Tests

		[Test]
		[Category(UITestCategories.GraphicsView)]
		public void GraphicsView_SetShadowProperties_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ShadowInputEntry");
			App.EnterText("ShadowInputEntry", "5,5,10,0.5");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Options");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.GraphicsView)]
		public void GraphicsView_SetInvalidShadowProperties_VerifyGracefulHandling()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ShadowInputEntry");
			App.EnterText("ShadowInputEntry", "invalid,input");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Options");
			VerifyScreenshot();
		}

		#endregion


#if TEST_FAILS_ON_WINDOWS       //Note:These tests are currently disabled on Windows due to a Graphicsview automationid doesn't work.                                                                              
		
		#region Interaction Event Tests

		[Test]
		[Category(UITestCategories.GraphicsView)]
		[Category(UITestCategories.Gestures)]
		public void GraphicsView_StartInteraction_VerifyEventTriggered()
		{
			App.WaitForElement("ClearEventsButton");
			App.Tap("ClearEventsButton");
			App.WaitForElement("GraphicsViewControl");
			// Touch and hold to simulate hover
			App.TouchAndHold("GraphicsViewControl");
			
			var interactionLabel = App.FindElement("InteractionEventLabel");
			Assert.That(interactionLabel.GetText(), Does.Contain("StartInteraction"));
		}

		[Test]
		[Category(UITestCategories.GraphicsView)]
		[Category(UITestCategories.Gestures)]
		public void GraphicsView_EndInteraction_VerifyEventTriggered()
		{
			App.WaitForElement("ClearEventsButton");
			App.Tap("ClearEventsButton");
			App.WaitForElement("GraphicsViewControl");
			// Tap on the GraphicsView
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
			App.WaitForElement("Options");

			Assert.That(App.FindElement("DrawableTypeLabel").GetText(), Is.EqualTo("Triangle"));
			var dimensionsText = App.FindElement("DrawableDimensionsLabel").GetText();
			Assert.That(dimensionsText, Does.Contain("Height: 120"));
			Assert.That(dimensionsText, Does.Contain("Width: 160"));
			VerifyScreenshot();
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
		[TestCase("1000", "1000", true, TestName = "GraphicsView_SetVeryLargeDimensions_VerifyHandling")]
		[TestCase("150", "200", true, TestName = "GraphicsView_SetValidDimensions_VerifyHandling")]
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
				App.WaitForElement("Options");
#if WINDOWS
				VerifyScreenshot();
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
				
			VerifyScreenshot();
		}

#endregion
	}
}
