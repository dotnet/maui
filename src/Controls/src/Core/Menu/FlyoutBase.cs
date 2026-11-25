#nullable disable
using System;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public abstract class FlyoutBase : Element, IFlyout
	{
		/// <summary>Bindable property for attached property <c>ContextFlyout</c>.</summary>
		/// <remarks>Use <see cref="ShowContextFlyout"/> to request programmatic presentation on supported platforms.
		/// Attaching a flyout does not enable long-press opening on Android or iOS.</remarks>
		public static readonly BindableProperty ContextFlyoutProperty = BindableProperty.CreateAttached("ContextFlyout", typeof(FlyoutBase), typeof(FlyoutBase), null,
			propertyChanged: (bo, oldV, newV) =>
			{
				bo.AddRemoveLogicalChildren(oldV, newV);
			});

		public static void SetContextFlyout(BindableObject b, FlyoutBase value)
		{
			b.SetValue(ContextFlyoutProperty, value);
		}

		public static FlyoutBase GetContextFlyout(BindableObject b)
		{
			return (FlyoutBase)b.GetValue(ContextFlyoutProperty);
		}

#nullable enable
		/// <summary>
		/// Requests presentation of the context flyout attached to <paramref name="target"/>.
		/// </summary>
		/// <param name="target">The loaded view to which a nonempty menu flyout is attached.</param>
		/// <remarks>
		/// Call this method on the target's UI thread. Supported on Android, Windows,
		/// iOS 15 and later, and Mac Catalyst 15 and later, subject to the application's MAUI platform minimums,
		/// using a native PopupMenu on Android,
		/// the attached WinUI menu flyout on Windows, and a native menu on iOS and Mac Catalyst 17.4 and later.
		/// Earlier supported Apple versions use a custom popover with submenus displayed as navigable pages.
		/// Existing long-press and right-click presentation is unchanged.
		/// Requests made while the programmatic menu is open are ignored.
		/// Android anchors the popup to the native target without registering a long-press listener.
		/// Returning from this method does not guarantee that the menu is visible.
		/// </remarks>
		/// <exception cref="ArgumentNullException"><paramref name="target"/> is null.</exception>
		/// <exception cref="InvalidOperationException">
		/// The call is off the UI thread, the view is not ready for interaction, or a nonempty menu flyout is not attached.
		/// </exception>
		/// <exception cref="PlatformNotSupportedException">The platform or OS version does not support presentation.</exception>
		public static void ShowContextFlyout(VisualElement target)
		{
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			if (target.Dispatcher.IsDispatchRequired)
				throw new InvalidOperationException("Context flyouts must be presented on the target's UI thread.");

			if (GetContextFlyout(target) is not MenuFlyout { Count: > 0 })
				throw new InvalidOperationException("A nonempty MenuFlyout must be attached to the target.");

			if (!target.IsEnabled || target.InputTransparent || !target.IsVisible)
				throw new InvalidOperationException("The context-flyout target must be visible, enabled, and accept input.");

			var handler = target.Handler;
			if (handler?.PlatformView is null || handler.VirtualView != target)
				throw new InvalidOperationException("The context-flyout target must have a connected handler.");

			handler.Invoke(ViewHandler.ShowContextFlyoutCommandKey);
		}
	}
}
