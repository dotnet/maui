using System;
using System.Threading.Tasks;
using CoreFoundation;
using Foundation;
using Microsoft.Maui.Graphics;
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

			WeakReference<UISwitch>? _platformView;

			UISwitch? PlatformView => _platformView is not null && _platformView.TryGetTarget(out var p) ? p : null;

			NSObject? _willEnterForegroundObserver;
			NSObject? _windowDidBecomeKeyObserver;

			public void Connect(ISwitch virtualView, UISwitch platformView)
			{
				_virtualView = new(virtualView);
				_platformView = new(platformView);
				platformView.ValueChanged += OnControlValueChanged;

#if MACCATALYST
				_windowDidBecomeKeyObserver = NSNotificationCenter.DefaultCenter.AddObserver(
					new NSString("NSWindowDidBecomeKeyNotification"), _ =>
					{
						if (PlatformView is not null)
						{
							UpdateTrackOffColor(PlatformView);
						}
					});
#elif IOS
				_willEnterForegroundObserver = NSNotificationCenter.DefaultCenter.AddObserver(
					UIApplication.WillEnterForegroundNotification, _ =>
					{
						if (PlatformView is not null)
						{
							UpdateTrackOffColor(PlatformView);
						}
					});
#endif
			}

			// Ensures the Switch track "OFF" color is updated correctly after system-level UI resets.
			// This is necessary because UIKit may re-apply default styles to internal views during certain lifecycle events,
			// especially when the app enters the background and returns to the foreground.
			void UpdateTrackOffColor(UISwitch platformView)
			{
				DispatchQueue.MainQueue.DispatchAsync(async () =>
				{
					if (!platformView.On)
					{
						await Task.Delay(10); // Small delay, necessary to allow UIKit to complete its internal layout and styling processes before re-applying the custom color

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

				if (_willEnterForegroundObserver is not null)
				{
					NSNotificationCenter.DefaultCenter.RemoveObserver(_willEnterForegroundObserver);
					_willEnterForegroundObserver = null;
				}
				if (_windowDidBecomeKeyObserver is not null)
				{
					NSNotificationCenter.DefaultCenter.RemoveObserver(_windowDidBecomeKeyObserver);
					_windowDidBecomeKeyObserver = null;
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