# FormSheet Modal Dismiss Attempt Event

This document provides an example of how to use the new `ModalDismissAttempted` event to handle when users attempt to dismiss a FormSheet modal on iOS.

## Overview

On iOS 13+, FormSheet modals can be dismissed by swiping down. When a developer wants to prevent this dismissal (e.g., to show a confirmation dialog or validate form data), they can set `isModalInPresentation = true` on the presenting view controller. When the user attempts to dismiss but is prevented, the `presentationControllerDidAttemptToDismiss` delegate method is called.

.NET MAUI now exposes this functionality through the `ModalDismissAttempted` event on both `Window` and `Application`.

## Example Usage

### Basic Example - Window Event

```csharp
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnOpenModalClicked(object sender, EventArgs e)
    {
        var modalPage = new ContentPage
        {
            Title = "Form",
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Entry { Placeholder = "Enter data..." },
                    new Button
                    {
                        Text = "Save and Close",
                        Command = new Command(async () => await Navigation.PopModalAsync())
                    }
                }
            }
        };

        // Set FormSheet presentation on iOS
        modalPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);

        // Subscribe to the dismiss attempt event
        if (Window != null)
        {
            Window.ModalDismissAttempted += OnModalDismissAttempted;
        }

        await Navigation.PushModalAsync(modalPage);

#if IOS
        // Prevent interactive dismissal so the event fires
        var handler = modalPage.Handler as Microsoft.Maui.Handlers.PageHandler;
        if (handler?.PlatformView is UIKit.UIViewController viewController)
        {
            if (viewController.PresentingViewController?.PresentedViewController is UIKit.UIViewController presentedVC)
            {
                presentedVC.ModalInPresentation = true;
            }
        }
#endif
    }

    private void OnModalDismissAttempted(object? sender, ModalDismissAttemptedEventArgs e)
    {
        // Show alert or handle the dismissal attempt
        DisplayAlert("Warning", "Please save your changes before closing", "OK");

        // Clean up event handler
        if (Window != null)
        {
            Window.ModalDismissAttempted -= OnModalDismissAttempted;
        }
    }
}
```

### Application-Level Event

```csharp
public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();

        // Subscribe at application level to handle all modals
        ModalDismissAttempted += OnModalDismissAttempted;
    }

    private void OnModalDismissAttempted(object? sender, ModalDismissAttemptedEventArgs e)
    {
        // Handle dismiss attempt for any modal in the app
        System.Diagnostics.Debug.WriteLine($"User attempted to dismiss modal: {e.Modal.Title}");
    }
}
```

## Event Arguments

The `ModalDismissAttemptedEventArgs` provides:
- `Modal` - The `Page` that the user attempted to dismiss

## Platform Availability

- **iOS 13+**: Fully supported
- **Android**: Not applicable (Android modals have different dismissal behavior)
- **Windows**: Not applicable
- **macOS/MacCatalyst**: Supported (same as iOS)

## Notes

1. The event fires only when the modal is prevented from being dismissed (e.g., via `isModalInPresentation = true`)
2. By default, iOS FormSheet modals are dismissible via swipe - you must explicitly prevent dismissal for the event to fire
3. The event does not automatically prevent the dismissal - you must prevent it via platform-specific code
4. For Shell navigation, the event still fires and can be used for custom handling

## Related APIs

- `Window.ModalDismissAttempted` event
- `Application.ModalDismissAttempted` event
- `ModalDismissAttemptedEventArgs` class
- `UIModalPresentationStyle.FormSheet` (iOS-specific)
