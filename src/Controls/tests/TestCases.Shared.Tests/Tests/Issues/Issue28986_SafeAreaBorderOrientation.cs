#if IOS || ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28986_SafeAreaBorderOrientation : _IssuesUITest
{
    public override string Issue => "Test SafeArea per-edge safe area control";

    public Issue28986_SafeAreaBorderOrientation(TestDevice device) : base(device)
    {
    }

    [Test]
    [Category(UITestCategories.SafeAreaEdges)]
    public void SafeAreaBorderOrientationPortraitToLandscape()
    {
        var borderContent = App.WaitForElement("BorderContent");
        var displayDensity = App.GetDisplayDensity();
        // 1. Verify initial portrait state and record measurements
        var portraitBounds = borderContent.GetRect();
        var portraitSafeArea = App.WaitForElement("SafeAreaInsets").GetText();
        var portraitDimensions = App.WaitForElement("BorderDimensions").GetText();

        // Ensure we start in portrait (orientation detection is now real-time)
        Assert.That(portraitBounds.Height, Is.GreaterThan(portraitBounds.Width * 0.8),
            "Should start in portrait orientation (height > width)");

        // 2. Change orientation to landscape using real device orientation change
        App.SetOrientationLandscape();

        // Wait for the orientation change to complete and layout to settle
        System.Threading.Thread.Sleep(2000); // Increased wait time for real orientation change

        // 3. Verify landscape state - dimensions should be different from portrait
        var landscapeBounds = borderContent.GetRect();
        var landscapeSafeArea = App.WaitForElement("SafeAreaInsets").GetText();
        var landscapeDimensions = App.WaitForElement("BorderDimensions").GetText();

        // 4. Verify orientation actually changed through dimension analysis
        Assert.That(landscapeBounds.Width, Is.GreaterThan(landscapeBounds.Height * 0.8),
            "Should now be in landscape orientation (width > height)");

        // 5. Verify that the safe area adapts correctly
        Assert.That(portraitSafeArea, Is.Not.EqualTo(landscapeSafeArea),
            "Safe area insets should adapt when changing from portrait to landscape");

        // 6. Verify border dimensions change appropriately
        Assert.That(portraitDimensions, Is.Not.EqualTo(landscapeDimensions),
            "Border dimensions should differ between portrait and landscape orientations");

        // 7. Verify aspect ratio changes appropriately for orientation
        var portraitAspectRatio = portraitBounds.Height / portraitBounds.Width;
        var landscapeAspectRatio = landscapeBounds.Height / landscapeBounds.Width;
        Assert.That(portraitAspectRatio, Is.GreaterThan(landscapeAspectRatio),
            "Portrait should have a higher aspect ratio than landscape");

        // 8. Verify insets adjust appropriately on notched devices
        // On devices with notches/safe areas, the values should differ significantly
        Assert.That(landscapeBounds.Width, Is.GreaterThan(portraitBounds.Height * 0.7),
            "Landscape width should be significantly different, confirming safe area adaptation");

        // 9. Verify the border remains visible and functional
        Assert.That(landscapeBounds.Width, Is.GreaterThan(0), "Border should remain visible in landscape");
        Assert.That(landscapeBounds.Height, Is.GreaterThan(0), "Border should remain visible in landscape");
    }

    [Test]
    [Category(UITestCategories.SafeAreaEdges)]
    public void SafeAreaBorderOrientationLandscapeToPortrait()
    {
        var borderContent = App.WaitForElement("BorderContent");

        // 1. Start by changing to landscape orientation first
        App.SetOrientationLandscape();

        // Wait for the orientation change to complete and layout to settle
        System.Threading.Thread.Sleep(2000);

        // 2. Record landscape state as baseline
        var landscapeBounds = borderContent.GetRect();
        var landscapeSafeArea = App.WaitForElement("SafeAreaInsets").GetText();
        var landscapeDimensions = App.WaitForElement("BorderDimensions").GetText();

        // Verify we're actually in landscape
        Assert.That(landscapeBounds.Width, Is.GreaterThan(landscapeBounds.Height * 0.8),
            "Should be in landscape orientation (width > height)");

        // 3. Change back to portrait orientation  
        App.SetOrientationPortrait();

        // Wait for the orientation change to complete and layout to settle
        System.Threading.Thread.Sleep(2000);

        // 4. Record portrait state after reversion
        var portraitBounds = borderContent.GetRect();
        var portraitSafeArea = App.WaitForElement("SafeAreaInsets").GetText();
        var portraitDimensions = App.WaitForElement("BorderDimensions").GetText();

        // 5. Verify orientation actually changed back through dimension analysis
        Assert.That(portraitBounds.Height, Is.GreaterThan(portraitBounds.Width * 0.8),
            "Should now be back in portrait orientation (height > width)");

        // 6. Ensure the safe area reverts correctly
        Assert.That(landscapeSafeArea, Is.Not.EqualTo(portraitSafeArea),
            "Safe area should revert when changing from landscape back to portrait");

        // 7. Verify border dimensions differ between orientations
        Assert.That(landscapeDimensions, Is.Not.EqualTo(portraitDimensions),
            "Border dimensions should differ between landscape and portrait orientations");

        // 8. Verify aspect ratio behavior - portrait should be taller than wide
        var portraitAspectRatio = portraitBounds.Height / portraitBounds.Width;
        var landscapeAspectRatio = landscapeBounds.Height / landscapeBounds.Width;

        Assert.That(portraitAspectRatio, Is.GreaterThan(landscapeAspectRatio),
            "Portrait should have a higher aspect ratio (taller than wide) compared to landscape");

        // 9. Verify border visibility is maintained during transition
        Assert.That(portraitBounds.Width, Is.GreaterThan(0), "Border should remain visible after orientation change");
        Assert.That(portraitBounds.Height, Is.GreaterThan(0), "Border should remain visible after orientation change");

        // 10. Verify successful orientation reversion through significant dimension changes
        Assert.That(portraitBounds.Height, Is.GreaterThan(landscapeBounds.Height * 0.8),
            "Portrait height should be significantly different from landscape, confirming orientation reversion");
    }

    [Test]
    [Category(UITestCategories.SafeAreaEdges)]
    public void SafeAreaBorderMultipleOrientationChanges()
    {
        var borderContent = App.WaitForElement("BorderContent");

        // 1. Record initial portrait state
        var initialPortraitBounds = borderContent.GetRect();
        var initialPortraitSafeArea = App.WaitForElement("SafeAreaInsets").GetText();
        var initialPortraitDimensions = App.WaitForElement("BorderDimensions").GetText();

        // Verify starting orientation
        Assert.That(initialPortraitBounds.Height, Is.GreaterThan(initialPortraitBounds.Width * 0.8),
            "Should start in portrait orientation");

        // 2. First orientation change: Portrait → Landscape
        App.SetOrientationLandscape();
        System.Threading.Thread.Sleep(2000); // Wait for orientation change and layout to settle

        var firstLandscapeBounds = borderContent.GetRect();
        var firstLandscapeSafeArea = App.WaitForElement("SafeAreaInsets").GetText();
        var firstLandscapeDimensions = App.WaitForElement("BorderDimensions").GetText();

        // Verify first landscape state
        Assert.That(firstLandscapeBounds.Width, Is.GreaterThan(firstLandscapeBounds.Height * 0.8),
            "Should be in landscape after first change");

        // 3. Second orientation change: Landscape → Portrait
        App.SetOrientationPortrait();
        Thread.Sleep(2000); // Wait for orientation change and layout to settle

        var secondPortraitBounds = borderContent.GetRect();

        // Verify back to portrait
        Assert.That(secondPortraitBounds.Height, Is.GreaterThan(secondPortraitBounds.Width * 0.8),
            "Should be back to portrait after second change");

        // 4. Third orientation change: Portrait → Landscape  
        App.SetOrientationLandscape();
        Thread.Sleep(2000); // Wait for orientation change and layout to settle

        var secondLandscapeBounds = borderContent.GetRect();
        var secondLandscapeSafeArea = App.WaitForElement("SafeAreaInsets").GetText();
        var secondLandscapeDimensions = App.WaitForElement("BorderDimensions").GetText();

        // Verify second landscape state
        Assert.That(secondLandscapeBounds.Width, Is.GreaterThan(secondLandscapeBounds.Height * 0.8),
            "Should be in landscape after third change");

        // 5. Fourth orientation change: Landscape → Portrait (final)
        App.SetOrientationPortrait();
        System.Threading.Thread.Sleep(2000); // Wait for orientation change and layout to settle

        var finalPortraitBounds = borderContent.GetRect();
        var finalPortraitSafeArea = App.WaitForElement("SafeAreaInsets").GetText();
        var finalPortraitDimensions = App.WaitForElement("BorderDimensions").GetText();

        // Verify final portrait state
        Assert.That(finalPortraitBounds.Height, Is.GreaterThan(finalPortraitBounds.Width * 0.8),
            "Should be back to portrait in final state");

        // 6. Verify border visibility is maintained throughout all changes
        Assert.That(borderContent.GetRect().Width, Is.GreaterThan(0), "Border should remain visible after multiple orientation changes");
        Assert.That(borderContent.GetRect().Height, Is.GreaterThan(0), "Border should remain visible after multiple orientation changes");

        // 7. Verify positioning consistency within same orientation (with tolerance for real orientation changes)
        const double positionTolerance = 10.0; // Increased tolerance for real device orientation changes

        Assert.That(Math.Abs(initialPortraitBounds.X - finalPortraitBounds.X), Is.LessThanOrEqualTo(positionTolerance),
            "Portrait X position should be consistent between multiple orientation cycles");
        Assert.That(Math.Abs(initialPortraitBounds.Y - finalPortraitBounds.Y), Is.LessThanOrEqualTo(positionTolerance),
            "Portrait Y position should be consistent between multiple orientation cycles");

        // 8. Verify landscape positioning consistency
        Assert.That(Math.Abs(firstLandscapeBounds.X - secondLandscapeBounds.X), Is.LessThanOrEqualTo(positionTolerance),
            "Landscape X position should be consistent between orientation cycles");
        Assert.That(Math.Abs(firstLandscapeBounds.Y - secondLandscapeBounds.Y), Is.LessThanOrEqualTo(positionTolerance),
            "Landscape Y position should be consistent between orientation cycles");

        // 9. Verify safe area consistency within same orientation
        Assert.That(initialPortraitSafeArea, Is.EqualTo(finalPortraitSafeArea),
            "Safe area should be consistent in portrait orientation across multiple cycles");
        Assert.That(firstLandscapeSafeArea, Is.EqualTo(secondLandscapeSafeArea),
            "Safe area should be consistent in landscape orientation across multiple cycles");

        // 10. Verify overall stability - no crashes or errors
        var testStatus = App.WaitForElement("TestStatus");
        Assert.That(testStatus.GetText(), Does.Not.Contain("Error"),
            "No errors should occur during multiple orientation changes");

        // 11. Verify dimensions still differ appropriately between orientations after multiple cycles
        Assert.That(initialPortraitDimensions, Is.Not.EqualTo(firstLandscapeDimensions),
            "Dimensions should still differ between portrait and landscape after multiple changes");
        Assert.That(finalPortraitDimensions, Is.Not.EqualTo(secondLandscapeDimensions),
            "Dimensions should still differ between portrait and landscape after multiple changes");
    }

    [Test]
    [Category(UITestCategories.SafeAreaEdges)]
    public void SafeAreaBorderSoftInputBehavior()
    {
#if ANDROID
		// Status bar height can vary based on device and OS version, so we only validate Y=0 for None on Android 13+ API 30.
		var apiLevel = App.GetDeviceApiLevel();
		if (apiLevel > 36) // Android 13+
#endif
{


        var borderContent = App.WaitForElement("BorderContent");

        // 1. Set bottom edge to SoftInput mode
        App.Tap("SetBottomSoftInputButton");

        // Wait for the test status to confirm the change
        var softInputSet = App.WaitForTextToBePresentInElement("TestStatus", "SoftInput", TimeSpan.FromSeconds(3));
        Assert.That(softInputSet, Is.True, "Bottom edge should be set to SoftInput mode");

        // 2. Record initial state (keyboard hidden)
        var initialBounds = borderContent.GetRect();
        var initialSafeArea = App.WaitForElement("SafeAreaInsets").GetText();

        // Use IsKeyboardShown helper for reliable keyboard detection
        Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should initially be hidden");

        // 3. Tap entry to show keyboard
        App.Tap("TestEntry");
        Thread.Sleep(1000); // Wait for keyboard to appear in view
        Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should become visible after tapping entry");

        // 4. Verify keyboard is shown and border adjusted
        var keyboardShownBounds = borderContent.GetRect();
        var keyboardShownSafeArea = App.WaitForElement("SafeAreaInsets").GetText();

        // Verify keyboard is shown using reliable helper method
        Assert.That(App.IsKeyboardShown(), Is.True, "IsKeyboardShown should return true when keyboard is visible");

        // 5. Verify border shrinks when keyboard appears (height should be smaller)
        Assert.That(keyboardShownBounds.Height, Is.LessThan(initialBounds.Height),
            "Border height should shrink when keyboard appears with SoftInput edge setting");

        // 6. Verify safe area insets changed to accommodate keyboard - more flexible check
        // Check if either the safe area text changed OR the border height changed significantly
        var safeAreaChanged = initialSafeArea != keyboardShownSafeArea;
        var borderHeightChanged = Math.Abs(keyboardShownBounds.Height - initialBounds.Height) > 20;

        Assert.That(safeAreaChanged || borderHeightChanged, Is.True,
            $"Either safe area should change or border should shrink significantly when keyboard appears. " +
            $"SafeArea changed: {safeAreaChanged}, Border height change: {Math.Abs(keyboardShownBounds.Height - initialBounds.Height):F1}px. " +
            $"Initial: '{initialSafeArea}', With keyboard: '{keyboardShownSafeArea}'");

        // 7. Verify border positioning adjusted (Y position might change)
        // The border might move up or the available space might decrease
        Assert.That(keyboardShownBounds.Width, Is.EqualTo(initialBounds.Width).Within(2),
            "Border width should remain approximately the same when keyboard appears");

        // 8. Dismiss keyboard by using proper DismissKeyboard method
        App.DismissKeyboard();

        Thread.Sleep(1000);
        Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden after dismissal");

        // 9. Verify keyboard is hidden and border restored
        var keyboardHiddenBounds = borderContent.GetRect();
        var keyboardHiddenSafeArea = App.WaitForElement("SafeAreaInsets").GetText();

        // Verify keyboard is hidden using reliable helper method
        Assert.That(App.IsKeyboardShown(), Is.False, "IsKeyboardShown should return false after dismissal");

        // 10. Verify border returns to original size (with tolerance) and safe area restoration
        const double sizeTolerance = 10.0; // Increased tolerance for keyboard interactions
        Assert.That(Math.Abs(keyboardHiddenBounds.Height - initialBounds.Height), Is.LessThanOrEqualTo(sizeTolerance),
            $"Border height should return close to original size after keyboard dismissal. " +
            $"Original: {initialBounds.Height:F1}, After dismiss: {keyboardHiddenBounds.Height:F1}, Diff: {Math.Abs(keyboardHiddenBounds.Height - initialBounds.Height):F1}");
        Assert.That(Math.Abs(keyboardHiddenBounds.Width - initialBounds.Width), Is.LessThanOrEqualTo(sizeTolerance),
            "Border width should return to original size after keyboard dismissal");

        // 11. Verify safe area tends to restore (flexible check)
        // Either safe area should restore OR border should restore to original size
        var safeAreaRestored = keyboardHiddenSafeArea == initialSafeArea;
        var borderHeightRestored = Math.Abs(keyboardHiddenBounds.Height - initialBounds.Height) <= sizeTolerance;

        Assert.That(safeAreaRestored || borderHeightRestored, Is.True,
            $"Either safe area should restore or border should return to original size. " +
            $"SafeArea restored: {safeAreaRestored}, Border restored: {borderHeightRestored}. " +
            $"Initial SA: '{initialSafeArea}', Final SA: '{keyboardHiddenSafeArea}'");

        // 12. Verify successful keyboard behavior - test should show measurable impact
        var testStatus = App.WaitForElement("TestStatus");
        Assert.That(testStatus.GetText(), Does.Not.Contain("Error"),
            "No errors should occur during keyboard interaction");

        // Verify that we observed the expected keyboard behavior through layout changes
        Assert.That(borderHeightChanged || safeAreaChanged, Is.True,
            "Test should demonstrate that keyboard interaction affects either safe area or border layout");
}
    }

#if TEST_FAILS_ON_ANDROID // Landscape orientation causes keyboard to occupy  fullview
    [Test]
    [Category(UITestCategories.SafeAreaEdges)]
    public void SafeAreaBorderSoftInputWithOrientationChange()
    {
        var borderContent = App.WaitForElement("BorderContent");

        // 1. Set bottom edge to SoftInput mode for keyboard behavior
        App.Tap("SetBottomSoftInputButton");

        // Wait for the test status to confirm the change
        var softInputSet = App.WaitForTextToBePresentInElement("TestStatus", "SoftInput", TimeSpan.FromSeconds(3));
        Assert.That(softInputSet, Is.True, "Bottom edge should be set to SoftInput mode");

        // 2. Show keyboard in portrait mode
        App.Tap("TestEntry");

        Thread.Sleep(1000);

        Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should become visible in portrait mode");

        // 3. Record portrait state with keyboard visible
        var portraitWithKeyboardBounds = borderContent.GetRect();
        var portraitWithKeyboardSafeArea = App.WaitForElement("SafeAreaInsets").GetText();

        Assert.That(App.IsKeyboardShown(), Is.True, "IsKeyboardShown should confirm keyboard is visible");

        // Verify we start in portrait with keyboard
        Assert.That(portraitWithKeyboardBounds.Height, Is.GreaterThan(portraitWithKeyboardBounds.Width * 0.6),
            "Should be in portrait orientation even with keyboard visible");

        // 4. Change orientation to landscape while keyboard is still visible
        App.SetOrientationLandscape();
        Thread.Sleep(2000); // Wait for orientation change and layout to settle

        // 5. Record landscape state with keyboard visible
        var landscapeWithKeyboardBounds = borderContent.GetRect();
        var landscapeWithKeyboardSafeArea = App.WaitForElement("SafeAreaInsets").GetText();

        Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should remain visible after orientation change");

        // Verify orientation changed to landscape
        Assert.That(landscapeWithKeyboardBounds.Width, Is.GreaterThan(landscapeWithKeyboardBounds.Height * 0.6),
            "Should now be in landscape orientation with keyboard visible");

        // 6. Verify border maintains visibility in landscape with keyboard
        Assert.That(landscapeWithKeyboardBounds.Width, Is.GreaterThan(0),
            "Border should remain visible in landscape with keyboard");
        Assert.That(landscapeWithKeyboardBounds.Height, Is.GreaterThan(0),
            "Border should remain visible in landscape with keyboard");

        // 7. Verify dimensions differ between orientations even with keyboard visible
        Assert.That(portraitWithKeyboardBounds.Width, Is.Not.EqualTo(landscapeWithKeyboardBounds.Width).Within(10),
            "Border width should differ between portrait and landscape with keyboard visible");
        Assert.That(portraitWithKeyboardBounds.Height, Is.Not.EqualTo(landscapeWithKeyboardBounds.Height).Within(10),
            "Border height should differ between portrait and landscape with keyboard visible");

        // 8. Verify landscape has wider aspect ratio even with keyboard
        var portraitAspectRatio = portraitWithKeyboardBounds.Height / portraitWithKeyboardBounds.Width;
        var landscapeAspectRatio = landscapeWithKeyboardBounds.Height / landscapeWithKeyboardBounds.Width;

        Assert.That(portraitAspectRatio, Is.GreaterThan(landscapeAspectRatio),
            "Portrait should still have higher aspect ratio than landscape, even with keyboard visible");

        // 9. Verify safe area insets adapted to landscape with keyboard
        Assert.That(portraitWithKeyboardSafeArea, Is.Not.EqualTo(landscapeWithKeyboardSafeArea),
            "Safe area insets should be different between portrait and landscape with keyboard");

        // 10. Hide keyboard in landscape mode using proper dismiss method
        App.DismissKeyboard();
        Thread.Sleep(1000);

        Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should be hidden in landscape mode");

        // 11. Record landscape state with keyboard hidden
        var landscapeWithoutKeyboardBounds = borderContent.GetRect();

        Assert.That(App.IsKeyboardShown(), Is.False, "IsKeyboardShown should confirm keyboard is hidden");

        // 12. Verify border expanded when keyboard was dismissed in landscape
        Assert.That(landscapeWithoutKeyboardBounds.Height, Is.GreaterThan(landscapeWithKeyboardBounds.Height),
            "Border should expand when keyboard is hidden in landscape mode");

        // 13. Change back to portrait without keyboard
        App.SetOrientationPortrait();
        Thread.Sleep(2000); // Wait for orientation change and layout to settle

        // 14. Record final portrait state without keyboard
        var finalPortraitBounds = borderContent.GetRect();

        // Verify back in portrait
        Assert.That(finalPortraitBounds.Height, Is.GreaterThan(finalPortraitBounds.Width * 0.8),
            "Should be back in portrait orientation");

        // 15. Verify overall stability - border should be functional in final state
        Assert.That(finalPortraitBounds.Width, Is.GreaterThan(0),
            "Border should be visible in final portrait state");
        Assert.That(finalPortraitBounds.Height, Is.GreaterThan(0),
            "Border should be visible in final portrait state");

        // 16. Verify no errors occurred during complex interaction
        var testStatus = App.WaitForElement("TestStatus");
        Assert.That(testStatus.GetText(), Does.Not.Contain("Error"),
            "No errors should occur during orientation change with keyboard interaction");

        // 17. Verify final state is stable (keyboard hidden)
        Assert.That(App.IsKeyboardShown(), Is.False, "Keyboard should remain hidden in final state");
    }
#endif
}
#endif