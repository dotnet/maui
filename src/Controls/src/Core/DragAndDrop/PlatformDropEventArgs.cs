using System;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Platform-specific arguments associated with the DropEventArgs.
/// </summary>
public class PlatformDropEventArgs
{
#if IOS || MACCATALYST
	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public UIKit.UIView? Sender { get; }

	/// <summary>
	/// Gets the interaction used for dropping items.
	/// </summary>
	public UIKit.UIDropInteraction DropInteraction { get; }

	/// <summary>
	/// Gets the associated information from the drop session.
	/// </summary>
	public UIKit.IUIDropSession DropSession { get; }

	internal PlatformDropEventArgs(UIKit.UIView? sender, UIKit.UIDropInteraction dropInteraction,
		UIKit.IUIDropSession dropSession)
	{
		Sender = sender;
		DropInteraction = dropInteraction;
		DropSession = dropSession;
	}

#elif ANDROID
	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public Android.Views.View Sender { get; }

	/// <summary>
	/// Gets the event containing information for drag and drop status.
	/// </summary>
	public Android.Views.DragEvent DragEvent { get; }

	internal PlatformDropEventArgs(Android.Views.View sender, Android.Views.DragEvent dragEvent)
	{
		Sender = sender;
		DragEvent = dragEvent;
	}

#elif WINDOWS
	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public Microsoft.UI.Xaml.UIElement? Sender { get; }

	/// <summary>
	/// Gets data for drag and drop events.
	/// </summary>
	public Microsoft.UI.Xaml.DragEventArgs DragEventArgs { get; }

	internal PlatformDropEventArgs(Microsoft.UI.Xaml.UIElement? sender,
		Microsoft.UI.Xaml.DragEventArgs dragEventArgs)
	{
		Sender = sender;
		DragEventArgs = dragEventArgs;
	}

#else
	internal PlatformDropEventArgs()
	{
	}
#endif
}
