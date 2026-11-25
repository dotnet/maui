#nullable enable
using System;
using Microsoft.Maui.Dispatching;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Controls.Animations
{
	internal abstract partial class AnimationFrameClock
	{
		static partial void CreatePlatformClockCore(IDispatcher? dispatcher, ref AnimationFrameClock? clock)
			=> clock = new RenderingFrameClock();

		/// <summary><c>CompositionTarget.Rendering</c> with <c>RenderingEventArgs.RenderingTime</c>.</summary>
		sealed class RenderingFrameClock : AnimationFrameClock
		{
			bool _subscribed;

			protected override void StartFrames()
			{
				if (_subscribed)
					return;

				_subscribed = true;
				CompositionTarget.Rendering += OnRendering;
			}

			protected override void StopFrames()
			{
				if (!_subscribed)
					return;

				_subscribed = false;
				CompositionTarget.Rendering -= OnRendering;
			}

			protected override void ReleaseFrames() => StopFrames();

			void OnRendering(object? sender, object e)
			{
				// RenderingTime has its own origin; OnFrame maps it onto the clock's time line.
				if (e is RenderingEventArgs args)
					OnFrame(args.RenderingTime);
			}
		}
	}
}
