#nullable enable

#if __IOS__
using PlatformView = UIKit.UIView;
#elif __MACOS__
using PlatformView = AppKit.NSView;
#elif __ANDROID__
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#endif

using System;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Base class for generalized user-defined behaviors that can respond to arbitrary conditions and events that has influence in the Platform specif layer.
/// </summary>
/// <typeparam name="TView">Virtual View</typeparam>
/// <typeparam name="TPlatformView">Platform View</typeparam>
public abstract partial class BasePlatformBehavior<TView, TPlatformView> : Behavior<TView>
	where TView : VisualElement
#if __IOS__ || __ANDROID__ || __WINDOWS
		where TPlatformView : PlatformView
#else
		where TPlatformView : class
#endif
{
	protected TPlatformView? PlatformView => View?.Handler?.PlatformView as TPlatformView;
	protected TView? View { get; private set; }

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
		if (bindable.Handler is null)
		{
			bindable.HandlerChanged += OnHandlerChanged;
			return;
		}
		View = bindable;
		OnPlatformAttachedBehavior(bindable.Handler);
	}

	/// <inheritdoc />
	protected sealed override void OnDetachingFrom(TView bindable)
	{
		OnPlatformDeattachedBehavior(bindable.Handler);
		bindable.HandlerChanged -= OnHandlerChanged;
	}

	private void OnHandlerChanged(object? sender, EventArgs e)
	{
		if (sender is not TView visualElement)
			return;

		if (visualElement.Handler is not null)
			OnAttachedTo(visualElement);
	}

	/// <summary>
	/// This method is called when the Handler is attached to the View. Use this method to perform your customizations in the control.
	/// </summary>
	/// <param name="handler">The <see cref="IViewHandler"/> for the <see cref="VisualElement"/>.</param>
	protected abstract void OnPlatformAttachedBehavior(IViewHandler handler);

	/// <summary>
	/// This method is called when the Handler is dettached from the View. Use this method to perform your customizations in the control.
	/// </summary>
	/// <param name="handler">The <see cref="IViewHandler"/> for the <see cref="VisualElement"/>.</param>
	protected abstract void OnPlatformDeattachedBehavior(IViewHandler? handler);
}
