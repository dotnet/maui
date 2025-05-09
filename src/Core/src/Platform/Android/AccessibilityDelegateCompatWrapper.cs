using Android.OS;
using Android.Views.Accessibility;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using PlatformView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	// This allows you to take an existing delegate and wrap it if you want to retain
	// the behavior of the Accessibility Delegate that's already assigned.
	// We use this inside controls if we want to add additional accessibility delegate behavior
	public class AccessibilityDelegateCompatWrapper : AccessibilityDelegateCompat
	{
		readonly AccessibilityDelegateCompat _originalDelegate;
		public AccessibilityDelegateCompat? WrappedDelegate
		{
			get
			{
				if (_originalDelegate == s_blankDelegate)
					return null;

				return _originalDelegate;
			}
		}

		static AccessibilityDelegateCompat? s_blankDelegate;
		static AccessibilityDelegateCompat BlankDelegate => s_blankDelegate ??= new AccessibilityDelegateCompat();

		public AccessibilityDelegateCompatWrapper(AccessibilityDelegateCompat? originalDelegate)
		{
			_originalDelegate = originalDelegate ?? BlankDelegate;
		}

		public override void OnInitializeAccessibilityNodeInfo(PlatformView? host, AccessibilityNodeInfoCompat? info)
		{
			_originalDelegate.OnInitializeAccessibilityNodeInfo(host, info);
		}

		public override void SendAccessibilityEvent(PlatformView? host, int eventType)
		{
			_originalDelegate.SendAccessibilityEvent(host, eventType);
		}

		public override void SendAccessibilityEventUnchecked(PlatformView? host, AccessibilityEvent? e)
		{
			_originalDelegate.SendAccessibilityEventUnchecked(host, e);
		}

		public override bool DispatchPopulateAccessibilityEvent(PlatformView? host, AccessibilityEvent? e)
		{
			return _originalDelegate.DispatchPopulateAccessibilityEvent(host, e);
		}

		public override void OnPopulateAccessibilityEvent(PlatformView? host, AccessibilityEvent? e)
		{
			_originalDelegate.OnPopulateAccessibilityEvent(host, e);
		}

		public override void OnInitializeAccessibilityEvent(PlatformView? host, AccessibilityEvent? e)
		{
			_originalDelegate.OnInitializeAccessibilityEvent(host, e);
		}

		public override bool OnRequestSendAccessibilityEvent(Android.Views.ViewGroup? host, PlatformView? child, AccessibilityEvent? e)
		{
			return _originalDelegate.OnRequestSendAccessibilityEvent(host, child, e);
		}

		public override bool PerformAccessibilityAction(PlatformView? host, int action, Bundle? args)
		{
			return _originalDelegate.PerformAccessibilityAction(host, action, args);
		}

		public override AccessibilityNodeProviderCompat? GetAccessibilityNodeProvider(PlatformView? host)
		{
			return _originalDelegate.GetAccessibilityNodeProvider(host);
		}
	}
}
