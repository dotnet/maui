#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	[Category(UITestCategories.SafeAreaEdges)]
	public class SafeArea_ScrollViewFeatureTests : _GalleryUITest
	{
		public const string SafeAreaFeatureMatrix = "SafeArea Feature Matrix";
		public override string GalleryPageName => SafeAreaFeatureMatrix;

		public SafeArea_ScrollViewFeatureTests(TestDevice device)
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
			// Calculate keyboard top Y position
			var (_, screenHeight) = GetScreenSize();
			var insets = GetSafeAreaInsets();
			return screenHeight - insets.KeyboardHeight;
#endif
		}

		/// <summary>
		/// Navigates to the ScrollView SafeArea test page from the SafeArea Feature Matrix landing page.
		/// </summary>
		public void ClickScrollViewSafeAreaButton()
		{
			var isButtonPresent = App.FindElement("ScrollViewSafeAreaButton");
			if (isButtonPresent != null)
			{
				App.WaitForElement("ScrollViewSafeAreaButton");
				App.Tap("ScrollViewSafeAreaButton");
			}
		}

		private (int Width, int Height) GetScreenSize()
		{
			var size = ((AppiumApp)App).Driver.Manage().Window.Size;
			return (size.Width, size.Height);
		}

		private int GetLandscapeRightInset(int right, int cutoutR)
		{
#if ANDROID
			return cutoutR;
#else
			return right;
#endif
		}

		/// <summary>
		/// Scrolls to the bottom of the ScrollView by tapping the ScrollToBottom button, then waits for the bottom indicator to become visible.
		/// </summary>
		private void ScrollToBottom()
		{
			App.WaitForElement("ScrollToBottom");
			App.Tap("ScrollToBottom");
			Thread.Sleep(500); // Wait for scroll animation to complete
			App.WaitForElement("BottomEdgeIndicator");
		}

		/// <summary>
		/// Scrolls back to the top of the ScrollView by tapping the ScrollToTop button, then waits for the top indicator to become visible.
		/// </summary>
		private void ScrollToTop()
		{
			App.WaitForElement("ScrollToTop");
			App.Tap("ScrollToTop");
			Thread.Sleep(500); // Wait for scroll animation to complete
			App.WaitForElement("TopEdgeIndicator");
		}

		// ──────────────────────────────────────────────
		// Uniform SafeAreaRegions via Buttons
		// ──────────────────────────────────────────────

		[Test, Order(1)]
		[Description("ScrollView content extends edge-to-edge behind system bars/notch")]
		public void Validate_ScrollView_SafeAreaEdges_None()
		{
			ClickScrollViewSafeAreaButton();

			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			// Verify top label: Y should be 0 (edge-to-edge, no safe area applied)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.EqualTo(0),
				$"None: top label Y ({topLabelRect.Y}) should be = 0 (edge-to-edge), safe area top inset is ignored");

			var (_, screenHeight) = GetScreenSize();

			// Scroll to bottom to verify bottom label
			ScrollToBottom();

			// Verify bottom label: bottom edge should be ≈ screenHeight (edge-to-edge)
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"None: bottom label Bottom ({bottomLabelRect.Bottom}) should be ≈ screenHeight ({screenHeight})");

			// Scroll back to top for next test
			ScrollToTop();
		}

		[Test, Order(2)]
		[Description("ScrollView content inset from all system UI (status bar, nav bar, notch, home indicator)")]
		public void Validate_ScrollView_SafeAreaEdges_All()
		{
			ClickScrollViewSafeAreaButton();

			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// Verify top label: Y should be ≈ insets.Top (safe area applied)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"All: top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			// Scroll to bottom to verify bottom label
			ScrollToBottom();

			// Verify bottom label: bottom edge should be ≈ screenBottom - insets.Bottom
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"All: bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// Scroll back to top for next test
			ScrollToTop();
		}

#if TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/36863

		[Test, Order(3)]
		[Description("ScrollView content avoids system bars/notch but can extend under keyboard area")]
		public void Validate_ScrollView_SafeAreaEdges_Container()
		{
			ClickScrollViewSafeAreaButton();

			App.Tap("SafeAreaContainerButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// Verify top label: Y should be ≈ insets.Top (safe area applied)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Container: top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			// Scroll to bottom to verify bottom label
			ScrollToBottom();

			// Verify bottom label: bottom edge should be ≈ screenBottom - insets.Bottom
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"Container: bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// Scroll back to top for next test
			ScrollToTop();
		}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/36770

		[Test, Order(4)]
		[Description("ScrollView SoftInput is edge-to-edge")]
		public void Validate_ScrollView_SafeAreaEdges_SoftInput()
		{
			ClickScrollViewSafeAreaButton();

			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			var (_, screenHeight) = GetScreenSize();

			// SoftInput flows beneath system bars and notches.
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.EqualTo(0),
				$"SoftInput: top label Y ({topLabelRect.Y}) should be 0 (edge-to-edge)");

			// Scroll to bottom to verify bottom label
			ScrollToBottom();

			// Verify bottom label: bottom edge should be ≈ screenHeight (edge-to-edge, no safe area on bottom without keyboard)
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"SoftInput: bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// Scroll back to top for next test
			ScrollToTop();
		}
#endif

#if TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/36863

		[Test, Order(5)]
		[Description("ScrollView Default applies safe area insets on all edges (behaves like Container)")]
		public void Validate_ScrollView_SafeAreaEdges_Default()
		{
			ClickScrollViewSafeAreaButton();

			App.WaitForElement("SafeAreaDefaultButton");
			App.Tap("SafeAreaDefaultButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Default"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// Verify top label: Y should be ≈ insets.Top (safe area applied)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Default: top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			// Scroll to bottom to verify bottom label
			ScrollToBottom();

			// Verify bottom label: bottom edge should be ≈ screenBottom - insets.Bottom
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"Default: bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// Scroll back to top for next test
			ScrollToTop();
		}
#endif
		// ──────────────────────────────────────────────
		// Per-Edge Configuration (via Options)
		// ──────────────────────────────────────────────

		[Test, Order(6)]
		[Description("ScrollView: Only top avoids status bar/notch. Bottom edge-to-edge.")]
		public void Validate_ScrollView_PerEdge_TopContainerOnly()
		{
			ClickScrollViewSafeAreaButton();

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

			// Verify top: Container — should be inset by safe area top
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Top (Container): label Y ({topLabelRect.Y}) should be ≈ insets.Top ({insets.Top})");

			// Scroll to bottom to verify bottom label
			ScrollToBottom();

			// Verify bottom: None — bottom should be edge-to-edge
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"None: bottom label Bottom ({bottomLabelRect.Bottom}) should be ≈ screenHeight ({screenHeight})");

			// Scroll back to top for next test
			ScrollToTop();
		}

#if TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/37264

		[Test, Order(7)]
		[Description("ScrollView: Top avoids system bars; bottom avoids only keyboard")]
		public void Validate_ScrollView_PerEdge_BottomSoftInput_TopContainer()
		{
			ClickScrollViewSafeAreaButton();

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

			// Verify top: Container — should be inset by safe area top
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Top (Container): label Y ({topLabelRect.Y}) should be ≈ insets.Top ({insets.Top})");

			// Scroll to bottom to verify bottom label
			ScrollToBottom();

			// Verify bottom: SoftInput — bottom should be edge-to-edge (no keyboard open)
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"SoftInput: bottom label Bottom ({bottomLabelRect.Bottom}) should be ≈ screenHeight ({screenHeight})");

			// Scroll back to top for next test
			ScrollToTop();
		}
#endif

		[Test, Order(8)]
		[Description("ScrollView: Top/bottom respect all insets")]
		public void Validate_ScrollView_PerEdge_TopBottomAll_SidesNone()
		{
			ClickScrollViewSafeAreaButton();

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

			// Verify top: All — should be inset by safe area top
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"All: top label Y ({topLabelRect.Y}) should be = insets.Top ({insets.Top})");

			// Scroll to bottom to verify bottom label
			ScrollToBottom();

			// Verify bottom: All — should be inset by safe area bottom
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"All: bottom label Bottom ({bottomLabelRect.Bottom}) should be = (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// Scroll back to top for next test
			ScrollToTop();
		}

		[Test, Order(9)]
		[Description("ScrollView: Each edge independently applies its behavior")]
		public void Validate_ScrollView_PerEdge_AllDifferent()
		{
			ClickScrollViewSafeAreaButton();

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

			// Verify top: Container — should be inset by safe area top
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Container: top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			// Scroll to bottom to verify bottom label
			ScrollToBottom();

			// Verify bottom: All — should be inset by safe area bottom
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"All: bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// Scroll back to top for next test
			ScrollToTop();
		}

		// ──────────────────────────────────────────────
		// Keyboard + SafeArea (Portrait)
		// ──────────────────────────────────────────────

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/36826

		[Test, Order(10)]
		[Description("With None, bottom indicator does NOT move when keyboard is shown, then switch to All and bottom moves up")]
		public void Validate_ScrollView_Keyboard_NoneThenAll_BottomMovesUp()
		{
			ClickScrollViewSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// Confirm None: top at 0, bottom at screenHeight
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelBeforeRect.Y, Is.EqualTo(0),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be 0 (edge-to-edge)");

			ScrollToBottom();
			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// Show keyboard
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			ScrollToTop();
			// Switch to All
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var keyboardY = GetKeyboardY();

			// Bottom should move up to keyboard top
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(keyboardY).Within(1),
				$"During keyboard (All) - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should equal keyboard Y ({keyboardY})");

			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelAfterRect.Y), Is.EqualTo(insets.Top),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should be equal to insets.Top ({insets.Top})");

			ScrollToBottom();
			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelAfterRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
			ScrollToTop();
		}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/36826

		[Test, Order(11)]
		[Description("With All, bottom indicator moves up when keyboard is shown")]
		public void Validate_ScrollView_Keyboard_All_BottomMovesUp()
		{
			ClickScrollViewSafeAreaButton();
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

			ScrollToBottom();
			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// ── Show keyboard ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var keyboardY = GetKeyboardY();

			// Bottom should have moved up to the keyboard top
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(keyboardY).Within(1),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should equal keyboard Y ({keyboardY})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom).Within(1),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");

			ScrollToTop();

			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should return to original ({topLabelBeforeRect.Y})");
		}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/36826

		[Test, Order(12)]
		[Description("With SoftInput, bottom indicator moves up when keyboard is shown")]
		public void Validate_ScrollView_Keyboard_SoftInput_BottomMovesUp()
		{
			ClickScrollViewSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			var (_, screenHeight) = GetScreenSize();

			// ── Before keyboard ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelBeforeRect.Y, Is.EqualTo(0),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be 0 (edge-to-edge)");

			ScrollToBottom();
			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// ── Show keyboard ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var keyboardY = GetKeyboardY();

			// Bottom should have moved up to the keyboard top
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(keyboardY).Within(1),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should equal keyboard Y ({keyboardY})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom).Within(1),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");

			ScrollToTop();

			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should return to original ({topLabelBeforeRect.Y})");
		}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/36864

		[Test, Order(13)]
		[Description("With None, bottom indicator does NOT move when keyboard is shown")]
		public void Validate_ScrollView_Keyboard_None_BottomStays()
		{
			ClickScrollViewSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			var (_, screenHeight) = GetScreenSize();

			// ── Before keyboard ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelBeforeRect.Y, Is.EqualTo(0),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be 0 (edge-to-edge)");

			ScrollToBottom();
			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// ── Show keyboard ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

#if IOS
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelDuringRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should be equal to screenHeight ({screenHeight})");
#else
			App.WaitForNoElement("BottomEdgeIndicator");
#endif
			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom).Within(1),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");

			ScrollToTop();

			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should return to original ({topLabelBeforeRect.Y})");
		}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/36864

		[Test, Order(14)]
		[Description("With Container, bottom indicator does NOT move when keyboard is shown")]
		public void Validate_ScrollView_Keyboard_Container_BottomStays()
		{
			ClickScrollViewSafeAreaButton();
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

			ScrollToBottom();
			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// ── Show keyboard ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

#if IOS
			// Bottom should not have moved up to the keyboard top
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should equal (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
#else
			App.WaitForNoElement("BottomEdgeIndicator");
#endif
			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom).Within(1),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");

			ScrollToTop();

			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should return to original ({topLabelBeforeRect.Y})");
		}
#endif
#endif

		// ──────────────────────────────────────────────
		// Keyboard + Runtime SafeArea Changes
		// ──────────────────────────────────────────────

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/36826

		[Test, Order(15)]
		[Description("Switch None to All while keyboard is open — bottom indicator moves up")]
		public void Validate_ScrollView_KeyboardRuntime_SwitchNoneToAll_WhileKeyboardOpen()
		{
			ClickScrollViewSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Before keyboard (None) ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelBeforeRect.Y, Is.EqualTo(0),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be 0 (edge-to-edge)");

			ScrollToBottom();
			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// ── Show keyboard (None) ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

#if IOS
			var bottomLabelDuringNoneRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelDuringNoneRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"During keyboard (None) - bottom label Bottom ({bottomLabelDuringNoneRect.Bottom}) should be equal to screenHeight ({screenHeight})");
#else
			App.WaitForNoElement("BottomEdgeIndicator");
#endif
			ScrollToTop();
			// With None, bottom should NOT move
			var topLabelDuringNoneRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringNoneRect.Y, Is.EqualTo(0),
				$"During keyboard (None) - top label Y ({topLabelDuringNoneRect.Y}) should be 0 (edge-to-edge)");

			// ── Switch to All while keyboard is open ──
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var keyboardY = GetKeyboardY();

			// With All, bottom should move up to keyboard top
			var topLabelDuringAllRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelDuringAllRect.Y), Is.EqualTo(insets.Top),
				$"During keyboard (All) - top label Y ({topLabelDuringAllRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelDuringAllRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringAllRect.Bottom, Is.EqualTo(keyboardY).Within(1),
				$"During keyboard (All) - bottom label Bottom ({bottomLabelDuringAllRect.Bottom}) should equal keyboard Y ({keyboardY})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelAfterRect.Y), Is.EqualTo(insets.Top),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should be equal to insets.Top ({insets.Top})");

			ScrollToBottom();
			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelAfterRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
			ScrollToTop();
		}

		[Test, Order(16)]
		[Description("Switch None to SoftInput while keyboard is open — bottom indicator moves up")]
		public void Validate_ScrollView_KeyboardRuntime_SwitchNoneToSoftInput_WhileKeyboardOpen()
		{
			ClickScrollViewSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Before keyboard (None) ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelBeforeRect.Y, Is.EqualTo(0),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be 0 (edge-to-edge)");

			ScrollToBottom();
			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// ── Show keyboard (None) ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

#if IOS
			var bottomLabelDuringNoneRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelDuringNoneRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"During keyboard (None) - bottom label Bottom ({bottomLabelDuringNoneRect.Bottom}) should be equal to screenHeight ({screenHeight})");
#else
			App.WaitForNoElement("BottomEdgeIndicator");
#endif
			ScrollToTop();
			var topLabelDuringNoneRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringNoneRect.Y, Is.EqualTo(0),
				$"During keyboard (None) - top label Y ({topLabelDuringNoneRect.Y}) should be 0 (edge-to-edge)");

			// ── Switch to SoftInput while keyboard is open ──
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			var keyboardY = GetKeyboardY();

			// With SoftInput, bottom should move up to keyboard top
			var topLabelDuringSoftInputRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringSoftInputRect.Y, Is.EqualTo(0),
				$"During keyboard (SoftInput) - top label Y ({topLabelDuringSoftInputRect.Y}) should be 0 (edge-to-edge)");

			var bottomLabelDuringSoftInputRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringSoftInputRect.Bottom, Is.EqualTo(keyboardY).Within(1),
				$"During keyboard (SoftInput) - bottom label Bottom ({bottomLabelDuringSoftInputRect.Bottom}) should equal keyboard Y ({keyboardY})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(0),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should be 0 (edge-to-edge)");

			ScrollToBottom();
			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelAfterRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should be equal to screenHeight ({screenHeight})");
			ScrollToTop();
		}

		[Test, Order(17)]
		[Description("Switch All to None while keyboard is open — bottom indicator drops back")]
		public void Validate_ScrollView_KeyboardRuntime_SwitchAllToNone_WhileKeyboardOpen()
		{
			ClickScrollViewSafeAreaButton();
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

			ScrollToBottom();
			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// ── Show keyboard (All) ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var keyboardY = GetKeyboardY();

			var bottomLabelDuringAllRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringAllRect.Bottom, Is.EqualTo(keyboardY).Within(1),
				$"During keyboard (All) - bottom label Bottom ({bottomLabelDuringAllRect.Bottom}) should equal keyboard Y ({keyboardY})");
			ScrollToTop();

			// With All, bottom should move up to keyboard top
			var topLabelDuringAllRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringAllRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"During keyboard (All) - top label Y ({topLabelDuringAllRect.Y}) should remain at ({topLabelBeforeRect.Y})");

			// ── Switch to None while keyboard is open ──
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			// With None, top goes edge-to-edge; bottom does NOT adjust for keyboard
			var topLabelDuringNoneRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringNoneRect.Y, Is.EqualTo(0),
				$"During keyboard (None) - top label Y ({topLabelDuringNoneRect.Y}) should be 0 (edge-to-edge)");
			
			ScrollToBottom();
#if IOS
			var bottomLabelDuringNoneRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelDuringNoneRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"During keyboard (None) - bottom label Bottom ({bottomLabelDuringNoneRect.Bottom}) should be equal to screenHeight ({screenHeight})");
#else
			App.WaitForNoElement("BottomEdgeIndicator");
#endif
			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelAfterRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should be equal to screenHeight ({screenHeight})");
			ScrollToTop();

			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(0),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should be 0 (edge-to-edge)");
		}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/36863

		[Test, Order(18)]
		[Description("Switch Container to SoftInput while keyboard is open — bottom indicator moves up")]
		public void Validate_ScrollView_KeyboardRuntime_SwitchContainerToSoftInput_WhileKeyboardOpen()
		{
			ClickScrollViewSafeAreaButton();
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

			ScrollToBottom();
			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// ── Show keyboard (Container) ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

#if IOS
			var bottomLabelDuringContainerRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringContainerRect.Bottom, Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"During keyboard (Container) - bottom label Bottom ({bottomLabelDuringContainerRect.Bottom}) should equal (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");	
#else
			App.WaitForNoElement("BottomEdgeIndicator");
#endif
			ScrollToTop();
			// With Container, bottom should NOT move
			var topLabelDuringContainerRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringContainerRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"During keyboard (Container) - top label Y ({topLabelDuringContainerRect.Y}) should remain at ({topLabelBeforeRect.Y})");

			// ── Switch to SoftInput while keyboard is open ──
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			var keyboardY = GetKeyboardY();

			// With SoftInput, bottom should move up to keyboard top
			var topLabelDuringSoftInputRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringSoftInputRect.Y, Is.EqualTo(0),
				$"During keyboard (SoftInput) - top label Y ({topLabelDuringSoftInputRect.Y}) should be 0 (edge-to-edge)");

			var bottomLabelDuringSoftInputRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringSoftInputRect.Bottom, Is.EqualTo(keyboardY).Within(1),
				$"During keyboard (SoftInput) - bottom label Bottom ({bottomLabelDuringSoftInputRect.Bottom}) should equal keyboard Y ({keyboardY})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(0),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should be 0 (edge-to-edge)");

			ScrollToBottom();
			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelAfterRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should be equal to screenHeight ({screenHeight})");
			ScrollToTop();
		}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/36826

		[Test, Order(19)]
		[Description("Keyboard open: cycle through None → All → Container → SoftInput → Default → None and verify positions")]
		public void Validate_ScrollView_KeyboardRuntime_CycleThroughAllModes_WhileKeyboardOpen()
		{
			ClickScrollViewSafeAreaButton();
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

			ScrollToBottom();
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"None (before keyboard) - bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			// ── Open keyboard ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var keyboardY = GetKeyboardY();

#if IOS
			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"None (keyboard open) - bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to screenHeight ({screenHeight})");
#else
			App.WaitForNoElement("BottomEdgeIndicator");
#endif
			ScrollToTop();
			// ── Verify None with keyboard (no adjustment) ──
			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.EqualTo(0),
				$"None (keyboard open) - top label Y ({topLabelRect.Y}) should be 0 (edge-to-edge)");
			// ── Switch to All (keyboard still open) ──
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			keyboardY = GetKeyboardY();

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"All (keyboard open) - top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			ScrollToBottom();
			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelRect.Bottom, Is.EqualTo(keyboardY).Within(1),
				$"All (keyboard open) - bottom label Bottom ({bottomLabelRect.Bottom}) should equal keyboard Y ({keyboardY})");
			ScrollToTop();

			// ── Switch to Container (keyboard still open) ──
			App.Tap("SafeAreaContainerButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Container (keyboard open) - top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			ScrollToBottom();
#if IOS
			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"Container (keyboard open) - bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
#else
			App.WaitForNoElement("BottomEdgeIndicator");
#endif
			ScrollToTop();
			// ── Switch to SoftInput (keyboard still open) ──
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.EqualTo(0),
				$"SoftInput (keyboard open) - top label Y ({topLabelRect.Y}) should be 0 (edge-to-edge)");

			ScrollToBottom();
			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelRect.Bottom, Is.EqualTo(keyboardY).Within(1),
				$"SoftInput (keyboard open) - bottom label Bottom ({bottomLabelRect.Bottom}) should equal keyboard Y ({keyboardY})");
			ScrollToTop();

			// ── Switch to Default (keyboard still open) ──
			App.Tap("SafeAreaDefaultButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Default"));

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Default (keyboard open) - top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			ScrollToBottom();
#if IOS
			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"Default (keyboard open) - bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
#else
			App.WaitForNoElement("BottomEdgeIndicator");
#endif
			ScrollToTop();
			// ── Switch back to None (keyboard still open) ──
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.EqualTo(0),
				$"None (keyboard open, after cycle) - top label Y ({topLabelRect.Y}) should be 0 (edge-to-edge)");

			ScrollToBottom();
#if IOS
			bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"None (keyboard open, after cycle) - bottom label Bottom ({bottomLabelRect.Bottom}) should be equal to screenHeight ({screenHeight})");
#else
			App.WaitForNoElement("BottomEdgeIndicator");
#endif
			ScrollToTop();
			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");
		}
#endif

		// ──────────────────────────────────────────────
		// Interaction with ContentPage Properties
		// ──────────────────────────────────────────────
#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/37323

		[Test, Order(20)]
		[Description("Safe area insets and padding are additive")]
		public void Validate_ScrollView_SafeArea_WithPadding()
		{
			ClickScrollViewSafeAreaButton();

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
#endif

		[Test, Order(21)]
		[Description("Background extends edge-to-edge behind system UI")]
		public void Validate_ScrollView_SafeArea_None_WithBackground()
		{
			ClickScrollViewSafeAreaButton();

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

		[Test, Order(22)]
		[Description("None: landscape left/right/bottom all edge-to-edge")]
		public void Validate_ScrollView_Orientation_None_Landscape()
		{
			ClickScrollViewSafeAreaButton();

			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			App.SetOrientationLandscape();
			Thread.Sleep(1000);

			var (screenWidth, screenHeight) = GetScreenSize();

			// Left: edge-to-edge
			var leftRectBeforeScroll = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftRectBeforeScroll.X, Is.EqualTo(0),
				$"None: left X ({leftRectBeforeScroll.X}) should be = 0 (edge-to-edge)");

			// Right: edge-to-edge
			var rightRectBeforeScroll = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdgeBeforeScroll = rightRectBeforeScroll.X + rightRectBeforeScroll.Width;
			Assert.That(Math.Abs(rightEdgeBeforeScroll), Is.EqualTo(screenWidth).Within(1),
				$"None: right edge ({rightEdgeBeforeScroll}) should be = screenWidth ({screenWidth})");

			// Bottom: edge-to-edge
			ScrollToBottom();
			var bottomRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"None: bottom edge ({bottomRect.Bottom}) should be = screenHeight ({screenHeight})");

			// Left: edge-to-edge
			var leftRectAfterScroll = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftRectAfterScroll.X, Is.EqualTo(0),
				$"None: left X ({leftRectAfterScroll.X}) should be = 0 (edge-to-edge)");

			// Right: edge-to-edge
			var rightRectAfterScroll = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdgeAfterScroll = rightRectAfterScroll.X + rightRectAfterScroll.Width;
			Assert.That(Math.Abs(rightEdgeAfterScroll), Is.EqualTo(screenWidth).Within(1),
				$"None: right edge ({rightEdgeAfterScroll}) should be = screenWidth ({screenWidth})");

			App.SetOrientationPortrait();
			Thread.Sleep(1000);
			ScrollToTop();
		}

		[Test, Order(23)]
		[Description("All: landscape left/right/bottom inset by safe area")]
		public void Validate_ScrollView_Orientation_All_Landscape()
		{
			ClickScrollViewSafeAreaButton();

			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			App.SetOrientationLandscape();
			Thread.Sleep(1000);

			var (screenWidth, screenHeight) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// Left: inset by safe area
			var leftRectBeforeScroll = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftRectBeforeScroll.X), Is.EqualTo(insetsLandscape.Left),
				$"All: left X ({leftRectBeforeScroll.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			// Right: inset by safe area (Android uses display cutout for right inset)
			var rightRectBeforeScroll = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdgeBeforeScroll = rightRectBeforeScroll.X + rightRectBeforeScroll.Width;
			var expectedRight = GetLandscapeRightInset(insetsLandscape.Right, insetsLandscape.CutoutR);
			Assert.That(Math.Abs(rightEdgeBeforeScroll), Is.EqualTo(screenWidth - expectedRight).Within(1),
				$"All: right edge ({rightEdgeBeforeScroll}) should be = screenWidth - expectedRight ({screenWidth - expectedRight})");

			// Bottom: inset by safe area
			ScrollToBottom();
			var bottomRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomRect.Bottom), Is.EqualTo(screenHeight - insetsLandscape.Bottom).Within(1),
				$"All: bottom edge ({bottomRect.Bottom}) should be = screenHeight - insetsLandscape.Bottom ({screenHeight - insetsLandscape.Bottom})");

			// Left: inset by safe area
			var leftRectAfterScroll = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftRectAfterScroll.X), Is.EqualTo(insetsLandscape.Left),
				$"All: left X ({leftRectAfterScroll.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			// Right: inset by safe area (Android uses display cutout for right inset)
			var rightRectAfterScroll = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdgeAfterScroll = rightRectAfterScroll.X + rightRectAfterScroll.Width;
			Assert.That(Math.Abs(rightEdgeAfterScroll), Is.EqualTo(screenWidth - expectedRight).Within(1),
				$"All: right edge ({rightEdgeAfterScroll}) should be = screenWidth - expectedRight ({screenWidth - expectedRight})");

			App.SetOrientationPortrait();
			Thread.Sleep(1000);
			ScrollToTop();
		}

#if TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/37263

		[Test, Order(24)]
		[Description("Container: landscape left/right/bottom inset by safe area")]
		public void Validate_ScrollView_Orientation_Container_Landscape()
		{
			ClickScrollViewSafeAreaButton();

			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			App.SetOrientationLandscape();
			Thread.Sleep(1000);

			var (screenWidth, screenHeight) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// Left: inset by safe area
			var leftRectBeforeScroll = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftRectBeforeScroll.X), Is.EqualTo(insetsLandscape.Left),
				$"Container: left X ({leftRectBeforeScroll.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			// Right: inset by safe area (Android uses display cutout for right inset)
			var rightRectBeforeScroll = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdgeBeforeScroll = rightRectBeforeScroll.X + rightRectBeforeScroll.Width;
			var expectedRight = GetLandscapeRightInset(insetsLandscape.Right, insetsLandscape.CutoutR);
			Assert.That(Math.Abs(rightEdgeBeforeScroll), Is.EqualTo(screenWidth - expectedRight).Within(1),
				$"Container: right edge ({rightEdgeBeforeScroll}) should be = screenWidth - expectedRight ({screenWidth - expectedRight})");

			// Bottom: inset by safe area
			ScrollToBottom();
			var bottomRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomRect.Bottom), Is.EqualTo(screenHeight - insetsLandscape.Bottom).Within(1),
				$"Container: bottom edge ({bottomRect.Bottom}) should be = screenHeight - insetsLandscape.Bottom ({screenHeight - insetsLandscape.Bottom})");

			// Left: inset by safe area
			var leftRectAfterScroll = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftRectAfterScroll.X), Is.EqualTo(insetsLandscape.Left),
				$"Container: left X ({leftRectAfterScroll.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			// Right: inset by safe area (Android uses display cutout for right inset)
			var rightRectAfterScroll = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdgeAfterScroll = rightRectAfterScroll.X + rightRectAfterScroll.Width;
			Assert.That(Math.Abs(rightEdgeAfterScroll), Is.EqualTo(screenWidth - expectedRight).Within(1),
				$"Container: right edge ({rightEdgeAfterScroll}) should be = screenWidth - expectedRight ({screenWidth - expectedRight})");

			App.SetOrientationPortrait();
			Thread.Sleep(1000);
			ScrollToTop();
		}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/36770

		[Test, Order(25)]
		[Description("SoftInput: landscape edges are edge-to-edge")]
		public void Validate_ScrollView_Orientation_SoftInput_Landscape()
		{
			ClickScrollViewSafeAreaButton();

			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			App.SetOrientationLandscape();
			Thread.Sleep(1000);

			var (screenWidth, screenHeight) = GetScreenSize();

			// Left: edge-to-edge
			var leftRectBeforeScroll = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftRectBeforeScroll.X, Is.EqualTo(0),
				$"SoftInput: left X ({leftRectBeforeScroll.X}) should be 0 (edge-to-edge)");

			// Right: edge-to-edge
			var rightRectBeforeScroll = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdgeBeforeScroll = rightRectBeforeScroll.X + rightRectBeforeScroll.Width;
			Assert.That(Math.Abs(rightEdgeBeforeScroll), Is.EqualTo(screenWidth).Within(1),
				$"SoftInput: right edge ({rightEdgeBeforeScroll}) should be = screenWidth ({screenWidth})");

			// Bottom: edge-to-edge (SoftInput doesn't avoid bottom without keyboard)
			ScrollToBottom();
			var bottomRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"SoftInput: bottom edge ({bottomRect.Bottom}) should be = screenHeight ({screenHeight})");

			// Left: edge-to-edge
			var leftRectAfterScroll = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftRectAfterScroll.X, Is.EqualTo(0),
				$"SoftInput: left X ({leftRectAfterScroll.X}) should be 0 (edge-to-edge)");

			// Right: edge-to-edge
			var rightRectAfterScroll = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdgeAfterScroll = rightRectAfterScroll.X + rightRectAfterScroll.Width;
			Assert.That(Math.Abs(rightEdgeAfterScroll), Is.EqualTo(screenWidth).Within(1),
				$"SoftInput: right edge ({rightEdgeAfterScroll}) should be = screenWidth ({screenWidth})");

			App.SetOrientationPortrait();
			Thread.Sleep(1000);
			ScrollToTop();
		}
#endif

#if TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/36863

		[Test, Order(26)]
		[Description("Default: landscape left/right/bottom inset by safe area (Default behaves like Container)")]
		public void Validate_ScrollView_Orientation_Default_Landscape()
		{
			ClickScrollViewSafeAreaButton();

			App.WaitForElement("SafeAreaDefaultButton");
			App.Tap("SafeAreaDefaultButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Default"));

			App.SetOrientationLandscape();
			Thread.Sleep(1000);

			var (screenWidth, screenHeight) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// Left: inset by safe area
			var leftRectBeforeScroll = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftRectBeforeScroll.X), Is.EqualTo(insetsLandscape.Left),
				$"Default: left X ({leftRectBeforeScroll.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			// Right: inset by safe area (Android uses display cutout for right inset)
			var rightRectBeforeScroll = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdgeBeforeScroll = rightRectBeforeScroll.X + rightRectBeforeScroll.Width;
			var expectedRight = GetLandscapeRightInset(insetsLandscape.Right, insetsLandscape.CutoutR);
			Assert.That(Math.Abs(rightEdgeBeforeScroll), Is.EqualTo(screenWidth - expectedRight).Within(1),
				$"Default: right edge ({rightEdgeBeforeScroll}) should be = screenWidth - expectedRight ({screenWidth - expectedRight})");

			// Bottom: inset by safe area
			ScrollToBottom();
			var bottomRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomRect.Bottom), Is.EqualTo(screenHeight - insetsLandscape.Bottom).Within(1),
				$"Default: bottom edge ({bottomRect.Bottom}) should be = screenHeight - insetsLandscape.Bottom ({screenHeight - insetsLandscape.Bottom})");

			// Left: inset by safe area
			var leftRectAfterScroll = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftRectAfterScroll.X), Is.EqualTo(insetsLandscape.Left),
				$"Default: left X ({leftRectAfterScroll.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			// Right: inset by safe area (Android uses display cutout for right inset)
			var rightRectAfterScroll = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdgeAfterScroll = rightRectAfterScroll.X + rightRectAfterScroll.Width;
			Assert.That(Math.Abs(rightEdgeAfterScroll), Is.EqualTo(screenWidth - expectedRight).Within(1),
				$"Default: right edge ({rightEdgeAfterScroll}) should be = screenWidth - expectedRight ({screenWidth - expectedRight})");

			App.SetOrientationPortrait();
			Thread.Sleep(1000);
			ScrollToTop();
		}
#endif
		// ──────────────────────────────────────────────
		// Landscape Keyboard Position Validation
		// ──────────────────────────────────────────────

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS // In landscape mode on Android, the keyboard covers the entire screen, and Appium cannot find elements to validate their positions. Issue Link: https://github.com/dotnet/maui/issues/37263

		[Test, Order(27)]
		[Description("Landscape All: bottom moves up to keyboard, left/right stay inset")]
		public void Validate_ScrollView_Keyboard_All_Landscape()
		{
			ClickScrollViewSafeAreaButton();
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
			Assert.That(Math.Abs(rightBeforeEdge), Is.EqualTo(screenWidth - insetsLandscape.Right).Within(1),
				$"Before keyboard - right edge ({rightBeforeEdge}) should be = screenWidth - insetsLandscape.Right ({screenWidth - insetsLandscape.Right})");

			ScrollToBottom();
			var bottomBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomBeforeRect.Bottom), Is.EqualTo(screenHeight - insetsLandscape.Bottom).Within(1),
				$"Before keyboard - bottom edge ({bottomBeforeRect.Bottom}) should be = screenHeight - insetsLandscape.Bottom ({screenHeight - insetsLandscape.Bottom})");

			// ── Show keyboard ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var keyboardY = GetKeyboardY();

			// Bottom should move up to keyboard top
			var bottomDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomDuringRect.Bottom, Is.EqualTo(keyboardY).Within(1),
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
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var leftAfterRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftAfterRect.X, Is.EqualTo(leftBeforeRect.X),
				$"After keyboard - left X ({leftAfterRect.X}) should return to original ({leftBeforeRect.X})");

			var rightAfterRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightAfterEdge = rightAfterRect.X + rightAfterRect.Width;
			Assert.That(rightAfterEdge, Is.EqualTo(rightBeforeEdge),
				$"After keyboard - right edge ({rightAfterEdge}) should return to original ({rightBeforeEdge})");

			var bottomAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomAfterRect.Bottom, Is.EqualTo(bottomBeforeRect.Bottom).Within(1),
				$"After keyboard - bottom edge ({bottomAfterRect.Bottom}) should return to original ({bottomBeforeRect.Bottom})");

			App.SetOrientationPortrait();
			Thread.Sleep(1000);
			ScrollToTop();
		}

		[Test, Order(28)]
		[Description("Landscape SoftInput: bottom moves up to keyboard while left/right stay edge-to-edge")]
		public void Validate_ScrollView_Keyboard_SoftInput_Landscape()
		{
			ClickScrollViewSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			App.SetOrientationLandscape();
			Thread.Sleep(1000);

			var (screenWidth, screenHeight) = GetScreenSize();

			// ── Before keyboard ──
			var leftBeforeRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftBeforeRect.X, Is.EqualTo(0),
				$"Before keyboard - left X ({leftBeforeRect.X}) should be 0 (edge-to-edge)");

			var rightBeforeRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightBeforeEdge = rightBeforeRect.X + rightBeforeRect.Width;
			Assert.That(Math.Abs(rightBeforeEdge), Is.EqualTo(screenWidth).Within(1),
				$"Before keyboard - right edge ({rightBeforeEdge}) should be = screenWidth ({screenWidth})");

			ScrollToBottom();
			var bottomBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomBeforeRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"Before keyboard - bottom edge ({bottomBeforeRect.Bottom}) should be = screenHeight ({screenHeight})");

			// ── Show keyboard ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var keyboardY = GetKeyboardY();

			// Bottom should move up to keyboard top
			var bottomDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomDuringRect.Bottom, Is.EqualTo(keyboardY).Within(1),
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
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var leftAfterRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftAfterRect.X, Is.EqualTo(leftBeforeRect.X),
				$"After keyboard - left X ({leftAfterRect.X}) should return to original ({leftBeforeRect.X})");

			var rightAfterRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightAfterEdge = rightAfterRect.X + rightAfterRect.Width;
			Assert.That(rightAfterEdge, Is.EqualTo(rightBeforeEdge),
				$"After keyboard - right edge ({rightAfterEdge}) should return to original ({rightBeforeEdge})");

			var bottomAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomAfterRect.Bottom, Is.EqualTo(bottomBeforeRect.Bottom).Within(1),
				$"After keyboard - bottom edge ({bottomAfterRect.Bottom}) should return to original ({bottomBeforeRect.Bottom})");

			App.SetOrientationPortrait();
			Thread.Sleep(1000);
			ScrollToTop();
		}

		[Test, Order(29)]
		[Description("Landscape None: bottom stays at screen edge with keyboard, left/right stay edge-to-edge")]
		public void Validate_ScrollView_Keyboard_None_Landscape()
		{
			ClickScrollViewSafeAreaButton();
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
			Assert.That(Math.Abs(rightBeforeEdge), Is.EqualTo(screenWidth).Within(1),
				$"Before keyboard - right edge ({rightBeforeEdge}) should be = screenWidth ({screenWidth})");

			ScrollToBottom();
			var bottomBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomBeforeRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"Before keyboard - bottom edge ({bottomBeforeRect.Bottom}) should be = screenHeight ({screenHeight})");

			// ── Show keyboard ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			// Bottom should NOT move (None ignores keyboard)
			var bottomDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomDuringRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"During keyboard - bottom edge ({bottomDuringRect.Bottom}) should remain at screenHeight ({screenHeight})");

			// Left/Right should remain unchanged
			var leftDuringRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftDuringRect.X, Is.EqualTo(0),
				$"During keyboard - left X ({leftDuringRect.X}) should remain at 0 (edge-to-edge)");

			var rightDuringRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightDuringEdge = rightDuringRect.X + rightDuringRect.Width;
			Assert.That(Math.Abs(rightDuringEdge), Is.EqualTo(screenWidth).Within(1),
				$"During keyboard - right edge ({rightDuringEdge}) should remain at screenWidth ({screenWidth})");

			// ── Dismiss keyboard ──
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var leftAfterRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftAfterRect.X, Is.EqualTo(leftBeforeRect.X),
				$"After keyboard - left X ({leftAfterRect.X}) should return to original ({leftBeforeRect.X})");

			var rightAfterRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightAfterEdge = rightAfterRect.X + rightAfterRect.Width;
			Assert.That(rightAfterEdge, Is.EqualTo(rightBeforeEdge),
				$"After keyboard - right edge ({rightAfterEdge}) should return to original ({rightBeforeEdge})");

			var bottomAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomAfterRect.Bottom, Is.EqualTo(bottomBeforeRect.Bottom).Within(1),
				$"After keyboard - bottom edge ({bottomAfterRect.Bottom}) should return to original ({bottomBeforeRect.Bottom})");

			App.SetOrientationPortrait();
			Thread.Sleep(1000);
			ScrollToTop();
		}

		[Test, Order(30)]
		[Description("Landscape Container: bottom stays at safe area inset with keyboard, left/right stay inset")]
		public void Validate_ScrollView_Keyboard_Container_Landscape()
		{
			ClickScrollViewSafeAreaButton();
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
			Assert.That(Math.Abs(rightBeforeEdge), Is.EqualTo(screenWidth - insetsLandscape.Right).Within(1),
				$"Before keyboard - right edge ({rightBeforeEdge}) should be = screenWidth - insetsLandscape.Right ({screenWidth - insetsLandscape.Right})");

			ScrollToBottom();
			var bottomBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomBeforeRect.Bottom), Is.EqualTo(screenHeight - insetsLandscape.Bottom).Within(1),
				$"Before keyboard - bottom edge ({bottomBeforeRect.Bottom}) should be = screenHeight - insetsLandscape.Bottom ({screenHeight - insetsLandscape.Bottom})");

			// ── Show keyboard ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			// Bottom should NOT move (Container ignores keyboard)
			var bottomDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomDuringRect.Bottom, Is.EqualTo(bottomBeforeRect.Bottom).Within(1),
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
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var leftAfterRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftAfterRect.X, Is.EqualTo(leftBeforeRect.X),
				$"After keyboard - left X ({leftAfterRect.X}) should return to original ({leftBeforeRect.X})");

			var rightAfterRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightAfterEdge = rightAfterRect.X + rightAfterRect.Width;
			Assert.That(rightAfterEdge, Is.EqualTo(rightBeforeEdge),
				$"After keyboard - right edge ({rightAfterEdge}) should return to original ({rightBeforeEdge})");

			var bottomAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomAfterRect.Bottom, Is.EqualTo(bottomBeforeRect.Bottom).Within(1),
				$"After keyboard - bottom edge ({bottomAfterRect.Bottom}) should return to original ({bottomBeforeRect.Bottom})");

			App.SetOrientationPortrait();
			Thread.Sleep(1000);
			ScrollToTop();
		}

		[Test, Order(31)]
		[Description("Landscape Default: bottom stays at safe area inset with keyboard (behaves like Container)")]
		public void Validate_ScrollView_Keyboard_Default_Landscape()
		{
			ClickScrollViewSafeAreaButton();
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
			Assert.That(Math.Abs(rightBeforeEdge), Is.EqualTo(screenWidth - insetsLandscape.Right).Within(1),
				$"Before keyboard - right edge ({rightBeforeEdge}) should be = screenWidth - insetsLandscape.Right ({screenWidth - insetsLandscape.Right})");

			ScrollToBottom();
			var bottomBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomBeforeRect.Bottom), Is.EqualTo(screenHeight - insetsLandscape.Bottom).Within(1),
				$"Before keyboard - bottom edge ({bottomBeforeRect.Bottom}) should be = screenHeight - insetsLandscape.Bottom ({screenHeight - insetsLandscape.Bottom})");

			// ── Show keyboard ──
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			// Bottom should NOT move (Default behaves like Container)
			var bottomDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomDuringRect.Bottom, Is.EqualTo(bottomBeforeRect.Bottom).Within(1),
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
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var leftAfterRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftAfterRect.X, Is.EqualTo(leftBeforeRect.X),
				$"After keyboard - left X ({leftAfterRect.X}) should return to original ({leftBeforeRect.X})");

			var rightAfterRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightAfterEdge = rightAfterRect.X + rightAfterRect.Width;
			Assert.That(rightAfterEdge, Is.EqualTo(rightBeforeEdge),
				$"After keyboard - right edge ({rightAfterEdge}) should return to original ({rightBeforeEdge})");

			var bottomAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomAfterRect.Bottom, Is.EqualTo(bottomBeforeRect.Bottom).Within(1),
				$"After keyboard - bottom edge ({bottomAfterRect.Bottom}) should return to original ({bottomBeforeRect.Bottom})");

			App.SetOrientationPortrait();
			Thread.Sleep(1000);
			ScrollToTop();
		}
#endif

		// ──────────────────────────────────────────────
		// Default + Keyboard (Portrait)
		// ──────────────────────────────────────────────
#if TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/36863

#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/36864
		[Test, Order(32)]
		[Description("With Default, bottom indicator does NOT move when keyboard is shown (behaves like Container)")]
		public void Validate_ScrollView_Keyboard_Default_BottomStays()
		{
			ClickScrollViewSafeAreaButton();
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

			ScrollToBottom();
			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

#if IOS
			// Bottom should NOT move (Default behaves like Container — ignores keyboard)
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should equal (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
#else
			App.WaitForNoElement("BottomEdgeIndicator");
#endif
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom).Within(1),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");

			ScrollToTop();

			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should return to original ({topLabelBeforeRect.Y})");
		}
#endif
		// ──────────────────────────────────────────────
		// Per-Edge + Keyboard (Portrait)
		// ──────────────────────────────────────────────

		[Test, Order(33)]
		[Description("Per-edge B:None + keyboard — bottom stays edge-to-edge when keyboard is shown")]
		public void Validate_ScrollView_PerEdgeKeyboard_BottomNone_BottomStays()
		{
			ClickScrollViewSafeAreaButton();
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

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// Verify top: Container — should be inset by safe area top
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Top (Container): label Y ({topLabelRect.Y}) should be ≈ insets.Top ({insets.Top})");

			ScrollToBottom();
			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

#if IOS
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelDuringRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should stay at screenHeight ({screenHeight})");
#else
			App.WaitForNoElement("BottomEdgeIndicator");
#endif
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom).Within(1),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
			ScrollToTop();
		}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/36864
		[Test, Order(34)]
		[Description("Per-edge B:Container + keyboard — bottom stays at safe area inset when keyboard is shown")]
		public void Validate_ScrollView_PerEdgeKeyboard_BottomContainer_BottomStays()
		{
			ClickScrollViewSafeAreaButton();
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

			// Verify top: Container — should be inset by safe area top
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Top (Container): label Y ({topLabelRect.Y}) should be ≈ insets.Top ({insets.Top})");

			ScrollToBottom();
			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

#if IOS
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should stay at (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");
#else
			App.WaitForNoElement("BottomEdgeIndicator");
#endif
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom).Within(1),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
			ScrollToTop();
		}
#endif

#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/36826
		[Test, Order(35)]
		[Description("Per-edge B:SoftInput + keyboard — bottom moves up to keyboard Y")]
		public void Validate_ScrollView_PerEdgeKeyboard_BottomSoftInput_BottomMovesUp()
		{
			ClickScrollViewSafeAreaButton();
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

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// Verify top: Container — should be inset by safe area top
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Top (Container): label Y ({topLabelRect.Y}) should be ≈ insets.Top ({insets.Top})");

			ScrollToBottom();
			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight).Within(1),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to screenHeight ({screenHeight})");

			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var keyboardY = GetKeyboardY();

			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(keyboardY).Within(1),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should equal keyboard Y ({keyboardY})");

			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom).Within(1),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
			ScrollToTop();
		}

		[Test, Order(36)]
		[Description("Per-edge B:All + keyboard — bottom moves up to keyboard Y")]
		public void Validate_ScrollView_PerEdgeKeyboard_BottomAll_BottomMovesUp()
		{
			ClickScrollViewSafeAreaButton();
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

			// Verify top: Container — should be inset by safe area top
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Top (Container): label Y ({topLabelRect.Y}) should be ≈ insets.Top ({insets.Top})");

			ScrollToBottom();
			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should not be visible before tapping entry");
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping entry");

			var keyboardY = GetKeyboardY();

			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(keyboardY).Within(1),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should equal keyboard Y ({keyboardY})");

			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
			Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom).Within(1),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");
			ScrollToTop();
		}
#endif
#endif

		// ──────────────────────────────────────────────
		// Orientation Roundtrip
		// ──────────────────────────────────────────────

		[Test, Order(37)]
		[Description("Rotate to landscape and back to portrait — positions restore correctly")]
		public void Validate_ScrollView_Orientation_Roundtrip_PositionsRestore()
		{
			ClickScrollViewSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var insets = GetSafeAreaInsets();
			var (_, screenHeight) = GetScreenSize();

			// ── Record portrait positions ──
			var topPortraitRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topPortraitRect.Y), Is.EqualTo(insets.Top),
				$"Portrait: top label Y ({topPortraitRect.Y}) should be equal to insets.Top ({insets.Top})");

			ScrollToBottom();
			var bottomPortraitRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomPortraitRect.Bottom), Is.EqualTo(screenHeight - insets.Bottom).Within(1),
				$"Portrait: bottom label Bottom ({bottomPortraitRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeight - insets.Bottom})");

			// ── Rotate to landscape ──
			App.SetOrientationLandscape();
			Thread.Sleep(1000);

			ScrollToTop();

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
			var expectedRight = GetLandscapeRightInset(insetsLandscape.Right, insetsLandscape.CutoutR);
			Assert.That(Math.Abs(rightLandscapeEdge), Is.EqualTo(screenWidthLandscape - expectedRight).Within(1),
				$"Landscape: right edge ({rightLandscapeEdge}) should be equal to screenWidth - expectedRight ({screenWidthLandscape - expectedRight})");

			ScrollToBottom();

			// ── Rotate back to portrait ──
			App.SetOrientationPortrait();
			Thread.Sleep(1000);

			ScrollToTop();

			var insetsAfter = GetSafeAreaInsets();
			var (_, screenHeightAfter) = GetScreenSize();

			var topAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topAfterRect.Y), Is.EqualTo(insetsAfter.Top),
				$"After roundtrip: top label Y ({topAfterRect.Y}) should be equal to insets.Top ({insetsAfter.Top})");

			ScrollToBottom();
			var bottomAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomAfterRect.Bottom), Is.EqualTo(screenHeightAfter - insetsAfter.Bottom).Within(1),
				$"After roundtrip: bottom label Bottom ({bottomAfterRect.Bottom}) should be equal to (screenHeight - insets.Bottom) ({screenHeightAfter - insetsAfter.Bottom})");

			// Verify positions match the original portrait positions
			Assert.That(topAfterRect.Y, Is.EqualTo(topPortraitRect.Y),
				$"After roundtrip: top label Y ({topAfterRect.Y}) should match original portrait ({topPortraitRect.Y})");

			Assert.That(bottomAfterRect.Bottom, Is.EqualTo(bottomPortraitRect.Bottom).Within(1),
				$"After roundtrip: bottom label Bottom ({bottomAfterRect.Bottom}) should match original portrait ({bottomPortraitRect.Bottom})");

			ScrollToTop();
		}

		// ──────────────────────────────────────────────
		// Left/Right Per-Edge in Landscape
		// ──────────────────────────────────────────────

		[Test, Order(38)]
		[Description("Landscape per-edge: L:Container, R:None — left inset by safe area, right edge-to-edge")]
		public void Validate_ScrollView_PerEdge_LeftContainerRightNone_Landscape()
		{
			ClickScrollViewSafeAreaButton();
			App.DismissKeyboard();

			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("LeftContainer");
			App.Tap("LeftContainer");
			App.Tap("RightNone");
			App.WaitForElement("Apply");
			App.Tap("Apply");

			App.SetOrientationLandscape();
			Thread.Sleep(1000);

			var (screenWidth, _) = GetScreenSize();
			var insetsLandscape = GetSafeAreaInsets();

			// Left: inset by safe area (Container)
			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftRect.X), Is.EqualTo(insetsLandscape.Left),
				$"Left (Container): X ({leftRect.X}) should be = insetsLandscape.Left ({insetsLandscape.Left})");

			// Right: edge-to-edge (None)
			var rightRect = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdge = rightRect.X + rightRect.Width;
			Assert.That(Math.Abs(rightEdge), Is.EqualTo(screenWidth).Within(1),
				$"Right (None): right edge ({rightEdge}) should be = screenWidth ({screenWidth})");

			App.SetOrientationPortrait();
			Thread.Sleep(1000);
		}
	}
}
#endif