using AVFoundation;
using AVKit;
using CoreGraphics;
using CoreMedia;
using Foundation;
using System;
using System.IO;
using UIKit;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.iOS
{
	public sealed class MediaElementRenderer : ViewRenderer<MediaElement, UIView>
	{
		IMediaElementController Controller => Element as IMediaElementController;
		
		AVPlayerViewController _avPlayerViewController = new AVPlayerViewController();
		NSObject _playedToEndObserver;
		NSObject _statusObserver;
		NSObject _rateObserver;
				
		bool _idleTimerDisabled = false;

		[Internals.Preserve(Conditional = true)]
		public MediaElementRenderer()
		{
			Xamarin.Forms.MediaElement.VerifyMediaElementFlagEnabled(nameof(MediaElementRenderer));

			_playedToEndObserver = NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, PlayedToEnd);
		}

		void SetKeepScreenOn(bool value)
		{
			if (value)
			{
				if (!UIApplication.SharedApplication.IdleTimerDisabled)
				{
					_idleTimerDisabled = true;
					UIApplication.SharedApplication.IdleTimerDisabled = true;
				}
			}
			else if (_idleTimerDisabled)
			{
				_idleTimerDisabled = false;
				UIApplication.SharedApplication.IdleTimerDisabled = false;
			}
		}

		void UpdateSource()
		{
			if (Element.Source != null)
			{
				AVAsset asset = null;
				
				var uriSource = Element.Source as UriMediaSource;
				if (uriSource != null)
				{
					if (uriSource.Uri.Scheme == "ms-appx")
					{
						if (uriSource.Uri.LocalPath.Length <= 1)
							return;

						// used for a file embedded in the application package
						asset = AVAsset.FromUrl(NSUrl.FromFilename(uriSource.Uri.LocalPath.Substring(1)));
					}
					else if (uriSource.Uri.Scheme == "ms-appdata")
					{
						string filePath = Platform.ResolveMsAppDataUri(uriSource.Uri);

						if (string.IsNullOrEmpty(filePath))
							throw new ArgumentException("Invalid Uri", "Source");

						asset = AVAsset.FromUrl(NSUrl.FromFilename(filePath));
					}
					else
					{
						asset = AVUrlAsset.Create(NSUrl.FromString(uriSource.Uri.AbsoluteUri));
					}
				}
				else
				{
					var fileSource = Element.Source as FileMediaSource;
					if (fileSource != null)
					{
						asset = AVAsset.FromUrl(NSUrl.FromFilename(fileSource.File));
					}
				}

				var item = new AVPlayerItem(asset);
				RemoveStatusObserver();

				_statusObserver = (NSObject)item.AddObserver("status", NSKeyValueObservingOptions.New, ObserveStatus);
				
				
				if (_avPlayerViewController.Player != null)
				{
					_avPlayerViewController.Player.ReplaceCurrentItemWithPlayerItem(item);
				}
				else
				{
					_avPlayerViewController.Player = new AVPlayer(item);
					_rateObserver = (NSObject)_avPlayerViewController.Player.AddObserver("rate", NSKeyValueObservingOptions.New, ObserveRate);
				}
				
				if (Element.AutoPlay)
					Play();
			}
			else
			{
				if (Element.CurrentState == MediaElementState.Playing || Element.CurrentState == MediaElementState.Buffering)
				{
					_avPlayerViewController.Player.Pause();
					Controller.CurrentState = MediaElementState.Stopped;
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if(_playedToEndObserver != null)
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver(_playedToEndObserver);
				_playedToEndObserver = null;
			}

			if(_rateObserver != null)
			{
				_rateObserver.Dispose();
				_rateObserver = null;
			}

			RemoveStatusObserver();

			_avPlayerViewController?.Player?.Pause();
			_avPlayerViewController?.Player?.ReplaceCurrentItemWithPlayerItem(null);

			base.Dispose(disposing);
		}

		void RemoveStatusObserver()
		{
			if (_statusObserver != null)
			{
				try
				{
					_avPlayerViewController?.Player?.CurrentItem?.RemoveObserver(_statusObserver, "status");
				}
				finally
				{

					_statusObserver = null;
				}
			}
		}

		void ObserveRate(NSObservedChange e)
		{
			if (Controller is object)
			{
				switch (_avPlayerViewController.Player.Rate)
				{
					case 0.0f:
						Controller.CurrentState = MediaElementState.Paused;
						break;

					case 1.0f:
						Controller.CurrentState = MediaElementState.Playing;
						break;
				}

				Controller.Position = Position;
			}
		}

		void ObserveStatus(NSObservedChange e)
		{
			Controller.Volume = _avPlayerViewController.Player.Volume;

			switch (_avPlayerViewController.Player.Status)
			{
				case AVPlayerStatus.Failed:
					Controller.OnMediaFailed();
					break;

				case AVPlayerStatus.ReadyToPlay:
					Controller.Duration = TimeSpan.FromSeconds(_avPlayerViewController.Player.CurrentItem.Duration.Seconds);
					Controller.VideoHeight = (int)_avPlayerViewController.Player.CurrentItem.Asset.NaturalSize.Height;
					Controller.VideoWidth = (int)_avPlayerViewController.Player.CurrentItem.Asset.NaturalSize.Width;
					Controller.OnMediaOpened();
					Controller.Position = Position;
					break;
			}
		}

		TimeSpan Position
		{
			get
			{
				if (_avPlayerViewController.Player.CurrentTime.IsInvalid)
					return TimeSpan.Zero;

				return TimeSpan.FromSeconds(_avPlayerViewController.Player.CurrentTime.Seconds);
			}
		}

		void PlayedToEnd(NSNotification notification)
		{
			if (Element.IsLooping)
			{
				_avPlayerViewController.Player.Seek(CMTime.Zero);
				Controller.Position = Position;
				_avPlayerViewController.Player.Play();
			}
			else
			{
				SetKeepScreenOn(false);
				Controller.Position = Position;

				try
				{
					Device.BeginInvokeOnMainThread(Controller.OnMediaEnded);
				}
				catch { }
			}
		}
		
		protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(MediaElement.Aspect):
					_avPlayerViewController.VideoGravity = AspectToGravity(Element.Aspect);
					break;

				case nameof(MediaElement.KeepScreenOn):
					if (!Element.KeepScreenOn)
					{
						SetKeepScreenOn(false);
					}
					else if(Element.CurrentState == MediaElementState.Playing)
					{
						// only toggle this on if property is set while video is already running
						SetKeepScreenOn(true);
					}
					break;

				case nameof(MediaElement.ShowsPlaybackControls):
					_avPlayerViewController.ShowsPlaybackControls = Element.ShowsPlaybackControls;
					break;

				case nameof(MediaElement.Source):
					UpdateSource();
					break;

				case nameof(MediaElement.Volume):
					_avPlayerViewController.Player.Volume = (float)Element.Volume;
					break;
			}
		}

		void MediaElementSeekRequested(object sender, SeekRequested e)
		{
			if (_avPlayerViewController.Player.Status != AVPlayerStatus.ReadyToPlay || _avPlayerViewController.Player.CurrentItem == null)
				return;

			NSValue[] ranges = _avPlayerViewController.Player.CurrentItem.SeekableTimeRanges;
			CMTime seekTo = new CMTime(Convert.ToInt64(e.Position.TotalMilliseconds), 1000);
			foreach (NSValue v in ranges)
			{
				if (seekTo >= v.CMTimeRangeValue.Start && seekTo < (v.CMTimeRangeValue.Start + v.CMTimeRangeValue.Duration))
				{
					_avPlayerViewController.Player.Seek(seekTo, SeekComplete);
					break;
				}
			}
		}

		void Play()
		{
			var audioSession = AVAudioSession.SharedInstance();
			NSError err = audioSession.SetCategory(AVAudioSession.CategoryPlayback);
			if (!(err is null))
				Log.Warning("MediaElement", "Failed to set AVAudioSession Category {0}", err.Code);

			audioSession.SetMode(AVAudioSession.ModeMoviePlayback, out err);
			if (!(err is null))
				Log.Warning("MediaElement", "Failed to set AVAudioSession Mode {0}", err.Code);
			
			err = audioSession.SetActive(true);
			if (!(err is null))
				Log.Warning("MediaElement", "Failed to set AVAudioSession Active {0}", err.Code);

			if (_avPlayerViewController.Player != null)
			{
				_avPlayerViewController.Player.Play();
				Controller.CurrentState = MediaElementState.Playing;
			}

			if (Element.KeepScreenOn)
			{
				SetKeepScreenOn(true);
			}
		}

		void MediaElementStateRequested(object sender, StateRequested e)
		{
			MediaElementVolumeRequested(this, EventArgs.Empty);

			switch (e.State)
			{
				case MediaElementState.Playing:
					Play();
					break;

				case MediaElementState.Paused:
					if (Element.KeepScreenOn)
					{
						SetKeepScreenOn(false);
					}

					if (_avPlayerViewController.Player != null)
					{
						_avPlayerViewController.Player.Pause();
						Controller.CurrentState = MediaElementState.Paused;
					}
					break;

				case MediaElementState.Stopped:
					if (Element.KeepScreenOn)
					{
						SetKeepScreenOn(false);
					}
					//ios has no stop...
					_avPlayerViewController?.Player.Pause();
					_avPlayerViewController?.Player.Seek(CMTime.Zero);
					Controller.CurrentState = MediaElementState.Stopped;

					NSError err = AVAudioSession.SharedInstance().SetActive(false);
					if (!(err is null))
						Log.Warning("MediaElement", "Failed to set AVAudioSession Inactive {0}", err.Code);
					break;
			}

			Controller.Position = Position;
		}

		static AVLayerVideoGravity AspectToGravity(Aspect aspect)
		{
			switch (aspect)
			{
				case Aspect.Fill:
					return AVLayerVideoGravity.Resize;

				case Aspect.AspectFill:
					return AVLayerVideoGravity.ResizeAspectFill;

				default:
					return AVLayerVideoGravity.ResizeAspect;
			}
		}

		void SeekComplete(bool finished)
		{
			if (finished)
			{
				Controller.OnSeekCompleted();
			}
		}

		private void MediaElementVolumeRequested(object sender, EventArgs e)
		{
			Controller.Volume = _avPlayerViewController.Player.Volume;
		}

		void MediaElementPositionRequested(object sender, EventArgs e)
		{
			Controller.Position = Position;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<MediaElement> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;
				e.OldElement.SeekRequested -= MediaElementSeekRequested;
				e.OldElement.StateRequested -= MediaElementStateRequested;
				e.OldElement.PositionRequested -= MediaElementPositionRequested;
				e.OldElement.VolumeRequested -= MediaElementVolumeRequested;

				if (_playedToEndObserver != null)
				{
					NSNotificationCenter.DefaultCenter.RemoveObserver(_playedToEndObserver);
					_playedToEndObserver = null;
				}

				// stop video if playing
				if (_avPlayerViewController?.Player?.CurrentItem != null)
				{
					RemoveStatusObserver();

					_avPlayerViewController.Player.Pause();
					_avPlayerViewController.Player.Seek(CMTime.Zero);
					_avPlayerViewController.Player.ReplaceCurrentItemWithPlayerItem(null);
					AVAudioSession.SharedInstance().SetActive(false);
				}
			}

			if (e.NewElement != null)
			{
				SetNativeControl(_avPlayerViewController.View);

				Element.PropertyChanged += OnElementPropertyChanged;
				Element.SeekRequested += MediaElementSeekRequested;
				Element.StateRequested += MediaElementStateRequested;
				Element.PositionRequested += MediaElementPositionRequested;
				Element.VolumeRequested += MediaElementVolumeRequested;

				_avPlayerViewController.ShowsPlaybackControls = Element.ShowsPlaybackControls;
				_avPlayerViewController.VideoGravity = AspectToGravity(Element.Aspect);
				if (Element.KeepScreenOn)
				{
					SetKeepScreenOn(true);
				}

				_playedToEndObserver = NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, PlayedToEnd);

				UpdateBackgroundColor();
				UpdateSource();
			}
		}

		void UpdateBackgroundColor()
		{
			BackgroundColor = Element.BackgroundColor.ToUIColor();
		}
	}
}
