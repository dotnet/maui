using System;
namespace Microsoft.Maui.Controls;

/// <summary>
/// Platform-specific arguments associated with the PointerEventArgs
/// </summary>
public class PlatformPointerEventArgs
{
#if IOS || MACCATALYST
	/// <summary>
	/// Gets the <see cref="UIKit.UIView"/> that has the attached <see cref="UIKit.UIGestureRecognizer"/>.
	/// </summary>
	public UIKit.UIView Sender { get; }

	/// <summary>
	/// Gets the <see cref="UIKit.UIGestureRecognizer"/> attached to the <see cref="UIKit.UIView"/>.
	/// </summary>
	public UIKit.UIGestureRecognizer GestureRecognizer { get; }

	internal PlatformPointerEventArgs(UIKit.UIView sender, UIKit.UIGestureRecognizer gestureRecognizer)
	{
		Sender = sender;
		GestureRecognizer = gestureRecognizer;
	}

#elif ANDROID
	/// <summary>
	/// Gets the <see cref="Android.Views.View"/> that has the attached <see cref="Android.Views.MotionEvent"/>.
	/// </summary>
	public Android.Views.View Sender { get; }

	/// <summary>
	/// Gets the <see cref="Android.Views.MotionEvent"/> attached to the <see cref="Android.Views.View"/>.
	/// </summary>
	public Android.Views.MotionEvent MotionEvent { get; }

	internal PlatformPointerEventArgs(Android.Views.View sender, Android.Views.MotionEvent motionEvent)
	{
		Sender = sender;
		MotionEvent = motionEvent;
	}

#elif WINDOWS
	/// <summary>
	/// Gets the <see cref="Microsoft.UI.Xaml.FrameworkElement"/> that has the attached <see cref="Microsoft.UI.Xaml.RoutedEventArgs"/>.
	/// </summary>
	public Microsoft.UI.Xaml.FrameworkElement Sender { get; }

	/// <summary>
	/// Gets the <see cref="Microsoft.UI.Xaml.RoutedEventArgs"/> attached to the <see cref="Microsoft.UI.Xaml.FrameworkElement"/>.
	/// </summary>
	public Microsoft.UI.Xaml.RoutedEventArgs RoutedEventArgs { get; }

	internal PlatformPointerEventArgs(Microsoft.UI.Xaml.FrameworkElement sender, Microsoft.UI.Xaml.RoutedEventArgs routedEventArgs)
	{
		Sender = sender;
		RoutedEventArgs = routedEventArgs;
	}

#else
	internal PlatformPointerEventArgs()
	{
	}
#endif
}
