#nullable enable
using System;
using System.Diagnostics;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Controls.Animations
{
	internal abstract partial class AnimationFrameClock
	{
		internal static partial AnimationFrameClock CreatePlatformClock() => new RenderingClock();

		sealed class RenderingClock : AnimationFrameClock
		{
			readonly Stopwatch _stopwatch = Stopwatch.StartNew();
			TimeSpan _lastFrameTime;
			TimeSpan _renderingOffset;
			bool _running;
			bool _hasFrame;

			// Frame timestamps while frames run, the stopwatch otherwise. RenderingTime counts from when the app started
			// rendering, so frames are mapped onto the stopwatch's time line at the first frame of each run of frames:
			// times read before that frame stay comparable, and frames still advance by exact rendering intervals.
			protected override TimeSpan Now
				=> _running && _hasFrame ? _lastFrameTime : _stopwatch.Elapsed;

			protected override void StartFrames()
			{
				if (_running)
					return;

				_running = true;
				_hasFrame = false;
				CompositionTarget.Rendering += OnRendering;
			}

			protected override void StopFrames()
			{
				if (!_running)
					return;

				_running = false;
				CompositionTarget.Rendering -= OnRendering;
			}

			void OnRendering(object? sender, object e)
			{
				if (e is RenderingEventArgs args)
				{
					if (!_hasFrame)
						_renderingOffset = _stopwatch.Elapsed - args.RenderingTime;

					_lastFrameTime = args.RenderingTime + _renderingOffset;
					_hasFrame = true;
				}

				OnFrame();
			}
		}
	}
}
