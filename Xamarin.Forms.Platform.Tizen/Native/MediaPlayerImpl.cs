using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Tizen.Multimedia;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;

[assembly: Xamarin.Forms.Dependency(typeof(IPlatformMediaPlayer))]
namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class MediaPlayerImpl : IPlatformMediaPlayer
	{
		bool _disposed  = false;
		bool _cancelToStart;
		DisplayAspectMode _aspectMode = DisplayAspectMode.AspectFit;
		Player _player;
		Task _taskPrepare;
		TaskCompletionSource<bool> _tcsForStreamInfo;
		IVideoOutput _videoOutput;
		MediaSource _source;

		public MediaPlayerImpl()
		{
			_player = new Player();
			_player.PlaybackCompleted += OnPlaybackCompleted;
			_player.BufferingProgressChanged += OnBufferingProgressChanged;
			_player.ErrorOccurred += OnErrorOccurred;
		}

		~MediaPlayerImpl()
		{
			Dispose(false);
		}

		public bool UsesEmbeddingControls { get; set; }

		public bool AutoPlay { get; set; }

		public bool AutoStop { get; set; }

		public double Volume
		{
			get => _player.Volume;
			set => _player.Volume = (float)value;
		}

		public bool IsMuted
		{
			get => _player.Muted;
			set => _player.Muted = value;
		}

		public bool IsLooping
		{
			get => _player.IsLooping;
			set => _player.IsLooping = value;
		}

		public int Duration => _player.StreamInfo.GetDuration();

		public int Position
		{
			get
			{
				if (_player.State == PlayerState.Idle || _player.State == PlayerState.Preparing)
					return 0;
				return _player.GetPlayPosition();
			}
		}

		public DisplayAspectMode AspectMode
		{
			get { return _aspectMode; }
			set
			{
				_aspectMode = value;
				ApplyAspectMode();
			}
		}

		bool HasSource => _source != null;

		IVideoOutput VideoOutput
		{
			get { return _videoOutput; }
			set
			{
				if (TargetView != null)
					TargetView.PropertyChanged -= OnTargetViewPropertyChanged;

				_videoOutput = value;

				if (TargetView != null)
				{
					TargetView.PropertyChanged += OnTargetViewPropertyChanged;
				}
			}
		}

		VisualElement TargetView => VideoOutput?.MediaView;

		Task TaskPrepare
		{
			get => _taskPrepare ?? Task.CompletedTask;
			set => _taskPrepare = value;
		}

		public event EventHandler UpdateStreamInfo;
		public event EventHandler PlaybackCompleted;
		public event EventHandler PlaybackStarted;
		public event EventHandler<BufferingProgressUpdatedEventArgs> BufferingProgressUpdated;
		public event EventHandler PlaybackStopped;
		public event EventHandler PlaybackPaused;
		public event EventHandler ErrorOccurred;

		public async Task<bool> Start()
		{
			_cancelToStart = false;
			if (!HasSource)
				return false;

			if (_player.State == PlayerState.Idle)
			{
				await Prepare();
			}

			if (_cancelToStart)
				return false;

			try
			{
				_player.Start();
			}
			catch (Exception e)
			{
				Log.Error($"Error On Start : {e.Message}");
				return false;
			}
			PlaybackStarted?.Invoke(this, EventArgs.Empty);
			return true;
		}

		public void Pause()
		{
			try
			{
				_player.Pause();
				PlaybackPaused?.Invoke(this, EventArgs.Empty);
			}
			catch (Exception e)
			{
				Log.Error($"Error on Pause : {e.Message}");
			}
		}

		public void Stop()
		{
			_cancelToStart = true;
			var unusedTask = ChangeToIdleState();
			PlaybackStopped?.Invoke(this, EventArgs.Empty);
		}

		public void SetDisplay(IVideoOutput output)
		{
			VideoOutput = output;
		}

		public async Task<int> Seek(int ms)
		{
			try
			{
				await _player.SetPlayPositionAsync(ms, false);
			}
			catch (Exception e)
			{
				Log.Error($"Fail to seek : {e.Message}");
			}
			return Position;
		}

		public void SetSource(MediaSource source)
		{
			_source = source;
			if (HasSource && AutoPlay)
			{
				var unusedTask = Start();
			}
		}

		public async Task<Stream> GetAlbumArts()
		{
			if (_player.State == PlayerState.Idle)
			{
				if (_tcsForStreamInfo == null || _tcsForStreamInfo.Task.IsCompleted)
				{
					_tcsForStreamInfo = new TaskCompletionSource<bool>();
				}
				await _tcsForStreamInfo.Task;
			}
			await TaskPrepare;

			var imageData = _player.StreamInfo.GetAlbumArt();
			if (imageData == null)
				return null;
			return new MemoryStream(imageData);
		}

		public async Task<IDictionary<string, string>> GetMetadata()
		{
			if (_player.State == PlayerState.Idle)
			{
				if (_tcsForStreamInfo == null || _tcsForStreamInfo.Task.IsCompleted)
				{
					_tcsForStreamInfo = new TaskCompletionSource<bool>();
				}
				await _tcsForStreamInfo.Task;
			}
			await TaskPrepare;

			Dictionary<string, string> metadata = new Dictionary<string, string>
			{
				[nameof(StreamMetadataKey.Album)] = _player.StreamInfo.GetMetadata(StreamMetadataKey.Album),
				[nameof(StreamMetadataKey.Artist)] = _player.StreamInfo.GetMetadata(StreamMetadataKey.Artist),
				[nameof(StreamMetadataKey.Author)] = _player.StreamInfo.GetMetadata(StreamMetadataKey.Author),
				[nameof(StreamMetadataKey.Genre)] = _player.StreamInfo.GetMetadata(StreamMetadataKey.Genre),
				[nameof(StreamMetadataKey.Title)] = _player.StreamInfo.GetMetadata(StreamMetadataKey.Title),
				[nameof(StreamMetadataKey.Year)] = _player.StreamInfo.GetMetadata(StreamMetadataKey.Year)
			};
			return metadata;
		}

		public async Task<Size> GetVideoSize()
		{
			if (_player.State == PlayerState.Idle)
			{
				if (_tcsForStreamInfo == null || _tcsForStreamInfo.Task.IsCompleted)
				{
					_tcsForStreamInfo = new TaskCompletionSource<bool>();
				}
				await _tcsForStreamInfo.Task;
			}
			await TaskPrepare;

			var videoSize = _player.StreamInfo.GetVideoProperties().Size;
			return new Size(videoSize.Width, videoSize.Height);
		}

		public View GetEmbeddingControlView(IMediaPlayer player)
		{
			return new EmbeddingControls
			{
				BindingContext = player
			};
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
				_player.PlaybackCompleted -= OnPlaybackCompleted;
				_player.BufferingProgressChanged -= OnBufferingProgressChanged;
				_player.ErrorOccurred -= OnErrorOccurred;
				_player.Dispose();
			}

			_disposed = true;
		}

		void ApplyDisplay()
		{
			if (VideoOutput == null)
			{
				_player.Display = null;
			}
			else
			{
				var renderer = Platform.GetRenderer(TargetView);
				if (renderer is IMediaViewProvider provider && provider.GetMediaView() != null)
				{
					try
					{
						Display display = new Display(provider.GetMediaView());
						_player.Display = display;
						_player.DisplaySettings.Mode = _aspectMode.ToNative();
					}
					catch
					{
						Log.Error("Error on MediaView");
					}
				}
			}
		}

		void ApplySource()
		{
			if (_source == null)
			{
				return;
			}

			if (_source is UriMediaSource uriSource)
			{
				var uri = uriSource.Uri;
				_player.SetSource(new MediaUriSource(uri.IsFile ? uri.LocalPath : uri.AbsoluteUri));
			}
			else if (_source is FileMediaSource fileSource)
			{
				_player.SetSource(new MediaUriSource(ResourcePath.GetPath(fileSource.File)));
			}
		}

		async void OnTargetViewPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Renderer")
			{
				if (Platform.GetRenderer(sender as BindableObject) != null && HasSource && AutoPlay)
				{
					await Start();
				}
				else if (Platform.GetRenderer(sender as BindableObject) == null && AutoStop)
				{
					Stop();
				}
			}
		}

		async Task Prepare()
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			var prevTask = TaskPrepare;
			TaskPrepare = tcs.Task;
			await prevTask;

			if (_player.State == PlayerState.Ready)
				return;

			ApplyDisplay();
			ApplySource();

			try
			{
				await _player.PrepareAsync();
				UpdateStreamInfo?.Invoke(this, EventArgs.Empty);
				_tcsForStreamInfo?.TrySetResult(true);
			}
			catch (Exception e)
			{
				Log.Error($"Error on prepare : {e.Message}");
			}
			tcs.SetResult(true);
		}

		async void ApplyAspectMode()
		{
			if (_player.State == PlayerState.Preparing)
			{
				await TaskPrepare;
			}
			_player.DisplaySettings.Mode = AspectMode.ToNative();
		}

		void OnBufferingProgressChanged(object sender, BufferingProgressChangedEventArgs e)
		{
			BufferingProgressUpdated?.Invoke(this, new BufferingProgressUpdatedEventArgs { Progress = e.Percent / 100.0 });
		}

		void OnPlaybackCompleted(object sender, EventArgs e)
		{
			PlaybackCompleted?.Invoke(this, EventArgs.Empty);
		}

		void OnErrorOccurred(object sender, PlayerErrorOccurredEventArgs e)
		{
			Log.Error($"Playback Error Occurred (code:{e.Error})-{e.ToString()}");
			ErrorOccurred?.Invoke(this, EventArgs.Empty);
		}

		async Task ChangeToIdleState()
		{
			switch (_player.State)
			{
				case PlayerState.Playing:
				case PlayerState.Paused:
					_player.Stop();
					_player.Unprepare();
					break;
				case PlayerState.Ready:
					_player.Unprepare();
					break;
				case PlayerState.Preparing:
					await TaskPrepare;
					_player.Unprepare();
					break;
			}
		}
	}
}
