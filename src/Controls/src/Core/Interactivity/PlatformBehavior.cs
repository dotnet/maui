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

namespace Microsoft.Maui.Controls;

/// <summary>
/// Base class for generalized user-defined behaviors that can respond to arbitrary conditions and events that has influence in the Platform specif layer.
/// </summary>
/// <typeparam name="TView">Virtual View</typeparam>
/// <typeparam name="TPlatformView">Platform View</typeparam>
public abstract partial class BasePlatformBehavior<TView, TPlatformView> : Behavior<TView>, IPlatformAttachedObject
	where TView : VisualElement
#if __IOS__ || __ANDROID__ || __WINDOWS
		where TPlatformView : PlatformView
#else
		where TPlatformView : class
#endif
{
	protected TPlatformView? PlatformView => View?.Handler?.PlatformView as TPlatformView;
	protected TView? View { get; private set; }
	bool _isRemoved;

	/// <inheritdoc />
	protected override void OnAttachedTo(TView bindable)
	{
		if (bindable.Handler is null)
			return;

		_isRemoved = false;
		base.OnAttachedTo(bindable);
		View = bindable;
		OnPlatformAttachedBehavior();
	}

	/// <inheritdoc />
	protected override void OnDetachingFrom(TView bindable)
	{
		if (_isRemoved)
			return;

		base.OnDetachingFrom(bindable);
		OnPlatformDeattachedBehavior();
		_isRemoved = true;
	}

	/// <summary>
	/// This method is called when the Handler is attached to the View. Use this method to perform your customizations in the control
	/// </summary>
	protected abstract void OnPlatformAttachedBehavior();

	/// <summary>
	/// This method is called when the Handler is dettached from the View. Use this method to perform your customizations in the control
	/// </summary>
	protected abstract void OnPlatformDeattachedBehavior();

	void IPlatformAttachedObject.OnPlatformAttachBehavior(BindableObject bindable)
	{
		_isRemoved = false;
		OnAttachedTo(bindable);
	}

	void IPlatformAttachedObject.OnPlatformDeattachBehavior(BindableObject bindable)
	{
		OnDetachingFrom(bindable);
		_isRemoved = true;
	}
}
