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
			bool _running;

			protected override TimeSpan Now => _stopwatch.Elapsed;

			protected override void StartFrames()
			{
				if (_running)
					return;

				_running = true;
				CompositionTarget.Rendering += OnRendering;
			}

			protected override void StopFrames()
			{
				if (!_running)
					return;

				_running = false;
				CompositionTarget.Rendering -= OnRendering;
			}

			void OnRendering(object? sender, object e) => OnFrame();
		}
	}
}
