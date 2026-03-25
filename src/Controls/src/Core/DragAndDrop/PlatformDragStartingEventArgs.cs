using System;

#if ANDROID
using Android.Content;
using Android.Views;
using DragShadowBuilder = Android.Views.View.DragShadowBuilder;
using AView = Android.Views.View;
#endif

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
	internal Func<UIKit.UIDragInteraction, UIKit.IUIDragSession, bool>? PrefersFullSizePreviews { get; private set; }

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
	/// <remarks>
	/// These dragItems will be used in place of the MAUI generated dragItem.
	/// </remarks>
	public void SetDragItems(UIKit.UIDragItem[] dragItems)
	{
		DragItems = dragItems;
	}

	/// <summary>
	/// Sets the func that requests to keep drag previews full-sized when dragging begins.
	/// </summary>
	/// <param name="prefersFullSizePreviews">Func that returns whether to request full size previews.</param>
	/// <remarks>
	/// The default behavior on iOS is to reduce the size of the drag shadow if not requested here.
	/// Even if requested, it is up to the system whether or not to fulfill the request.
	/// This method exists inside <see cref="PlatformDragStartingEventArgs"/> since the preview must
	/// have this value set when dragging begins.
	/// </remarks>
	public void SetPrefersFullSizePreviews(Func<UIKit.UIDragInteraction, UIKit.IUIDragSession, bool>? prefersFullSizePreviews)
	{
		PrefersFullSizePreviews = prefersFullSizePreviews;
	}

#elif ANDROID
	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public AView Sender { get; }

	/// <summary>
	/// Gets the event containing information for drag and drop status.
	/// </summary>
	public MotionEvent MotionEvent { get; }

	internal DragShadowBuilder? DragShadowBuilder { get; private set; }
	internal ClipData? ClipData { get; private set; }
	internal Java.Lang.Object? LocalData { get; private set; }
	internal DragFlags? DragFlags { get; private set; }

	internal PlatformDragStartingEventArgs(AView sender, MotionEvent motionEvent)
	{
		Sender = sender;
		MotionEvent = motionEvent;
	}

	/// <summary>
	/// Sets the drag shadow when dragging begins.
	/// </summary>
	/// <param name="dragShadowBuilder">The custom drag shadow builder to use.</param>
	public void SetDragShadowBuilder(DragShadowBuilder dragShadowBuilder)
	{
		DragShadowBuilder = dragShadowBuilder;
	}

	/// <summary>
	/// Sets the clip data when dragging begins.
	/// </summary>
	/// <param name="clipData">The custom clip data to use.</param>
	public void SetClipData(ClipData clipData)
	{
		ClipData = clipData;
	}

	/// <summary>
	/// Sets the local data when dragging begins.
	/// </summary>
	/// <param name="localData">The custom local data to use.</param>
	public void SetLocalData(Java.Lang.Object localData)
	{
		LocalData = localData;
	}

	/// <summary>
	/// Sets the drag flags when dragging begins.
	/// </summary>
	/// <param name="dragFlags">The custom drag flags to use.</param>
	public void SetDragFlags(DragFlags dragFlags)
	{
		DragFlags = dragFlags;
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
