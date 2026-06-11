using System;
using CoreFoundation;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Platform;
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
			return new MauiSwitch(RectangleF.Empty);
		}

		protected override void ConnectHandler(UISwitch platformView)
		{
			base.ConnectHandler(platformView);
			_proxy.Connect(this, VirtualView, platformView);
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
			static readonly TimeSpan ColorReapplyDelay = TimeSpan.FromMilliseconds(10);
			const int MaxColorReapplyAttempts = 5;

			WeakReference<SwitchHandler>? _handler;
			WeakReference<ISwitch>? _virtualView;

			SwitchHandler? Handler => _handler is not null && _handler.TryGetTarget(out var h) ? h : null;

			ISwitch? VirtualView => _virtualView is not null && _virtualView.TryGetTarget(out var v) ? v : null;

			WeakReference<UISwitch>? _platformView;

			UISwitch? PlatformView => _platformView is not null && _platformView.TryGetTarget(out var p) ? p : null;

			NSObject? _willEnterForegroundObserver;
			NSObject? _windowDidBecomeKeyObserver;
			IUITraitChangeRegistration? _traitChangeRegistration;
			UISwitch? _queuedColorReapplyPlatformView;
			ColorReapplyKind _queuedColorReapplyKind;
			bool _colorReapplyDispatchPending;

			public void Connect(SwitchHandler handler, ISwitch virtualView, UISwitch platformView)
			{
				_handler = new(handler);
				_virtualView = new(virtualView);
				_platformView = new(platformView);

				if (platformView is MauiSwitch mauiSwitch)
				{
					mauiSwitch.ColorReapplyRequested += OnColorReapplyRequested;
					if (IsSlidingStyleRequiredForCustomColors())
					{
						mauiSwitch.StartMapperColorOverrideDetection();
					}
					mauiSwitch.SetNeedsColorReapply();
				}

				platformView.ValueChanged += OnControlValueChanged;

#if MACCATALYST
				_windowDidBecomeKeyObserver = NSNotificationCenter.DefaultCenter.AddObserver(
					new NSString("NSWindowDidBecomeKeyNotification"), _ =>
					{
						if (PlatformView is not null)
						{
							QueueTrackOffColorReapply(PlatformView);
							if (OperatingSystem.IsMacCatalystVersionAtLeast(26))
							{
								QueueCustomColorReapply(PlatformView);
							}
						}
					});
#elif IOS
				_willEnterForegroundObserver = NSNotificationCenter.DefaultCenter.AddObserver(
					UIApplication.WillEnterForegroundNotification, _ =>
					{
						if (PlatformView is not null)
						{
							QueueTrackOffColorReapply(PlatformView);
							if (OperatingSystem.IsIOSVersionAtLeast(26))
							{
								QueueCustomColorReapply(PlatformView);
							}
						}
					});
#endif

				// iOS/MacCatalyst 26+ can reset custom switch colors during initial UIKit styling and trait changes.
				// Re-apply after UIKit completes styling; MauiSwitch handles later layout/window reapply paths.
				if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
				{
					if (_traitChangeRegistration is not null)
					{
						platformView.UnregisterForTraitChanges(_traitChangeRegistration);
					}

					_traitChangeRegistration = platformView.RegisterForTraitChanges<UITraitUserInterfaceStyle>(
						(IUITraitEnvironment view, UITraitCollection _) =>
						{
							if (view is UISwitch uiSwitch)
							{
								QueueCustomColorReapply(uiSwitch);
							}
						});

					QueueCustomColorReapply(platformView);
				}
			}

			void QueueTrackOffColorReapply(UISwitch platformView)
			{
				QueueColorReapply(platformView, ColorReapplyKind.TrackOff);
			}

			void QueueCustomColorReapply(UISwitch platformView)
			{
				QueueColorReapply(platformView, ColorReapplyKind.CustomColors);
			}

			void OnColorReapplyRequested(object? sender, EventArgs e)
			{
				if (sender is UISwitch platformView)
				{
					var kind = ColorReapplyKind.Lifecycle;
					if (sender is MauiSwitch { NeedsNativeDefaultCleanup: true })
					{
						kind |= ColorReapplyKind.NativeDefaults;
					}

					QueueColorReapply(platformView, kind);
				}
			}

			void QueueColorReapply(UISwitch platformView, ColorReapplyKind kind)
			{
				if (!ReferenceEquals(PlatformView, platformView))
				{
					return;
				}

				_queuedColorReapplyKind |= kind;

				var platformViewChanged = !ReferenceEquals(_queuedColorReapplyPlatformView, platformView);
				if (platformViewChanged)
				{
					_queuedColorReapplyPlatformView = platformView;
				}

				if (!_colorReapplyDispatchPending || platformViewChanged)
				{
					DispatchColorReapply(platformView, 0);
				}
			}

			void DispatchColorReapply(UISwitch platformView, int attempt)
			{
				_colorReapplyDispatchPending = true;
				DispatchQueue.MainQueue.DispatchAfter(
					new DispatchTime(DispatchTime.Now, ColorReapplyDelay),
					() => ProcessColorReapply(platformView, attempt));
			}

			void ProcessColorReapply(UISwitch platformView, int attempt)
			{
				try
				{
					if (!ReferenceEquals(_queuedColorReapplyPlatformView, platformView))
					{
						(platformView as MauiSwitch)?.ClearNeedsColorReapply();
						return;
					}

					_colorReapplyDispatchPending = false;

					if (VirtualView is not ISwitch view ||
						Handler is not SwitchHandler handler ||
						!ReferenceEquals(PlatformView, platformView) ||
						!ReferenceEquals(handler.PlatformView, platformView))
					{
						ClearAllQueuedColorReapply(platformView);
						return;
					}

					if (!platformView.IsReadyForColorReapply())
					{
						if (attempt < MaxColorReapplyAttempts)
						{
							DispatchColorReapply(platformView, attempt + 1);
							return;
						}

						return;
					}

					var reapplyKind = _queuedColorReapplyKind;
					ClearQueuedColorReapply(platformView);

					if (TryDetectMapperColorOverride(handler, view, platformView))
					{
						if (reapplyKind.HasFlag(ColorReapplyKind.NativeDefaults))
						{
							(platformView as MauiSwitch)?.ClearNeedsNativeDefaultCleanup();
						}

						return;
					}

					if (reapplyKind.HasFlag(ColorReapplyKind.NativeDefaults))
					{
						if (platformView.ShouldPreserveNativeDefaults(view))
						{
							platformView.ClearCustomColorState();
							(platformView as MauiSwitch)?.ClearNeedsNativeDefaultCleanup();
							return;
						}

						(platformView as MauiSwitch)?.ClearNeedsNativeDefaultCleanup();
					}

					if ((reapplyKind.HasFlag(ColorReapplyKind.CustomColors) && platformView.HasCustomColorIntent(view)) ||
						(reapplyKind.HasFlag(ColorReapplyKind.Lifecycle) && !platformView.ShouldPreserveNativeDefaults(view)))
					{
						UpdateColorMappings(handler, view, platformView);
						return;
					}

					if (reapplyKind.HasFlag(ColorReapplyKind.TrackOff) && !platformView.On && view.TrackColor is not null)
					{
						handler.UpdateValue(nameof(ISwitch.TrackColor));
					}
				}
				catch (Exception ex)
				{
					ClearAllQueuedColorReapply(platformView);
					Handler?.MauiContext?.CreateLogger<SwitchHandler>()?.LogError(ex, "Unable to reapply switch colors.");
				}
			}

			void ClearQueuedColorReapply(UISwitch platformView)
			{
				if (ReferenceEquals(_queuedColorReapplyPlatformView, platformView))
				{
					_queuedColorReapplyKind = ColorReapplyKind.None;
					_queuedColorReapplyPlatformView = null;
				}

				_colorReapplyDispatchPending = false;
				(platformView as MauiSwitch)?.ClearNeedsColorReapply();
			}

			void ClearAllQueuedColorReapply(UISwitch platformView)
			{
				ClearQueuedColorReapply(platformView);
				(platformView as MauiSwitch)?.ClearNeedsNativeDefaultCleanup();
			}

			public void Disconnect(UISwitch platformView)
			{
				platformView.ValueChanged -= OnControlValueChanged;
				if (platformView is MauiSwitch mauiSwitch)
				{
					mauiSwitch.ColorReapplyRequested -= OnColorReapplyRequested;
					mauiSwitch.ClearMapperColorOverride();
				}

				_handler = null;
				_virtualView = null;
				_platformView = null;
				_queuedColorReapplyPlatformView = null;
				_queuedColorReapplyKind = ColorReapplyKind.None;
				_colorReapplyDispatchPending = false;
				(platformView as MauiSwitch)?.ClearPendingColorReapply();

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
				if (_traitChangeRegistration is not null)
				{
					platformView.UnregisterForTraitChanges(_traitChangeRegistration);
					_traitChangeRegistration = null;
				}
			}

			void OnControlValueChanged(object? sender, EventArgs e)
			{
				if (VirtualView is ISwitch virtualView && sender is UISwitch platformView && virtualView.IsOn != platformView.On)
				{
					virtualView.IsOn = platformView.On;
				}
			}

			static bool TryDetectMapperColorOverride(SwitchHandler handler, ISwitch view, UISwitch platformView)
			{
				if (!IsSlidingStyleRequiredForCustomColors() ||
					view.HasCustomColors() ||
					platformView is not MauiSwitch mauiSwitch ||
					!mauiSwitch.ShouldDetectMapperColorOverride)
				{
					return false;
				}

				platformView.ClearCustomColorState(clearTrackColor: true);
				mauiSwitch.CompleteMapperColorOverrideDetection();

				var beforeTrackColor = platformView.CaptureColorState();
				handler.UpdateValue(nameof(ISwitch.TrackColor));

				if (platformView.HasColorStateChanged(beforeTrackColor))
				{
					mauiSwitch.MarkMapperColorOverride();
				}

				var beforeThumbColor = platformView.CaptureColorState();
				handler.UpdateValue(nameof(ISwitch.ThumbColor));

				if (platformView.HasColorStateChanged(beforeThumbColor))
				{
					mauiSwitch.MarkMapperColorOverride();
				}

				if (!mauiSwitch.HasMapperColorOverride)
				{
					return false;
				}

				UpdateColorMappings(handler, view, platformView);
				return true;
			}

			static void UpdateColorMappings(SwitchHandler handler, ISwitch view, UISwitch platformView)
			{
				if (!view.HasCustomColors() && platformView is MauiSwitch { HasMapperColorOverride: true })
				{
					handler.UpdateValue(nameof(ISwitch.ThumbColor));
					handler.UpdateValue(nameof(ISwitch.TrackColor));
					return;
				}

				handler.UpdateValue(nameof(ISwitch.TrackColor));
				handler.UpdateValue(nameof(ISwitch.ThumbColor));
			}

			static bool IsSlidingStyleRequiredForCustomColors()
			{
				return OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26);
			}

			[Flags]
			enum ColorReapplyKind
			{
				None = 0,
				TrackOff = 1,
				CustomColors = 2,
				Lifecycle = 4,
				NativeDefaults = 8
			}
		}
	}
}
