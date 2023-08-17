using System;
namespace Microsoft.Maui.Controls;

#pragma warning disable RS0016 // Add public types and members to the declared API
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

	internal UIKit.UIDropProposal? _dropProposal;

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
		_dropProposal = dropProposal;
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

	internal PlatformDragEventArgs(Android.Views.View sender, Android.Views.DragEvent dragEvent)
	{
		Sender = sender;
		DragEvent = dragEvent;
	}

#elif WINDOWS
	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public object Sender { get; }

	/// <summary>
	/// Gets data for drag and drop events.
	/// </summary>
	public Microsoft.UI.Xaml.DragEventArgs DragEventArgs { get; }

	internal PlatformDragEventArgs(object sender,
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
