using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform;

/// <summary>
/// Callback interface for <see cref="FlyoutContainerManager"/> to communicate
/// state changes back to the handler layer without referencing Controls types.
/// </summary>
internal interface IFlyoutContainerDelegate
{
    /// <summary>
    /// Called when a user gesture (pan or tap-to-close) changes the presented state.
    /// The handler should write this back to the virtual view's IsPresented property.
    /// </summary>
    void OnPresentedChangedByGesture(bool isPresented);

    /// <summary>
    /// Called after layout completes, providing the computed bounds of each pane.
    /// The handler should write these back to the virtual view for measure/arrange.
    /// </summary>
    void OnLayoutBoundsChanged(Rect flyoutBounds, Rect detailBounds);

    /// <summary>
    /// Called when the detail content changes or split mode toggles,
    /// indicating the hamburger bar button item needs updating.
    /// </summary>
    void OnLeftBarButtonNeedsUpdate();

    /// <summary>
    /// Called when the container VC's view has appeared (ViewDidAppear).
    /// Currently unused — the framework handles Appearing automatically.
    /// Kept for future-proofing if manual lifecycle notification is needed.
    /// </summary>
    void OnViewDidAppear();

    /// <summary>
    /// Called when the container VC's view is about to disappear (ViewWillDisappear).
    /// Currently unused — the framework handles Disappearing automatically.
    /// Kept for future-proofing if manual lifecycle notification is needed.
    /// </summary>
    void OnViewWillDisappear();

    /// <summary>
    /// Returns the current IsPresented value from the virtual view.
    /// Used during initial layout to read the developer's intended value.
    /// </summary>
    bool GetCurrentIsPresented();

    /// <summary>
    /// Returns whether the virtual view has opted out of safe area insets
    /// (<see cref="ISafeAreaView.IgnoreSafeArea"/>). Used during layout to decide
    /// whether the flyout/detail container frame should be inset from the safe area.
    /// </summary>
    bool GetIgnoreSafeArea();
}
