using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class MediaPlayer : Element, IMediaPlayer, IDisposable
	{
		public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(MediaSource), typeof(MediaPlayer), default(MediaSource), propertyChanged: OnSourceChanged);

		public static readonly BindableProperty VideoOutputProperty = BindableProperty.Create(nameof(VideoOutput), typeof(IVideoOutput), typeof(MediaPlayer), null, propertyChanging: null, propertyChanged: (b, o, n) => ((MediaPlayer)b).OnVideoOutputChanged());

		public static readonly BindableProperty UsesEmbeddingControlsProperty = BindableProperty.Create(nameof(UsesEmbeddingControls), typeof(bool), typeof(MediaPlayer), true, propertyChanged: (b, o, n) => ((MediaPlayer)b).OnUsesEmbeddingControlsChanged());

		public static readonly BindableProperty VolumeProperty = BindableProperty.Create(nameof(Volume), typeof(double), typeof(MediaPlayer), 1d, coerceValue: (bindable, value) => ((double)value).Clamp(0, 1), propertyChanged: (b, o, n) => ((MediaPlayer)b).OnVolumeChanged());

		public static readonly BindableProperty IsMutedProperty = BindableProperty.Create(nameof(IsMuted), typeof(bool), typeof(MediaPlayer), false, propertyChanged: (b, o, n) => ((MediaPlayer)b).UpdateIsMuted());

		public static readonly BindableProperty AspectModeProperty = BindableProperty.Create(nameof(AspectMode), typeof(DisplayAspectMode), typeof(MediaPlayer), DisplayAspectMode.AspectFit, propertyChanged: (b, o, n) => ((MediaPlayer)b).OnAspectModeChanged());

		public static readonly BindableProperty AutoPlayProperty = BindableProperty.Create(nameof(AutoPlay), typeof(bool), typeof(MediaPlayer), false, propertyChanged: (b, o, n) => ((MediaPlayer)b).UpdateAutoPlay());

		public static readonly BindableProperty AutoStopProperty = BindableProperty.Create(nameof(AutoStop), typeof(bool), typeof(MediaPlayer), true, propertyChanged: (b, o, n) => ((MediaPlayer)b).UpdateAutoStop());

		public static readonly BindableProperty IsLoopingProperty = BindableProperty.Create(nameof(IsLooping), typeof(bool), typeof(MediaPlayer), false, propertyChanged: (b, o, n) => ((MediaPlayer)b).UpdateIsLooping());

		static readonly BindablePropertyKey DurationPropertyKey = BindableProperty.CreateReadOnly(nameof(Duration), typeof(int), typeof(MediaPlayer), 0);

		public static readonly BindableProperty DurationProperty = DurationPropertyKey.BindableProperty;

		static readonly BindablePropertyKey BufferingProgressPropertyKey = BindableProperty.CreateReadOnly(nameof(BufferingProgress), typeof(double), typeof(MediaPlayer), 0d);

		public static readonly BindableProperty BufferingProgressProperty = BufferingProgressPropertyKey.BindableProperty;

		static readonly BindablePropertyKey PositionPropertyKey = BindableProperty.CreateReadOnly(nameof(Position), typeof(int), typeof(MediaPlayer), 0);

		public static readonly BindableProperty PositionProperty = PositionPropertyKey.BindableProperty;

		static readonly BindablePropertyKey StatePropertyKey = BindableProperty.CreateReadOnly(nameof(State), typeof(PlaybackState), typeof(MediaPlayer), PlaybackState.Stopped);

		public static readonly BindableProperty StateProperty = StatePropertyKey.BindableProperty;

		public static readonly BindableProperty PositionUpdateIntervalProperty = BindableProperty.Create(nameof(PositionUpdateInterval), typeof(int), typeof(MediaPlayer), 500);

		static readonly BindablePropertyKey IsBufferingPropertyKey = BindableProperty.CreateReadOnly(nameof(IsBuffering), typeof(bool), typeof(MediaPlayer), false);

		public static readonly BindableProperty IsBufferingProperty = IsBufferingPropertyKey.BindableProperty;

		bool _disposed = false;
		bool _isPlaying;
		bool _controlsAlwaysVisible;
		IPlatformMediaPlayer _impl;
		CancellationTokenSource _hideTimerCTS = new CancellationTokenSource();
		Lazy<View> _controls;

		public MediaPlayer()
		{
			_impl = new MediaPlayerImpl();
			_impl.UpdateStreamInfo += OnUpdateStreamInfo;
			_impl.PlaybackCompleted += SendPlaybackCompleted;
			_impl.PlaybackStarted += SendPlaybackStarted;
			_impl.PlaybackPaused += SendPlaybackPaused;
			_impl.PlaybackStopped += SendPlaybackStopped;
			_impl.BufferingProgressUpdated += OnUpdateBufferingProgress;
			_impl.ErrorOccurred += OnErrorOccurred;
			_impl.UsesEmbeddingControls = true;
			_impl.Volume = 1d;
			_impl.AspectMode = DisplayAspectMode.AspectFit;
			_impl.AutoPlay = false;
			_impl.AutoStop = true;

			_controlsAlwaysVisible = false;
			_controls = new Lazy<View>(() =>
			{
				return _impl.GetEmbeddingControlView(this);
			});
		}

		~MediaPlayer()
		{
			Dispose(false);
		}

		public DisplayAspectMode AspectMode
		{
			get { return (DisplayAspectMode)GetValue(AspectModeProperty); }
			set { SetValue(AspectModeProperty, value); }
		}

		public bool AutoPlay
		{
			get
			{
				return (bool)GetValue(AutoPlayProperty);
			}
			set
			{
				SetValue(AutoPlayProperty, value);
			}
		}

		public bool AutoStop
		{
			get
			{
				return (bool)GetValue(AutoStopProperty);
			}
			set
			{
				SetValue(AutoStopProperty, value);
			}
		}

		public bool IsLooping
		{
			get
			{
				return (bool)GetValue(IsLoopingProperty);
			}
			set
			{
				SetValue(IsLoopingProperty, value);
			}
		}

		public double BufferingProgress
		{
			get
			{
				return (double)GetValue(BufferingProgressProperty);
			}
			private set
			{
				SetValue(BufferingProgressPropertyKey, value);
			}
		}

		public int Duration
		{
			get
			{
				return (int)GetValue(DurationProperty);
			}
			private set
			{
				SetValue(DurationPropertyKey, value);
			}
		}

		[Xamarin.Forms.TypeConverter(typeof(MediaSourceConverter))]
		public MediaSource Source
		{
			get { return (MediaSource)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		public IVideoOutput VideoOutput
		{
			get { return (IVideoOutput)GetValue(VideoOutputProperty); }
			set { SetValue(VideoOutputProperty, value); }
		}

		public double Volume
		{
			get { return (double)GetValue(VolumeProperty); }
			set { SetValue(VolumeProperty, value); }
		}

		public bool IsMuted
		{
			get { return (bool)GetValue(IsMutedProperty); }
			set { SetValue(IsMutedProperty, value); }
		}

		public int PositionUpdateInterval
		{
			get { return (int)GetValue(PositionUpdateIntervalProperty); }
			set { SetValue(PositionUpdateIntervalProperty, value); }
		}

		public bool UsesEmbeddingControls
		{
			get
			{
				return (bool)GetValue(UsesEmbeddingControlsProperty);
			}
			set
			{
				SetValue(UsesEmbeddingControlsProperty, value);
				_impl.UsesEmbeddingControls = value;
			}
		}

		public int Position
		{
			get
			{
				return _impl.Position;
			}
			private set
			{
				SetValue(PositionPropertyKey, value);
				OnPropertyChanged(nameof(Progress));
			}
		}

		public PlaybackState State
		{
			get
			{
				return (PlaybackState)GetValue(StateProperty);
			}
			private set
			{
				SetValue(StatePropertyKey, value);
			}
		}

		public bool IsBuffering
		{
			get
			{
				return (bool)GetValue(IsBufferingProperty);
			}
			private set
			{
				SetValue(IsBufferingPropertyKey, value);
			}
		}

		public double Progress
		{
			get
			{
				return Position / (double)Math.Max(Position, Duration);
			}
		}

		public Command StartCommand => new Command(() =>
		{
			if (State == PlaybackState.Playing)
			{
				Pause();
			}
			else
			{
				Start();
			}
		});

		public Command FastForwardCommand => new Command(() =>
		{
			if (State != PlaybackState.Stopped)
			{
				Seek(Math.Min(Position + 5000, Duration));
			}
		}, () => State != PlaybackState.Stopped);

		public Command RewindCommand => new Command(() =>
		{
			if (State != PlaybackState.Stopped)
			{
				Seek(Math.Max(Position - 5000, 0));
			}
		}, () => State != PlaybackState.Stopped);

		public event EventHandler PlaybackCompleted;
		public event EventHandler PlaybackStarted;
		public event EventHandler PlaybackPaused;
		public event EventHandler PlaybackStopped;
		public event EventHandler<BufferingProgressUpdatedEventArgs> BufferingProgressUpdated;
		public event EventHandler BufferingStarted;
		public event EventHandler BufferingCompleted;
		public event EventHandler ErrorOccurred;
		public event EventHandler MediaPrepared;

		public void Pause()
		{
			_impl.Pause();
		}

		public Task<int> Seek(int ms)
		{
			ShowController();
			return _impl.Seek(ms).ContinueWith((t) => Position = _impl.Position);
		}

		public Task<bool> Start()
		{
			return _impl.Start();
		}

		public void Stop()
		{
			_impl.Stop();
		}

		public Task<Stream> GetAlbumArts()
		{
			return _impl.GetAlbumArts();
		}

		public Task<IDictionary<string, string>> GetMetadata()
		{
			return _impl.GetMetadata();
		}

		public Task<Size> GetVideoSize()
		{
			return _impl.GetVideoSize();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				_impl.UpdateStreamInfo -= OnUpdateStreamInfo;
				_impl.PlaybackCompleted -= SendPlaybackCompleted;
				_impl.PlaybackStarted -= SendPlaybackStarted;
				_impl.PlaybackPaused -= SendPlaybackPaused;
				_impl.PlaybackStopped -= SendPlaybackStopped;
				_impl.BufferingProgressUpdated -= OnUpdateBufferingProgress;
				_impl.ErrorOccurred -= OnErrorOccurred;
				_impl.Dispose();
			}

			_disposed = true;
		}

		void UpdateAutoPlay()
		{
			_impl.AutoPlay = AutoPlay;
		}

		void UpdateAutoStop()
		{
			_impl.AutoStop = AutoStop;
		}

		void UpdateIsMuted()
		{
			_impl.IsMuted = IsMuted;
		}

		void UpdateIsLooping()
		{
			_impl.IsLooping = IsLooping;
		}

		void OnUpdateStreamInfo(object sender, EventArgs e)
		{
			Duration = _impl.Duration;
			MediaPrepared?.Invoke(this, EventArgs.Empty);
		}

		void SendPlaybackCompleted(object sender, EventArgs e)
		{
			PlaybackCompleted?.Invoke(this, EventArgs.Empty);
		}

		void SendPlaybackStarted(object sender, EventArgs e)
		{
			_isPlaying = true;
			State = PlaybackState.Playing;
			StartPostionPollingTimer();
			PlaybackStarted?.Invoke(this, EventArgs.Empty);
			_controlsAlwaysVisible = false;
			ShowController();
		}

		void SendPlaybackPaused(object sender, EventArgs e)
		{
			_isPlaying = false;
			State = PlaybackState.Paused;
			PlaybackPaused?.Invoke(this, EventArgs.Empty);
			_controlsAlwaysVisible = true;
			ShowController();
		}

		void SendPlaybackStopped(object sender, EventArgs e)
		{
			_isPlaying = false;
			State = PlaybackState.Stopped;
			Position = 0;
			PlaybackStopped?.Invoke(this, EventArgs.Empty);
			_controlsAlwaysVisible = true;
			ShowController();
		}

		void StartPostionPollingTimer()
		{
			Device.StartTimer(TimeSpan.FromMilliseconds(PositionUpdateInterval), () =>
			{
				Position = _impl.Position;
				return _isPlaying;
			});
		}

		void OnSourceChanged(object sender, EventArgs e)
		{
			_impl.SetSource(Source);
		}

		void OnVideoOutputChanged()
		{
			if (VideoOutput != null)
			{
				if (UsesEmbeddingControls)
				{
					VideoOutput.Controller = _controls.Value;
				}
				VideoOutput.MediaView.Focused += OnVideoOutputFocused;
				if (VideoOutput.MediaView is View outputView)
				{
					TapGestureRecognizer tapGesture = new TapGestureRecognizer();
					tapGesture.Tapped += OnOutputTapped;
					outputView.GestureRecognizers.Add(tapGesture);
				}
			}
			_impl.SetDisplay(VideoOutput);
		}

		void OnOutputTapped(object sender, EventArgs e)
		{
			if (!UsesEmbeddingControls)
				return;
			if (!_controls.Value.IsVisible)
			{
				ShowController();
			}
		}

		async void OnUsesEmbeddingControlsChanged()
		{
			if (UsesEmbeddingControls)
			{
				if (VideoOutput != null)
				{
					VideoOutput.Controller = _controls.Value;
					ShowController();
				}
			}
			else
			{
				if (VideoOutput != null)
				{
					HideController(0);
					await Task.Delay(200);
					VideoOutput.Controller = null;
				}
			}
		}

		void OnVideoOutputFocused(object sender, FocusEventArgs e)
		{
			if (UsesEmbeddingControls)
			{
				ShowController();
			}
		}

		void OnVolumeChanged()
		{
			_impl.Volume = Volume;
		}

		void OnAspectModeChanged()
		{
			_impl.AspectMode = AspectMode;
		}

		void OnUpdateBufferingProgress(object sender, BufferingProgressUpdatedEventArgs e)
		{
			if (!IsBuffering && e.Progress >= 0)
			{
				IsBuffering = true;
				BufferingStarted?.Invoke(this, EventArgs.Empty);
			}
			else if (IsBuffering && e.Progress == 1.0)
			{
				IsBuffering = false;
				BufferingCompleted?.Invoke(this, EventArgs.Empty);
			}
			BufferingProgress = e.Progress;
			BufferingProgressUpdated?.Invoke(this, new BufferingProgressUpdatedEventArgs { Progress = BufferingProgress });
		}

		void OnErrorOccurred(object sender, EventArgs e)
		{
			ErrorOccurred?.Invoke(this, EventArgs.Empty);
		}

		async void HideController(int after)
		{
			if (!_controls.IsValueCreated)
				return;

			_hideTimerCTS?.Cancel();
			_hideTimerCTS?.Dispose();
			_hideTimerCTS = new CancellationTokenSource();
			try
			{
				await Task.Delay(after, _hideTimerCTS.Token);

				if (!_controlsAlwaysVisible)
				{
					await _controls.Value.FadeTo(0, 200);
					_controls.Value.IsVisible = false;
				}
			}
			catch (Exception)
			{
				//Exception when canceled
			}
		}

		void ShowController()
		{
			if (_controls.IsValueCreated)
			{
				_controls.Value.IsVisible = true;
				_controls.Value.FadeTo(1.0, 200);
				HideController(5000);
			}
		}

		static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as MediaPlayer)?.OnSourceChanged(bindable, EventArgs.Empty);
		}
	}
}
