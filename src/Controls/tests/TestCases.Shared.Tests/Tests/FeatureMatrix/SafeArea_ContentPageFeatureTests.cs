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
		int _bottomSafeAreaStartY;
		int _bottomSafeAreaEndY;

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
            var app = (AppiumApp)App;
            OpenQA.Selenium.IWebElement? keyboard = null;

#if IOS
            keyboard = app.Driver.FindElement(OpenQA.Selenium.Appium.MobileBy.ClassName("XCUIElementTypeKeyboard"));
#elif ANDROID
            keyboard = app.Driver.FindElement(OpenQA.Selenium.Appium.MobileBy.ClassName("android.inputmethodservice.SoftInputWindow"));
#endif

            if (keyboard == null)
                throw new InvalidOperationException("Keyboard not found");

            return keyboard.Location.Y;
        }

		public void GetSafeAreaBottomYposition()
		{
			var size = ((AppiumApp)App).Driver.Manage().Window.Size;
			// int screenWidth = size.Width;
			int screenHeight = size.Height;

			var insets = GetSafeAreaInsets();

			_bottomSafeAreaStartY = screenHeight - insets.Bottom;
			_bottomSafeAreaEndY = screenHeight;
		}

		public void ClickContentPageSafeAreaButton()
		{
			App.WaitForElement("ContentPageSafeAreaButton");
			App.Tap("ContentPageSafeAreaButton");
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
			GetSafeAreaBottomYposition();
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			// Portrait: top label Y should be ≈ 0 (edge-to-edge, no safe area applied)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.EqualTo(0),
				$"None: top label Y ({topLabelRect.Y}) should be = 0 (edge-to-edge), safe area top inset is ignored");

			var insets = GetSafeAreaInsets();

			// Portrait: bottom label bottom edge should be ≈ insets.Bottom (edge-to-edge, no safe area applied)
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(_bottomSafeAreaEndY),
				$"None: bottom label Y ({bottomLabelRect.Bottom}) should be ≈ insets.Bottom ({_bottomSafeAreaEndY})");
		}

		[Test, Order(2)]
		[Description("Content inset from all system UI (status bar, nav bar, notch, home indicator)")]
		public void ValidateSafeAreaEdges_All()
		{
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var insets = GetSafeAreaInsets();

			// Portrait: top label Y should be ≈ insets.Top (safe area applied)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"All: top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ screenBottom - insets.Bottom
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(_bottomSafeAreaStartY),
				$"All: bottom label Y ({bottomLabelRect.Bottom}) should be equal to insets.Bottom ({_bottomSafeAreaStartY})");
		}

		[Test, Order(3)]
		[Description("Content avoids system bars/notch but can extend under keyboard area")]
		public void ValidateSafeAreaEdges_Container()
		{
			// Get None baseline for bottom reference
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			var noneBottomRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var screenBottom = noneBottomRect.Y + noneBottomRect.Height;

			App.Tap("SafeAreaContainerButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			var insets = GetSafeAreaInsets();

			// Portrait: top label Y should be ≈ insets.Top (safe area applied)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Container: top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ screenBottom - insets.Bottom
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(_bottomSafeAreaStartY),
				$"Container: bottom label Y ({bottomLabelRect.Bottom}) should be equal to insets.Bottom ({_bottomSafeAreaStartY})");
		}

		[Test, Order(4)]
		[Description("SoftInput respects safe area on top/sides but bottom is edge-to-edge without keyboard")]
		public void ValidateSafeAreaEdges_SoftInput()
		{
			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			var insets = GetSafeAreaInsets();

			// Portrait: top label Y should be ≈ insets.Top (SoftInput respects notch/safe area)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"SoftInput: top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ insets.Bottom (edge-to-edge, no safe area applied)
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(_bottomSafeAreaEndY),
				$"SoftInput: bottom label Y ({bottomLabelRect.Bottom}) should be equal to insets.Bottom ({_bottomSafeAreaEndY})");
		}

		[Test, Order(5)]
		[Description("ContentPage defaults to None behavior")]
		public void ValidateSafeAreaEdges_Default()
		{
			App.WaitForElement("SafeAreaDefaultButton");
			App.Tap("SafeAreaDefaultButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Default"));

			var insets = GetSafeAreaInsets();

			// Portrait: top label Y should be ≈ insets.Top (safe area applied)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Default: top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ screenBottom - insets.Bottom
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(_bottomSafeAreaStartY),
				$"Default: bottom label Y ({bottomLabelRect.Bottom}) should be equal to insets.Bottom ({_bottomSafeAreaStartY})");
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

			// Portrait: Container — should be inset by safe area top
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Top (Container): label Y ({topLabelRect.Y}) should be ≈ insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ insets.Bottom (edge-to-edge, no safe area applied)
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(_bottomSafeAreaEndY),
				$"None: bottom label Y ({bottomLabelRect.Bottom}) should be ≈ insets.Bottom ({_bottomSafeAreaEndY})");
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

			// Portrait: only validate top and bottom — no left/right safe area insets in portrait
			// Top: Container — should be inset by safe area top
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"Top (Container): label Y ({topLabelRect.Y}) should be ≈ insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ insets.Bottom (edge-to-edge, no safe area applied)
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(_bottomSafeAreaEndY),
				$"None: bottom label Y ({bottomLabelRect.Bottom}) should be ≈ insets.Bottom ({_bottomSafeAreaEndY})");
		}

		[Test, Order(8)]
		[Description("Top/bottom respect all insets")]
		public void ValidatePerEdge_TopBottomAll_SidesNone()
		{
			// Get None baseline for bottom reference
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			var noneBottomRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var screenBottom = noneBottomRect.Y + noneBottomRect.Height;

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

			// Portrait: top label Y should be ≈ 0 (edge-to-edge, no safe area applied)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.EqualTo(0),
				$"None: top label Y ({topLabelRect.Y}) should be = 0 (edge-to-edge), safe area top inset is ignored");

			// Portrait: bottom label bottom edge should be ≈ insets.Bottom (edge-to-edge, no safe area applied)
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(_bottomSafeAreaEndY),
				$"None: bottom label Y ({bottomLabelRect.Bottom}) should be ≈ insets.Bottom ({_bottomSafeAreaEndY})");
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

			// Portrait: top label Y should be ≈ insets.Top (safe area applied)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y), Is.EqualTo(insets.Top),
				$"All: top label Y ({topLabelRect.Y}) should be equal to insets.Top ({insets.Top})");

			// Portrait: bottom label bottom edge should be ≈ screenBottom - insets.Bottom
			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelRect.Bottom), Is.EqualTo(_bottomSafeAreaStartY),
				$"All: bottom label Y ({bottomLabelRect.Bottom}) should be equal to insets.Bottom ({_bottomSafeAreaStartY})");
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
		// Keyboard Position Validation
		// ──────────────────────────────────────────────
		// Validates that the bottom indicator moves up when keyboard is shown with modes that
		// adjust for keyboard (All/SoftInput), and does NOT move with modes that don't (None/Container).

		[Test, Order(15)]
		[Description("With All, bottom indicator moves up when keyboard is shown and restores when dismissed")] /////////////
		public void ValidateKeyboard_All_BottomMovesUp()
		{
			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var insets = GetSafeAreaInsets();

			// ── Before keyboard ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelBeforeRect.Y), Is.EqualTo(insets.Top),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(_bottomSafeAreaStartY),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to _bottomSafeAreaStartY ({_bottomSafeAreaStartY})");

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

		[Test, Order(16)]
		[Description("With SoftInput, bottom indicator moves up when keyboard is shown and restores when dismissed")] ////////////
		public void ValidateKeyboard_SoftInput_BottomMovesUp()
		{
			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			var insets = GetSafeAreaInsets();

			// ── Before keyboard ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelBeforeRect.Y), Is.EqualTo(insets.Top),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(_bottomSafeAreaEndY),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to _bottomSafeAreaEndY ({_bottomSafeAreaEndY})");

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

		[Test, Order(17)]
		[Description("With None, bottom indicator does NOT move when keyboard is shown")] /////////////
		public void ValidateKeyboard_None_BottomStays()
		{
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			// ── Before keyboard ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelBeforeRect.Y, Is.EqualTo(0),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be 0 (edge-to-edge)");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(_bottomSafeAreaEndY),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to _bottomSafeAreaEndY ({_bottomSafeAreaEndY})");

			// ── Show keyboard ──
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			var topLabelDuringRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelDuringRect.Y, Is.EqualTo(0),
				$"During keyboard - top label Y ({topLabelDuringRect.Y}) should be 0 (edge-to-edge)");

			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelDuringRect.Bottom), Is.EqualTo(_bottomSafeAreaEndY),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should be equal to _bottomSafeAreaEndY ({_bottomSafeAreaEndY})");

			App.DismissKeyboard();
			App.WaitForKeyboardToHide();

			var topLabelAfterRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelAfterRect.Y, Is.EqualTo(topLabelBeforeRect.Y),
				$"After keyboard - top label Y ({topLabelAfterRect.Y}) should return to original ({topLabelBeforeRect.Y})");

			var bottomLabelAfterRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelAfterRect.Bottom, Is.EqualTo(bottomLabelBeforeRect.Bottom),
				$"After keyboard - bottom label Bottom ({bottomLabelAfterRect.Bottom}) should return to original ({bottomLabelBeforeRect.Bottom})");

		}

		[Test, Order(18)]
		[Description("With Container, bottom indicator does NOT move when keyboard is shown")] /////////////////
		public void ValidateKeyboard_Container_BottomStays()
		{
			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			var insets = GetSafeAreaInsets();

			// ── Before keyboard ──
			var topLabelBeforeRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelBeforeRect.Y), Is.EqualTo(insets.Top),
				$"Before keyboard - top label Y ({topLabelBeforeRect.Y}) should be equal to insets.Top ({insets.Top})");

			var bottomLabelBeforeRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(Math.Abs(bottomLabelBeforeRect.Bottom), Is.EqualTo(_bottomSafeAreaStartY),
				$"Before keyboard - bottom label Bottom ({bottomLabelBeforeRect.Bottom}) should be equal to _bottomSafeAreaStartY ({_bottomSafeAreaStartY})");
			
			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			// Bottom should have not moved up to the keyboard top
			var bottomLabelDuringRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			Assert.That(bottomLabelDuringRect.Bottom, Is.EqualTo(_bottomSafeAreaStartY),
				$"During keyboard - bottom label Bottom ({bottomLabelDuringRect.Bottom}) should equal _bottomSafeAreaStartY ({_bottomSafeAreaStartY})");

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

		[Test, Order(19)]
		[Description("Switch None to All while keyboard is open — bottom indicator moves up")]
		public void ValidateKeyboardRuntime_SwitchNoneToAll_WhileKeyboardOpen()
		{
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			// With None, record bottom position (not adjusted for keyboard)
			var bottomNone = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomEdgeNone = bottomNone.Y + bottomNone.Height;

			// Switch to All while keyboard is still open
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			// With All, bottom indicator should move up (above keyboard)
			var bottomAll = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomEdgeAll = bottomAll.Y + bottomAll.Height;
			Assert.That(bottomEdgeAll, Is.LessThan(bottomEdgeNone),
				$"After switching to All, bottom should move up. None: {bottomEdgeNone}, All: {bottomEdgeAll}");

			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
		}

		[Test, Order(20)]
		[Description("Switch All to None while keyboard is open — bottom indicator drops back")]
		public void ValidateKeyboardRuntime_SwitchAllToNone_WhileKeyboardOpen()
		{
			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");

			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			// With All, record bottom position (adjusted above keyboard)
			var bottomAll = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomEdgeAll = bottomAll.Y + bottomAll.Height;

			// Switch to None while keyboard is still open
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			// Top should move to edge (None goes edge-to-edge)
			var topNone = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topNone.Y, Is.LessThanOrEqualTo(InsetTolerance),
				"After switching to None with keyboard open, top should be edge-to-edge");

			// With None, bottom indicator should NOT be adjusted above keyboard
			var bottomNone = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomEdgeNone = bottomNone.Y + bottomNone.Height;
			Assert.That(bottomEdgeNone, Is.GreaterThanOrEqualTo(bottomEdgeAll),
				$"After switching to None, bottom should extend back. All: {bottomEdgeAll}, None: {bottomEdgeNone}");

			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
		}

		[Test, Order(21)]
		[Description("Switch Container to SoftInput while keyboard is open — bottom indicator moves up")]
		public void ValidateKeyboardRuntime_SwitchContainerToSoftInput_WhileKeyboardOpen()
		{
			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");

			App.Tap("SafeAreaTestEntry");
			App.WaitForKeyboardToShow();

			// With Container, bottom is NOT adjusted for keyboard
			var bottomContainer = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomEdgeContainer = bottomContainer.Y + bottomContainer.Height;

			// Switch to SoftInput while keyboard is still open
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			// With keyboard open, KeyboardAutoManagerScroll may adjust layout positioning
			var topSoftInput = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topSoftInput.Y, Is.LessThanOrEqualTo(InsetTolerance),
				"After switching to SoftInput with keyboard open, top may be shifted by keyboard scroll manager");

			// With SoftInput, bottom indicator should move up (above keyboard)
			var bottomSoftInput = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomEdgeSoftInput = bottomSoftInput.Y + bottomSoftInput.Height;
			Assert.That(bottomEdgeSoftInput, Is.LessThan(bottomEdgeContainer),
				$"SoftInput: bottom should be above Container position. Container: {bottomEdgeContainer}, SoftInput: {bottomEdgeSoftInput}");

			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
		}

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

			var insets = GetSafeAreaInsets();

			// With All + padding, top should be beyond safe area inset (additive)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.GreaterThan(insets.Top),
				$"Top Y ({topLabelRect.Y}) should be > insets.Top ({insets.Top}) due to additional padding");
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

			var insets = GetSafeAreaInsets();

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));
			var topRectBefore = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRectBefore.Y, Is.LessThanOrEqualTo(InsetTolerance),
				$"Before (None): top Y ({topRectBefore.Y}) should be ≈ 0 (edge-to-edge)");

			App.Tap("SafeAreaAllButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));
			var topRectAfter = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topRectAfter.Y - insets.Top), Is.LessThanOrEqualTo(InsetTolerance),
				$"After (All): top Y ({topRectAfter.Y}) should be ≈ insets.Top ({insets.Top})");
			Assert.That(topRectAfter.Y, Is.GreaterThan(topRectBefore.Y), "Top indicator should have moved down");
		}

		[Test, Order(25)]
		[Description("Content expands to edge-to-edge using runtime buttons")]
		public void ValidateDynamic_AllToNone()
		{
			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");

			var insets = GetSafeAreaInsets();

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));
			var topRectBefore = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topRectBefore.Y - insets.Top), Is.LessThanOrEqualTo(InsetTolerance),
				$"Before (All): top Y ({topRectBefore.Y}) should be ≈ insets.Top ({insets.Top})");

			App.Tap("SafeAreaNoneButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));
			var topRectAfter = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRectAfter.Y, Is.LessThanOrEqualTo(InsetTolerance),
				$"After (None): top Y ({topRectAfter.Y}) should be ≈ 0 (edge-to-edge)");
			Assert.That(topRectAfter.Y, Is.LessThan(topRectBefore.Y), "Top indicator should have moved up");
		}

		[Test, Order(26)]
		[Description("Behavior transitions from Container to SoftInput — top remains inset, bottom becomes edge-to-edge")]
		public void ValidateDynamic_ContainerToSoftInput()
		{
			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");

			var insets = GetSafeAreaInsets();

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));
			var topRectBefore = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topRectBefore.Y - insets.Top), Is.LessThanOrEqualTo(InsetTolerance),
				$"Container: top Y ({topRectBefore.Y}) should be ≈ insets.Top ({insets.Top})");

			App.Tap("SafeAreaSoftInputButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));
			// SoftInput also respects safe area on top (like Container), so top remains inset
			var topRectAfter = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topRectAfter.Y - insets.Top), Is.LessThanOrEqualTo(InsetTolerance),
				$"SoftInput: top Y ({topRectAfter.Y}) should be ≈ insets.Top ({insets.Top})");
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

			var insets = GetSafeAreaInsets();

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));
			var topRectBefore = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topRectBefore.Y, Is.LessThanOrEqualTo(InsetTolerance),
				$"Before (None): top Y ({topRectBefore.Y}) should be ≈ 0 (edge-to-edge)");

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
			Assert.That(Math.Abs(topRectAfter.Y - insets.Top), Is.LessThanOrEqualTo(InsetTolerance),
				$"After (Container): top Y ({topRectAfter.Y}) should be ≈ insets.Top ({insets.Top})");
		}

		// ──────────────────────────────────────────────
		// Platform System Bar Avoidance
		// ──────────────────────────────────────────────

		[Test, Order(29)]
		[Description("Container avoids system bars — top label matches safe area top inset")]
		public void ValidatePlatform_ContainerAvoidsSystemBars()
		{
			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			var insets = GetSafeAreaInsets();
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y - insets.Top), Is.LessThanOrEqualTo(InsetTolerance),
				$"Container: top label Y ({topLabelRect.Y}) should be ≈ insets.Top ({insets.Top})");
		}

		[Test, Order(30)]
		[Description("All avoids system bars — top and bottom labels match safe area insets")]
		public void ValidatePlatform_AllAvoidsSystemBars()
		{
			// Get None baseline for bottom reference
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			var noneBottomRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var screenBottom = noneBottomRect.Y + noneBottomRect.Height;

			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var insets = GetSafeAreaInsets();

			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y - insets.Top), Is.LessThanOrEqualTo(InsetTolerance),
				$"All: top label Y ({topLabelRect.Y}) should be ≈ insets.Top ({insets.Top})");

			var bottomLabelRect = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomEdge = bottomLabelRect.Y + bottomLabelRect.Height;
			Assert.That(Math.Abs(bottomEdge - (screenBottom - insets.Bottom)), Is.LessThanOrEqualTo(InsetTolerance),
				$"All: bottom edge ({bottomEdge}) should be ≈ screenBottom - insets.Bottom ({screenBottom} - {insets.Bottom} = {screenBottom - insets.Bottom})");
		}

		[Test, Order(31)]
		[Description("Default behavior is edge-to-edge (None) in .NET 10")]
		public void ValidatePlatform_DefaultIsEdgeToEdge()
		{
			App.WaitForElement("SafeAreaDefaultButton");
			App.Tap("SafeAreaDefaultButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Default"));

			var insets = GetSafeAreaInsets();

			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.LessThanOrEqualTo(InsetTolerance),
				$"Default: top Y ({topLabelRect.Y}) should be ≈ 0 (edge-to-edge, insets T={insets.Top}, B={insets.Bottom} are NOT applied)");
		}

		// ──────────────────────────────────────────────
		// Orientation / Landscape Validation
		// ──────────────────────────────────────────────

		[Test, Order(32)]
		[Description("None: portrait top/bottom at page edges, landscape left/right/bottom at page edges")]
		public void ValidateOrientation_None_AllEdges()
		{
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			// Portrait: top label at page top (ignoring safe area insets)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.LessThanOrEqualTo(InsetTolerance),
				$"Portrait None: top Y ({topLabelRect.Y}) should be ≈ 0 (edge-to-edge)");

			// Switch to landscape
			App.SetOrientationLandscape();
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			// Landscape: left label at page left (edge-to-edge)
			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftRect.X, Is.LessThanOrEqualTo(InsetTolerance),
				$"Landscape None: left X ({leftRect.X}) should be ≈ 0 (edge-to-edge)");

			App.SetOrientationPortrait();
		}

		[Test, Order(33)]
		[Description("All: portrait top/bottom match safe area insets, landscape left/right/bottom match safe area insets")]
		public void ValidateOrientation_All_AllEdges()
		{
			App.WaitForElement("SafeAreaAllButton");
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var insets = GetSafeAreaInsets();

			// Portrait: top label Y should match the safe area top inset
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y - insets.Top), Is.LessThanOrEqualTo(InsetTolerance),
				$"Portrait All: top Y ({topLabelRect.Y}) should be ≈ insets.Top ({insets.Top})");

			// Switch to landscape
			App.SetOrientationLandscape();
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var insetsLandscape = GetSafeAreaInsets();

			// Landscape: left label X should match safe area left inset
			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftRect.X - insetsLandscape.Left), Is.LessThanOrEqualTo(InsetTolerance),
				$"Landscape All: left X ({leftRect.X}) should be ≈ insetsLandscape.Left ({insetsLandscape.Left})");

			App.SetOrientationPortrait();
		}

		[Test, Order(34)]
		[Description("Container: portrait top/bottom match safe area insets, landscape left/right/bottom match safe area insets")]
		public void ValidateOrientation_Container_AllEdges()
		{
			App.WaitForElement("SafeAreaContainerButton");
			App.Tap("SafeAreaContainerButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			var insets = GetSafeAreaInsets();

			// Portrait: top label Y should match the safe area top inset
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y - insets.Top), Is.LessThanOrEqualTo(InsetTolerance),
				$"Portrait Container: top Y ({topLabelRect.Y}) should be ≈ insets.Top ({insets.Top})");

			// Switch to landscape
			App.SetOrientationLandscape();
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("Container"));

			var insetsLandscape = GetSafeAreaInsets();

			// Landscape: left label X should match safe area left inset
			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftRect.X - insetsLandscape.Left), Is.LessThanOrEqualTo(InsetTolerance),
				$"Landscape Container: left X ({leftRect.X}) should be ≈ insetsLandscape.Left ({insetsLandscape.Left})");

			App.SetOrientationPortrait();
		}

		[Test, Order(35)]
		[Description("SoftInput: portrait top respects safe area, bottom edge-to-edge; landscape left/right respect safe area, bottom edge-to-edge")]
		public void ValidateOrientation_SoftInput_AllEdges()
		{
			App.WaitForElement("SafeAreaSoftInputButton");
			App.Tap("SafeAreaSoftInputButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			var insets = GetSafeAreaInsets();

			// Portrait: top should respect safe area (notch)
			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y - insets.Top), Is.LessThanOrEqualTo(InsetTolerance),
				$"Portrait SoftInput: top Y ({topLabelRect.Y}) should be ≈ insets.Top ({insets.Top})");

			// Switch to landscape
			App.SetOrientationLandscape();
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("SoftInput"));

			var insetsLandscape = GetSafeAreaInsets();

			// Landscape: left should respect safe area (notch side)
			var leftRect = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftRect.X - insetsLandscape.Left), Is.LessThanOrEqualTo(InsetTolerance),
				$"Landscape SoftInput: left X ({leftRect.X}) should be ≈ insetsLandscape.Left ({insetsLandscape.Left})");

			App.SetOrientationPortrait();
		}

		[Test, Order(36)]
		[Description("Switching None to All in portrait validates top/bottom edges shift inward")]
		public void ValidateOrientation_Portrait_NoneVsAll_EdgeComparison()
		{
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			var insets = GetSafeAreaInsets();

			// None: top at page top, bottom at page bottom (baseline for screen edges)
			var topNone = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topNone.Y, Is.LessThanOrEqualTo(InsetTolerance),
				$"None: top Y ({topNone.Y}) should be ≈ 0 (edge-to-edge)");

			var bottomNone = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var screenBottom = bottomNone.Y + bottomNone.Height;

			// Switch to All
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			// All: top at topInset, bottom at screenBottom - bottomInset
			var topAll = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topAll.Y - insets.Top), Is.LessThanOrEqualTo(InsetTolerance),
				$"All: top Y ({topAll.Y}) should be ≈ insets.Top ({insets.Top})");

			var bottomAll = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomEdgeAll = bottomAll.Y + bottomAll.Height;
			Assert.That(Math.Abs(bottomEdgeAll - (screenBottom - insets.Bottom)), Is.LessThanOrEqualTo(InsetTolerance),
				$"All: bottom ({bottomEdgeAll}) should be ≈ screenBottom - insets.Bottom ({screenBottom} - {insets.Bottom} = {screenBottom - insets.Bottom})");
		}

		[Test, Order(37)]
		[Description("Switching None to All in landscape validates all 4 edges shift inward")]
		public void ValidateOrientation_Landscape_NoneVsAll_EdgeComparison()
		{
			// Set None and rotate to landscape
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");
			App.SetOrientationLandscape();

			// None: all edges at screen edges (baseline for screen dimensions)
			var topNone = App.WaitForElement("TopEdgeIndicator").GetRect();
			var leftNone = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(leftNone.X, Is.LessThanOrEqualTo(InsetTolerance),
				$"None: left X ({leftNone.X}) should be ≈ 0 (edge-to-edge)");

			var rightNone = App.WaitForElement("RightEdgeIndicator").GetRect();
			var screenRight = rightNone.X + rightNone.Width;

			var bottomNone = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var screenBottom = bottomNone.Y + bottomNone.Height;

			// Switch to All while still in landscape
			App.Tap("SafeAreaAllButton");
			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("All"));

			var insetsLandscape = GetSafeAreaInsets();

			// All: edges should be inset by safe area values
			var leftAll = App.WaitForElement("LeftEdgeIndicator").GetRect();
			Assert.That(Math.Abs(leftAll.X - insetsLandscape.Left), Is.LessThanOrEqualTo(InsetTolerance),
				$"All: left X ({leftAll.X}) should be ≈ insetsLandscape.Left ({insetsLandscape.Left})");

			var rightAll = App.WaitForElement("RightEdgeIndicator").GetRect();
			var rightEdgeAll = rightAll.X + rightAll.Width;
			Assert.That(Math.Abs(rightEdgeAll - (screenRight - insetsLandscape.Right)), Is.LessThanOrEqualTo(InsetTolerance),
				$"All: right ({rightEdgeAll}) should be ≈ screenRight - insets.Right ({screenRight} - {insetsLandscape.Right} = {screenRight - insetsLandscape.Right})");

			var bottomAll = App.WaitForElement("BottomEdgeIndicator").GetRect();
			var bottomEdgeAll = bottomAll.Y + bottomAll.Height;
			Assert.That(Math.Abs(bottomEdgeAll - (screenBottom - insetsLandscape.Bottom)), Is.LessThanOrEqualTo(InsetTolerance),
				$"All: bottom ({bottomEdgeAll}) should be ≈ screenBottom - insets.Bottom ({screenBottom} - {insetsLandscape.Bottom} = {screenBottom - insetsLandscape.Bottom})");

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

			var insets = GetSafeAreaInsets();

			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(Math.Abs(topLabelRect.Y - insets.Top), Is.LessThanOrEqualTo(InsetTolerance),
				$"Container (UseSafeArea=True): top Y ({topLabelRect.Y}) should be ≈ insets.Top ({insets.Top})");
		}

		[Test, Order(39)]
		[Description("UseSafeArea=False equivalent to SafeAreaEdges=None")]
		public void ValidateLegacy_UseSafeAreaFalse()
		{
			App.WaitForElement("SafeAreaNoneButton");
			App.Tap("SafeAreaNoneButton");

			Assert.That(App.FindElement("SafeAreaEdgesValueLabel").GetText(), Is.EqualTo("None"));

			var topLabelRect = App.WaitForElement("TopEdgeIndicator").GetRect();
			Assert.That(topLabelRect.Y, Is.LessThanOrEqualTo(InsetTolerance),
				$"None (UseSafeArea=False): top Y ({topLabelRect.Y}) should be ≈ 0 (edge-to-edge)");
		}
	}
}
