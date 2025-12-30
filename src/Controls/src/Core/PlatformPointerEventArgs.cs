using System;

#if ANDROID
using Android.Views;
using AView = Android.Views.View;
#endif

namespace Microsoft.Maui.Controls;

/// <summary>
/// Platform-specific arguments associated with the PointerEventArgs.
/// </summary>
public class PlatformPointerEventArgs
{
#if IOS || MACCATALYST
	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public UIKit.UIView Sender { get; }

	/// <summary>
	/// Gets the native event or handler attached to the view.
	/// </summary>
	public UIKit.UIGestureRecognizer GestureRecognizer { get; }

	internal PlatformPointerEventArgs(UIKit.UIView sender, UIKit.UIGestureRecognizer gestureRecognizer)
	{
		Sender = sender;
		GestureRecognizer = gestureRecognizer;
	}

#elif ANDROID
	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public AView Sender { get; }

	/// <summary>
	/// Gets the native event or handler attached to the view.
	/// </summary>
	public MotionEvent MotionEvent { get; }

	internal PlatformPointerEventArgs(AView sender, MotionEvent motionEvent)
	{
		Sender = sender;
		MotionEvent = motionEvent;
	}

#elif WINDOWS
	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public Microsoft.UI.Xaml.FrameworkElement Sender { get; }

	/// <summary>
	/// Gets the native event or handler attached to the view.
	/// </summary>
	public Microsoft.UI.Xaml.Input.PointerRoutedEventArgs PointerRoutedEventArgs { get; }

	internal PlatformPointerEventArgs(Microsoft.UI.Xaml.FrameworkElement sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs pointerRoutedEventArgs)
	{
		Sender = sender;
		PointerRoutedEventArgs = pointerRoutedEventArgs;
	}

#else
	internal PlatformPointerEventArgs()
	{
	}
#endif
}
