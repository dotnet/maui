#if IOS
using PlatformView = UIKit.UIView;
#elif MACOS
using PlatformView = AppKit.NSView;
#elif ANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
#elif NETSTANDARD || !PLATFORM
using PlatformView = System.Object;
#endif

using System;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Base class for generalized user-defined behaviors that can respond to arbitrary conditions and events when connected to the platform view hierarchy.
/// </summary>
/// <typeparam name="TView">Virtual View</typeparam>
/// <typeparam name="TPlatformView">Platform View</typeparam>
public abstract partial class PlatformBehavior<TView, TPlatformView> : Behavior<TView>
	where TView : Element
	where TPlatformView : class
{
	TPlatformView? _platformView;

	/// <inheritdoc />
	protected sealed override void OnAttachedTo(BindableObject bindable)
	{
		base.OnAttachedTo(bindable);
	}

	/// <inheritdoc />
	protected sealed override void OnDetachingFrom(BindableObject bindable)
	{
		base.OnDetachingFrom(bindable);
	}

	/// <inheritdoc />
	protected sealed override void OnAttachedTo(TView bindable)
	{
		if (bindable is VisualElement ve)
		{
			ve.Loaded += OnLoaded;
			ve.Unloaded += OnUnloaded;
		}
		else
		{
			if (bindable.Handler != null)
				FireAttachedTo(bindable);

			bindable.HandlerChanged += OnHandlerChanged;
		}
	}

	/// <inheritdoc />
	protected sealed override void OnDetachingFrom(TView bindable)
	{
		if (bindable is VisualElement ve)
		{
			ve.Loaded -= OnLoaded;
			ve.Unloaded -= OnUnloaded;
		}
		else
		{
			bindable.HandlerChanged -= OnHandlerChanged;
		}

		FireDetachedFrom(bindable);
	}



	void FireAttachedTo(TView bindable)
	{
		if (bindable?.Handler?.PlatformView is TPlatformView platformView)
		{
			_platformView = platformView;
			OnAttachedTo(bindable, platformView);
		}
	}

	void FireDetachedFrom(TView bindable)
	{
		if (_platformView != null)
		{
			OnDetachedFrom(bindable, _platformView);
			_platformView = null;
		}
	}

	void OnUnloaded(object? sender, EventArgs e)
	{
		if (sender is TView view)
		{
			FireDetachedFrom(view);
		}
	}

	void OnLoaded(object? sender, EventArgs e)
	{
		if (sender is TView view && view.Handler.PlatformView is TPlatformView platformView)
		{
			OnAttachedTo(view, platformView);
			_platformView = platformView;
		}
	}

	void OnHandlerChanged(object? sender, EventArgs e)
	{
		if (sender is not TView visualElement)
			return;

		if (visualElement.Handler is not null)
			FireAttachedTo(visualElement);
		else
			FireDetachedFrom(visualElement);
	}

	/// <summary>
	/// This method is called when the bindable is attached to the platform view hierarchy. 
	/// </summary>
	/// <param name="bindable">The bindable object to which the behavior was attached.</param>
	/// <param name="platformView">The platform control connected to the bindable object.</param>
	protected virtual void OnAttachedTo(TView bindable, TPlatformView platformView) { }

	/// <summary>
	/// This method is called when the bindable is detached from the platform view hierarchy. 
	/// </summary>
	/// <param name="bindable">The bindable object to which the behavior was attached.</param>
	/// <param name="platformView">The platform control connected to the bindable object.</param>
	protected virtual void OnDetachedFrom(TView bindable, TPlatformView platformView) { }
}

/// <summary>
/// Base class for generalized user-defined behaviors that can respond to arbitrary conditions and events when connected to the platform view hierarchy.
/// </summary>
/// <typeparam name="TView">Virtual View</typeparam>
public abstract partial class PlatformBehavior<TView> : PlatformBehavior<TView, PlatformView>
	where TView : VisualElement
{

}
