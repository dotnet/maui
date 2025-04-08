using System;
#if ANDROID
using Android.Views;
using AView = Android.Views.View;
#endif

namespace Microsoft.Maui.Controls;

/// <summary>
/// Platform-specific arguments associated with the <see cref="DragEventArgs"/>.
/// </summary>
public class PlatformDragEventArgs
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

	internal UIKit.UIDropProposal? DropProposal { get; private set; }

	internal PlatformDragEventArgs(UIKit.UIView? sender, UIKit.UIDropInteraction dropInteraction,
		UIKit.IUIDropSession dropSession)
	{
		Sender = sender;
		DropInteraction = dropInteraction;
		DropSession = dropSession;
	}

	/// <summary>
	/// Sets the drop proposal when dragging over a view.
	/// </summary>
	/// <param name="dropProposal">The custom drop proposal to use.</param>
	/// <remarks>
	/// <see cref="PlatformDragEventArgs"/> is used for DragOver and DragLeave events, but this method
	/// only has an effect with DragOver events.
	/// </remarks>
	public void SetDropProposal(UIKit.UIDropProposal dropProposal)
	{
		DropProposal = dropProposal;
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

	internal PlatformDragEventArgs(AView sender, DragEvent dragEvent)
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

	/// <summary>
	/// Gets or sets a value that indicates whether the DragEventArgs are changed.
	/// </summary>
	/// <remarks>
	/// Set the value of this property to true when changing the DragEventArgs so the system does not override the changes.
	/// </remarks>
	public bool Handled { get; set; }

	internal PlatformDragEventArgs(Microsoft.UI.Xaml.UIElement? sender,
		Microsoft.UI.Xaml.DragEventArgs dragEventArgs)
	{
		Sender = sender;
		DragEventArgs = dragEventArgs;
	}

#else
	internal PlatformDragEventArgs()
	{
	}
#endif
}
