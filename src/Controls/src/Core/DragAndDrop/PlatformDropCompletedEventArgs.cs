using System;

#if ANDROID
using Android.Views;
using AView = Android.Views.View;
#endif

namespace Microsoft.Maui.Controls;

/// <summary>
/// Platform-specific arguments associated with the DropCompletedEventArgs
/// </summary>
public class PlatformDropCompletedEventArgs
{
#if IOS || MACCATALYST
	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public UIKit.UIView? Sender { get; }

	/// <summary>
	/// Gets the interaction used for dragging items.
	/// </summary>
	/// /// <remarks>
	/// This property is used when <see cref="PlatformDropCompletedEventArgs"/> is called from the SessionWillEnd method.
	/// </remarks>
	public UIKit.UIDragInteraction? DragInteraction { get; }

	/// <summary>
	/// Gets the associated information from the drag session.
	/// </summary>
	/// <remarks>
	/// This property is used when <see cref="PlatformDropCompletedEventArgs"/> is called from the SessionWillEnd method.
	/// </remarks>
	public UIKit.IUIDragSession? DragSession { get; }

	/// <summary>
	/// Gets the value representing the response to a drop.
	/// </summary>
	/// <remarks>
	/// This property is used when <see cref="PlatformDropCompletedEventArgs"/> is called from the SessionWillEnd method.
	/// </remarks>
	public UIKit.UIDropOperation? DropOperation { get; }

	/// <summary>
	/// Gets the interaction used for dropping items.
	/// </summary>
	/// /// <remarks>
	/// This property is used when <see cref="PlatformDropCompletedEventArgs"/> is called from the PerformDrop method.
	/// </remarks>
	public UIKit.UIDropInteraction? DropInteraction { get; }

	/// <summary>
	/// Gets the associated information from the drop session.
	/// </summary>
	/// <remarks>
	/// This property is used when <see cref="PlatformDropCompletedEventArgs"/> is called from the PerformDrop method.
	/// </remarks>
	public UIKit.IUIDropSession? DropSession { get; }

	internal PlatformDropCompletedEventArgs(UIKit.UIView? sender, UIKit.UIDragInteraction dragInteraction,
		UIKit.IUIDragSession dragSession, UIKit.UIDropOperation dropOperation)
	{
		Sender = sender;
		DragInteraction = dragInteraction;
		DragSession = dragSession;
		DropOperation = dropOperation;
	}

	internal PlatformDropCompletedEventArgs(UIKit.UIView? sender, UIKit.UIDropInteraction dropInteraction,
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
	public AView Sender { get; }

	/// <summary>
	/// Gets the event containing information for drag and drop status.
	/// </summary>
	public DragEvent DragEvent { get; }

	internal PlatformDropCompletedEventArgs(AView sender, DragEvent dragEvent)
	{
		Sender = sender;
		DragEvent = dragEvent;
	}

#elif WINDOWS
	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public Microsoft.UI.Xaml.UIElement Sender { get; }

	/// <summary>
	/// Gets data for the DropCompleted event.
	/// </summary>
	public Microsoft.UI.Xaml.DropCompletedEventArgs DropCompletedEventArgs { get; }

	internal PlatformDropCompletedEventArgs(Microsoft.UI.Xaml.UIElement sender,
		Microsoft.UI.Xaml.DropCompletedEventArgs dropCompletedEventArgs)
	{
		Sender = sender;
		DropCompletedEventArgs = dropCompletedEventArgs;
	}

#else
	internal PlatformDropCompletedEventArgs()
	{
	}
#endif
}
