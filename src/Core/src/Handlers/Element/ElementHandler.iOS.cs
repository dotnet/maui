using Foundation;

#nullable enable
namespace Microsoft.Maui.Handlers
{

	public abstract partial class ElementHandler : NSObject
	{
		protected void RegisterToNotifications(NSObject view, string? notificationName = null)
		{
			NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onNativeNotification:"), (NSString?)notificationName, view);
		}

		// methods that will allow to receive nsnotifications from views, this way
		// we do not need to have a event handler from the view directly.
		// The main idea between this solution is to remove the need to have an event handler
		// in the view that will have a reference to the handler and therefore create a circular ref
		[Export("onNativeNotification:")]
		private void InternalOnNativeNotification(NSNotification notification)
		{
			// there are two types of notifications we are interested:
			//
			//  1. When the native object is disposed (should be implemented by the inherting maui types)
			//  2. All other notifications.
			//
			// 1. is very important, we want to remove ourselfs from the NSNotification center, 2 we fwd to the virtual method  
			if (notification.Name == "Disposing")
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver(this);
				OnNativeViewDisposed();
			}
			else
			{
				OnNativeViewChanged(notification);
			}
		}

		protected virtual void OnNativeViewDisposed()
		{
		}

		protected virtual void OnNativeViewChanged(NSNotification notification)
		{
		}

		protected override void Dispose(bool disposing)
		{
			// we need to remove ourselfs from the NSNotification center
			NSNotificationCenter.DefaultCenter.RemoveObserver(this);
			base.Dispose(disposing);
		}
	}
}