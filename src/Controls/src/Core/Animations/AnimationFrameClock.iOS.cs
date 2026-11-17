#nullable enable
using System;
using CoreAnimation;
using Foundation;

namespace Microsoft.Maui.Controls.Animations
{
	internal abstract partial class AnimationFrameClock
	{
		internal static partial AnimationFrameClock CreatePlatformClock() => new DisplayLinkClock();

		sealed class DisplayLinkClock : AnimationFrameClock
		{
			CADisplayLink? _link;
			double _lastTargetTimestamp;

			protected override TimeSpan Now
				=> TimeSpan.FromSeconds(_link is not null && _lastTargetTimestamp > 0 ? _lastTargetTimestamp : CAAnimation.CurrentMediaTime());

			protected override void StartFrames()
			{
				if (_link is not null)
					return;

				_lastTargetTimestamp = 0;
				_link = CADisplayLink.Create(OnLinkFrame);

				// Up to the display's maximum (120 Hz on ProMotion). On iPhone, rates above 60 Hz also need
				// CADisableMinimumFrameDurationOnPhone in the app's Info.plist.
				_link.PreferredFrameRateRange = CAFrameRateRange.Create(30, 120, 120);
				_link.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Common);
			}

			protected override void StopFrames()
			{
				if (_link is null)
					return;

				_link.Invalidate();
				_link.Dispose();
				_link = null;
			}

			void OnLinkFrame()
			{
				if (_link is null)
					return;

				// The time at which this frame will be displayed: what the user actually sees.
				_lastTargetTimestamp = _link.TargetTimestamp;
				OnFrame();
			}
		}
	}
}
