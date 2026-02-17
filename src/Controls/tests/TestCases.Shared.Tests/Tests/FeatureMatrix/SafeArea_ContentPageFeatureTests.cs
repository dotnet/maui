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

		// Tolerance in pixels/points for comparing label positions to safe area inset values
		const int InsetTolerance = 5;

		public SafeArea_ContentPageFeatureTests(TestDevice device)
			: base(device)
		{
		}

		/// <summary>
		/// Reads and parses safe area inset values from the SafeAreaInsetsLabel.
		/// Reuses the same platform-specific approach as Issue28986_SafeAreaBorderOrientation.
		/// Format: "L:{left},T:{top},R:{right},B:{bottom}"
		/// </summary>
		private (int Left, int Top, int Right, int Bottom) GetSafeAreaInsets()
		{
			var text = App.WaitForElement("SafeAreaInsetsLabel").GetText() ?? string.Empty;
			var match = System.Text.RegularExpressions.Regex.Match(text, @"L:(\d+),T:(\d+),R:(\d+),B:(\d+)");
			if (!match.Success)
				throw new InvalidOperationException($"Failed to parse safe area insets from: '{text}'");
			return (
				int.Parse(match.Groups[1].Value),
				int.Parse(match.Groups[2].Value),
				int.Parse(match.Groups[3].Value),
				int.Parse(match.Groups[4].Value)
			);
		}

		private int GetKeyboardY()
        {
#if IOS
            var app = (AppiumApp)App;
            var toolbar = app.Driver.FindElement(OpenQA.Selenium.Appium.MobileBy.ClassName("XCUIElementTypeToolbar"));
            return toolbar.Location.Y;
#elif ANDROID
            var app = (AppiumApp)App;

            // Get the InputMethod window frame via dumpsys (physical pixels)
            var result = app.Driver.ExecuteScript("mobile: shell", new Dictionary<string, object>
            {
                { "command", "dumpsys" },
                { "args", new[] { "window", "InputMethod" } }
            });
            var output = result?.ToString() ?? string.Empty;

            // Parse mFrame=[left,top][right,bottom] from the InputMethod window
            var frameMatch = System.Text.RegularExpressions.Regex.Match(output, @"mFrame=\[(\d+),(\d+)\]\[(\d+),(\d+)\]");
            if (!frameMatch.Success)
                throw new InvalidOperationException("Could not parse keyboard frame from dumpsys window InputMethod output");

            var topPx = int.Parse(frameMatch.Groups[2].Value);

            // Get physical screen size to calculate density for px-to-dp conversion
            var sizeResult = app.Driver.ExecuteScript("mobile: shell", new Dictionary<string, object>
            {
                { "command", "wm" },
                { "args", new[] { "size" } }
            });
            var sizeOutput = sizeResult?.ToString() ?? string.Empty;
            var sizeMatch = System.Text.RegularExpressions.Regex.Match(sizeOutput, @"(\d+)x(\d+)");
            if (!sizeMatch.Success)
                throw new InvalidOperationException("Could not parse physical screen size from wm size output");

            var physicalHeight = double.Parse(sizeMatch.Groups[2].Value);
            var (_, logicalHeight) = GetScreenSize();
            var density = physicalHeight / logicalHeight;

            return (int)(topPx / density);
#else
			return 0;
#endif
        }



		public void ClickContentPageSafeAreaButton()
		{
			App.WaitForElement("ContentPageSafeAreaButton");
			App.Tap("ContentPageSafeAreaButton");
		}

		private (int Width, int Height) GetScreenSize()
		{
			var size = ((AppiumApp)App).Driver.Manage().Window.Size;
			return (size.Width, size.Height);
		}

		// ──────────────────────────────────────────────
		// Uniform SafeAreaRegions via Buttons
		// ──────────────────────────────────────────────

		[Test, Order(1)]
		[Description("Content extends edge-to-edge behind system bars/notch")]
		public void ValidateSafeAreaEdges_None()
		{
			ClickContentPageSafeAreaButton();
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			// Portrait: top label Y should be ≈ 0 (edge-to-edge, no safe area applied)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.EqualTo(0),
				$"None: top label Y ({topLabelRect.Y}) should be = 0 (edge-to-edge), safe area top inset is ignored");

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// Portrait: bottom label bottom edge should be ≈ screenHeight (edge-to-edge, no safe area applied)
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight),
				$"None: bottom label Y ({bottomLabelRect.Bottom}) should be ≈ screenHeight ({screenHeight})");
		}

		[Test, Order(2)]
		[Description("Content inset from all system UI (status bar, nav bar, notch, home indicator)")]
		public void ValidateSafeAreaEdges_All()
		{
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// Portrait: top label Y should be ≈ insets.Top (safe area applied)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"All: top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ screenBottom - insets.Bottom
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"All: bottom label Y ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
		}

		[Test, Order(3)]
		[Description("Content avoids system bars/notch but can extend under keyboard area")]
		public void ValidateSafeAreaEdges_Container()
		{
			App.Tap("SafeAreaContainerButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// Portrait: top label Y should be ≈ insets.Top (safe area applied)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Container: top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ screenBottom - insets.Bottom
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"Container: bottom label Y ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
		}

		[Test, Order(4)]
		[Description("SoftInput respects safe area on top/sides but bottom is edge-to-edge without keyboard")]
		public void ValidateSafeAreaEdges_SoftInput()
		{
			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// Portrait: top label Y should be ≈ insets.Top (SoftInput respects notch/safe area)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"SoftInput: top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ screenHeight (edge-to-edge, no safe area applied)
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight),
				$"SoftInput: bottom label Y ({bottomLabelRect.Bottom}) should be equal to screenHeight ({screenHeight})");
		}

		[Test, Order(5)]
		[Description("ContentPage defaults to None behavior")]
		public void ValidateSafeAreaEdges_Default()
		{
			App.WaitForElement("SafeAreaDefaultButton");
			App.Tap("SafeAreaDefaultButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Default"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// Portrait: top label Y should be ≈ insets.Top (safe area applied)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Default: top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ screenBottom - insets.Bottom
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"Default: bottom label Y ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
		}

		// ──────────────────────────────────────────────
		// Per-Edge Configuration (via Options)
		// ──────────────────────────────────────────────

		[Test, Order(6)]
		[Description("Only top avoids status bar/notch. Bottom edge-to-edge.")]
		public void ValidatePerEdge_TopContainerOnly()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TopContainer");
			App.Tap("TopContainer");
			App.Tap("BottomNone");
			App.WaitForElement("Apply");
			App.Tap("Apply");

			App.WaitForElement("SafeAreaEdgesValueLabel");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("L:None, T:Container, R:None, B:None"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// Portrait: Container — should be inset by safe area top
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Top (Container): label Y ({topLabelRect.Y}) should be ≈ insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ screenHeight (edge-to-edge, no safe area applied)
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight),
				$"None: bottom label Y ({bottomLabelRect.Bottom}) should be ≈ screenHeight ({screenHeight})");
		}

		[Test, Order(7)]
		[Description("Top avoids system bars; bottom avoids only keyboard")]
		public void ValidatePerEdge_BottomSoftInput_TopContainer()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TopContainer");
			App.Tap("TopContainer");
			App.Tap("BottomSoftInput");
			App.WaitForElement("Apply");
			App.Tap("Apply");

			App.WaitForElement("SafeAreaEdgesValueLabel");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("L:None, T:Container, R:None, B:SoftInput"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// Portrait: only validate top and bottom — no left/right safe area insets in portrait
			// Top: Container — should be inset by safe area top
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Top (Container): label Y ({topLabelRect.Y}) should be ≈ insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ screenHeight (edge-to-edge, no safe area applied)
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight),
				$"None: bottom label Y ({bottomLabelRect.Bottom}) should be ≈ screenHeight ({screenHeight})");
		}

		[Test, Order(8)]
		[Description("Top/bottom respect all insets")]
		public void ValidatePerEdge_TopBottomAll_SidesNone()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TopAll");
			App.Tap("TopAll");
			App.Tap("BottomAll");
			App.WaitForElement("Apply");
			App.Tap("Apply");

			App.WaitForElement("SafeAreaEdgesValueLabel");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("L:None, T:All, R:None, B:All"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// Portrait: top label Y should be ≈ insets.Top (All applies safe area on top)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"All: top label Y ({topLabelRect.Y}) should be = insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ screenBottom - insets.Bottom (All applies safe area on bottom)
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"All: bottom label Y ({bottomLabelRect.Bottom}) should be = (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
		}

		[Test, Order(9)]
		[Description("Each edge independently applies its behavior")]
		public void ValidatePerEdge_AllDifferent()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TopContainer");
			App.Tap("TopContainer");
			App.Tap("BottomAll");
			App.WaitForElement("Apply");
			App.Tap("Apply");

			App.WaitForElement("SafeAreaEdgesValueLabel");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("L:None, T:Container, R:None, B:All"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// Portrait: top label Y should be ≈ insets.Top (safe area applied)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"All: top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ screenBottom - insets.Bottom
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"All: bottom label Y ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
		}

		// ──────────────────────────────────────────────
		// Keyboard Interaction (SoftInput)
		// ──────────────────────────────────────────────
#if TEST_FAILS_ON_IOS

		[Test, Order(10)]
		[Description("Validates label positions across None, All, keyboard open, switch to Container, dismiss, then All again")]
		public void ValidateSafeArea_NoneThenAllKeyboardContainerDismissAll()
		{
			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Step 1: Click None and verify ──
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.EqualTo(0),
				$"None: top label Y ({topLabelRect.Y}) should be 0 (edge-to-edge)");

			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight),
				$"None: bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// ── Step 2: Click All and verify ──
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"All: top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"All: bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// ── Step 3: Open keyboard and verify (All adjusts for keyboard) ──
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			var keyboardY = GetKeyboardY();

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"All (keyboard open): top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelRect.Bottom, Is.EqualTo(keyboardY),
				$"All (keyboard open): bottom label Bottom ({bottomLabelRect.Bottom}) should equal keyboard Y ({keyboardY})");

			// ── Step 4: Switch to Container while keyboard is open ──
			App.Tap("SafeAreaContainerButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Container (keyboard open): top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"Container (keyboard open): bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// ── Step 5: Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();

			// ── Step 6: Click All and verify ──
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"All (after dismiss): top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"All (after dismiss): bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
		}
#endif

		// ──────────────────────────────────────────────
		// Keyboard Position Validation
		// ──────────────────────────────────────────────
		// Validates that the bottom indicator moves up when keyboard is shown with modes that
		// adjust for keyboard (All/SoftInput), and does NOT move with modes that don't (None/Container).

		[Test, Order(11)]
		[Description("With All, bottom indicator moves up when keyboard is shown and restores when dismissed")] /////////////
		public void ValidateKeyboard_All_BottomMovesUp()
		{
			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Before keyboard ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelBeforeRect.Y), Is.EqualTo(insets.Top),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// ── Show keyboard ──
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			var keyboardY = GetKeyboardY();

			// Bottom should have moved up to the keyboard top
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(keyboardY),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should equal keyboard Y ({keyboardY})");

			// Top should remain unchanged
			var topLabelDuringRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"During keyboard - top label Y ({topLabelDuringRect.Y}) should remain at ({topLabelBeforeRect.Y})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();

			// Top should return to its original position
			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should return to original ({topLabelBeforeRect.Y})");

			// Bottom should return to its original position
			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
		}

		[Test, Order(12)]
		[Description("With SoftInput, bottom indicator moves up when keyboard is shown and restores when dismissed")] ////////////
		public void ValidateKeyboard_SoftInput_BottomMovesUp()
		{
			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Before keyboard ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelBeforeRect.Y), Is.EqualTo(insets.Top),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// ── Show keyboard ──
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			var keyboardY = GetKeyboardY();

			// Bottom should have moved up to the keyboard top
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(keyboardY),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should equal keyboard Y ({keyboardY})");

			// Top should remain unchanged
			var topLabelDuringRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"During keyboard - top label Y ({topLabelDuringRect.Y}) should remain at ({topLabelBeforeRect.Y})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();

			// Top should return to its original position
			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should return to original ({topLabelBeforeRect.Y})");

			// Bottom should return to its original position
			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
		}

		[Test, Order(13)]
		[Description("With None, bottom indicator does NOT move when keyboard is shown")] /////////////
		public void ValidateKeyboard_None_BottomStays()
		{
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			var (_, screenHeight) = GetScreenSize();

			// ── Before keyboard ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelBeforeRect.Y, Is.EqualTo(0),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be 0 (edge-to-edge)");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// ── Show keyboard ──
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			var topLabelDuringRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringRect.Y, Is.EqualTo(0),
				$"During keyboard - top label Y ({topLabelDuringRect.Y}) should be 0 (edge-to-edge)");

			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelDuringRect.Bottom), Is.EqualTo(screenHeight),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			App.DismissKeyboard();
			App.WaitForKeyboardToHide();

			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should return to original ({topLabelBeforeRect.Y})");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");

		}

		[Test, Order(14)]
		[Description("With Container, bottom indicator does NOT move when keyboard is shown")] /////////////////
		public void ValidateKeyboard_Container_BottomStays()
		{
			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Before keyboard ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelBeforeRect.Y), Is.EqualTo(insets.Top),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
			
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			// Bottom should have not moved up to the keyboard top
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(screenHeight - insets.Bottom),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should equal (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// Top should remain unchanged
			var topLabelDuringRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"During keyboard - top label Y ({topLabelDuringRect.Y}) should remain at ({topLabelBeforeRect.Y})");

			App.DismissKeyboard();
			App.WaitForKeyboardToHide();

			// Top should return to its original position
			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should return to original ({topLabelBeforeRect.Y})");

			// Bottom should return to its original position
			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
			
		}

		// ──────────────────────────────────────────────
		// Keyboard + Runtime SafeArea Changes
		// ──────────────────────────────────────────────

		[Test, Order(15)]
		[Description("Switch None to All while keyboard is open — bottom indicator moves up")] //Issue in iOS
		public void ValidateKeyboardRuntime_SwitchNoneToAll_WhileKeyboardOpen()
		{
			App.DismissKeyboard();
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Before keyboard (None) ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelBeforeRect.Y, Is.EqualTo(0),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be 0 (edge-to-edge)");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// ── Show keyboard (None) ──
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			// With None, bottom should NOT move
			var topLabelDuringNoneRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringNoneRect.Y, Is.EqualTo(0),
				$"During keyboard (None) - top label Y ({topLabelDuringNoneRect.Y}) should be 0 (edge-to-edge)");

			var bottomLabelDuringNoneRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelDuringNoneRect.Bottom), Is.EqualTo(screenHeight),
				$"During keyboard (None) - bottom label Bottom ({bottomLabelDuringNoneRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// ── Switch to All while keyboard is open ──
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var keyboardY = GetKeyboardY();

			// With All, bottom should move up to keyboard top
			var topLabelDuringAllRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelDuringAllRect.Y), Is.EqualTo(insets.Top),
				$"During keyboard (All) - top label Y ({topLabelDuringAllRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelDuringAllRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringAllRect.Bottom, Is.EqualTo(keyboardY),
				$"During keyboard (All) - bottom label Bottom ({bottomLabelDuringAllRect.Bottom}) should equal keyboard Y ({keyboardY})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();

			// After keyboard (All): top at insets.Top, bottom at (screenHeight - insets.Bottom)
			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelAfterRect.Y), Is.EqualTo(insets.Top),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelAfterRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
		}

		[Test, Order(16)]
		[Description("Switch All to None while keyboard is open — bottom indicator drops back")]
		public void ValidateKeyboardRuntime_SwitchAllToNone_WhileKeyboardOpen()
		{
			// Navigating to the Options page to reset the ViewModel to its default settings before the test to ensure consistent testing
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("Apply");
			App.Tap("Apply");

			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Before keyboard (All) ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelBeforeRect.Y), Is.EqualTo(insets.Top),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// ── Show keyboard (All) ──
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			var keyboardY = GetKeyboardY();

			// With All, bottom should move up to keyboard top
			var topLabelDuringAllRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringAllRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"During keyboard (All) - top label Y ({topLabelDuringAllRect.Y}) should remain at ({topLabelBeforeRect.Y})");

			var bottomLabelDuringAllRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringAllRect.Bottom, Is.EqualTo(keyboardY),
				$"During keyboard (All) - bottom label Bottom ({bottomLabelDuringAllRect.Bottom}) should equal keyboard Y ({keyboardY})");

			// ── Switch to None while keyboard is open ──
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			// With None, top goes edge-to-edge; bottom does NOT adjust for keyboard
			var topLabelDuringNoneRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringNoneRect.Y, Is.EqualTo(0),
				$"During keyboard (None) - top label Y ({topLabelDuringNoneRect.Y}) should be 0 (edge-to-edge)");

			var bottomLabelDuringNoneRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelDuringNoneRect.Bottom), Is.EqualTo(screenHeight),
				$"During keyboard (None) - bottom label Bottom ({bottomLabelDuringNoneRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();

			// After keyboard (None): top at 0, bottom at screenHeight
			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(0),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should be 0 (edge-to-edge)");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelAfterRect.Bottom), Is.EqualTo(screenHeight),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should be equal to screenHeight ({screenHeight})");
		}

		[Test, Order(17)]
		[Description("Switch Container to SoftInput while keyboard is open — bottom indicator moves up")]
		public void ValidateKeyboardRuntime_SwitchContainerToSoftInput_WhileKeyboardOpen()
		{
			App.DismissKeyboard();

			// Navigating to the Options page to reset the ViewModel to its default settings before the test to ensure consistent testing
			App.WaitForElement("Options");
			App.Tap("Options");

			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Before keyboard (Container) ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelBeforeRect.Y), Is.EqualTo(insets.Top),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// ── Show keyboard (Container) ──
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			// With Container, bottom should NOT move
			var topLabelDuringContainerRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringContainerRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"During keyboard (Container) - top label Y ({topLabelDuringContainerRect.Y}) should remain at ({topLabelBeforeRect.Y})");

			var bottomLabelDuringContainerRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringContainerRect.Bottom, Is.EqualTo(screenHeight - insets.Bottom),
				$"During keyboard (Container) - bottom label Bottom ({bottomLabelDuringContainerRect.Bottom}) should equal (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// ── Switch to SoftInput while keyboard is open ──
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			var keyboardY = GetKeyboardY();

			// With SoftInput, bottom should move up to keyboard top
			var topLabelDuringSoftInputRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelDuringSoftInputRect.Y), Is.EqualTo(insets.Top),
				$"During keyboard (SoftInput) - top label Y ({topLabelDuringSoftInputRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelDuringSoftInputRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringSoftInputRect.Bottom, Is.EqualTo(keyboardY),
				$"During keyboard (SoftInput) - bottom label Bottom ({bottomLabelDuringSoftInputRect.Bottom}) should equal keyboard Y ({keyboardY})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();

			// After keyboard (SoftInput): top at insets.Top, bottom at screenHeight (edge-to-edge without keyboard)
			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelAfterRect.Y), Is.EqualTo(insets.Top),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelAfterRect.Bottom), Is.EqualTo(screenHeight),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should be equal to screenHeight ({screenHeight})");
		}

		[Test, Order(18)]
		[Description("Cycle through all SafeArea modes while keyboard is open — verify positions without dismissing keyboard")] // Issue in ios
		public void ValidateKeyboardRuntime_CycleThroughAllModes_WhileKeyboardOpen()
		{
			// ── Start with None ──
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Verify None positions before keyboard ──
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.EqualTo(0),
				$"None (before keyboard) - top label Y ({topLabelRect.Y}) should be 0 (edge-to-edge)");

			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight),
				$"None (before keyboard) - bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// ── Open keyboard ──
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			var keyboardY = GetKeyboardY();

			// ── Verify None with keyboard (no adjustment) ──
			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.EqualTo(0),
				$"None (keyboard open) - top label Y ({topLabelRect.Y}) should be 0 (edge-to-edge)");

			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight),
				$"None (keyboard open) - bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// ── Switch to All (keyboard still open) ──
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			keyboardY = GetKeyboardY();

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"All (keyboard open) - top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelRect.Bottom, Is.EqualTo(keyboardY),
				$"All (keyboard open) - bottom label Bottom ({bottomLabelRect.Bottom}) should equal keyboard Y ({keyboardY})");

			// ── Switch to Container (keyboard still open) ──
			App.Tap("SafeAreaContainerButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Container (keyboard open) - top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"Container (keyboard open) - bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// ── Switch to SoftInput (keyboard still open) ──
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			keyboardY = GetKeyboardY();

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"SoftInput (keyboard open) - top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelRect.Bottom, Is.EqualTo(keyboardY),
				$"SoftInput (keyboard open) - bottom label Bottom ({bottomLabelRect.Bottom}) should equal keyboard Y ({keyboardY})");

			// ── Switch to Default (keyboard still open) ──
			App.Tap("SafeAreaDefaultButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Default"));

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Default (keyboard open) - top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"Default (keyboard open) - bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// ── Switch back to None (keyboard still open) ──
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.EqualTo(0),
				$"None (keyboard open, after cycle) - top label Y ({topLabelRect.Y}) should be 0 (edge-to-edge)");

			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight),
				$"None (keyboard open, after cycle) - bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
		}

		// ──────────────────────────────────────────────
		// Interaction with ContentPage Properties
		// ──────────────────────────────────────────────

		[Test, Order(19)]
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

			var insets = GetSafeAreaInsets();

			// With All + padding, top should be beyond safe area inset (additive)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.GreaterThan(insets.Top),
				$"Top Y ({topLabelRect.Y}) should be > insets.Top ({insets.Top}) due to additional padding");
		}

		[Test, Order(20)]
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
		// Orientation / Landscape Validation
		// ──────────────────────────────────────────────

		[Test, Order(21)]
		[Description("None: landscape left/right/bottom all edge-to-edge")]
		public void ValidateOrientation_None_Landscape()
		{
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			App.SetOrientationLandscape();

			var (screenWidth, screenHeight) = GetScreenSize();

			// Left: edge-to-edge
			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftRect.X, Is.EqualTo(0),
				$"None: left X ({leftRect.X}) should be = 0 (edge-to-edge)");

			// Right: edge-to-edge
			var rightRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdge = rightRect.X + rightRect.Width;
			Assert.That(Math.Abs(rightEdge), Is.EqualTo(screenWidth),
				$"None: right edge ({rightEdge}) should be = screenWidth ({screenWidth})");

			// Bottom: edge-to-edge
			var bottomRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomRect.Bottom), Is.EqualTo(screenHeight),
				$"None: bottom edge ({bottomRect.Bottom}) should be = screenHeight ({screenHeight})");

			App.SetOrientationPortrait();
		}

		[Test, Order(22)]
		[Description("All: landscape left/right/bottom inset by safe area")]
		public void ValidateOrientation_All_Landscape()
		{
			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			App.SetOrientationLandscape();

			var (screenWidth, screenHeight) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// Left: inset by safe area
			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftRect.X), Is.EqualTo(insetsLandscape.Left),
				$"All: left X ({leftRect.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			// Right: inset by safe area
			var rightRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdge = rightRect.X + rightRect.Width;
			Assert.That(Math.Abs(rightEdge), Is.EqualTo(screenWidth - insetsLandscape.Right),
				$"All: right edge ({rightEdge}) should be = screenWidth - insetsLandscape.Right ({screenWidth - insetsLandscape.Right})");

			// Bottom: inset by safe area
			var bottomRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomRect.Bottom), Is.EqualTo(screenHeight - insetsLandscape.Bottom),
				$"All: bottom edge ({bottomRect.Bottom}) should be = screenHeight - insetsLandscape.Bottom ({screenHeight - insetsLandscape.Bottom})");

			App.SetOrientationPortrait();
		}

		[Test, Order(23)]
		[Description("Container: landscape left/right/bottom inset by safe area")]
		public void ValidateOrientation_Container_Landscape()
		{
			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			App.SetOrientationLandscape();

			var (screenWidth, screenHeight) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// Left: inset by safe area
			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftRect.X), Is.EqualTo(insetsLandscape.Left),
				$"Container: left X ({leftRect.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			// Right: inset by safe area
			var rightRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdge = rightRect.X + rightRect.Width;
			Assert.That(Math.Abs(rightEdge), Is.EqualTo(screenWidth - insetsLandscape.Right),
				$"Container: right edge ({rightEdge}) should be = screenWidth - insetsLandscape.Right ({screenWidth - insetsLandscape.Right})");

			// Bottom: inset by safe area
			var bottomRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomRect.Bottom), Is.EqualTo(screenHeight - insetsLandscape.Bottom),
				$"Container: bottom edge ({bottomRect.Bottom}) should be = screenHeight - insetsLandscape.Bottom ({screenHeight - insetsLandscape.Bottom})");

			App.SetOrientationPortrait();
		}

		[Test, Order(24)]
		[Description("SoftInput: landscape left/right inset by safe area, bottom edge-to-edge")]
		public void ValidateOrientation_SoftInput_Landscape()
		{
			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			App.SetOrientationLandscape();

			var (screenWidth, screenHeight) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// Left: inset by safe area
			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftRect.X), Is.EqualTo(insetsLandscape.Left),
				$"SoftInput: left X ({leftRect.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			// Right: inset by safe area
			var rightRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdge = rightRect.X + rightRect.Width;
			Assert.That(Math.Abs(rightEdge), Is.EqualTo(screenWidth - insetsLandscape.Right),
				$"SoftInput: right edge ({rightEdge}) should be = screenWidth - insetsLandscape.Right ({screenWidth - insetsLandscape.Right})");

			// Bottom: edge-to-edge (SoftInput doesn't avoid bottom without keyboard)
			var bottomRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomRect.Bottom), Is.EqualTo(screenHeight),
				$"SoftInput: bottom edge ({bottomRect.Bottom}) should be = screenHeight ({screenHeight})");

			App.SetOrientationPortrait();
		}

		[Test, Order(25)]
		[Description("Default: landscape left/right/bottom inset by safe area (Default behaves like Container)")]
		public void ValidateOrientation_Default_Landscape()
		{
			App.WaitForElement("SafeAreaDefaultButton");
			App.Tap("SafeAreaDefaultButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Default"));

			App.SetOrientationLandscape();

			var (screenWidth, screenHeight) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// Left: inset by safe area
			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftRect.X), Is.EqualTo(insetsLandscape.Left),
				$"Default: left X ({leftRect.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			// Right: inset by safe area
			var rightRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdge = rightRect.X + rightRect.Width;
			Assert.That(Math.Abs(rightEdge), Is.EqualTo(screenWidth - insetsLandscape.Right),
				$"Default: right edge ({rightEdge}) should be = screenWidth - insetsLandscape.Right ({screenWidth - insetsLandscape.Right})");

			// Bottom: inset by safe area
			var bottomRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomRect.Bottom), Is.EqualTo(screenHeight - insetsLandscape.Bottom),
				$"Default: bottom edge ({bottomRect.Bottom}) should be = screenHeight - insetsLandscape.Bottom ({screenHeight - insetsLandscape.Bottom})");

			App.SetOrientationPortrait();
		}

		// ──────────────────────────────────────────────
		// Landscape Keyboard Position Validation
		// ──────────────────────────────────────────────

		[Test, Order(26)]
		[Description("Landscape All: bottom moves up to keyboard, left/right stay inset")]
		public void ValidateKeyboard_All_Landscape()
		{
			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			App.SetOrientationLandscape();

			var (screenWidth, screenHeight) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// ── Before keyboard ──
			var leftBeforeRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftBeforeRect.X), Is.EqualTo(insetsLandscape.Left),
				$"Before keyboard - left X ({leftBeforeRect.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			var rightBeforeRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightBeforeEdge = rightBeforeRect.X + rightBeforeRect.Width;
			Assert.That(Math.Abs(rightBeforeEdge), Is.EqualTo(screenWidth - insetsLandscape.Right),
				$"Before keyboard - right edge ({rightBeforeEdge}) should be = screenWidth - insetsLandscape.Right ({screenWidth - insetsLandscape.Right})");

			var bottomBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomBeforeRect.Bottom), Is.EqualTo(screenHeight - insetsLandscape.Bottom),
				$"Before keyboard - bottom edge ({bottomBeforeRect.Bottom}) should be = screenHeight - insetsLandscape.Bottom ({screenHeight - insetsLandscape.Bottom})");

			// ── Show keyboard ──
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			var keyboardY = GetKeyboardY();

			// Bottom should move up to keyboard top
			var bottomDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomDuringRect.Bottom, Is.EqualTo(keyboardY),
				$"During keyboard - bottom edge ({bottomDuringRect.Bottom}) should equal keyboard Y ({keyboardY})");

			// Left/Right should remain unchanged
			var leftDuringRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftDuringRect.X, Is.EqualTo(leftBeforeRect.X),
				$"During keyboard - left X ({leftDuringRect.X}) should remain at ({leftBeforeRect.X})");

			var rightDuringRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightDuringEdge = rightDuringRect.X + rightDuringRect.Width;
			Assert.That(rightDuringEdge, Is.EqualTo(rightBeforeEdge),
				$"During keyboard - right edge ({rightDuringEdge}) should remain at ({rightBeforeEdge})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();

			// All edges should return to original positions
			var leftAfterRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftAfterRect.X, Is.EqualTo(leftBeforeRect.X),
				$"After keyboard - left X ({leftAfterRect.X}) should return to original ({leftBeforeRect.X})");

			var rightAfterRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightAfterEdge = rightAfterRect.X + rightAfterRect.Width;
			Assert.That(rightAfterEdge, Is.EqualTo(rightBeforeEdge),
				$"After keyboard - right edge ({rightAfterEdge}) should return to original ({rightBeforeEdge})");

			var bottomAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomAfterRect.Bottom, Is.EqualTo(bottomBeforeRect.Bottom),
				$"After keyboard - bottom edge ({bottomAfterRect.Bottom}) should return to original ({bottomBeforeRect.Bottom})");

			App.SetOrientationPortrait();
		}

		[Test, Order(27)]
		[Description("Landscape SoftInput: bottom moves up to keyboard, left/right stay inset")]
		public void ValidateKeyboard_SoftInput_Landscape()
		{
			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			App.SetOrientationLandscape();

			var (screenWidth, screenHeight) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// ── Before keyboard ──
			var leftBeforeRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftBeforeRect.X), Is.EqualTo(insetsLandscape.Left),
				$"Before keyboard - left X ({leftBeforeRect.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			var rightBeforeRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightBeforeEdge = rightBeforeRect.X + rightBeforeRect.Width;
			Assert.That(Math.Abs(rightBeforeEdge), Is.EqualTo(screenWidth - insetsLandscape.Right),
				$"Before keyboard - right edge ({rightBeforeEdge}) should be = screenWidth - insetsLandscape.Right ({screenWidth - insetsLandscape.Right})");

			var bottomBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomBeforeRect.Bottom), Is.EqualTo(screenHeight),
				$"Before keyboard - bottom edge ({bottomBeforeRect.Bottom}) should be = screenHeight ({screenHeight})");

			// ── Show keyboard ──
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			var keyboardY = GetKeyboardY();

			// Bottom should move up to keyboard top
			var bottomDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomDuringRect.Bottom, Is.EqualTo(keyboardY),
				$"During keyboard - bottom edge ({bottomDuringRect.Bottom}) should equal keyboard Y ({keyboardY})");

			// Left/Right should remain unchanged
			var leftDuringRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftDuringRect.X, Is.EqualTo(leftBeforeRect.X),
				$"During keyboard - left X ({leftDuringRect.X}) should remain at ({leftBeforeRect.X})");

			var rightDuringRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightDuringEdge = rightDuringRect.X + rightDuringRect.Width;
			Assert.That(rightDuringEdge, Is.EqualTo(rightBeforeEdge),
				$"During keyboard - right edge ({rightDuringEdge}) should remain at ({rightBeforeEdge})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();

			// All edges should return to original positions
			var leftAfterRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftAfterRect.X, Is.EqualTo(leftBeforeRect.X),
				$"After keyboard - left X ({leftAfterRect.X}) should return to original ({leftBeforeRect.X})");

			var rightAfterRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightAfterEdge = rightAfterRect.X + rightAfterRect.Width;
			Assert.That(rightAfterEdge, Is.EqualTo(rightBeforeEdge),
				$"After keyboard - right edge ({rightAfterEdge}) should return to original ({rightBeforeEdge})");

			var bottomAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomAfterRect.Bottom, Is.EqualTo(bottomBeforeRect.Bottom),
				$"After keyboard - bottom edge ({bottomAfterRect.Bottom}) should return to original ({bottomBeforeRect.Bottom})");

			App.SetOrientationPortrait();
		}

		[Test, Order(28)]
		[Description("Landscape None: bottom stays at screen edge with keyboard, left/right stay edge-to-edge")]
		public void ValidateKeyboard_None_Landscape()
		{
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			App.SetOrientationLandscape();

			var (screenWidth, screenHeight) = GetScreenSize();

			// ── Before keyboard ──
			var leftBeforeRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftBeforeRect.X, Is.EqualTo(0),
				$"Before keyboard - left X ({leftBeforeRect.X}) should be = 0 (edge-to-edge)");

			var rightBeforeRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightBeforeEdge = rightBeforeRect.X + rightBeforeRect.Width;
			Assert.That(Math.Abs(rightBeforeEdge), Is.EqualTo(screenWidth),
				$"Before keyboard - right edge ({rightBeforeEdge}) should be = screenWidth ({screenWidth})");

			var bottomBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomBeforeRect.Bottom), Is.EqualTo(screenHeight),
				$"Before keyboard - bottom edge ({bottomBeforeRect.Bottom}) should be = screenHeight ({screenHeight})");

			// ── Show keyboard ──
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			// Bottom should NOT move (None ignores keyboard)
			var bottomDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomDuringRect.Bottom), Is.EqualTo(screenHeight),
				$"During keyboard - bottom edge ({bottomDuringRect.Bottom}) should remain at screenHeight ({screenHeight})");

			// Left/Right should remain unchanged
			var leftDuringRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftDuringRect.X, Is.EqualTo(0),
				$"During keyboard - left X ({leftDuringRect.X}) should remain at 0 (edge-to-edge)");

			var rightDuringRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightDuringEdge = rightDuringRect.X + rightDuringRect.Width;
			Assert.That(Math.Abs(rightDuringEdge), Is.EqualTo(screenWidth),
				$"During keyboard - right edge ({rightDuringEdge}) should remain at screenWidth ({screenWidth})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();

			var leftAfterRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftAfterRect.X, Is.EqualTo(leftBeforeRect.X),
				$"After keyboard - left X ({leftAfterRect.X}) should return to original ({leftBeforeRect.X})");

			var rightAfterRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightAfterEdge = rightAfterRect.X + rightAfterRect.Width;
			Assert.That(rightAfterEdge, Is.EqualTo(rightBeforeEdge),
				$"After keyboard - right edge ({rightAfterEdge}) should return to original ({rightBeforeEdge})");

			var bottomAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomAfterRect.Bottom, Is.EqualTo(bottomBeforeRect.Bottom),
				$"After keyboard - bottom edge ({bottomAfterRect.Bottom}) should return to original ({bottomBeforeRect.Bottom})");

			App.SetOrientationPortrait();
		}

		[Test, Order(29)]
		[Description("Landscape Container: bottom stays at safe area inset with keyboard, left/right stay inset")]
		public void ValidateKeyboard_Container_Landscape()
		{
			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			App.SetOrientationLandscape();

			var (screenWidth, screenHeight) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// ── Before keyboard ──
			var leftBeforeRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftBeforeRect.X), Is.EqualTo(insetsLandscape.Left),
				$"Before keyboard - left X ({leftBeforeRect.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			var rightBeforeRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightBeforeEdge = rightBeforeRect.X + rightBeforeRect.Width;
			Assert.That(Math.Abs(rightBeforeEdge), Is.EqualTo(screenWidth - insetsLandscape.Right),
				$"Before keyboard - right edge ({rightBeforeEdge}) should be = screenWidth - insetsLandscape.Right ({screenWidth - insetsLandscape.Right})");

			var bottomBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomBeforeRect.Bottom), Is.EqualTo(screenHeight - insetsLandscape.Bottom),
				$"Before keyboard - bottom edge ({bottomBeforeRect.Bottom}) should be = screenHeight - insetsLandscape.Bottom ({screenHeight - insetsLandscape.Bottom})");

			// ── Show keyboard ──
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			// Bottom should NOT move (Container ignores keyboard)
			var bottomDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomDuringRect.Bottom, Is.EqualTo(bottomBeforeRect.Bottom),
				$"During keyboard - bottom edge ({bottomDuringRect.Bottom}) should remain at ({bottomBeforeRect.Bottom})");

			// Left/Right should remain unchanged
			var leftDuringRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftDuringRect.X, Is.EqualTo(leftBeforeRect.X),
				$"During keyboard - left X ({leftDuringRect.X}) should remain at ({leftBeforeRect.X})");

			var rightDuringRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightDuringEdge = rightDuringRect.X + rightDuringRect.Width;
			Assert.That(rightDuringEdge, Is.EqualTo(rightBeforeEdge),
				$"During keyboard - right edge ({rightDuringEdge}) should remain at ({rightBeforeEdge})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();

			var leftAfterRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftAfterRect.X, Is.EqualTo(leftBeforeRect.X),
				$"After keyboard - left X ({leftAfterRect.X}) should return to original ({leftBeforeRect.X})");

			var rightAfterRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightAfterEdge = rightAfterRect.X + rightAfterRect.Width;
			Assert.That(rightAfterEdge, Is.EqualTo(rightBeforeEdge),
				$"After keyboard - right edge ({rightAfterEdge}) should return to original ({rightBeforeEdge})");

			var bottomAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomAfterRect.Bottom, Is.EqualTo(bottomBeforeRect.Bottom),
				$"After keyboard - bottom edge ({bottomAfterRect.Bottom}) should return to original ({bottomBeforeRect.Bottom})");

			App.SetOrientationPortrait();
		}
	}
}
