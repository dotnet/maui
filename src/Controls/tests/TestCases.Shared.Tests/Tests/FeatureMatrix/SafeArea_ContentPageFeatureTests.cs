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

		private void NavigateToContentPage()
		{
			App.WaitForElement("ContentPageSafeAreaButton");
			App.Tap("ContentPageSafeAreaButton");
			App.WaitForElement("SafeAreaEdgesValueLabel");
		}

		private void OpenOptionsAndApply(System.Action configureOptions)
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("Apply");
			configureOptions();
			App.Tap("Apply");
			App.WaitForElement("SafeAreaEdgesValueLabel");
		}

		private void SelectUniform(string automationId)
		{
			App.WaitForElement(automationId);
			App.Tap(automationId);
		}

		private string GetDisplayString()
		{
			return App.FindElement("SafeAreaEdgesValueLabel").GetText();
		}

		// ──────────────────────────────────────────────
		// Category 1: Uniform SafeAreaRegions Values
		// ──────────────────────────────────────────────

		[Test, Order(1)]
		[Description("Content extends edge-to-edge behind system bars/notch")]
		public void ValidateSafeAreaEdges_None()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformNone");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("None"));

			// With None, content goes edge-to-edge: top indicator should be at or near Y=0
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.LessThanOrEqualTo(5), "Top indicator should be at the very top (edge-to-edge)");
		}

		[Test]
		[Description("Content inset from all system UI (status bar, nav bar, notch, home indicator)")]
		public void ValidateSafeAreaEdges_All()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformAll");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("All"));

			// With All, content is inset: top indicator should be pushed down from Y=0
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "Top indicator should be inset from the top edge");
		}

		[Test]
		[Description("Content avoids system bars/notch but can extend under keyboard area")]
		public void ValidateSafeAreaEdges_Container()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformContainer");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("Container"));

			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "Top indicator should be inset from the top edge for Container");
		}

		[Test]
		[Description("Content flows under system bars/notch but avoids keyboard")]
		public void ValidateSafeAreaEdges_SoftInput()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformSoftInput");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("SoftInput"));

			// SoftInput only avoids keyboard, content goes under system bars
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.LessThanOrEqualTo(5), "Top indicator should be at the top (SoftInput doesn't avoid system bars)");
		}

		[Test]
		[Description("ContentPage defaults to None behavior")]
		public void ValidateSafeAreaEdges_Default()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformDefault");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("Default"));
		}

		// ──────────────────────────────────────────────
		// Category 2: Per-Edge Configuration
		// ──────────────────────────────────────────────

		[Test]
		[Description("Only top avoids status bar/notch. Other edges edge-to-edge.")]
		public void ValidatePerEdge_TopContainerOnly()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				App.Tap("LeftNone");
				App.Tap("TopContainer");
				App.Tap("RightNone");
				App.Tap("BottomNone");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("L:None, T:Container, R:None, B:None"));

			// Top should be inset, left should be at x=0
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "Top should be inset (Container)");

			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftRect.X, Is.LessThanOrEqualTo(5), "Left should be edge-to-edge (None)");
		}

		[Test]
		[Description("Sides/top avoid system bars; bottom avoids only keyboard")]
		public void ValidatePerEdge_BottomSoftInput_SidesContainer()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				App.Tap("LeftContainer");
				App.Tap("TopContainer");
				App.Tap("RightContainer");
				App.Tap("BottomSoftInput");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("L:Container, T:Container, R:Container, B:SoftInput"));

			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "Top should be inset (Container)");

			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftRect.X, Is.GreaterThan(0), "Left should be inset (Container)");
		}

		[Test]
		[Description("Top/bottom respect all insets; left/right edge-to-edge")]
		public void ValidatePerEdge_TopBottomAll_SidesNone()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				App.Tap("LeftNone");
				App.Tap("TopAll");
				App.Tap("RightNone");
				App.Tap("BottomAll");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("L:None, T:All, R:None, B:All"));

			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "Top should be inset (All)");

			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftRect.X, Is.LessThanOrEqualTo(5), "Left should be edge-to-edge (None)");
		}

		[Test]
		[Description("Each edge independently applies its behavior")]
		public void ValidatePerEdge_AllDifferent()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				App.Tap("LeftNone");
				App.Tap("TopContainer");
				App.Tap("RightSoftInput");
				App.Tap("BottomAll");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("L:None, T:Container, R:SoftInput, B:All"));
		}

		// ──────────────────────────────────────────────
		// Category 3: Keyboard Interaction (SoftInput)
		// ──────────────────────────────────────────────

		[Test]
		[Description("Content shifts to avoid keyboard when SafeAreaEdges is All")]
		public void ValidateKeyboard_All()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformAll");
			});

			var entryRectBefore = App.WaitForElement("SafeAreaTestEntry").GetRect();
			App.Tap("SafeAreaTestEntry");

			// Entry should still be visible after keyboard opens
			var entryRectAfter = App.WaitForElement("SafeAreaTestEntry").GetRect();
			Assert.That(GetDisplayString(), Is.EqualTo("All"));
		}

		[Test]
		[Description("Content under system bars but above keyboard")]
		public void ValidateKeyboard_SoftInput()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformSoftInput");
			});

			App.WaitForElement("SafeAreaTestEntry");
			App.Tap("SafeAreaTestEntry");

			Assert.That(GetDisplayString(), Is.EqualTo("SoftInput"));
		}

		[Test]
		[Description("Keyboard overlaps content, no adjustment")]
		public void ValidateKeyboard_None()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformNone");
			});

			App.WaitForElement("SafeAreaTestEntry");
			App.Tap("SafeAreaTestEntry");

			Assert.That(GetDisplayString(), Is.EqualTo("None"));
		}

		[Test]
		[Description("System bars avoided, keyboard may overlap")]
		public void ValidateKeyboard_Container()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformContainer");
			});

			App.WaitForElement("SafeAreaTestEntry");
			App.Tap("SafeAreaTestEntry");

			Assert.That(GetDisplayString(), Is.EqualTo("Container"));
		}

		[Test]
		[Description("Entry at bottom shifts above keyboard with per-edge SoftInput on bottom")]
		public void ValidateKeyboard_BottomSoftInput()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				App.Tap("LeftContainer");
				App.Tap("TopContainer");
				App.Tap("RightContainer");
				App.Tap("BottomSoftInput");
			});

			App.WaitForElement("SafeAreaTestEntry");
			App.Tap("SafeAreaTestEntry");

			Assert.That(GetDisplayString(), Is.EqualTo("L:Container, T:Container, R:Container, B:SoftInput"));
		}

		// ──────────────────────────────────────────────
		// Category 4: Interaction with ContentPage Properties
		// ──────────────────────────────────────────────

		[Test]
		[Description("Safe area insets and padding are additive")]
		public void ValidateSafeArea_WithPadding()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformAll");
				App.Tap("PaddingCheckBox");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("All"));

			// With padding + safe area, the top indicator should be pushed down even further
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(20), "Top indicator should be inset by safe area + padding");
		}

		[Test]
		[Description("Background extends edge-to-edge behind system UI")]
		public void ValidateSafeArea_None_WithBackground()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformNone");
				App.Tap("BackgroundCheckBox");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("None"));
		}

		// ──────────────────────────────────────────────
		// Category 5: Dynamic Runtime Changes
		// ──────────────────────────────────────────────

		[Test]
		[Description("Content shifts from edge-to-edge to inset")]
		public void ValidateDynamic_NoneToAll()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformNone");
			});

			// Verify initial state (None)
			Assert.That(GetDisplayString(), Is.EqualTo("None"));
			var topRectBefore = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRectBefore.Y, Is.LessThanOrEqualTo(5), "Before: top should be edge-to-edge");

			// Toggle to All via ChangeSafeAreaButton
			App.WaitForElement("ChangeSafeAreaButton");
			App.Tap("ChangeSafeAreaButton");

			Assert.That(GetDisplayString(), Is.EqualTo("All"));
			var topRectAfter = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRectAfter.Y, Is.GreaterThan(5), "After: top should be inset");
			Assert.That(topRectAfter.Y, Is.GreaterThan(topRectBefore.Y), "Top indicator should have moved down");
		}

		[Test]
		[Description("Content expands to edge-to-edge")]
		public void ValidateDynamic_AllToNone()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformAll");
			});

			// Verify initial state (All)
			Assert.That(GetDisplayString(), Is.EqualTo("All"));
			var topRectBefore = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRectBefore.Y, Is.GreaterThan(5), "Before: top should be inset");

			// Toggle to None via ChangeSafeAreaButton
			App.WaitForElement("ChangeSafeAreaButton");
			App.Tap("ChangeSafeAreaButton");

			Assert.That(GetDisplayString(), Is.EqualTo("None"));
			var topRectAfter = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRectAfter.Y, Is.LessThanOrEqualTo(5), "After: top should be edge-to-edge");
			Assert.That(topRectAfter.Y, Is.LessThan(topRectBefore.Y), "Top indicator should have moved up");
		}

		[Test]
		[Description("Behavior transitions correctly from Container to SoftInput")]
		public void ValidateDynamic_ContainerToSoftInput()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformContainer");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("Container"));
			var topRectBefore = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRectBefore.Y, Is.GreaterThan(5), "Container: top should be inset");

			// Navigate to options and switch to SoftInput
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformSoftInput");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("SoftInput"));
			var topRectAfter = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRectAfter.Y, Is.LessThanOrEqualTo(5), "SoftInput: top should be edge-to-edge");
		}

		[Test]
		[Description("Per-edge layout updates correctly at runtime")]
		public void ValidateDynamic_PerEdgeChange()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				App.Tap("LeftNone");
				App.Tap("TopNone");
				App.Tap("RightNone");
				App.Tap("BottomNone");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("None"));
			var topRectBefore = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRectBefore.Y, Is.LessThanOrEqualTo(5), "Before: all edges None");

			// Change to per-edge Container on top/bottom
			OpenOptionsAndApply(() =>
			{
				App.Tap("TopContainer");
				App.Tap("BottomContainer");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("L:None, T:Container, R:None, B:Container"));
			var topRectAfter = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRectAfter.Y, Is.GreaterThan(5), "After: top should be inset (Container)");
		}

		// ──────────────────────────────────────────────
		// Category 6: Platform-Specific Behavior
		// ──────────────────────────────────────────────

#if IOS
		[Test]
		[Description("Container/All avoids notch/Dynamic Island on iOS")]
		public void ValidateiOS_NotchAvoidance()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformContainer");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("Container"));
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(20), "Container should avoid notch/Dynamic Island on iOS");
		}

		[Test]
		[Description("Container/All avoids home indicator on iOS")]
		public void ValidateiOS_HomeIndicatorAvoidance()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformAll");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("All"));
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(20), "All should avoid notch/home indicator on iOS");
		}
#endif

#if ANDROID
		[Test]
		[Description("Container/All avoids Android status bar")]
		public void ValidateAndroid_StatusBarAvoidance()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformContainer");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("Container"));
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "Container should avoid status bar on Android");
		}

		[Test]
		[Description("Container/All avoids navigation bar on Android")]
		public void ValidateAndroid_NavBarAvoidance()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformAll");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("All"));
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "All should avoid status bar on Android");
		}

		[Test]
		[Description("ContentPage defaults to None (edge-to-edge) in .NET 10")]
		public void ValidateAndroid_DefaultIsNone()
		{
			NavigateToContentPage();
			// Do not change any options - verify default behavior
			Assert.That(GetDisplayString(), Is.EqualTo("None"));
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.LessThanOrEqualTo(5), "Default (None) should be edge-to-edge on Android");
		}
#endif

		[Test]
		[Description("Safe area insets update on device rotation")]
		public void ValidateOrientationChange()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformContainer");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("Container"));
			var topRectPortrait = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRectPortrait.Y, Is.GreaterThan(5), "Portrait: top should be inset");

			App.SetOrientationLandscape();

			var topRectLandscape = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(GetDisplayString(), Is.EqualTo("Container"));

			// In landscape, left edge should be inset (on devices with notch)
			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();

			App.SetOrientationPortrait();
		}

		// ──────────────────────────────────────────────
		// Category 7: Legacy API Migration
		// ──────────────────────────────────────────────

		[Test]
		[Description("UseSafeArea=True equivalent to SafeAreaEdges=Container")]
		public void ValidateLegacy_UseSafeAreaTrue()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformContainer");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("Container"));
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "Container (legacy UseSafeArea=True) should inset from system bars");
		}

		[Test]
		[Description("UseSafeArea=False equivalent to SafeAreaEdges=None")]
		public void ValidateLegacy_UseSafeAreaFalse()
		{
			NavigateToContentPage();
			OpenOptionsAndApply(() =>
			{
				SelectUniform("UniformNone");
			});

			Assert.That(GetDisplayString(), Is.EqualTo("None"));
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.LessThanOrEqualTo(5), "None (legacy UseSafeArea=False) should be edge-to-edge");
		}
	}
}
