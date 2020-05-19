using System;
using System.Globalization;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;
using Xamarin.Forms.Platform.Tizen.Native;
using ElmSharp;
using Tizen.Multimedia;
using XForms = Xamarin.Forms.Forms;

namespace Xamarin.Forms.Platform.Tizen
{
	public class MediaElementRenderer : ViewRenderer<MediaElement, LayoutCanvas>, IMediaViewProvider, IVideoOutput
	{
		MediaPlayer _player;
		MediaView _mediaView;
		View _controller;
		EvasObject _nativeController;

		IMediaElementController Controller => Element as IMediaElementController;

		VisualElement IVideoOutput.MediaView => Element;

		View IVideoOutput.Controller
		{
			get
			{
				return _controller;
			}
			set
			{
				if (_controller != null)
				{
					_controller.Parent = null;
					Control.Children.Remove(_nativeController);
					_nativeController.Unrealize();
				}

				_controller = value;

				if (_controller != null)
				{
					_controller.Parent = Element;
					_nativeController = Platform.GetOrCreateRenderer(_controller).NativeView;
					Control.Children.Add(_nativeController);
				}
			}
		}

		VideoOuputType IVideoOutput.OuputType => VideoOuputType.Buffer;

		MediaView IMediaViewProvider.GetMediaView()
		{
			return _mediaView;
		}

		public MediaElementRenderer()
		{
			MediaElement.VerifyMediaElementFlagEnabled(nameof(MediaElementRenderer));
		}

		protected override void OnElementChanged(ElementChangedEventArgs<MediaElement> e)
		{
			if (e.OldElement != null)
			{
				e.OldElement.SeekRequested -= OnSeekRequested;
				e.OldElement.StateRequested -= OnStateRequested;
				e.OldElement.PositionRequested -= OnPositionRequested;
			}

			if (Control == null)
			{
				SetNativeControl(new LayoutCanvas(XForms.NativeParent));
				_mediaView = new MediaView(XForms.NativeParent);
				Control.LayoutUpdated += (s, evt) => OnLayout();
				Control.Children.Add(_mediaView);
				Control.AllowFocus(true);

				_player = new MediaPlayer()
				{
					VideoOutput = this
				};
				_player.PlaybackStarted += OnPlaybackStarted;
				_player.PlaybackPaused += OnPlaybackPaused;
				_player.PlaybackStopped += OnPlaybackStopped;
				_player.PlaybackCompleted += OnPlaybackCompleted;
				_player.BufferingProgressUpdated += OnBufferingProgressUpdated;
				_player.ErrorOccurred += OnErrorOccurred;
				_player.MediaPrepared += OnMediaPrepared;
				_player.BindingContext = Element;
				_player.SetBinding(MediaPlayer.SourceProperty, "Source");
				_player.SetBinding(MediaPlayer.UsesEmbeddingControlsProperty, "ShowsPlaybackControls");
				_player.SetBinding(MediaPlayer.AutoPlayProperty, "AutoPlay");
				_player.SetBinding(MediaPlayer.VolumeProperty, "Volume");
				_player.SetBinding(MediaPlayer.IsLoopingProperty, "IsLooping");
				_player.SetBinding(MediaPlayer.AspectModeProperty, new Binding
				{
					Path = "Aspect",
					Converter = new AspectToDisplayAspectModeConverter()
				});
				BindableObject.SetInheritedBindingContext(_player, Element.BindingContext);

				Element.SeekRequested += OnSeekRequested;
				Element.StateRequested += OnStateRequested;
				Element.PositionRequested += OnPositionRequested;
			}
			base.OnElementChanged(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Element.SeekRequested -= OnSeekRequested;
				Element.StateRequested -= OnStateRequested;
				Element.PositionRequested -= OnPositionRequested;
				_mediaView?.Unrealize();
				if (_player != null)
				{
					_player.PlaybackStarted -= OnPlaybackStarted;
					_player.PlaybackPaused -= OnPlaybackPaused;
					_player.PlaybackStopped -= OnPlaybackStopped;
					_player.PlaybackCompleted -= OnPlaybackCompleted;
					_player.BufferingProgressUpdated -= OnBufferingProgressUpdated;
					_player.ErrorOccurred -= OnErrorOccurred;
					_player.MediaPrepared -= OnMediaPrepared;
					_player.Dispose();
				}
				if (_controller != null )
				{
					_controller.Parent = null;
					Platform.SetRenderer(_controller, null);
				}
				_nativeController?.Unrealize();
				Control?.Unrealize();
			}
			base.Dispose(disposing);
		}

		protected void OnLayout()
		{
			_mediaView.Geometry = Control.Geometry;
			_controller?.Layout(Element.Bounds);
			if (_nativeController != null)
				_nativeController.Geometry = Control.Geometry;
		}

		protected void OnSeekRequested(object sender, SeekRequested e)
		{
			_player.Seek((int)e.Position.TotalMilliseconds);
		}

		protected void OnStateRequested(object sender, StateRequested e)
		{
			switch (e.State)
			{
				case MediaElementState.Playing:
					_player.Start();
					break;
				case MediaElementState.Paused:
					_player.Pause();
					break;
				case MediaElementState.Stopped:
					_player.Stop();
					break;
			}
		}

		protected void OnPositionRequested(object sender, EventArgs e)
		{
			Controller.Position = TimeSpan.FromMilliseconds(_player.Position);
		}

		protected void OnPlaybackStarted(object sender, EventArgs e)
		{
			Controller.CurrentState = MediaElementState.Playing;
		}

		protected void OnPlaybackPaused(object sender, EventArgs e)
		{
			Controller.CurrentState = MediaElementState.Paused;
		}

		protected void OnPlaybackStopped(object sender, EventArgs e)
		{
			Controller.CurrentState = MediaElementState.Stopped;
		}

		protected void OnPlaybackCompleted(object sender, EventArgs e)
		{
			Controller.OnMediaEnded();
		}

		protected void OnBufferingProgressUpdated(object sender, BufferingProgressUpdatedEventArgs e)
		{
			if (e.Progress == 1.0)
			{
				switch (_player.State)
				{
					case PlaybackState.Paused:
						Controller.CurrentState = MediaElementState.Paused;
						break;
					case PlaybackState.Playing:
						Controller.CurrentState = MediaElementState.Playing;
						break;
					case PlaybackState.Stopped:
						Controller.CurrentState = MediaElementState.Stopped;
						break;
				}
			}
			else if (Controller.CurrentState != MediaElementState.Buffering && e.Progress >= 0)
			{
				Controller.CurrentState = MediaElementState.Buffering;
			}
			Controller.BufferingProgress = e.Progress;
		}

		protected void OnErrorOccurred(object sender, EventArgs e)
		{
			Controller.OnMediaFailed();
		}

		protected async void OnMediaPrepared(object sender, EventArgs e)
		{
			Controller.OnMediaOpened();
			Controller.Duration = TimeSpan.FromMilliseconds(_player.Duration);
			var videoSize = await _player.GetVideoSize();
			Controller.VideoWidth = (int)videoSize.Width;
			Controller.VideoHeight = (int)videoSize.Height;
		}
	}

	public interface IMediaViewProvider
	{
		MediaView GetMediaView();
	}

	public class AspectToDisplayAspectModeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((Aspect)value).ToDisplayAspectMode();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
