using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	[Category(UITestCategories.SafeAreaEdges)]
	public class SafeArea_ContentPageFeatureTests : _GalleryUITest
	{
		public const string SafeAreaFeatureMatrix = "SafeArea Feature Matrix";
		public override string GalleryPageName => SafeAreaFeatureMatrix;

		public SafeArea_ContentPageFeatureTests(TestDevice device)
			: base(device)
		{
		}

		// ──────────────────────────────────────────────
		// Uniform SafeAreaRegions via Buttons
		// ──────────────────────────────────────────────

		[Test]
		[Description("Content extends edge-to-edge behind system bars/notch")]
		public void ValidateSafeAreaEdges_None()
		{
			
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.LessThanOrEqualTo(5), "Top indicator should be at the very top (edge-to-edge)");
		}

		[Test]
		[Description("Content inset from all system UI (status bar, nav bar, notch, home indicator)")]
		public void ValidateSafeAreaEdges_All()
		{
			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "Top indicator should be inset from the top edge");
		}

		[Test]
		[Description("Content avoids system bars/notch but can extend under keyboard area")]
		public void ValidateSafeAreaEdges_Container()
		{
			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "Top indicator should be inset from the top edge for Container");
		}

		[Test]
		[Description("Content flows under system bars/notch but avoids keyboard")]
		public void ValidateSafeAreaEdges_SoftInput()
		{
			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.LessThanOrEqualTo(5), "Top indicator should be at the top (SoftInput doesn't avoid system bars)");
		}

		[Test]
		[Description("ContentPage defaults to None behavior")]
		public void ValidateSafeAreaEdges_Default()
		{
			App.WaitForElement("SafeAreaDefaultButton");
			App.Tap("SafeAreaDefaultButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Default"));
		}

		// ──────────────────────────────────────────────
		// Per-Edge Configuration (via Options)
		// ──────────────────────────────────────────────

		[Test]
		[Description("Only top avoids status bar/notch. Other edges edge-to-edge.")]
		public void ValidatePerEdge_TopContainerOnly()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("LeftNone");
			App.Tap("LeftNone");
			App.Tap("TopContainer");
			App.Tap("RightNone");
			App.Tap("BottomNone");
			App.WaitForElement("Apply");
			App.Tap("Apply");

			App.WaitForElement("SafeAreaEdgesValueLabel");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("L:None, T:Container, R:None, B:None"));

			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "Top should be inset (Container)");

			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftRect.X, Is.LessThanOrEqualTo(5), "Left should be edge-to-edge (None)");
		}

		[Test]
		[Description("Sides/top avoid system bars; bottom avoids only keyboard")]
		public void ValidatePerEdge_BottomSoftInput_SidesContainer()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("LeftContainer");
			App.Tap("LeftContainer");
			App.Tap("TopContainer");
			App.Tap("RightContainer");
			App.Tap("BottomSoftInput");
			App.WaitForElement("Apply");
			App.Tap("Apply");

			App.WaitForElement("SafeAreaEdgesValueLabel");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("L:Container, T:Container, R:Container, B:SoftInput"));

			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "Top should be inset (Container)");

			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftRect.X, Is.GreaterThan(0), "Left should be inset (Container)");
		}

		[Test]
		[Description("Top/bottom respect all insets; left/right edge-to-edge")]
		public void ValidatePerEdge_TopBottomAll_SidesNone()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("LeftNone");
			App.Tap("LeftNone");
			App.Tap("TopAll");
			App.Tap("RightNone");
			App.Tap("BottomAll");
			App.WaitForElement("Apply");
			App.Tap("Apply");

			App.WaitForElement("SafeAreaEdgesValueLabel");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("L:None, T:All, R:None, B:All"));

			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "Top should be inset (All)");

			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftRect.X, Is.LessThanOrEqualTo(5), "Left should be edge-to-edge (None)");
		}

		[Test]
		[Description("Each edge independently applies its behavior")]
		public void ValidatePerEdge_AllDifferent()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("LeftNone");
			App.Tap("LeftNone");
			App.Tap("TopContainer");
			App.Tap("RightSoftInput");
			App.Tap("BottomAll");
			App.WaitForElement("Apply");
			App.Tap("Apply");

			App.WaitForElement("SafeAreaEdgesValueLabel");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("L:None, T:Container, R:SoftInput, B:All"));
		}

		// ──────────────────────────────────────────────
		// Keyboard Interaction (SoftInput)
		// ──────────────────────────────────────────────

		[Test]
		[Description("Content shifts to avoid keyboard when SafeAreaEdges is All")]
		public void ValidateKeyboard_All()
		{
			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var entryRectBefore = App.WaitForElement("SafeAreaTestEntry").GetRect();
			App.Tap("SafeAreaTestEntry");

			var entryRectAfter = App.WaitForElement("SafeAreaTestEntry").GetRect();
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));
		}

		[Test]
		[Description("Content under system bars but above keyboard")]
		public void ValidateKeyboard_SoftInput()
		{
			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			App.WaitForElement("SafeAreaTestEntry");
			App.Tap("SafeAreaTestEntry");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));
		}

		[Test]
		[Description("Keyboard overlaps content, no adjustment")]
		public void ValidateKeyboard_None()
		{
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			App.WaitForElement("SafeAreaTestEntry");
			App.Tap("SafeAreaTestEntry");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));
		}

		[Test]
		[Description("System bars avoided, keyboard may overlap")]
		public void ValidateKeyboard_Container()
		{
			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			App.WaitForElement("SafeAreaTestEntry");
			App.Tap("SafeAreaTestEntry");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));
		}

		[Test]
		[Description("Entry at bottom shifts above keyboard with per-edge SoftInput on bottom")]
		public void ValidateKeyboard_BottomSoftInput()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("LeftContainer");
			App.Tap("LeftContainer");
			App.Tap("TopContainer");
			App.Tap("RightContainer");
			App.Tap("BottomSoftInput");
			App.WaitForElement("Apply");
			App.Tap("Apply");

			App.WaitForElement("SafeAreaTestEntry");
			App.Tap("SafeAreaTestEntry");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("L:Container, T:Container, R:Container, B:SoftInput"));
		}

		// ──────────────────────────────────────────────
		// Interaction with ContentPage Properties
		// ──────────────────────────────────────────────

		[Test]
		[Description("Safe area insets and padding are additive")]
		public void ValidateSafeArea_WithPadding()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("UniformAll");
			App.Tap("UniformAll");
			App.Tap("PaddingCheckBox");
			App.WaitForElement("Apply");
			App.Tap("Apply");

			App.WaitForElement("SafeAreaEdgesValueLabel");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(20), "Top indicator should be inset by safe area + padding");
		}

		[Test]
		[Description("Background extends edge-to-edge behind system UI")]
		public void ValidateSafeArea_None_WithBackground()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("UniformNone");
			App.Tap("UniformNone");
			App.Tap("BackgroundCheckBox");
			App.WaitForElement("Apply");
			App.Tap("Apply");

			App.WaitForElement("SafeAreaEdgesValueLabel");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));
		}

		// ──────────────────────────────────────────────
		// Dynamic Runtime Changes via Buttons
		// ──────────────────────────────────────────────

		[Test]
		[Description("Content shifts from edge-to-edge to inset using runtime buttons")]
		public void ValidateDynamic_NoneToAll()
		{
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));
			var topRectBefore = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRectBefore.Y, Is.LessThanOrEqualTo(5), "Before: top should be edge-to-edge");

			App.Tap("SafeAreaAllButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));
			var topRectAfter = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRectAfter.Y, Is.GreaterThan(5), "After: top should be inset");
			Assert.That(topRectAfter.Y, Is.GreaterThan(topRectBefore.Y), "Top indicator should have moved down");
		}

		[Test]
		[Description("Content expands to edge-to-edge using runtime buttons")]
		public void ValidateDynamic_AllToNone()
		{
			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));
			var topRectBefore = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRectBefore.Y, Is.GreaterThan(5), "Before: top should be inset");

			App.Tap("SafeAreaNoneButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));
			var topRectAfter = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRectAfter.Y, Is.LessThanOrEqualTo(5), "After: top should be edge-to-edge");
			Assert.That(topRectAfter.Y, Is.LessThan(topRectBefore.Y), "Top indicator should have moved up");
		}

		[Test]
		[Description("Behavior transitions correctly from Container to SoftInput")]
		public void ValidateDynamic_ContainerToSoftInput()
		{
			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));
			var topRectBefore = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRectBefore.Y, Is.GreaterThan(5), "Container: top should be inset");

			App.Tap("SafeAreaSoftInputButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));
			var topRectAfter = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRectAfter.Y, Is.LessThanOrEqualTo(5), "SoftInput: top should be edge-to-edge");
		}

		[Test]
		[Description("Cycle through all values: None, All, Container, SoftInput, Default")]
		public void ValidateDynamic_CycleThroughAll()
		{
			App.WaitForElement("SafeAreaNoneButton");

			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			App.Tap("SafeAreaContainerButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			App.Tap("SafeAreaDefaultButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Default"));
		}

		[Test]
		[Description("Per-edge layout updates correctly at runtime via Options")]
		public void ValidateDynamic_PerEdgeChange()
		{
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));
			var topRectBefore = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRectBefore.Y, Is.LessThanOrEqualTo(5), "Before: all edges None");

			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TopContainer");
			App.Tap("TopContainer");
			App.Tap("BottomContainer");
			App.WaitForElement("Apply");
			App.Tap("Apply");

			App.WaitForElement("SafeAreaEdgesValueLabel");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("L:None, T:Container, R:None, B:Container"));
			var topRectAfter = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRectAfter.Y, Is.GreaterThan(5), "After: top should be inset (Container)");
		}

		// ──────────────────────────────────────────────
		// Platform-Specific Behavior
		// ──────────────────────────────────────────────

#if IOS
		[Test]
		[Description("Container/All avoids notch/Dynamic Island on iOS")]
		public void ValidateiOS_NotchAvoidance()
		{
			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(20), "Container should avoid notch/Dynamic Island on iOS");
		}

		[Test]
		[Description("Container/All avoids home indicator on iOS")]
		public void ValidateiOS_HomeIndicatorAvoidance()
		{
			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(20), "All should avoid notch/home indicator on iOS");
		}
#endif

#if ANDROID
		[Test]
		[Description("Container/All avoids Android status bar")]
		public void ValidateAndroid_StatusBarAvoidance()
		{
			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "Container should avoid status bar on Android");
		}

		[Test]
		[Description("Container/All avoids navigation bar on Android")]
		public void ValidateAndroid_NavBarAvoidance()
		{
			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "All should avoid status bar on Android");
		}

		[Test]
		[Description("ContentPage defaults to None (edge-to-edge) in .NET 10")]
		public void ValidateAndroid_DefaultIsNone()
		{
			App.WaitForElement("SafeAreaEdgesValueLabel");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.LessThanOrEqualTo(5), "Default (None) should be edge-to-edge on Android");
		}
#endif

		// ──────────────────────────────────────────────
		// Orientation / Landscape Validation
		// ──────────────────────────────────────────────

		[Test]
		[Description("None: portrait top/bottom and landscape left/right are all edge-to-edge")]
		public void ValidateOrientation_None_AllEdges()
		{
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			// Portrait: top and bottom
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.LessThanOrEqualTo(5), "Portrait: top should be edge-to-edge");
			App.WaitForElement("BottomEdgeIndicator");

			// Landscape: left and right
			App.SetOrientationLandscape();
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));
			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftRect.X, Is.LessThanOrEqualTo(5), "Landscape: left should be edge-to-edge");
			App.WaitForElement("RightEdgeIndicator");

			App.SetOrientationPortrait();
		}

		[Test]
		[Description("All: portrait top/bottom and landscape left/right are all inset")]
		public void ValidateOrientation_All_AllEdges()
		{
			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			// Portrait: top and bottom
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "Portrait: top should be inset");
			App.WaitForElement("BottomEdgeIndicator");

			// Landscape: left and right
			App.SetOrientationLandscape();
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));
			App.WaitForElement("LeftEdgeIndicator");
			App.WaitForElement("RightEdgeIndicator");

			App.SetOrientationPortrait();
		}

		[Test]
		[Description("Container: portrait top/bottom and landscape left/right respect system bars")]
		public void ValidateOrientation_Container_AllEdges()
		{
			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			// Portrait: top and bottom
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "Portrait: top should be inset");
			App.WaitForElement("BottomEdgeIndicator");

			// Landscape: left and right
			App.SetOrientationLandscape();
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));
			App.WaitForElement("LeftEdgeIndicator");
			App.WaitForElement("RightEdgeIndicator");

			App.SetOrientationPortrait();
		}

		[Test]
		[Description("SoftInput: portrait top/bottom and landscape left/right are edge-to-edge for system bars")]
		public void ValidateOrientation_SoftInput_AllEdges()
		{
			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			// Portrait: top and bottom
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.LessThanOrEqualTo(5), "Portrait: top should be edge-to-edge");
			App.WaitForElement("BottomEdgeIndicator");

			// Landscape: left and right
			App.SetOrientationLandscape();
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));
			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftRect.X, Is.LessThanOrEqualTo(5), "Landscape: left should be edge-to-edge");
			App.WaitForElement("RightEdgeIndicator");

			App.SetOrientationPortrait();
		}

		[Test]
		[Description("Switching None to All in portrait validates all 4 edges shift inward")]
		public void ValidateOrientation_Portrait_NoneVsAll_EdgeComparison()
		{
			// Capture all 4 edge positions with None
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			var topNone = App.WaitForElement("TopEdgeIndicator").GetRect();
			var bottomNone = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var leftNone = App.WaitForElement("LeftEdgeIndicator").GetRect();
			var rightNone = App.WaitForElement("RightEdgeIndicator").GetRect();

			// Switch to All and capture again
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var topAll = App.WaitForElement("TopEdgeIndicator").GetRect();
			var bottomAll = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var leftAll = App.WaitForElement("LeftEdgeIndicator").GetRect();
			var rightAll = App.WaitForElement("RightEdgeIndicator").GetRect();

			// Top and left edges should move inward (greater values)
			Assert.That(topAll.Y, Is.GreaterThanOrEqualTo(topNone.Y), "Portrait: All top inset >= None top");
			Assert.That(leftAll.X, Is.GreaterThanOrEqualTo(leftNone.X), "Portrait: All left inset >= None left");

			// Bottom and right edges should move inward (smaller end positions)
			var bottomEdgeNone = bottomNone.Y + bottomNone.Height;
			var bottomEdgeAll = bottomAll.Y + bottomAll.Height;
			Assert.That(bottomEdgeAll, Is.LessThanOrEqualTo(bottomEdgeNone), "Portrait: All bottom edge <= None bottom edge");

			var rightEdgeNone = rightNone.X + rightNone.Width;
			var rightEdgeAll = rightAll.X + rightAll.Width;
			Assert.That(rightEdgeAll, Is.LessThanOrEqualTo(rightEdgeNone), "Portrait: All right edge <= None right edge");
		}

		[Test]
		[Description("Switching None to All in landscape validates all 4 edges shift inward")]
		public void ValidateOrientation_Landscape_NoneVsAll_EdgeComparison()
		{
			// Set None and rotate to landscape
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			App.SetOrientationLandscape();

			var topNone = App.WaitForElement("TopEdgeIndicator").GetRect();
			var bottomNone = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var leftNone = App.WaitForElement("LeftEdgeIndicator").GetRect();
			var rightNone = App.WaitForElement("RightEdgeIndicator").GetRect();

			// Switch to All while still in landscape
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var topAll = App.WaitForElement("TopEdgeIndicator").GetRect();
			var bottomAll = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var leftAll = App.WaitForElement("LeftEdgeIndicator").GetRect();
			var rightAll = App.WaitForElement("RightEdgeIndicator").GetRect();

			// Top and left edges should move inward (greater values)
			Assert.That(topAll.Y, Is.GreaterThanOrEqualTo(topNone.Y), "Landscape: All top inset >= None top");
			Assert.That(leftAll.X, Is.GreaterThanOrEqualTo(leftNone.X), "Landscape: All left inset >= None left");

			// Bottom and right edges should move inward (smaller end positions)
			var bottomEdgeNone = bottomNone.Y + bottomNone.Height;
			var bottomEdgeAll = bottomAll.Y + bottomAll.Height;
			Assert.That(bottomEdgeAll, Is.LessThanOrEqualTo(bottomEdgeNone), "Landscape: All bottom edge <= None bottom edge");

			var rightEdgeNone = rightNone.X + rightNone.Width;
			var rightEdgeAll = rightAll.X + rightAll.Width;
			Assert.That(rightEdgeAll, Is.LessThanOrEqualTo(rightEdgeNone), "Landscape: All right edge <= None right edge");

			App.SetOrientationPortrait();
		}

		// ──────────────────────────────────────────────
		// Legacy API Migration
		// ──────────────────────────────────────────────

		[Test]
		[Description("UseSafeArea=True equivalent to SafeAreaEdges=Container")]
		public void ValidateLegacy_UseSafeAreaTrue()
		{
			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "Container (legacy UseSafeArea=True) should inset from system bars");
		}

		[Test]
		[Description("UseSafeArea=False equivalent to SafeAreaEdges=None")]
		public void ValidateLegacy_UseSafeAreaFalse()
		{
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.LessThanOrEqualTo(5), "None (legacy UseSafeArea=False) should be edge-to-edge");
		}
	}
}
