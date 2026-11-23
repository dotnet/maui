#nullable enable
using System;
using Microsoft.Maui.Dispatching;
using CoreAnimation;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Controls.Animations
{
	internal abstract partial class AnimationFrameClock
	{
		static partial void CreatePlatformClockCore(IDispatcher? dispatcher, ref AnimationFrameClock? clock)
			=> clock = new DisplayLinkFrameClock();

		/// <summary>
		/// <c>CADisplayLink</c> with <c>targetTimestamp</c>, asking for up to the display's maximum rate (120 Hz on
		/// ProMotion; above 60 Hz an iPhone app also needs <c>CADisableMinimumFrameDurationOnPhone</c> in its Info.plist).
		/// </summary>
		sealed class DisplayLinkFrameClock : AnimationFrameClock
		{
			CADisplayLink? _link;

			protected override void StartFrames()
			{
				if (_link is null)
				{
					var weak = new WeakReference<DisplayLinkFrameClock>(this);
					_link = CADisplayLink.Create(() =>
					{
						if (weak.TryGetTarget(out var clock))
							clock.OnLink();
					});

					if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsMacCatalystVersionAtLeast(15))
					{
						var maximum = (float)UIScreen.MainScreen.MaximumFramesPerSecond;
						_link.PreferredFrameRateRange = CAFrameRateRange.Create(Math.Min(60f, maximum), maximum, maximum);
					}

					_link.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Common);
				}

				_link.Paused = false;
			}

			protected override void StopFrames()
			{
				if (_link is not null)
					_link.Paused = true;
			}

			void OnLink()
			{
				if (_link is { } link)
					OnFrame(TimeSpan.FromSeconds(link.TargetTimestamp));
			}
		}
	}
}
