#if ANDROID || IOS
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

		/// <summary>
		/// Reads and parses safe area inset values from the SafeAreaInsetsLabel.
		/// Reuses the same platform-specific approach as Issue28986_SafeAreaBorderOrientation.
		/// Format: "L:{left},T:{top},R:{right},B:{bottom},KeyboardHeight:{keyboardHeight}"
		/// </summary>
		private (int Left, int Top, int Right, int Bottom, int KeyboardHeight) GetSafeAreaInsets()
		{
			var text = App.WaitForElement("SafeAreaInsetsLabel").GetText() ?? string.Empty;
			var match = System.Text.RegularExpressions.Regex.Match(text, @"L:(\d+),T:(\d+),R:(\d+),B:(\d+),KeyboardHeight:(\d+)");
			if (!match.Success)
				throw new InvalidOperationException($"Failed to parse safe area insets from: '{text}'");
			return (
				int.Parse(match.Groups[1].Value),
				int.Parse(match.Groups[2].Value),
				int.Parse(match.Groups[3].Value),
				int.Parse(match.Groups[4].Value),
				int.Parse(match.Groups[5].Value)
			);
		}

		private int GetKeyboardY()
		{
#if IOS
			return App.WaitForElement("Done").GetRect().Y;
#elif ANDROID
			// Calculate keyboard top Y position
			var (_, screenHeight) = GetScreenSize();
			var insets = GetSafeAreaInsets();
			return screenHeight - insets.KeyboardHeight;
#endif
		}

		public void ClickContentPageSafeAreaButton()
		{
			var isButtonPresent = App.FindElement("ContentPageSafeAreaButton");
			if (isButtonPresent != null)
			{
				App.WaitForElement("ContentPageSafeAreaButton");
				App.Tap("ContentPageSafeAreaButton");
			}
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
			ClickContentPageSafeAreaButton();

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
			ClickContentPageSafeAreaButton();

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
			ClickContentPageSafeAreaButton();

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
			ClickContentPageSafeAreaButton();

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
			ClickContentPageSafeAreaButton();

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
			ClickContentPageSafeAreaButton();

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
			ClickContentPageSafeAreaButton();

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
			ClickContentPageSafeAreaButton();

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
			ClickContentPageSafeAreaButton();
			App.DismissKeyboard();

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
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

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

#if !ANDROID // On Android, Appium does not find the bottom label when the keyboard is open
			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"Container (keyboard open): bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
#endif
			// ── Step 5: Dismiss keyboard ──
			App.DismissKeyboard();
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

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
			ClickContentPageSafeAreaButton();
			App.DismissKeyboard();

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
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

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
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

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
		[Description("With SoftInput, bottom indicator moves up when keyboard is shown and restores when dismissed")]
		public void ValidateKeyboard_SoftInput_BottomMovesUp()
		{
			ClickContentPageSafeAreaButton();
			App.DismissKeyboard();

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
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

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
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

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
		[Description("With None, bottom indicator does NOT move when keyboard is shown")]
		public void ValidateKeyboard_None_BottomStays()
		{
			ClickContentPageSafeAreaButton();
			App.DismissKeyboard();

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
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var topLabelDuringRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringRect.Y, Is.EqualTo(0),
				$"During keyboard - top label Y ({topLabelDuringRect.Y}) should be 0 (edge-to-edge)");

#if !ANDROID // On Android, Appium does not find the bottom label when the keyboard is open
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelDuringRect.Bottom), Is.EqualTo(screenHeight),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should be equal to screenHeight ({screenHeight})");
#endif
			App.DismissKeyboard();
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should return to original ({topLabelBeforeRect.Y})");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");

		}

		[Test, Order(14)]
		[Description("With Container, bottom indicator does NOT move when keyboard is shown")]
		public void ValidateKeyboard_Container_BottomStays()
		{
			ClickContentPageSafeAreaButton();
			App.DismissKeyboard();

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

			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

#if !ANDROID // On Android, Appium does not find the bottom label when the keyboard is open
			// Bottom should have not moved up to the keyboard top
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(screenHeight - insets.Bottom),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should equal (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
#endif
			// Top should remain unchanged
			var topLabelDuringRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"During keyboard - top label Y ({topLabelDuringRect.Y}) should remain at ({topLabelBeforeRect.Y})");

			App.DismissKeyboard();
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

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
			ClickContentPageSafeAreaButton();
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
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			// With None, bottom should NOT move
			var topLabelDuringNoneRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringNoneRect.Y, Is.EqualTo(0),
				$"During keyboard (None) - top label Y ({topLabelDuringNoneRect.Y}) should be 0 (edge-to-edge)");

#if !ANDROID // On Android, Appium does not find the bottom label when the keyboard is open
			var bottomLabelDuringNoneRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelDuringNoneRect.Bottom), Is.EqualTo(screenHeight),
				$"During keyboard (None) - bottom label Bottom ({bottomLabelDuringNoneRect.Bottom}) should be equal to screenHeight ({screenHeight})");
#endif
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
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

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
			ClickContentPageSafeAreaButton();
			App.DismissKeyboard();

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
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

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

#if !ANDROID // On Android, Appium does not find the bottom label when the keyboard is open
			var bottomLabelDuringNoneRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelDuringNoneRect.Bottom), Is.EqualTo(screenHeight),
				$"During keyboard (None) - bottom label Bottom ({bottomLabelDuringNoneRect.Bottom}) should be equal to screenHeight ({screenHeight})");
#endif
			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

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
			ClickContentPageSafeAreaButton();
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
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			// With Container, bottom should NOT move
			var topLabelDuringContainerRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringContainerRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"During keyboard (Container) - top label Y ({topLabelDuringContainerRect.Y}) should remain at ({topLabelBeforeRect.Y})");

#if !ANDROID // On Android, Appium does not find the bottom label when the keyboard is open
			var bottomLabelDuringContainerRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringContainerRect.Bottom, Is.EqualTo(screenHeight - insets.Bottom),
				$"During keyboard (Container) - bottom label Bottom ({bottomLabelDuringContainerRect.Bottom}) should equal (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
#endif
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
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

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
			ClickContentPageSafeAreaButton();
			App.DismissKeyboard();

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
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var keyboardY = GetKeyboardY();

			// ── Verify None with keyboard (no adjustment) ──
			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.EqualTo(0),
				$"None (keyboard open) - top label Y ({topLabelRect.Y}) should be 0 (edge-to-edge)");

#if !ANDROID // On Android, Appium does not find the bottom label when the keyboard is open
			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight),
				$"None (keyboard open) - bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to screenHeight ({screenHeight})");
#endif
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

#if !ANDROID // On Android, Appium does not find the bottom label when the keyboard is open
			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"Container (keyboard open) - bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
#endif
			// ── Switch to SoftInput (keyboard still open) ──
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

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

#if !ANDROID // On Android, Appium does not find the bottom label when the keyboard is open
			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"Default (keyboard open) - bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
#endif
			// ── Switch back to None (keyboard still open) ──
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.EqualTo(0),
				$"None (keyboard open, after cycle) - top label Y ({topLabelRect.Y}) should be 0 (edge-to-edge)");

#if !ANDROID // On Android, Appium does not find the bottom label when the keyboard is open
			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight),
				$"None (keyboard open, after cycle) - bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to screenHeight ({screenHeight})");
#endif
			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");
		}

		// ──────────────────────────────────────────────
		// Interaction with ContentPage Properties
		// ──────────────────────────────────────────────

		[Test, Order(19)]
		[Description("Safe area insets and padding are additive")]
		public void ValidateSafeArea_WithPadding()
		{
			ClickContentPageSafeAreaButton();

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
			ClickContentPageSafeAreaButton();

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
			ClickContentPageSafeAreaButton();

			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			App.SetOrientationLandscape();
			Thread.Sleep(1000);

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
			Thread.Sleep(1000);
		}

		[Test, Order(22)]
		[Description("All: landscape left/right/bottom inset by safe area")]
		public void ValidateOrientation_All_Landscape()
		{
			ClickContentPageSafeAreaButton();

			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			App.SetOrientationLandscape();
			Thread.Sleep(1000);

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
			Thread.Sleep(1000);
		}

		[Test, Order(23)]
		[Description("Container: landscape left/right/bottom inset by safe area")]
		public void ValidateOrientation_Container_Landscape()
		{
			ClickContentPageSafeAreaButton();

			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			App.SetOrientationLandscape();
			Thread.Sleep(1000);

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
			Thread.Sleep(1000);
		}

		[Test, Order(24)]
		[Description("SoftInput: landscape left/right inset by safe area, bottom edge-to-edge")]
		public void ValidateOrientation_SoftInput_Landscape()
		{
			ClickContentPageSafeAreaButton();

			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			App.SetOrientationLandscape();
			Thread.Sleep(1000);

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
			Thread.Sleep(1000);
		}

		[Test, Order(25)]
		[Description("Default: landscape left/right/bottom inset by safe area (Default behaves like Container)")]
		public void ValidateOrientation_Default_Landscape()
		{
			ClickContentPageSafeAreaButton();

			App.WaitForElement("SafeAreaDefaultButton");
			App.Tap("SafeAreaDefaultButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Default"));

			App.SetOrientationLandscape();
			Thread.Sleep(1000);

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
			Thread.Sleep(1000);
		}

		// ──────────────────────────────────────────────
		// Landscape Keyboard Position Validation
		// ──────────────────────────────────────────────

#if TEST_FAILS_ON_ANDROID // In landscape mode on Android, the keyboard covers the entire screen, and Appium cannot find elements to validate their positions

		[Test, Order(26)]
		[Description("Landscape All: bottom moves up to keyboard, left/right stay inset")]
		public void ValidateKeyboard_All_Landscape()
		{
			ClickContentPageSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			App.SetOrientationLandscape();
			Thread.Sleep(1000);

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
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

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
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

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
			Thread.Sleep(1000);
		}

		[Test, Order(27)]
		[Description("Landscape SoftInput: bottom moves up to keyboard, left/right stay inset")]
		public void ValidateKeyboard_SoftInput_Landscape()
		{
			ClickContentPageSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			App.SetOrientationLandscape();
			Thread.Sleep(1000);

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
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

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
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

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
			Thread.Sleep(1000);
		}

		[Test, Order(28)]
		[Description("Landscape None: bottom stays at screen edge with keyboard, left/right stay edge-to-edge")]
		public void ValidateKeyboard_None_Landscape()
		{
			ClickContentPageSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			App.SetOrientationLandscape();
			Thread.Sleep(1000);

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
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

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
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

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
			Thread.Sleep(1000);
		}

		[Test, Order(29)]
		[Description("Landscape Container: bottom stays at safe area inset with keyboard, left/right stay inset")]
		public void ValidateKeyboard_Container_Landscape()
		{
			ClickContentPageSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			App.SetOrientationLandscape();
			Thread.Sleep(1000);

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
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

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
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

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
			Thread.Sleep(1000);
		}

		[Test, Order(30)]
		[Description("Landscape Default: bottom stays at safe area inset with keyboard (behaves like Container)")]
		public void ValidateKeyboard_Default_Landscape()
		{
			ClickContentPageSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaDefaultButton");
			App.Tap("SafeAreaDefaultButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Default"));

			App.SetOrientationLandscape();
			Thread.Sleep(1000);

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
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			// Bottom should NOT move (Default behaves like Container)
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
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var bottomAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomAfterRect.Bottom, Is.EqualTo(bottomBeforeRect.Bottom),
				$"After keyboard - bottom edge ({bottomAfterRect.Bottom}) should return to original ({bottomBeforeRect.Bottom})");

			App.SetOrientationPortrait();
			Thread.Sleep(1000);
		}
#endif

		// ──────────────────────────────────────────────
		// Default + Keyboard (Portrait)
		// ──────────────────────────────────────────────

		[Test, Order(31)]
		[Description("With Default, bottom indicator does NOT move when keyboard is shown (behaves like Container)")]
		public void ValidateKeyboard_Default_BottomStays()
		{
			ClickContentPageSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaDefaultButton");
			App.Tap("SafeAreaDefaultButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Default"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Before keyboard ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelBeforeRect.Y), Is.EqualTo(insets.Top),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

#if !ANDROID // On Android, Appium does not find the bottom label when the keyboard is open
			// Bottom should NOT move (Default behaves like Container — ignores keyboard)
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(screenHeight - insets.Bottom),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should equal (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
#endif
			// Top should remain unchanged
			var topLabelDuringRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"During keyboard - top label Y ({topLabelDuringRect.Y}) should remain at ({topLabelBeforeRect.Y})");

			App.DismissKeyboard();
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should return to original ({topLabelBeforeRect.Y})");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
		}

		// ──────────────────────────────────────────────
		// Per-Edge + Keyboard (Portrait)
		// ──────────────────────────────────────────────

		[Test, Order(32)]
		[Description("Per-edge B:None + keyboard — bottom stays edge-to-edge when keyboard shows")]
		public void ValidatePerEdgeKeyboard_BottomNone_BottomStays()
		{
			ClickContentPageSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TopContainer");
			App.Tap("TopContainer");
			App.Tap("BottomNone");
			App.WaitForElement("Apply");
			App.Tap("Apply");

			App.WaitForElement("SafeAreaEdgesValueLabel");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("L:None, T:Container, R:None, B:None"));

			var (_, screenHeight) = GetScreenSize();

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

#if !ANDROID // On Android, Appium does not find the bottom label when the keyboard is open
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelDuringRect.Bottom), Is.EqualTo(screenHeight),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should stay at screenHeight ({screenHeight})");
#endif
			App.DismissKeyboard();
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
		}

		[Test, Order(33)]
		[Description("Per-edge B:Container + keyboard — bottom stays at safe area inset when keyboard shows")]
		public void ValidatePerEdgeKeyboard_BottomContainer_BottomStays()
		{
			ClickContentPageSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TopContainer");
			App.Tap("TopContainer");
			App.Tap("BottomContainer");
			App.WaitForElement("Apply");
			App.Tap("Apply");

			App.WaitForElement("SafeAreaEdgesValueLabel");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("L:None, T:Container, R:None, B:Container"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

#if !ANDROID // On Android, Appium does not find the bottom label when the keyboard is open
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(screenHeight - insets.Bottom),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should stay at (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
#endif
			App.DismissKeyboard();
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
		}

		[Test, Order(34)]
		[Description("Per-edge B:SoftInput + keyboard — bottom moves up to keyboard Y")]
		public void ValidatePerEdgeKeyboard_BottomSoftInput_BottomMovesUp()
		{
			ClickContentPageSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TopContainer");
			App.Tap("TopContainer");
			App.Tap("BottomSoftInput");
			App.WaitForElement("Apply");
			App.Tap("Apply");

			App.WaitForElement("SafeAreaEdgesValueLabel");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("L:None, T:Container, R:None, B:SoftInput"));

			var (_, screenHeight) = GetScreenSize();

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var keyboardY = GetKeyboardY();

			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(keyboardY),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should equal keyboard Y ({keyboardY})");

			App.DismissKeyboard();
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
		}

		[Test, Order(35)]
		[Description("Per-edge B:All + keyboard — bottom moves up to keyboard Y")]
		public void ValidatePerEdgeKeyboard_BottomAll_BottomMovesUp()
		{
			ClickContentPageSafeAreaButton();
			App.DismissKeyboard();

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

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var keyboardY = GetKeyboardY();

			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(keyboardY),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should equal keyboard Y ({keyboardY})");

			App.DismissKeyboard();
			Thread.Sleep(1000);
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
		}

		// ──────────────────────────────────────────────
		// Left/Right Per-Edge in Landscape
		// ──────────────────────────────────────────────

		[Test, Order(36)]
		[Description("Landscape per-edge: L:Container, R:None — left inset by safe area, right edge-to-edge")]
		public void ValidatePerEdge_LeftContainerRightNone_Landscape()
		{
			ClickContentPageSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("LeftContainer");
			App.Tap("LeftContainer");
			App.Tap("RightNone");
			App.WaitForElement("Apply");
			App.Tap("Apply");

			App.WaitForElement("SafeAreaEdgesValueLabel");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("L:Container, T:None, R:None, B:None"));

			App.SetOrientationLandscape();
			Thread.Sleep(1000);

			var (screenWidth, screenHeight) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// Left: inset by safe area (Container)
			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftRect.X), Is.EqualTo(insetsLandscape.Left),
				$"Left (Container): X ({leftRect.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			// Right: edge-to-edge (None)
			var rightRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdge = rightRect.X + rightRect.Width;
			Assert.That(Math.Abs(rightEdge), Is.EqualTo(screenWidth),
				$"Right (None): right edge ({rightEdge}) should be = screenWidth ({screenWidth})");

			App.SetOrientationPortrait();
			Thread.Sleep(1000);
		}

		// ──────────────────────────────────────────────
		// Orientation Roundtrip
		// ──────────────────────────────────────────────

		[Test, Order(37)]
		[Description("Rotate to landscape and back to portrait — positions restore correctly")]
		public void ValidateOrientation_Roundtrip_PositionsRestore()

		{
			ClickContentPageSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Record portrait positions ──
			var topPortraitRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			var bottomPortraitRect = App.WaitForElement("BottomEdgeIndicator").GetRect();

			Assert.That(Math.Abs(topPortraitRect.Y), Is.EqualTo(insets.Top),
				$"Portrait: top label Y ({topPortraitRect.Y}) should be equal to insets.Top ({insets.Top})");
			Assert.That(Math.Abs(bottomPortraitRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom),
				$"Portrait: bottom label Bottom ({bottomPortraitRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// ── Rotate to landscape ──
			App.SetOrientationLandscape();
			Thread.Sleep(1000);

			var (screenWidthLandscape, screenHeightLandscape) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			var topLandscapeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLandscapeRect.Y), Is.EqualTo(insetsLandscape.Top),
				$"Landscape: top label Y ({topLandscapeRect.Y}) should be equal to insetsLandscape.Top ({insetsLandscape.Top})");

			var leftLandscapeRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftLandscapeRect.X), Is.EqualTo(insetsLandscape.Left),
				$"Landscape: left X ({leftLandscapeRect.X}) should be equal to insetsLandscape.Left ({insetsLandscape.Left})");

			var rightLandscapeRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightLandscapeEdge = rightLandscapeRect.X + rightLandscapeRect.Width;
			Assert.That(Math.Abs(rightLandscapeEdge), Is.EqualTo(screenWidthLandscape - insetsLandscape.Right),
				$"Landscape: right edge ({rightLandscapeEdge}) should be equal to screenWidth - insetsLandscape.Right ({screenWidthLandscape - insetsLandscape.Right})");

			// ── Rotate back to portrait ──
			App.SetOrientationPortrait();
			Thread.Sleep(1000);

			var insetsAfter = GetSafeAreaInsets();
			var (_, screenHeightAfter) = GetScreenSize();

			var topAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topAfterRect.Y), Is.EqualTo(insetsAfter.Top),
				$"After roundtrip: top label Y ({topAfterRect.Y}) should be equal to insets.Top ({insetsAfter.Top})");

			var bottomAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomAfterRect.Bottom), Is.EqualTo(screenHeightAfter - insetsAfter.Bottom),
				$"After roundtrip: bottom label Bottom ({bottomAfterRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeightAfter - insetsAfter.Bottom})");

			// Verify positions match the original portrait positions
			Assert.That(topAfterRect.Y, Is.EqualTo(topPortraitRect.Y),
				$"After roundtrip: top label Y ({topAfterRect.Y}) should match original portrait ({topPortraitRect.Y})");

			Assert.That(bottomAfterRect.Bottom, Is.EqualTo(bottomPortraitRect.Bottom),
				$"After roundtrip: bottom label Bottom ({bottomAfterRect.Bottom}) should match original portrait ({bottomPortraitRect.Bottom})");
		}
	}
}
#endif