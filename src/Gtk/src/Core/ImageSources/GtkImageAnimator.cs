using System;

namespace Microsoft.Maui.Gtk.ImageSources
{
	/// <summary>
	/// Plays a <see cref="GtkImageAnimation"/> on a widget: while playing, a frame-clock tick callback advances the animation and hands each new
	/// frame to <c>show</c>. Not playing (or no animation) leaves the current frame on screen and no callback registered.
	/// </summary>
	internal sealed class GtkImageAnimator : IDisposable
	{
		readonly global::Gtk.Widget _host;
		readonly Action<Gdk.Texture> _show;
		GtkImageAnimation? _animation;
		bool _playing;
		uint _tickId;

		public GtkImageAnimator(global::Gtk.Widget host, Action<Gdk.Texture> show)
		{
			_host = host;
			_show = show;
		}

		/// <summary>Whether the tick callback is registered (playing an animation).</summary>
		public bool IsTicking => _tickId != 0;

		/// <summary>The animation being driven, if any.</summary>
		public GtkImageAnimation? Animation => _animation;

		/// <summary>Replaces the animation (<see langword="null"/> for a still image).</summary>
		public void SetAnimation(GtkImageAnimation? animation)
		{
			_animation = animation;
			Refresh();
		}

		/// <summary>Starts or stops playback (<c>IsAnimationPlaying</c>).</summary>
		public void SetPlaying(bool playing)
		{
			_playing = playing;
			Refresh();
		}

		void Refresh()
		{
			var shouldTick = _playing && _animation is { IsDisposed: false };
			if (shouldTick && _tickId == 0)
				_tickId = _host.AddTickCallback(OnTick);
			else if (!shouldTick && _tickId != 0)
				Stop();
		}

		void Stop()
		{
			_host.RemoveTickCallback(_tickId);
			_tickId = 0;
		}

		bool OnTick(global::Gtk.Widget widget, Gdk.FrameClock clock)
		{
			Step();
			return _tickId != 0;
		}

		/// <summary>Advances to the frame due now and shows it when it changed. Called by the tick callback.</summary>
		internal void Step()
		{
			if (_animation is not { IsDisposed: false } animation)
			{
				// The loader released the animation under us; the callback ends by returning false.
				_tickId = 0;
				return;
			}

			if (animation.Advance())
				_show(animation.CurrentTexture);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			_playing = false;
			_animation = null;
			if (_tickId != 0)
				Stop();
		}
	}
}
