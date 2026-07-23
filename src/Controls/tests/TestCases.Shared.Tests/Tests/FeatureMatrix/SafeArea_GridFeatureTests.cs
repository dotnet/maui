#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	[Category(UITestCategories.SafeAreaEdges)]
	public class SafeArea_GridFeatureTests : _GalleryUITest
	{
		public const string SafeAreaFeatureMatrix = "SafeArea Feature Matrix";
		public override string GalleryPageName => SafeAreaFeatureMatrix;
		private const int PixelTolerance = 1;

		public SafeArea_GridFeatureTests(TestDevice device)
			: base(device)
		{
		}

		/// <summary>
		/// Reads and parses safe area inset values from the SafeAreaInsetsLabel.
		/// Format: "L:{left},T:{top},R:{right},B:{bottom},KH:{keyboardHeight},CoL:{cutoutLeft},CoR:{cutoutRight}"
		/// </summary>
		private (int Left, int Top, int Right, int Bottom, int KeyboardHeight, int CutoutL, int CutoutR) GetSafeAreaInsets()
		{
			var text = App.WaitForElement("SafeAreaInsetsLabel").GetText() ?? string.Empty;
			var match = System.Text.RegularExpressions.Regex.Match(text, @"L:(\d+),T:(\d+),R:(\d+),B:(\d+),KH:(\d+),CoL:(\d+),CoR:(\d+)");
			if (!match.Success)
				throw new InvalidOperationException($"Failed to parse safe area insets from: '{text}'");
			return (
				int.Parse(match.Groups[1].Value),
				int.Parse(match.Groups[2].Value),
				int.Parse(match.Groups[3].Value),
				int.Parse(match.Groups[4].Value),
				int.Parse(match.Groups[5].Value),
				int.Parse(match.Groups[6].Value),
				int.Parse(match.Groups[7].Value)
			);
		}

		private int GetKeyboardY()
		{
#if IOS
			if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
			{
				var rect = App.WaitForElement("Toolbar").GetRect();
				return rect.Y;
			}
			else
			{
				var rect = App.WaitForElement("Done").GetRect();
				return rect.Y;
			}
#elif ANDROID
			var (_, screenHeight) = GetScreenSize();
			var insets = GetSafeAreaInsets();
			return screenHeight - insets.KeyboardHeight;
#endif
		}

		/// <summary>
		/// Navigates to SafeAreaGridPage from the feature matrix main page.
		/// If already on SafeAreaGridPage (button not visible), skips navigation.
		/// </summary>
		public void ClickGridSafeAreaButton()
		{
			var isButtonPresent = App.FindElement("GridSafeAreaButton");
			if (isButtonPresent != null)
			{
				App.WaitForElement("GridSafeAreaButton");
				App.Tap("GridSafeAreaButton");
			}
		}

		private (int Width, int Height) GetScreenSize()
		{
			var size = ((AppiumApp)App).Driver.Manage().Window.Size;
			return (size.Width, size.Height);
		}

		/// <summary>
		/// Returns the effective right inset to use in landscape orientation.
		/// On Android, the standard safe area <paramref name="right"/> inset is not updated correctly
		/// when rotating to landscape. The display cutout value (<paramref name="cutoutR"/>) is correctly
		/// updated and reflects the actual physical notch/cutout position in landscape,
		/// so it is used instead.
		/// On iOS, the regular <paramref name="right"/> safe area inset is accurate in all orientations.
		/// </summary>
		private int GetLandscapeRightInset(int right, int cutoutR)
		{
#if ANDROID
			return cutoutR;
#else
			return right;
#endif
		}

		// ──────────────────────────────────────────────
		// Uniform SafeAreaRegions via Buttons
		// ──────────────────────────────────────────────

		[Test, Order(1)]
		[Description("Grid content extends edge-to-edge behind system bars/notch")]
		public void ValidateSafeAreaEdges_None_Grid()
		{
			ClickGridSafeAreaButton();

			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			// Portrait: top label Y should be ≈ 0 (edge-to-edge, Grid applies no safe area padding)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.EqualTo(0).Within(PixelTolerance),
				$"None: top label Y ({topLabelRect.Y}) should be = 0 (edge-to-edge)");

			var (_, screenHeight) = GetScreenSize();

			// Portrait: bottom label bottom edge should be ≈ screenHeight (edge-to-edge, no safe area applied)
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"None: bottom label Y ({bottomLabelRect.Bottom}) should be ≈ screenHeight ({screenHeight})");
		}

		[Test, Order(2)]
		[Description("Grid content inset from all system UI (status bar, nav bar, notch, home indicator)")]
		public void ValidateSafeAreaEdges_All_Grid()
		{
			ClickGridSafeAreaButton();

			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// Portrait: top label Y should be ≈ insets.Top (safe area)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"All: top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ screenBottom - insets.Bottom
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"All: bottom label Y ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
		}

		[Test, Order(3)]
		[Description("Grid content avoids system bars/notch but can extend under keyboard area")]
		public void ValidateSafeAreaEdges_Container_Grid()
		{
			ClickGridSafeAreaButton();

			App.Tap("SafeAreaContainerButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// Portrait: top label Y should be ≈ insets.Top (safe area)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"Container: top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ screenBottom - insets.Bottom
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"Container: bottom label Y ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
		}

		[Test, Order(4)]
		[Description("Grid SoftInput respects safe area on top/sides but bottom is edge-to-edge without keyboard")]
		public void ValidateSafeAreaEdges_SoftInput_Grid()
		{
			ClickGridSafeAreaButton();

			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// Portrait: top label Y should be ≈ insets.Top (safe area)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"SoftInput: top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ screenHeight (edge-to-edge, no safe area applied)
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"SoftInput: bottom label Y ({bottomLabelRect.Bottom}) should be equal to screenHeight ({screenHeight})");
		}

		[Test, Order(5)]
		[Description("Default on Grid behaves as Container — content inset from system UI (status bar, notch, home indicator)")]
		public void ValidateSafeAreaEdges_Default_Grid()
		{
			ClickGridSafeAreaButton();

			App.WaitForElement("SafeAreaDefaultButton");
			App.Tap("SafeAreaDefaultButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Default"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// Portrait: top label Y should be ≈ insets.Top (Default on Grid = Container)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"Default: top label Y ({topLabelRect.Y}) should be ≈ insets.Top ({insets.Top}) (Default on Grid = Container)");

			// Portrait: bottom label bottom edge should be ≈ screenHeight - insets.Bottom (Default on Grid = Container)
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"Default: bottom label Bottom ({bottomLabelRect.Bottom}) should be ≈ screenHeight - insets.Bottom ({screenHeight - insets.Bottom}) (Default on Grid = Container)");
		}

		// ──────────────────────────────────────────────
		// Per-Edge Configuration (via Options)
		// ──────────────────────────────────────────────

		[Test, Order(6)]
		[Description("Only top avoids status bar/notch. Bottom edge-to-edge.")]
		public void ValidatePerEdge_TopContainerOnly_Grid()
		{
			ClickGridSafeAreaButton();

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

			// Portrait: Container — Border top padding absorbs safe area
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"Top (Container): label Y ({topLabelRect.Y}) should be ≈ insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ screenHeight (edge-to-edge, no safe area applied)
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"None: bottom label Y ({bottomLabelRect.Bottom}) should be ≈ screenHeight ({screenHeight})");
		}

		[Test, Order(7)]
		[Description("Top avoids system bars; bottom avoids only keyboard")]
		public void ValidatePerEdge_BottomSoftInput_TopContainer_Grid()
		{
			ClickGridSafeAreaButton();

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
			// Top: Container — Border top padding absorbs safe area
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"Top (Container): label Y ({topLabelRect.Y}) should be ≈ insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ screenHeight (edge-to-edge, SoftInput without keyboard)
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"SoftInput: bottom label Y ({bottomLabelRect.Bottom}) should be ≈ screenHeight ({screenHeight})");
		}

		[Test, Order(8)]
		[Description("Top/bottom respect all insets")]
		public void ValidatePerEdge_TopBottomAll_SidesNone_Grid()
		{
			ClickGridSafeAreaButton();

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

			// Portrait: top label Y should be ≈ insets.Top (safe area)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"All: top label Y ({topLabelRect.Y}) should be = insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ screenBottom - insets.Bottom
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"All: bottom label Y ({bottomLabelRect.Bottom}) should be = (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
		}

		[Test, Order(9)]
		[Description("Each edge independently applies its behavior")]
		public void ValidatePerEdge_AllDifferent_Grid()
		{
			ClickGridSafeAreaButton();

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

			// Portrait: top label Y should be ≈ insets.Top (safe area)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"Container: top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ screenBottom - insets.Bottom
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"All: bottom label Y ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
		}

		// ──────────────────────────────────────────────
		// Keyboard Interaction (SoftInput)
		// ──────────────────────────────────────────────

		[Test, Order(10)]
		[Description("None → All → keyboard open → Container → dismiss → All: positions correct at each step")]
		public void ValidateSafeArea_NoneThenAllKeyboardContainerDismissAll_Grid()
		{
			ClickGridSafeAreaButton();
			App.DismissKeyboard();

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Step 1: Click None and verify ──
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.EqualTo(0).Within(PixelTolerance),
				$"None: top label Y ({topLabelRect.Y}) should be 0 (edge-to-edge)");

			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"None: bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// ── Step 2: Click All and verify ──
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"All: top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"All: bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// ── Step 3: Open keyboard and verify (All adjusts for keyboard) ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var keyboardY = GetKeyboardY();

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"All (keyboard open): top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelRect.Bottom, Is.EqualTo(keyboardY).Within(PixelTolerance),
				$"All (keyboard open): bottom label Bottom ({bottomLabelRect.Bottom}) should equal keyboardY ({keyboardY})");

			// ── Step 4: Switch to Container while keyboard is open ──
			App.Tap("SafeAreaContainerButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"Container (keyboard open): top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

#if IOS
			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"Container (keyboard open): bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
#else
			App.WaitForNoElement("BottomEdgeIndicator"); // On Android, Appium does not find the bottom label when the keyboard is open
#endif
			// ── Step 5: Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			// ── Step 6: Click All and verify ──
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"All (after dismiss): top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"All (after dismiss): bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
		}

		// ──────────────────────────────────────────────
		// Keyboard Position Validation
		// ──────────────────────────────────────────────
		// Validates that the bottom indicator moves up when keyboard is shown with modes that
		// adjust for keyboard (All/SoftInput), and does NOT move with modes that don't (None/Container).

		[Test, Order(11)]
		[Description("With All, bottom indicator moves up when keyboard is shown and restores when dismissed")]
		public void ValidateKeyboard_All_BottomMovesUp_Grid()
		{
			ClickGridSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Before keyboard ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelBeforeRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// ── Show keyboard ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var keyboardY = GetKeyboardY();

			// Bottom should have moved up to the keyboard top
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(keyboardY).Within(PixelTolerance),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should equal keyboardY ({keyboardY})");

			// Top should remain unchanged
			var topLabelDuringRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringRect.Y, Is.EqualTo(topLabelBeforeRect.Y).Within(PixelTolerance),
				$"During keyboard - top label Y ({topLabelDuringRect.Y}) should remain at ({topLabelBeforeRect.Y})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			// Top should return to its original position
			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(topLabelBeforeRect.Y).Within(PixelTolerance),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should return to original ({topLabelBeforeRect.Y})");

			// Bottom should return to its original position
			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom).Within(PixelTolerance),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
		}

		[Test, Order(12)]
		[Description("With SoftInput, bottom indicator moves up when keyboard is shown and restores when dismissed")]
		public void ValidateKeyboard_SoftInput_BottomMovesUp_Grid()
		{
			ClickGridSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Before keyboard ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelBeforeRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// ── Show keyboard ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var keyboardY = GetKeyboardY();

			// Bottom should have moved up to the keyboard top
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(keyboardY).Within(PixelTolerance),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should equal keyboardY ({keyboardY})");

			// Top should remain unchanged
			var topLabelDuringRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringRect.Y, Is.EqualTo(topLabelBeforeRect.Y).Within(PixelTolerance),
				$"During keyboard - top label Y ({topLabelDuringRect.Y}) should remain at ({topLabelBeforeRect.Y})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			// Top should return to its original position
			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(topLabelBeforeRect.Y).Within(PixelTolerance),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should return to original ({topLabelBeforeRect.Y})");

			// Bottom should return to its original position
			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom).Within(PixelTolerance),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
		}

		[Test, Order(13)]
		[Description("With None, bottom indicator does NOT move when keyboard is shown")]
		public void ValidateKeyboard_None_BottomStays_Grid()
		{
			ClickGridSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			var (_, screenHeight) = GetScreenSize();

			// ── Before keyboard ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelBeforeRect.Y, Is.EqualTo(0).Within(PixelTolerance),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be 0 (edge-to-edge)");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// ── Show keyboard ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var topLabelDuringRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringRect.Y, Is.EqualTo(0).Within(PixelTolerance),
				$"During keyboard - top label Y ({topLabelDuringRect.Y}) should be 0 (edge-to-edge)");

#if IOS
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelDuringRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should be equal to screenHeight ({screenHeight})");
#else
			App.WaitForNoElement("BottomEdgeIndicator"); // On Android, Appium does not find the bottom label when the keyboard is open
#endif
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(topLabelBeforeRect.Y).Within(PixelTolerance),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should return to original ({topLabelBeforeRect.Y})");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom).Within(PixelTolerance),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
		}

		[Test, Order(14)]
		[Description("With Container, bottom indicator does NOT move when keyboard is shown")]
		public void ValidateKeyboard_Container_BottomStays_Grid()
		{
			ClickGridSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Before keyboard ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelBeforeRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

#if IOS
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should equal (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
#else
			App.WaitForNoElement("BottomEdgeIndicator"); // On Android, Appium does not find the bottom label when the keyboard is open
#endif
			// Top should remain unchanged
			var topLabelDuringRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringRect.Y, Is.EqualTo(topLabelBeforeRect.Y).Within(PixelTolerance),
				$"During keyboard - top label Y ({topLabelDuringRect.Y}) should remain at ({topLabelBeforeRect.Y})");

			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			// Top should return to its original position
			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(topLabelBeforeRect.Y).Within(PixelTolerance),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should return to original ({topLabelBeforeRect.Y})");

			// Bottom should return to its original position
			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom).Within(PixelTolerance),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
		}

		// ──────────────────────────────────────────────
		// Keyboard + Runtime SafeArea Changes
		// ──────────────────────────────────────────────

#if TEST_FAILS_ON_IOS // Issue Link - https://github.com/dotnet/maui/issues/34847

		[Test, Order(15)]
		[Description("Switch None to All while keyboard is open — bottom indicator moves up")]
		public void ValidateKeyboardRuntime_SwitchNoneToAll_WhileKeyboardOpen_Grid()
		{
			ClickGridSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Before keyboard (None) ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelBeforeRect.Y, Is.EqualTo(0).Within(PixelTolerance),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be 0 (edge-to-edge)");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// ── Show keyboard (None) ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			// With None, bottom should NOT move
			var topLabelDuringNoneRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringNoneRect.Y, Is.EqualTo(0).Within(PixelTolerance),
				$"During keyboard (None) - top label Y ({topLabelDuringNoneRect.Y}) should be 0 (edge-to-edge)");

#if IOS
			var bottomLabelDuringNoneRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelDuringNoneRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"During keyboard (None) - bottom label Bottom ({bottomLabelDuringNoneRect.Bottom}) should be equal to screenHeight ({screenHeight})");
#else
			App.WaitForNoElement("BottomEdgeIndicator"); // On Android, Appium does not find the bottom label when the keyboard is open
#endif
			// ── Switch to All while keyboard is open ──
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var keyboardY = GetKeyboardY();

			// With All, bottom should move up to keyboard top
			var topLabelDuringAllRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelDuringAllRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"During keyboard (All) - top label Y ({topLabelDuringAllRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelDuringAllRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringAllRect.Bottom, Is.EqualTo(keyboardY).Within(PixelTolerance),
				$"During keyboard (All) - bottom label Bottom ({bottomLabelDuringAllRect.Bottom}) should equal keyboardY ({keyboardY})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			// After keyboard (All): top at insets.Top, bottom at (screenHeight - insets.Bottom)
			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelAfterRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelAfterRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
		}

		[Test, Order(16)]
		[Description("Switch None to SoftInput while keyboard is open — bottom indicator moves up")]
		public void ValidateKeyboardRuntime_SwitchNoneToSoftInput_WhileKeyboardOpen_Grid()
		{
			ClickGridSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Before keyboard (None) ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelBeforeRect.Y, Is.EqualTo(0).Within(PixelTolerance),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be 0 (edge-to-edge)");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// ── Show keyboard (None) ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			// With None, bottom should NOT move
			var topLabelDuringNoneRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringNoneRect.Y, Is.EqualTo(0).Within(PixelTolerance),
				$"During keyboard (None) - top label Y ({topLabelDuringNoneRect.Y}) should be 0 (edge-to-edge)");

#if IOS
			var bottomLabelDuringNoneRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelDuringNoneRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"During keyboard (None) - bottom label Bottom ({bottomLabelDuringNoneRect.Bottom}) should be equal to screenHeight ({screenHeight})");
#else
			App.WaitForNoElement("BottomEdgeIndicator"); // On Android, Appium does not find the bottom label when the keyboard is open
#endif
			// ── Switch to SoftInput while keyboard is open ──
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			var keyboardY = GetKeyboardY();

			// With SoftInput, bottom should move up to keyboard top
			var topLabelDuringSoftInputRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelDuringSoftInputRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"During keyboard (SoftInput) - top label Y ({topLabelDuringSoftInputRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelDuringSoftInputRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringSoftInputRect.Bottom, Is.EqualTo(keyboardY).Within(PixelTolerance),
				$"During keyboard (SoftInput) - bottom label Bottom ({bottomLabelDuringSoftInputRect.Bottom}) should equal keyboardY ({keyboardY})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			// After keyboard (SoftInput): top at insets.Top, bottom at screenHeight (edge-to-edge without keyboard)
			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelAfterRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelAfterRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should be equal to screenHeight ({screenHeight})");
		}
#endif

		[Test, Order(17)]
		[Description("Switch All to None while keyboard is open — bottom indicator drops back")]
		public void ValidateKeyboardRuntime_SwitchAllToNone_WhileKeyboardOpen_Grid()
		{
			ClickGridSafeAreaButton();
			App.DismissKeyboard();

			// Navigate to Options to reset the ViewModel before the test
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
			Assert.That(Math.Abs(topLabelBeforeRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// ── Show keyboard (All) ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var keyboardY = GetKeyboardY();

			// With All, bottom should move up to keyboard top
			var topLabelDuringAllRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringAllRect.Y, Is.EqualTo(topLabelBeforeRect.Y).Within(PixelTolerance),
				$"During keyboard (All) - top label Y ({topLabelDuringAllRect.Y}) should remain at ({topLabelBeforeRect.Y})");

			var bottomLabelDuringAllRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringAllRect.Bottom, Is.EqualTo(keyboardY).Within(PixelTolerance),
				$"During keyboard (All) - bottom label Bottom ({bottomLabelDuringAllRect.Bottom}) should equal keyboardY ({keyboardY})");

			// ── Switch to None while keyboard is open ──
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			// With None, top goes edge-to-edge; bottom does NOT adjust for keyboard
			var topLabelDuringNoneRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringNoneRect.Y, Is.EqualTo(0).Within(PixelTolerance),
				$"During keyboard (None) - top label Y ({topLabelDuringNoneRect.Y}) should be 0 (edge-to-edge)");

#if IOS
			var bottomLabelDuringNoneRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelDuringNoneRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"During keyboard (None) - bottom label Bottom ({bottomLabelDuringNoneRect.Bottom}) should be equal to screenHeight ({screenHeight})");
#else
			App.WaitForNoElement("BottomEdgeIndicator"); // On Android, Appium does not find the bottom label when the keyboard is open
#endif
			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			// After keyboard (None): top at 0, bottom at screenHeight
			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(0).Within(PixelTolerance),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should be 0 (edge-to-edge)");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelAfterRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should be equal to screenHeight ({screenHeight})");
		}

		[Test, Order(18)]
		[Description("Switch Container to SoftInput while keyboard is open — bottom indicator moves up")]
		public void ValidateKeyboardRuntime_SwitchContainerToSoftInput_WhileKeyboardOpen_Grid()
		{
			ClickGridSafeAreaButton();
			App.DismissKeyboard();

			// Navigate to Options to reset the ViewModel before the test
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
			Assert.That(Math.Abs(topLabelBeforeRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// ── Show keyboard (Container) ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			// With Container, bottom should NOT move
			var topLabelDuringContainerRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringContainerRect.Y, Is.EqualTo(topLabelBeforeRect.Y).Within(PixelTolerance),
				$"During keyboard (Container) - top label Y ({topLabelDuringContainerRect.Y}) should remain at ({topLabelBeforeRect.Y})");

#if IOS
			var bottomLabelDuringContainerRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringContainerRect.Bottom, Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"During keyboard (Container) - bottom label Bottom ({bottomLabelDuringContainerRect.Bottom}) should equal (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
#else
			App.WaitForNoElement("BottomEdgeIndicator"); // On Android, Appium does not find the bottom label when the keyboard is open
#endif
			// ── Switch to SoftInput while keyboard is open ──
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			var keyboardY = GetKeyboardY();

			// With SoftInput, bottom should move up to keyboard top
			var topLabelDuringSoftInputRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelDuringSoftInputRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"During keyboard (SoftInput) - top label Y ({topLabelDuringSoftInputRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelDuringSoftInputRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringSoftInputRect.Bottom, Is.EqualTo(keyboardY).Within(PixelTolerance),
				$"During keyboard (SoftInput) - bottom label Bottom ({bottomLabelDuringSoftInputRect.Bottom}) should equal keyboardY ({keyboardY})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			// After keyboard (SoftInput): top at insets.Top, bottom at screenHeight (edge-to-edge without keyboard)
			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelAfterRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelAfterRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should be equal to screenHeight ({screenHeight})");
		}

#if TEST_FAILS_ON_IOS // Issue Link - https://github.com/dotnet/maui/issues/34847

		[Test, Order(19)]
		[Description("Keyboard open: cycle through None → All → Container → SoftInput → Default → None and verify positions")]
		public void ValidateKeyboardRuntime_CycleThroughAllModes_WhileKeyboardOpen_Grid()
		{
			ClickGridSafeAreaButton();
			App.DismissKeyboard();

			// ── Start with None ──
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Verify None positions before keyboard ──
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.EqualTo(0).Within(PixelTolerance),
				$"None (before keyboard) - top label Y ({topLabelRect.Y}) should be 0 (edge-to-edge)");

			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"None (before keyboard) - bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// ── Open keyboard ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var keyboardY = GetKeyboardY();

			// ── Verify None with keyboard (no adjustment) ──
			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.EqualTo(0).Within(PixelTolerance),
				$"None (keyboard open) - top label Y ({topLabelRect.Y}) should be 0 (edge-to-edge)");

#if IOS
			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"None (keyboard open) - bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to screenHeight ({screenHeight})");
#else
			App.WaitForNoElement("BottomEdgeIndicator"); // On Android, Appium does not find the bottom label when the keyboard is open
#endif
			// ── Switch to All (keyboard still open) ──
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			keyboardY = GetKeyboardY();

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"All (keyboard open) - top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelRect.Bottom, Is.EqualTo(keyboardY).Within(PixelTolerance),
				$"All (keyboard open) - bottom label Bottom ({bottomLabelRect.Bottom}) should equal keyboardY ({keyboardY})");

			// ── Switch to Container (keyboard still open) ──
			App.Tap("SafeAreaContainerButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"Container (keyboard open) - top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

#if IOS
			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"Container (keyboard open) - bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
#else
			App.WaitForNoElement("BottomEdgeIndicator"); // On Android, Appium does not find the bottom label when the keyboard is open
#endif
			// ── Switch to SoftInput (keyboard still open) ──
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"SoftInput (keyboard open) - top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelRect.Bottom, Is.EqualTo(keyboardY).Within(PixelTolerance),
				$"SoftInput (keyboard open) - bottom label Bottom ({bottomLabelRect.Bottom}) should equal keyboardY ({keyboardY})");

			// ── Switch to Default (keyboard still open) ── (Default on Grid = Container — inset from system UI, ignores keyboard)
			App.Tap("SafeAreaDefaultButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Default"));

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"Default (keyboard open) - top label Y ({topLabelRect.Y}) should be ≈ insets.Top ({insets.Top}) (Default on Grid = Container)");

#if IOS
			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"Default (keyboard open) - bottom label Bottom ({bottomLabelRect.Bottom}) should be ≈ screenHeight - insets.Bottom ({screenHeight - insets.Bottom}) (Default on Grid = Container, ignores keyboard)");
#else
			App.WaitForNoElement("BottomEdgeIndicator"); // On Android, Appium does not find the bottom label when the keyboard is open
#endif
			// ── Switch back to None (keyboard still open) ──
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.EqualTo(0).Within(PixelTolerance),
				$"None (keyboard open, after cycle) - top label Y ({topLabelRect.Y}) should be 0 (edge-to-edge)");

#if IOS
			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"None (keyboard open, after cycle) - bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to screenHeight ({screenHeight})");
#else
			App.WaitForNoElement("BottomEdgeIndicator"); // On Android, Appium does not find the bottom label when the keyboard is open
#endif
			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");
		}
#endif

		// ──────────────────────────────────────────────
		// Interaction with Grid Properties
		// ──────────────────────────────────────────────

		[Test, Order(20)]
		[Description("Safe area insets and Grid padding are additive")]
		public void ValidateSafeArea_WithPadding_Grid()
		{
			ClickGridSafeAreaButton();

			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("UniformAll");
			App.Tap("UniformAll");
			App.WaitForElement("PaddingCheckBox");
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

		[Test, Order(21)]
		[Description("Grid background extends edge-to-edge behind system UI")]
		public void ValidateSafeArea_None_WithBackground_Grid()
		{
			ClickGridSafeAreaButton();

			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("UniformNone");
			App.Tap("UniformNone");
			App.WaitForElement("BackgroundCheckBox");
			App.Tap("BackgroundCheckBox");
			App.WaitForElement("Apply");
			App.Tap("Apply");

			App.WaitForElement("SafeAreaEdgesValueLabel");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));
		}

		// ──────────────────────────────────────────────
		// Orientation / Landscape Validation
		// ──────────────────────────────────────────────

		[Test, Order(22)]
		[Description("None: landscape left/right/bottom all edge-to-edge")]
		public void ValidateOrientation_None_Landscape_Grid()
		{
			ClickGridSafeAreaButton();

			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			App.SetOrientationLandscape();
			App.WaitForElement("SafeAreaEdgesValueLabel");

			var (screenWidth, screenHeight) = GetScreenSize();

			// Left: edge-to-edge
			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftRect.X, Is.EqualTo(0).Within(PixelTolerance),
				$"None: left X ({leftRect.X}) should be = 0 (edge-to-edge)");

			// Right: edge-to-edge
			var rightRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdge = rightRect.X + rightRect.Width;
			Assert.That(Math.Abs(rightEdge), Is.EqualTo(screenWidth).Within(PixelTolerance),
				$"None: right edge ({rightEdge}) should be = screenWidth ({screenWidth})");

			// Bottom: edge-to-edge
			var bottomRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"None: bottom edge ({bottomRect.Bottom}) should be = screenHeight ({screenHeight})");

			App.SetOrientationPortrait();
			App.WaitForElement("SafeAreaEdgesValueLabel");
		}

		[Test, Order(23)]
		[Description("All: landscape left/right/bottom inset by safe area")]
		public void ValidateOrientation_All_Landscape_Grid()
		{
			ClickGridSafeAreaButton();

			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			App.SetOrientationLandscape();
			App.WaitForElement("SafeAreaEdgesValueLabel");

			var (screenWidth, screenHeight) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// Left: inset by safe area
			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftRect.X), Is.EqualTo(insetsLandscape.Left).Within(PixelTolerance),
				$"All: left X ({leftRect.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			// Right: inset by safe area (Android uses display cutout for right inset)
			var rightRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdge = rightRect.X + rightRect.Width;
			var expectedRight = GetLandscapeRightInset(insetsLandscape.Right, insetsLandscape.CutoutR);
			Assert.That(Math.Abs(rightEdge), Is.EqualTo(screenWidth - expectedRight).Within(PixelTolerance),
				$"All: right edge ({rightEdge}) should be = screenWidth - expectedRight ({screenWidth - expectedRight})");

			// Bottom: inset by safe area
			var bottomRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomRect.Bottom), Is.EqualTo(screenHeight - insetsLandscape.Bottom).Within(PixelTolerance),
				$"All: bottom edge ({bottomRect.Bottom}) should be = screenHeight - insetsLandscape.Bottom ({screenHeight - insetsLandscape.Bottom})");

			App.SetOrientationPortrait();
			App.WaitForElement("SafeAreaEdgesValueLabel");
		}

		[Test, Order(24)]
		[Description("Container: landscape left/right/bottom inset by safe area")]
		public void ValidateOrientation_Container_Landscape_Grid()
		{
			ClickGridSafeAreaButton();

			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			App.SetOrientationLandscape();
			App.WaitForElement("SafeAreaEdgesValueLabel");

			var (screenWidth, screenHeight) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// Left: inset by safe area
			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftRect.X), Is.EqualTo(insetsLandscape.Left).Within(PixelTolerance),
				$"Container: left X ({leftRect.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			// Right: inset by safe area (Android uses display cutout for right inset)
			var rightRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdge = rightRect.X + rightRect.Width;
			var expectedRight = GetLandscapeRightInset(insetsLandscape.Right, insetsLandscape.CutoutR);
			Assert.That(Math.Abs(rightEdge), Is.EqualTo(screenWidth - expectedRight).Within(PixelTolerance),
				$"Container: right edge ({rightEdge}) should be = screenWidth - expectedRight ({screenWidth - expectedRight})");

			// Bottom: inset by safe area
			var bottomRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomRect.Bottom), Is.EqualTo(screenHeight - insetsLandscape.Bottom).Within(PixelTolerance),
				$"Container: bottom edge ({bottomRect.Bottom}) should be = screenHeight - insetsLandscape.Bottom ({screenHeight - insetsLandscape.Bottom})");

			App.SetOrientationPortrait();
			App.WaitForElement("SafeAreaEdgesValueLabel");
		}

		[Test, Order(25)]
		[Description("SoftInput: landscape left/right inset by safe area, bottom edge-to-edge")]
		public void ValidateOrientation_SoftInput_Landscape_Grid()
		{
			ClickGridSafeAreaButton();

			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			App.SetOrientationLandscape();
			App.WaitForElement("SafeAreaEdgesValueLabel");

			var (screenWidth, screenHeight) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// Left: inset by safe area
			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftRect.X), Is.EqualTo(insetsLandscape.Left).Within(PixelTolerance),
				$"SoftInput: left X ({leftRect.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			// Right: inset by safe area (Android uses display cutout for right inset)
			var rightRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdge = rightRect.X + rightRect.Width;
			var expectedRight = GetLandscapeRightInset(insetsLandscape.Right, insetsLandscape.CutoutR);
			Assert.That(Math.Abs(rightEdge), Is.EqualTo(screenWidth - expectedRight).Within(PixelTolerance),
				$"SoftInput: right edge ({rightEdge}) should be = screenWidth - expectedRight ({screenWidth - expectedRight})");

			// Bottom: edge-to-edge (SoftInput doesn't avoid bottom without keyboard)
			var bottomRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"SoftInput: bottom edge ({bottomRect.Bottom}) should be = screenHeight ({screenHeight})");

			App.SetOrientationPortrait();
			App.WaitForElement("SafeAreaEdgesValueLabel");
		}

		[Test, Order(26)]
		[Description("Default: landscape left/right/bottom all inset from system UI (Default on Grid = Container)")]
		public void ValidateOrientation_Default_Landscape_Grid()
		{
			ClickGridSafeAreaButton();

			App.WaitForElement("SafeAreaDefaultButton");
			App.Tap("SafeAreaDefaultButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Default"));

			App.SetOrientationLandscape();
			App.WaitForElement("SafeAreaEdgesValueLabel");

			var (screenWidth, screenHeight) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// Left: inset from system UI (Default on Grid = Container)
			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftRect.X), Is.EqualTo(insetsLandscape.Left).Within(PixelTolerance),
				$"Default: left X ({leftRect.X}) should be ≈ insetsLandscape.Left ({insetsLandscape.Left}) (Default on Grid = Container)");

			// Right: inset from system UI (Android uses display cutout for right inset in landscape)
			var rightRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdge = rightRect.X + rightRect.Width;
			var expectedRight = GetLandscapeRightInset(insetsLandscape.Right, insetsLandscape.CutoutR);
			Assert.That(Math.Abs(rightEdge), Is.EqualTo(screenWidth - expectedRight).Within(PixelTolerance),
				$"Default: right edge ({rightEdge}) should be ≈ screenWidth - expectedRight ({screenWidth - expectedRight}) (Default on Grid = Container)");

			// Bottom: inset from system UI
			var bottomRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomRect.Bottom), Is.EqualTo(screenHeight - insetsLandscape.Bottom).Within(PixelTolerance),
				$"Default: bottom edge ({bottomRect.Bottom}) should be ≈ screenHeight - insetsLandscape.Bottom ({screenHeight - insetsLandscape.Bottom}) (Default on Grid = Container)");

			App.SetOrientationPortrait();
			App.WaitForElement("SafeAreaEdgesValueLabel");
		}

		// ──────────────────────────────────────────────
		// Landscape Keyboard Position Validation
		// ──────────────────────────────────────────────

#if TEST_FAILS_ON_ANDROID // In landscape mode on Android, the keyboard covers the entire screen, and Appium cannot find elements to validate their positions

		[Test, Order(27)]
		[Description("Landscape All: bottom moves up to keyboard, left/right stay inset")]
		public void ValidateKeyboard_All_Landscape_Grid()
		{
			ClickGridSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			App.SetOrientationLandscape();
			App.WaitForElement("SafeAreaEdgesValueLabel");

			var (screenWidth, screenHeight) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// ── Before keyboard ──
			var leftBeforeRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftBeforeRect.X), Is.EqualTo(insetsLandscape.Left).Within(PixelTolerance),
				$"Before keyboard - left X ({leftBeforeRect.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			var rightBeforeRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightBeforeEdge = rightBeforeRect.X + rightBeforeRect.Width;
			Assert.That(Math.Abs(rightBeforeEdge), Is.EqualTo(screenWidth - insetsLandscape.Right).Within(PixelTolerance),
				$"Before keyboard - right edge ({rightBeforeEdge}) should be = screenWidth - insetsLandscape.Right ({screenWidth - insetsLandscape.Right})");

			var bottomBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomBeforeRect.Bottom), Is.EqualTo(screenHeight - insetsLandscape.Bottom).Within(PixelTolerance),
				$"Before keyboard - bottom edge ({bottomBeforeRect.Bottom}) should be = screenHeight - insetsLandscape.Bottom ({screenHeight - insetsLandscape.Bottom})");

			// ── Show keyboard ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var keyboardY = GetKeyboardY();

			// Bottom should move up to keyboard top
			var bottomDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomDuringRect.Bottom, Is.EqualTo(keyboardY).Within(PixelTolerance),
				$"During keyboard - bottom edge ({bottomDuringRect.Bottom}) should equal keyboardY ({keyboardY})");

			// Left/Right should remain unchanged
			var leftDuringRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftDuringRect.X, Is.EqualTo(leftBeforeRect.X).Within(PixelTolerance),
				$"During keyboard - left X ({leftDuringRect.X}) should remain at ({leftBeforeRect.X})");

			var rightDuringRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightDuringEdge = rightDuringRect.X + rightDuringRect.Width;
			Assert.That(rightDuringEdge, Is.EqualTo(rightBeforeEdge).Within(PixelTolerance),
				$"During keyboard - right edge ({rightDuringEdge}) should remain at ({rightBeforeEdge})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			// All edges should return to original positions
			var leftAfterRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftAfterRect.X, Is.EqualTo(leftBeforeRect.X).Within(PixelTolerance),
				$"After keyboard - left X ({leftAfterRect.X}) should return to original ({leftBeforeRect.X})");

			var rightAfterRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightAfterEdge = rightAfterRect.X + rightAfterRect.Width;
			Assert.That(rightAfterEdge, Is.EqualTo(rightBeforeEdge).Within(PixelTolerance),
				$"After keyboard - right edge ({rightAfterEdge}) should return to original ({rightBeforeEdge})");

			var bottomAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomAfterRect.Bottom, Is.EqualTo(bottomBeforeRect.Bottom).Within(PixelTolerance),
				$"After keyboard - bottom edge ({bottomAfterRect.Bottom}) should return to original ({bottomBeforeRect.Bottom})");

			App.SetOrientationPortrait();
			App.WaitForElement("SafeAreaEdgesValueLabel");
		}

		[Test, Order(28)]
		[Description("Landscape SoftInput: bottom moves up to keyboard, left/right stay inset")]
		public void ValidateKeyboard_SoftInput_Landscape_Grid()
		{
			ClickGridSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			App.SetOrientationLandscape();
			App.WaitForElement("SafeAreaEdgesValueLabel");

			var (screenWidth, screenHeight) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// ── Before keyboard ──
			var leftBeforeRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftBeforeRect.X), Is.EqualTo(insetsLandscape.Left).Within(PixelTolerance),
				$"Before keyboard - left X ({leftBeforeRect.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			var rightBeforeRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightBeforeEdge = rightBeforeRect.X + rightBeforeRect.Width;
			Assert.That(Math.Abs(rightBeforeEdge), Is.EqualTo(screenWidth - insetsLandscape.Right).Within(PixelTolerance),
				$"Before keyboard - right edge ({rightBeforeEdge}) should be = screenWidth - insetsLandscape.Right ({screenWidth - insetsLandscape.Right})");

			var bottomBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomBeforeRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"Before keyboard - bottom edge ({bottomBeforeRect.Bottom}) should be = screenHeight ({screenHeight})");

			// ── Show keyboard ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var keyboardY = GetKeyboardY();

			// Bottom should move up to keyboard top
			var bottomDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomDuringRect.Bottom, Is.EqualTo(keyboardY).Within(PixelTolerance),
				$"During keyboard - bottom edge ({bottomDuringRect.Bottom}) should equal keyboardY ({keyboardY})");

			// Left/Right should remain unchanged
			var leftDuringRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftDuringRect.X, Is.EqualTo(leftBeforeRect.X).Within(PixelTolerance),
				$"During keyboard - left X ({leftDuringRect.X}) should remain at ({leftBeforeRect.X})");

			var rightDuringRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightDuringEdge = rightDuringRect.X + rightDuringRect.Width;
			Assert.That(rightDuringEdge, Is.EqualTo(rightBeforeEdge).Within(PixelTolerance),
				$"During keyboard - right edge ({rightDuringEdge}) should remain at ({rightBeforeEdge})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			// All edges should return to original positions
			var leftAfterRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftAfterRect.X, Is.EqualTo(leftBeforeRect.X).Within(PixelTolerance),
				$"After keyboard - left X ({leftAfterRect.X}) should return to original ({leftBeforeRect.X})");

			var rightAfterRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightAfterEdge = rightAfterRect.X + rightAfterRect.Width;
			Assert.That(rightAfterEdge, Is.EqualTo(rightBeforeEdge).Within(PixelTolerance),
				$"After keyboard - right edge ({rightAfterEdge}) should return to original ({rightBeforeEdge})");

			var bottomAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomAfterRect.Bottom, Is.EqualTo(bottomBeforeRect.Bottom).Within(PixelTolerance),
				$"After keyboard - bottom edge ({bottomAfterRect.Bottom}) should return to original ({bottomBeforeRect.Bottom})");

			App.SetOrientationPortrait();
			App.WaitForElement("SafeAreaEdgesValueLabel");
		}

		[Test, Order(29)]
		[Description("Landscape None: bottom stays at screen edge with keyboard, left/right stay edge-to-edge")]
		public void ValidateKeyboard_None_Landscape_Grid()
		{
			ClickGridSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			App.SetOrientationLandscape();
			App.WaitForElement("SafeAreaEdgesValueLabel");

			var (screenWidth, screenHeight) = GetScreenSize();

			// ── Before keyboard ──
			var leftBeforeRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftBeforeRect.X, Is.EqualTo(0).Within(PixelTolerance),
				$"Before keyboard - left X ({leftBeforeRect.X}) should be = 0 (edge-to-edge)");

			var rightBeforeRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightBeforeEdge = rightBeforeRect.X + rightBeforeRect.Width;
			Assert.That(Math.Abs(rightBeforeEdge), Is.EqualTo(screenWidth).Within(PixelTolerance),
				$"Before keyboard - right edge ({rightBeforeEdge}) should be = screenWidth ({screenWidth})");

			var bottomBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomBeforeRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"Before keyboard - bottom edge ({bottomBeforeRect.Bottom}) should be = screenHeight ({screenHeight})");

			// ── Show keyboard ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			// Bottom should NOT move (None ignores keyboard)
			var bottomDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomDuringRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"During keyboard - bottom edge ({bottomDuringRect.Bottom}) should remain at screenHeight ({screenHeight})");

			// Left/Right should remain unchanged
			var leftDuringRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftDuringRect.X, Is.EqualTo(0).Within(PixelTolerance),
				$"During keyboard - left X ({leftDuringRect.X}) should remain at 0 (edge-to-edge)");

			var rightDuringRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightDuringEdge = rightDuringRect.X + rightDuringRect.Width;
			Assert.That(Math.Abs(rightDuringEdge), Is.EqualTo(screenWidth).Within(PixelTolerance),
				$"During keyboard - right edge ({rightDuringEdge}) should remain at screenWidth ({screenWidth})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var leftAfterRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftAfterRect.X, Is.EqualTo(leftBeforeRect.X).Within(PixelTolerance),
				$"After keyboard - left X ({leftAfterRect.X}) should return to original ({leftBeforeRect.X})");

			var rightAfterRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightAfterEdge = rightAfterRect.X + rightAfterRect.Width;
			Assert.That(rightAfterEdge, Is.EqualTo(rightBeforeEdge).Within(PixelTolerance),
				$"After keyboard - right edge ({rightAfterEdge}) should return to original ({rightBeforeEdge})");

			var bottomAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomAfterRect.Bottom, Is.EqualTo(bottomBeforeRect.Bottom).Within(PixelTolerance),
				$"After keyboard - bottom edge ({bottomAfterRect.Bottom}) should return to original ({bottomBeforeRect.Bottom})");

			App.SetOrientationPortrait();
			App.WaitForElement("SafeAreaEdgesValueLabel");
		}

		[Test, Order(30)]
		[Description("Landscape Container: bottom stays at safe area inset with keyboard, left/right stay inset")]
		public void ValidateKeyboard_Container_Landscape_Grid()
		{
			ClickGridSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			App.SetOrientationLandscape();
			App.WaitForElement("SafeAreaEdgesValueLabel");

			var (screenWidth, screenHeight) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// ── Before keyboard ──
			var leftBeforeRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftBeforeRect.X), Is.EqualTo(insetsLandscape.Left).Within(PixelTolerance),
				$"Before keyboard - left X ({leftBeforeRect.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			var rightBeforeRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightBeforeEdge = rightBeforeRect.X + rightBeforeRect.Width;
			Assert.That(Math.Abs(rightBeforeEdge), Is.EqualTo(screenWidth - insetsLandscape.Right).Within(PixelTolerance),
				$"Before keyboard - right edge ({rightBeforeEdge}) should be = screenWidth - insetsLandscape.Right ({screenWidth - insetsLandscape.Right})");

			var bottomBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomBeforeRect.Bottom), Is.EqualTo(screenHeight - insetsLandscape.Bottom).Within(PixelTolerance),
				$"Before keyboard - bottom edge ({bottomBeforeRect.Bottom}) should be = screenHeight - insetsLandscape.Bottom ({screenHeight - insetsLandscape.Bottom})");

			// ── Show keyboard ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			// Bottom should NOT move (Container ignores keyboard)
			var bottomDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomDuringRect.Bottom, Is.EqualTo(bottomBeforeRect.Bottom).Within(PixelTolerance),
				$"During keyboard - bottom edge ({bottomDuringRect.Bottom}) should remain at ({bottomBeforeRect.Bottom})");

			// Left/Right should remain unchanged
			var leftDuringRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftDuringRect.X, Is.EqualTo(leftBeforeRect.X).Within(PixelTolerance),
				$"During keyboard - left X ({leftDuringRect.X}) should remain at ({leftBeforeRect.X})");

			var rightDuringRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightDuringEdge = rightDuringRect.X + rightDuringRect.Width;
			Assert.That(rightDuringEdge, Is.EqualTo(rightBeforeEdge).Within(PixelTolerance),
				$"During keyboard - right edge ({rightDuringEdge}) should remain at ({rightBeforeEdge})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var leftAfterRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftAfterRect.X, Is.EqualTo(leftBeforeRect.X).Within(PixelTolerance),
				$"After keyboard - left X ({leftAfterRect.X}) should return to original ({leftBeforeRect.X})");

			var rightAfterRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightAfterEdge = rightAfterRect.X + rightAfterRect.Width;
			Assert.That(rightAfterEdge, Is.EqualTo(rightBeforeEdge).Within(PixelTolerance),
				$"After keyboard - right edge ({rightAfterEdge}) should return to original ({rightBeforeEdge})");

			var bottomAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomAfterRect.Bottom, Is.EqualTo(bottomBeforeRect.Bottom).Within(PixelTolerance),
				$"After keyboard - bottom edge ({bottomAfterRect.Bottom}) should return to original ({bottomBeforeRect.Bottom})");

			App.SetOrientationPortrait();
			App.WaitForElement("SafeAreaEdgesValueLabel");
		}

		[Test, Order(31)]
		[Description("Landscape Default: all edges inset from system UI; keyboard does not move bottom (Default on Grid = Container)")]
		public void ValidateKeyboard_Default_Landscape_Grid()
		{
			ClickGridSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaDefaultButton");
			App.Tap("SafeAreaDefaultButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Default"));

			App.SetOrientationLandscape();
			App.WaitForElement("SafeAreaEdgesValueLabel");

			var (screenWidth, screenHeight) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// ── Before keyboard (Default on Grid = Container — inset from system UI) ──
			var leftBeforeRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftBeforeRect.X), Is.EqualTo(insetsLandscape.Left).Within(PixelTolerance),
				$"Before keyboard - left X ({leftBeforeRect.X}) should be ≈ insetsLandscape.Left ({insetsLandscape.Left}) (Default on Grid = Container)");

			var rightBeforeRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightBeforeEdge = rightBeforeRect.X + rightBeforeRect.Width;
			var expectedRightDefault = GetLandscapeRightInset(insetsLandscape.Right, insetsLandscape.CutoutR);
			Assert.That(Math.Abs(rightBeforeEdge), Is.EqualTo(screenWidth - expectedRightDefault).Within(PixelTolerance),
				$"Before keyboard - right edge ({rightBeforeEdge}) should be ≈ screenWidth - expectedRight ({screenWidth - expectedRightDefault}) (Default on Grid = Container)");

			var bottomBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomBeforeRect.Bottom), Is.EqualTo(screenHeight - insetsLandscape.Bottom).Within(PixelTolerance),
				$"Before keyboard - bottom edge ({bottomBeforeRect.Bottom}) should be ≈ screenHeight - insetsLandscape.Bottom ({screenHeight - insetsLandscape.Bottom}) (Default on Grid = Container)");

			// ── Show keyboard ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			// Bottom should NOT move (Default = Container ignores keyboard)
			var bottomDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomDuringRect.Bottom), Is.EqualTo(screenHeight - insetsLandscape.Bottom).Within(PixelTolerance),
				$"During keyboard - bottom edge ({bottomDuringRect.Bottom}) should remain at screenHeight - insetsLandscape.Bottom ({screenHeight - insetsLandscape.Bottom}) (Container ignores keyboard)");

			// Left/Right should remain unchanged (Container — inset from system UI)
			var leftDuringRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftDuringRect.X), Is.EqualTo(insetsLandscape.Left).Within(PixelTolerance),
				$"During keyboard - left X ({leftDuringRect.X}) should remain at insetsLandscape.Left ({insetsLandscape.Left}) (Default on Grid = Container)");

			var rightDuringRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightDuringEdge = rightDuringRect.X + rightDuringRect.Width;
			Assert.That(Math.Abs(rightDuringEdge), Is.EqualTo(screenWidth - expectedRightDefault).Within(PixelTolerance),
				$"During keyboard - right edge ({rightDuringEdge}) should remain at screenWidth - expectedRight ({screenWidth - expectedRightDefault}) (Default on Grid = Container)");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var bottomAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomAfterRect.Bottom, Is.EqualTo(bottomBeforeRect.Bottom).Within(PixelTolerance),
				$"After keyboard - bottom edge ({bottomAfterRect.Bottom}) should return to original ({bottomBeforeRect.Bottom})");

			App.SetOrientationPortrait();
			App.WaitForElement("SafeAreaEdgesValueLabel");
		}
#endif

		// ──────────────────────────────────────────────
		// Default + Keyboard (Portrait)
		// ──────────────────────────────────────────────

		[Test, Order(32)]
		[Description("With Default (Container on Grid), top/bottom are inset from system UI and do NOT move when keyboard is shown")]
		public void ValidateKeyboard_Default_BottomStays_Grid()
		{
			ClickGridSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaDefaultButton");
			App.Tap("SafeAreaDefaultButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Default"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Before keyboard (Default on Grid = Container — inset from system UI) ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelBeforeRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be ≈ insets.Top ({insets.Top}) (Default on Grid = Container)");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be ≈ screenHeight - insets.Bottom ({screenHeight - insets.Bottom}) (Default on Grid = Container)");

			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

#if IOS
			// Bottom should NOT move (Default = Container on Grid — ignores keyboard)
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelDuringRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should remain at screenHeight - insets.Bottom ({screenHeight - insets.Bottom}) (Container ignores keyboard)");
#else
			App.WaitForNoElement("BottomEdgeIndicator"); // On Android, Appium does not find the bottom label when the keyboard is open
#endif
			// Top should remain unchanged (Container — inset from system UI)
			var topLabelDuringRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelDuringRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"During keyboard - top label Y ({topLabelDuringRect.Y}) should remain at insets.Top ({insets.Top}) (Default on Grid = Container)");

			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(topLabelBeforeRect.Y).Within(PixelTolerance),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should return to original ({topLabelBeforeRect.Y})");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom).Within(PixelTolerance),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
		}

		// ──────────────────────────────────────────────
		// Per-Edge + Keyboard (Portrait)
		// ──────────────────────────────────────────────

		[Test, Order(33)]
		[Description("Per-edge B:None + keyboard — bottom stays edge-to-edge when keyboard is shown")]
		public void ValidatePerEdgeKeyboard_BottomNone_BottomStays_Grid()
		{
			ClickGridSafeAreaButton();
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
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

#if IOS
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelDuringRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should stay at screenHeight ({screenHeight})");
#else
			App.WaitForNoElement("BottomEdgeIndicator"); // On Android, Appium does not find the bottom label when the keyboard is open
#endif
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom).Within(PixelTolerance),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
		}

		[Test, Order(34)]
		[Description("Per-edge B:Container + keyboard — bottom stays at safe area inset when keyboard is shown")]
		public void ValidatePerEdgeKeyboard_BottomContainer_BottomStays_Grid()
		{
			ClickGridSafeAreaButton();
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
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

#if IOS
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should stay at (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
#else
			App.WaitForNoElement("BottomEdgeIndicator"); // On Android, Appium does not find the bottom label when the keyboard is open
#endif
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom).Within(PixelTolerance),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
		}

		[Test, Order(35)]
		[Description("Per-edge B:SoftInput + keyboard — bottom moves up to keyboard Y")]
		public void ValidatePerEdgeKeyboard_BottomSoftInput_BottomMovesUp_Grid()
		{
			ClickGridSafeAreaButton();
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
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight).Within(PixelTolerance),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var keyboardY = GetKeyboardY();

			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(keyboardY).Within(PixelTolerance),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should equal keyboardY ({keyboardY})");

			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom).Within(PixelTolerance),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
		}

		[Test, Order(36)]
		[Description("Per-edge B:All + keyboard — bottom moves up to keyboard Y")]
		public void ValidatePerEdgeKeyboard_BottomAll_BottomMovesUp_Grid()
		{
			ClickGridSafeAreaButton();
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
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var keyboardY = GetKeyboardY();

			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(keyboardY).Within(PixelTolerance),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should equal keyboardY ({keyboardY})");

			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom).Within(PixelTolerance),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
		}

		// ──────────────────────────────────────────────
		// Left/Right Per-Edge in Landscape
		// ──────────────────────────────────────────────

		[Test, Order(37)]
		[Description("Landscape per-edge: L:Container, R:None — left inset by safe area, right edge-to-edge")]
		public void ValidatePerEdge_LeftContainerRightNone_Landscape_Grid()
		{
			ClickGridSafeAreaButton();
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
			App.WaitForElement("SafeAreaEdgesValueLabel");

			var (screenWidth, screenHeight) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// Left: inset by safe area (Container)
			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftRect.X), Is.EqualTo(insetsLandscape.Left).Within(PixelTolerance),
				$"Left (Container): X ({leftRect.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			// Right: edge-to-edge (None)
			var rightRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdge = rightRect.X + rightRect.Width;
			Assert.That(Math.Abs(rightEdge), Is.EqualTo(screenWidth).Within(PixelTolerance),
				$"Right (None): right edge ({rightEdge}) should be = screenWidth ({screenWidth})");

			App.SetOrientationPortrait();
			App.WaitForElement("SafeAreaEdgesValueLabel");
		}

		// ──────────────────────────────────────────────
		// Orientation Roundtrip
		// ──────────────────────────────────────────────

		[Test, Order(38)]
		[Description("Rotate to landscape and back to portrait — positions restore correctly")]
		public void ValidateOrientation_Roundtrip_PositionsRestore_Grid()
		{
			ClickGridSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Record portrait positions ──
			var topPortraitRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			var bottomPortraitRect = App.WaitForElement("BottomEdgeIndicator").GetRect();

			Assert.That(Math.Abs(topPortraitRect.Y), Is.EqualTo(insets.Top).Within(PixelTolerance),
				$"Portrait: top label Y ({topPortraitRect.Y}) should be equal to insets.Top ({insets.Top})");
			Assert.That(Math.Abs(bottomPortraitRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(PixelTolerance),
				$"Portrait: bottom label Bottom ({bottomPortraitRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// ── Rotate to landscape ──
			App.SetOrientationLandscape();
			App.WaitForElement("SafeAreaEdgesValueLabel");

			var (screenWidthLandscape, screenHeightLandscape) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			var topLandscapeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLandscapeRect.Y), Is.EqualTo(insetsLandscape.Top).Within(PixelTolerance),
				$"Landscape: top label Y ({topLandscapeRect.Y}) should be equal to insetsLandscape.Top ({insetsLandscape.Top})");

			var leftLandscapeRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftLandscapeRect.X), Is.EqualTo(insetsLandscape.Left).Within(PixelTolerance),
				$"Landscape: left X ({leftLandscapeRect.X}) should be equal to insetsLandscape.Left ({insetsLandscape.Left})");

			var rightLandscapeRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightLandscapeEdge = rightLandscapeRect.X + rightLandscapeRect.Width;
			var expectedRight = GetLandscapeRightInset(insetsLandscape.Right, insetsLandscape.CutoutR);
			Assert.That(Math.Abs(rightLandscapeEdge), Is.EqualTo(screenWidthLandscape - expectedRight).Within(PixelTolerance),
				$"Landscape: right edge ({rightLandscapeEdge}) should be equal to screenWidth - expectedRight - 0 ({screenWidthLandscape - expectedRight - 0})");

			// ── Rotate back to portrait ──
			App.SetOrientationPortrait();
			App.WaitForElement("SafeAreaEdgesValueLabel");

			var insetsAfter = GetSafeAreaInsets();
			var (_, screenHeightAfter) = GetScreenSize();

			var topAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topAfterRect.Y), Is.EqualTo(insetsAfter.Top).Within(PixelTolerance),
				$"After roundtrip: top label Y ({topAfterRect.Y}) should be equal to insets.Top + 0 ({insetsAfter.Top + 0})");

			var bottomAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomAfterRect.Bottom), Is.EqualTo(screenHeightAfter - insetsAfter.Bottom).Within(PixelTolerance),
				$"After roundtrip: bottom label Bottom ({bottomAfterRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeightAfter - insetsAfter.Bottom - 0})");

			// Verify positions match the original portrait positions
			Assert.That(topAfterRect.Y, Is.EqualTo(topPortraitRect.Y).Within(PixelTolerance),
				$"After roundtrip: top label Y ({topAfterRect.Y}) should match original portrait ({topPortraitRect.Y})");

			Assert.That(bottomAfterRect.Bottom, Is.EqualTo(bottomPortraitRect.Bottom).Within(PixelTolerance),
				$"After roundtrip: bottom label Bottom ({bottomAfterRect.Bottom}) should match original portrait ({bottomPortraitRect.Bottom})");
		}
	}
}
#endif
