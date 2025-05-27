using System;
using Microsoft.Maui.Graphics;
using System.Threading.Tasks;
using CoreFoundation;
using Foundation;
using ObjCRuntime;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler : ViewHandler<ISwitch, UISwitch>
	{
		readonly SwitchProxy _proxy = new();

		// the UISwitch control becomes inaccessible if it grows to a width > 101
		// An issue has been logged with Apple
		// This ensures that the UISwitch remains the natural size that iOS expects
		// But the container can be used for setting BGColors and other features.
		public override bool NeedsContainer => true;

		protected override UISwitch CreatePlatformView()
		{
			return new UISwitch(RectangleF.Empty);
		}

		protected override void ConnectHandler(UISwitch platformView)
		{
			base.ConnectHandler(platformView);
			_proxy.Connect(VirtualView, platformView);
		}

		protected override void DisconnectHandler(UISwitch platformView)
		{
			base.DisconnectHandler(platformView);
			_proxy.Disconnect(platformView);
		}

		public static void MapIsOn(ISwitchHandler handler, ISwitch view)
		{
			UpdateIsOn(handler);
			handler.PlatformView?.UpdateIsOn(view);
		}

		public static void MapTrackColor(ISwitchHandler handler, ISwitch view)
		{
			handler.PlatformView?.UpdateTrackColor(view);
		}

		public static void MapThumbColor(ISwitchHandler handler, ISwitch view)
		{
			handler.PlatformView?.UpdateThumbColor(view);
		}

		static void UpdateIsOn(ISwitchHandler handler)
		{
			handler.UpdateValue(nameof(ISwitch.TrackColor));
		}

		class SwitchProxy
		{
			WeakReference<ISwitch>? _virtualView;

			ISwitch? VirtualView => _virtualView is not null && _virtualView.TryGetTarget(out var v) ? v : null;

			NSObject? _willEnterForegroundObserveriOS;
			NSObject? _windowDidBecomeKeyObserverMac;

			public void Connect(ISwitch virtualView, UISwitch platformView)
			{
				_virtualView = new(virtualView);
				platformView.ValueChanged += OnControlValueChanged;

#if MACCATALYST
				_windowDidBecomeKeyObserverMac = NSNotificationCenter.DefaultCenter.AddObserver(
					new NSString("NSWindowDidBecomeKeyNotification"), _ => UpdateTrackOffColor(platformView));
#elif IOS
				_willEnterForegroundObserveriOS = NSNotificationCenter.DefaultCenter.AddObserver(
					UIApplication.WillEnterForegroundNotification, _ => UpdateTrackOffColor(platformView));
#endif
			}

			// Ensures the Switch track "OFF" color is updated correctly after system-level UI resets.
			// This is necessary because UIKit may re-apply default styles to internal views during certain lifecycle events,
			// especially when the app enters the background and returns to the foreground.
			void UpdateTrackOffColor(UISwitch platformView)
			{
				DispatchQueue.MainQueue.DispatchAsync(async () =>
				{
					await Task.Delay(10); // Small delay, necessary to allow UIKit to complete its internal layout and styling processes before re-applying the custom color

					if (!platformView.On)
					{
						if (VirtualView is ISwitch view && view.TrackColor is not null)
						{
							platformView.UpdateTrackColor(view);
						}
					}
				});
			}

			public void Disconnect(UISwitch platformView)
			{
				platformView.ValueChanged -= OnControlValueChanged;

				if (_willEnterForegroundObserveriOS is not null)
				{
					NSNotificationCenter.DefaultCenter.RemoveObserver(_willEnterForegroundObserveriOS);
					_willEnterForegroundObserveriOS = null;
				}
				if (_windowDidBecomeKeyObserverMac is not null)
				{
					NSNotificationCenter.DefaultCenter.RemoveObserver(_windowDidBecomeKeyObserverMac);
					_windowDidBecomeKeyObserverMac = null;
				}
			}

			void OnControlValueChanged(object? sender, EventArgs e)
			{
				if (VirtualView is ISwitch virtualView && sender is UISwitch platformView && virtualView.IsOn != platformView.On)
				{
					virtualView.IsOn = platformView.On;
				}
			}
		}
	}
}