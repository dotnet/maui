using System;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Platform-specific arguments associated with the DragStartingEventArgs.
/// </summary>
public class PlatformDragStartingEventArgs
{
#if IOS || MACCATALYST
	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public UIKit.UIView? Sender { get; }

	/// <summary>
	/// Gets the interaction used for dragging items.
	/// </summary>
	public UIKit.UIDragInteraction DragInteraction { get; }

	/// <summary>
	/// Gets the associated information from the drag session.
	/// </summary>
	public UIKit.IUIDragSession DragSession { get; }

	internal Foundation.NSItemProvider? ItemProvider { get; private set; }
	internal Func<UIKit.UIDragPreview?>? PreviewProvider { get; private set; }
	internal UIKit.UIDragItem[]? DragItems { get; private set; }

	internal PlatformDragStartingEventArgs(UIKit.UIView? sender, UIKit.UIDragInteraction dragInteraction,
		UIKit.IUIDragSession dragSession)
	{
		Sender = sender;
		DragInteraction = dragInteraction;
		DragSession = dragSession;
	}

	/// <summary>
	/// Sets the item provider when dragging begins.
	/// </summary>
	/// <param name="itemProvider">The custom item provider to use.</param>
	/// <remarks>
	/// This itemProvider will be applied to the MAUI generated dragItem.
	/// </remarks>
	public void SetItemProvider (Foundation.NSItemProvider itemProvider)
	{
		ItemProvider = itemProvider;
	}

	/// <summary>
	/// Sets the preview provider when dragging begins.
	/// </summary>
	/// <param name="previewProvider">The custom preview provider to use.</param>
	/// <remarks>
	/// This previewProvider will be applied to the MAUI generated dragItem.
	/// </remarks>
	public void SetPreviewProvider(Func<UIKit.UIDragPreview?> previewProvider)
	{
		PreviewProvider = previewProvider;
	}

	/// <summary>
	/// Sets the drag items when dragging begins.
	/// </summary>
	/// <param name="dragItems">The custom drag items to use.</param>
	/// <exception cref="NotSupportedException">UIDragItem.LocalObject is set</exception>
	/// <remarks>
	/// These dragItems will be used in place of the MAUI generated dragItem with an exception to the LocalObject.
	/// Do not set LocalObject as it will be overwritten internally.
	/// </remarks>
	public void SetDragItems(UIKit.UIDragItem[] dragItems)
	{
		foreach (var item in dragItems)
		{
			if (item.LocalObject is not null)
				throw new NotSupportedException("Do not assign the UIDragItem.LocalObject as this property will be overwritten internally.");
		}
		DragItems = dragItems;
	}

#elif ANDROID
	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public Android.Views.View Sender { get; }

	/// <summary>
	/// Gets the event containing information for drag and drop status.
	/// </summary>
	public Android.Views.MotionEvent MotionEvent { get; }

	internal Android.Views.View.DragShadowBuilder? DragShadowBuilder { get; private set; }
	internal Android.Content.ClipData? ClipData { get; private set; }

	internal PlatformDragStartingEventArgs(Android.Views.View sender, Android.Views.MotionEvent motionEvent)
	{
		Sender = sender;
		MotionEvent = motionEvent;
	}

	/// <summary>
	/// Sets the drag shadow when dragging begins.
	/// </summary>
	/// <param name="dragShadowBuilder">The custom drag shadow builder to use.</param>
	public void SetDragShadowBuilder(Android.Views.View.DragShadowBuilder dragShadowBuilder)
	{
		DragShadowBuilder = dragShadowBuilder;
	}

	/// <summary>
	/// Sets the clip data when dragging begins.
	/// </summary>
	/// <param name="clipData">The custom clip data to use.</param>
	public void SetClipData(Android.Content.ClipData clipData)
	{
		ClipData = clipData;
	}

#elif WINDOWS
	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public Microsoft.UI.Xaml.UIElement Sender { get; }

	/// <summary>
	/// Gets data for the DragStarting event.
	/// </summary>
	public Microsoft.UI.Xaml.DragStartingEventArgs DragStartingEventArgs { get; }

	/// <summary>
	/// Gets or sets a value that indicates whether the DragStartingEventArgs are changed.
	/// </summary>
	/// <remarks>
	/// Set this property's value to true when changing the DragStartingEventArgs so the system does not override the changes.
	/// </remarks>
	public bool Handled { get; set; }

	internal PlatformDragStartingEventArgs(Microsoft.UI.Xaml.UIElement sender,
		Microsoft.UI.Xaml.DragStartingEventArgs dragStartingEventArgs)
	{
		Sender = sender;
		DragStartingEventArgs = dragStartingEventArgs;
	}

#else
	internal PlatformDragStartingEventArgs()
	{
	}
#endif
}
