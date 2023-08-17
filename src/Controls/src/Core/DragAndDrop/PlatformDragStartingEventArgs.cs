using System;

namespace Microsoft.Maui.Controls;

#pragma warning disable RS0016 // Add public types and members to the declared API
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

	internal Foundation.NSItemProvider? _itemProvider;
	internal Func<UIKit.UIDragPreview?>? _previewProvider;
	internal DataPackage? _dataPackage;

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
	public void SetItemProvider (Foundation.NSItemProvider itemProvider)
	{
		_itemProvider = itemProvider;
	}

	/// <summary>
	/// Sets the preview provider when dragging begins.
	/// </summary>
	/// <param name="previewProvider">The custom preview provider to use.</param>
	public void SetPreviewProvider(Func<UIKit.UIDragPreview?> previewProvider)
	{
		_previewProvider = previewProvider;
	}

	/// <summary>
	/// Sets the data package when dragging begins.
	/// </summary>
	/// <param name="dataPackage">The custom data package to use.</param>
	public void SetDataPackage(DataPackage dataPackage)
	{
		_dataPackage = dataPackage;
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

	internal DataPackage? _dataPackage;
	internal Android.Views.View.DragShadowBuilder? _dragShadowBuilder;
	internal Android.Content.ClipData? _clipData;

	internal PlatformDragStartingEventArgs(Android.Views.View sender, Android.Views.MotionEvent motionEvent)
	{
		Sender = sender;
		MotionEvent = motionEvent;
	}

	/// <summary>
	/// Sets the data package when dragging begins.
	/// </summary>
	/// <param name="dataPackage">The custom data package to use.</param>
	public void SetDataPackage(DataPackage dataPackage)
	{
		_dataPackage = dataPackage;
	}

	/// <summary>
	/// Sets the drag shadow when dragging begins.
	/// </summary>
	/// <param name="dragShadowBuilder">The custom drag shadow builder to use.</param>
	public void SetDragShadowBuilder(Android.Views.View.DragShadowBuilder dragShadowBuilder)
	{
		_dragShadowBuilder = dragShadowBuilder;
	}

	/// <summary>
	/// Sets the clip data when dragging begins.
	/// </summary>
	/// <param name="clipData">The custom clip data to use.</param>
	public void SetClipData(Android.Content.ClipData clipData)
	{
		_clipData = clipData;
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
