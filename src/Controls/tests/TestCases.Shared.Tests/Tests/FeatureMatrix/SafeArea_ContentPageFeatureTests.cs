using NUnit.Framework;
using NUnit.Framework.Legacy;
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

		[Test, Order(1)]
		[Description("Content extends edge-to-edge behind system bars/notch")]
		public void ValidateSafeAreaEdges_None()
		{
			App.WaitForElement("ContentPageSafeAreaButton");
			App.Tap("ContentPageSafeAreaButton");	
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.LessThanOrEqualTo(5), "Top indicator should be at the very top (edge-to-edge)");
		}

		[Test, Order(2)]
		[Description("Content inset from all system UI (status bar, nav bar, notch, home indicator)")]
		public void ValidateSafeAreaEdges_All()
		{
			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "Top indicator should be inset from the top edge");
		}

		[Test, Order(3)]
		[Description("Content avoids system bars/notch but can extend under keyboard area")]
		public void ValidateSafeAreaEdges_Container()
		{
			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "Top indicator should be inset from the top edge for Container");
		}

		[Test, Order(4)]
		[Description("SoftInput allows content under system bars but avoids keyboard")]
		public void ValidateSafeAreaEdges_SoftInput()
		{
			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.LessThanOrEqualTo(5), "SoftInput: top should be edge-to-edge (no container adjustment)");
		}

		[Test, Order(5)]
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

		[Test, Order(6)]
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

		[Test, Order(7)]
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

		[Test, Order(8)]
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

		[Test, Order(9)]
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

		[Test, Order(10)]
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

		[Test, Order(11)]
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

		[Test, Order(12)]
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

		[Test, Order(13)]
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

		[Test, Order(14)]
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
		// Keyboard Rectangle Validation (iOS)
		// ──────────────────────────────────────────────
		// Uses KeyboardScrolling.FindiOSKeyboardLocation to get the keyboard's top edge Y coordinate.
		// Validates that the bottom indicator's bottom edge is strictly above the keyboard's top edge
		// when SafeArea requires adjustment (All/SoftInput), and NOT adjusted when it doesn't (None/Container).

#if IOS
		// Tolerance in pixels for comparing bottom label's bottom edge to keyboard's top edge.
		// On iOS the content area shrinks above the keyboard but the bottom indicator may not
		// align pixel-perfectly with the keyboard top due to layout padding and home indicator.
		const int KeyboardAlignmentTolerance = 15;

		[Test, Order(15)]
		[Description("With All, bottom indicator's bottom edge aligns with the keyboard's top edge")]
		public void ValidateKeyboardRect_All_BottomAlignsWithKeyboard()
		{
			var app = App as AppiumApp;
			if (app is null)
				return;

			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");

			// Record bottom indicator position before keyboard
			var bottomBefore = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomEdgeBefore = bottomBefore.Y + bottomBefore.Height;

			// Show keyboard
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			// Get keyboard top edge
			var keyboardPos = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver);
			ClassicAssert.NotNull(keyboardPos, "Keyboard location should be available");
			var keyboardTop = keyboardPos!.Value.Y;

			// Bottom indicator should have moved up (content area shrinks above keyboard)
			var bottomAfter = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomAfterEdge = bottomAfter.Y + bottomAfter.Height;
			Assert.That(Math.Abs(bottomAfterEdge - keyboardTop), Is.LessThanOrEqualTo(KeyboardAlignmentTolerance),
				$"With All, bottom indicator's bottom edge ({bottomAfterEdge}) should align with keyboard top ({keyboardTop}). Diff: {Math.Abs(bottomAfterEdge - keyboardTop)}px");

			// Entry should also be above keyboard top
			var entryRect = App.WaitForElement("SafeAreaTestEntry").GetRect();
			var entryBottom = entryRect.Y + entryRect.Height;
			Assert.That(entryBottom, Is.LessThanOrEqualTo(keyboardTop + KeyboardAlignmentTolerance),
				$"With All, entry's bottom edge ({entryBottom}) should be above keyboard top ({keyboardTop})");

			// Dismiss keyboard and verify bottom indicator restores
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();

			var bottomRestored = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomEdgeRestored = bottomRestored.Y + bottomRestored.Height;
			Assert.That(Math.Abs(bottomEdgeRestored - bottomEdgeBefore), Is.LessThanOrEqualTo(10),
				$"Bottom indicator should return to original position after keyboard dismiss. " +
				$"Original: {bottomEdgeBefore:F0}, Restored: {bottomEdgeRestored:F0}");
		}

		[Test, Order(16)]
		[Description("With SoftInput, bottom indicator's bottom edge aligns with the keyboard's top edge")]
		public void ValidateKeyboardRect_SoftInput_BottomAlignsWithKeyboard()
		{
			var app = App as AppiumApp;
			if (app is null)
				return;

			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");

			// Record bottom indicator position before keyboard
			var bottomBefore = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomEdgeBefore = bottomBefore.Y + bottomBefore.Height;

			// Show keyboard
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			// Get keyboard top edge
			var keyboardPos = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver);
			ClassicAssert.NotNull(keyboardPos, "Keyboard location should be available");
			var keyboardTop = keyboardPos!.Value.Y;

			// Bottom indicator should have moved up (content area shrinks above keyboard)
			var bottomAfter = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomAfterEdge = bottomAfter.Y + bottomAfter.Height;
			Assert.That(Math.Abs(bottomAfterEdge - keyboardTop), Is.LessThanOrEqualTo(KeyboardAlignmentTolerance),
				$"With SoftInput, bottom indicator's bottom edge ({bottomAfterEdge}) should align with keyboard top ({keyboardTop}). Diff: {Math.Abs(bottomAfterEdge - keyboardTop)}px");

			// Dismiss keyboard and verify restoration
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();

			var bottomRestored = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomEdgeRestored = bottomRestored.Y + bottomRestored.Height;
			Assert.That(Math.Abs(bottomEdgeRestored - bottomEdgeBefore), Is.LessThanOrEqualTo(10),
				$"Bottom indicator should return to original position after keyboard dismiss. " +
				$"Original: {bottomEdgeBefore:F0}, Restored: {bottomEdgeRestored:F0}");
		}

		[Test, Order(17)]
		[Description("With None, bottom indicator is NOT adjusted above the keyboard — keyboard overlaps content")]
		public void ValidateKeyboardRect_None_BottomNotAdjusted()
		{
			var app = App as AppiumApp;
			if (app is null)
				return;

			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			// Record bottom indicator position before keyboard
			var bottomBefore = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomEdgeBefore = bottomBefore.Y + bottomBefore.Height;

			// Show keyboard
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			// Get keyboard top edge
			var keyboardPos = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver);
			ClassicAssert.NotNull(keyboardPos, "Keyboard location should be available");

			// Bottom indicator should NOT have moved — keyboard overlaps content with None
			var bottomAfter = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomEdgeAfter = bottomAfter.Y + bottomAfter.Height;
			Assert.That(Math.Abs(bottomEdgeAfter - bottomEdgeBefore), Is.LessThanOrEqualTo(5),
				$"With None, bottom indicator should not shift when keyboard opens. " +
				$"Before: {bottomEdgeBefore:F0}, After: {bottomEdgeAfter:F0}");

			// Dismiss keyboard
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
		}

		[Test, Order(18)]
		[Description("With Container, bottom indicator is NOT adjusted above the keyboard — keyboard may overlap content")]
		public void ValidateKeyboardRect_Container_BottomNotAdjusted()
		{
			var app = App as AppiumApp;
			if (app is null)
				return;

			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");

			// Record bottom indicator position before keyboard
			var bottomBefore = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomEdgeBefore = bottomBefore.Y + bottomBefore.Height;

			// Show keyboard
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			// Get keyboard top edge
			var keyboardPos = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver);
			ClassicAssert.NotNull(keyboardPos, "Keyboard location should be available");
			var keyboardTop = keyboardPos!.Value.Y;

			// Container should NOT adjust for keyboard — only system bars are avoided
			var bottomAfter = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomEdgeAfter = bottomAfter.Y + bottomAfter.Height;
			Assert.That(Math.Abs(bottomEdgeAfter - bottomEdgeBefore), Is.LessThanOrEqualTo(5),
				$"With Container, bottom indicator should not shift when keyboard opens. " +
				$"Before: {bottomEdgeBefore:F0}, After: {bottomEdgeAfter:F0}");

			// Dismiss keyboard
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
		}
#endif

		// ──────────────────────────────────────────────
		// Keyboard + Runtime SafeArea Changes (iOS)
		// ──────────────────────────────────────────────

#if IOS
		[Test, Order(19)]
		[Description("Switch None to All while keyboard is open — bottom indicator moves above keyboard's top edge")]
		public void ValidateKeyboardRuntime_SwitchNoneToAll_WhileKeyboardOpen()
		{
			var app = App as AppiumApp;
			if (app is null)
				return;

			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			// Open keyboard
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			// With None, bottom indicator should NOT be above keyboard
			var keyboardPos = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver);
			ClassicAssert.NotNull(keyboardPos, "Keyboard location should be available");

			var bottomNone = App.WaitForElement("BottomEdgeIndicator").GetRect();
			// None does not adjust — bottom may be at or below keyboard top
			var bottomEdgeNone = bottomNone.Y + bottomNone.Height;

			// Switch to All while keyboard is still open
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			// Re-query keyboard position (may have shifted)
			var keyboardPosAll = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver);
			ClassicAssert.NotNull(keyboardPosAll, "Keyboard should still be visible after switching to All");

			// With All, bottom indicator should be above keyboard
			var bottomAll = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var keyboardTopAll = keyboardPosAll!.Value.Y;
			var bottomEdgeAll = bottomAll.Y + bottomAll.Height;
			Assert.That(Math.Abs(bottomEdgeAll - keyboardTopAll), Is.LessThanOrEqualTo(KeyboardAlignmentTolerance),
				$"After switching to All, bottom indicator's bottom edge ({bottomEdgeAll}) should align with keyboard top ({keyboardTopAll}). Diff: {Math.Abs(bottomEdgeAll - keyboardTopAll)}px");

			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
		}

		[Test, Order(20)]
		[Description("Switch All to None while keyboard is open — bottom indicator drops back behind keyboard")]
		public void ValidateKeyboardRuntime_SwitchAllToNone_WhileKeyboardOpen()
		{
			var app = App as AppiumApp;
			if (app is null)
				return;

			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");

			// Open keyboard
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			// With All, bottom indicator should be above keyboard
			var keyboardPos = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver);
			ClassicAssert.NotNull(keyboardPos, "Keyboard location should be available");
			var keyboardTop = keyboardPos!.Value.Y;

			var bottomAll = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomEdgeAll = bottomAll.Y + bottomAll.Height;
			Assert.That(Math.Abs(bottomEdgeAll - keyboardTop), Is.LessThanOrEqualTo(KeyboardAlignmentTolerance),
				$"With All, bottom indicator ({bottomEdgeAll}) should align with keyboard top ({keyboardTop}). Diff: {Math.Abs(bottomEdgeAll - keyboardTop)}px");

			// Switch to None while keyboard is still open
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			// Top should move to edge (None goes edge-to-edge)
			var topNone = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topNone.Y, Is.LessThanOrEqualTo(5),
				"After switching to None with keyboard open, top should be edge-to-edge");

			// With None, bottom indicator should NOT be adjusted above keyboard
			var bottomNone = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomEdgeNone = bottomNone.Y + bottomNone.Height;
			Assert.That(bottomEdgeNone, Is.GreaterThanOrEqualTo(bottomAll.Bottom),
				$"After switching to None, bottom indicator ({bottomEdgeNone:F0}) should extend back behind keyboard (was {bottomAll.Bottom:F0} with All)");

			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
		}

		[Test, Order(21)]
		[Description("Switch Container to SoftInput while keyboard is open — bottom indicator moves above keyboard's top edge")]
		public void ValidateKeyboardRuntime_SwitchContainerToSoftInput_WhileKeyboardOpen()
		{
			var app = App as AppiumApp;
			if (app is null)
				return;

			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");

			// Open keyboard
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			// Record initial bottom position with Container (before keyboard adjustments are measured)
			var keyboardPos = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver);
			ClassicAssert.NotNull(keyboardPos, "Keyboard location should be available");

			// Switch to SoftInput while keyboard is still open
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			// SoftInput should be edge-to-edge on top (no container adjustment)
			var topSoftInput = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topSoftInput.Y, Is.LessThanOrEqualTo(5),
				"After switching to SoftInput, top should be edge-to-edge");

			// Re-query keyboard position
			var keyboardPosSoftInput = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver);
			ClassicAssert.NotNull(keyboardPosSoftInput, "Keyboard should still be visible after switching to SoftInput");

			// With SoftInput, bottom indicator should be above keyboard
			var bottomSoftInput = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var keyboardTopSoftInput = keyboardPosSoftInput!.Value.Y;
			var bottomEdgeSoftInput = bottomSoftInput.Y + bottomSoftInput.Height;
			Assert.That(Math.Abs(bottomEdgeSoftInput - keyboardTopSoftInput), Is.LessThanOrEqualTo(KeyboardAlignmentTolerance),
				$"SoftInput: bottom indicator's bottom edge ({bottomEdgeSoftInput}) should align with keyboard top ({keyboardTopSoftInput}). Diff: {Math.Abs(bottomEdgeSoftInput - keyboardTopSoftInput)}px");

			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
		}
#endif

		// ──────────────────────────────────────────────
		// Interaction with ContentPage Properties
		// ──────────────────────────────────────────────

		[Test, Order(22)]
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

		[Test, Order(23)]
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

		[Test, Order(24)]
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

		[Test, Order(25)]
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

		[Test, Order(26)]
		[Description("Behavior transitions from Container inset to SoftInput edge-to-edge")]
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

		[Test, Order(27)]
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

		[Test, Order(28)]
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
		[Test, Order(29)]
		[Description("Container/All avoids notch/Dynamic Island on iOS")]
		public void ValidateiOS_NotchAvoidance()
		{
			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(20), "Container should avoid notch/Dynamic Island on iOS");
		}

		[Test, Order(30)]
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
		[Test, Order(29)]
		[Description("Container/All avoids Android status bar")]
		public void ValidateAndroid_StatusBarAvoidance()
		{
			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "Container should avoid status bar on Android");
		}

		[Test, Order(30)]
		[Description("Container/All avoids navigation bar on Android")]
		public void ValidateAndroid_NavBarAvoidance()
		{
			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "All should avoid status bar on Android");
		}

		[Test, Order(31)]
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

		[Test, Order(32)]
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

		[Test, Order(33)]
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

		[Test, Order(34)]
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

		[Test, Order(35)]
		[Description("SoftInput: portrait top/bottom edge-to-edge, landscape validates safe area")]
		public void ValidateOrientation_SoftInput_AllEdges()
		{
			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			// Portrait: top should be edge-to-edge (SoftInput only avoids keyboard, not system bars)
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.LessThanOrEqualTo(5), "Portrait: SoftInput top should be edge-to-edge");
			App.WaitForElement("BottomEdgeIndicator");

			// Landscape: validate safe area is respected
			App.SetOrientationLandscape();
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));
			App.WaitForElement("LeftEdgeIndicator");
			App.WaitForElement("RightEdgeIndicator");

			App.SetOrientationPortrait();
		}

		[Test, Order(36)]
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

		[Test, Order(37)]
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

		[Test, Order(38)]
		[Description("UseSafeArea=True equivalent to SafeAreaEdges=Container")]
		public void ValidateLegacy_UseSafeAreaTrue()
		{
			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));
			var topRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRect.Y, Is.GreaterThan(5), "Container (legacy UseSafeArea=True) should inset from system bars");
		}

		[Test, Order(39)]
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
