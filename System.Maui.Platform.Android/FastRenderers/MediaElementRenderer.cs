using Android.Content;
using Android.Media;
using Android.Views;
using Android.Widget;
using System;
using System.ComponentModel;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android.FastRenderers;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public sealed class MediaElementRenderer : FrameLayout, IVisualElementRenderer, IViewRenderer, IEffectControlProvider, MediaPlayer.IOnCompletionListener, MediaPlayer.IOnInfoListener, MediaPlayer.IOnPreparedListener, MediaPlayer.IOnErrorListener
	{
		bool _isDisposed;
		int? _defaultLabelFor;
		MediaElement MediaElement { get; set; }
		IMediaElementController Controller => MediaElement as IMediaElementController;

		readonly AutomationPropertiesProvider _automationPropertiesProvider;
		readonly EffectControlProvider _effectControlProvider;
		VisualElementTracker _tracker;

		MediaController _controller;
		MediaPlayer _mediaPlayer;
		FormsVideoView _view;
		
		public MediaElementRenderer(Context context) : base(context)
		{
			Xamarin.Forms.MediaElement.VerifyMediaElementFlagEnabled(nameof(MediaElementRenderer));
			_automationPropertiesProvider = new AutomationPropertiesProvider(this);
			_effectControlProvider = new EffectControlProvider(this);

			_view = new FormsVideoView(Context);
			_view.SetZOrderMediaOverlay(true);
			_view.SetOnCompletionListener(this);
			_view.SetOnInfoListener(this);
			_view.SetOnPreparedListener(this);
			_view.SetOnErrorListener(this);
			_view.MetadataRetrieved += MetadataRetrieved;

			AddView(_view, -1, -1);

			_controller = new MediaController(Context);
			_controller.SetAnchorView(this);
			_view.SetMediaController(_controller);
		}

		public VisualElement Element => MediaElement;

		VisualElementTracker IVisualElementRenderer.Tracker => _tracker;

		ViewGroup IVisualElementRenderer.ViewGroup => null;

		AView IVisualElementRenderer.View => this;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			AView view = this;
			view.Measure(widthConstraint, heightConstraint);

			return new SizeRequest(new Size(MeasuredWidth, MeasuredHeight), new Size());
		}

		void IViewRenderer.MeasureExactly()
		{
			ViewRenderer.MeasureExactly(this, Element, Context);
		}

		void UnsubscribeFromEvents(MediaElement element)
		{
			if (element == null)
				return;

			element.PropertyChanged -= OnElementPropertyChanged;
			element.SeekRequested -= SeekRequested;
			element.StateRequested -= StateRequested;
			element.PositionRequested -= OnPositionRequested;

		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			if (element is null)
				throw new ArgumentNullException(nameof(element));

			if (!(element is MediaElement))
				throw new ArgumentException($"{nameof(element)} must be of type {nameof(MediaElement)}");

			MediaElement oldElement = MediaElement;
			MediaElement = (MediaElement)element;

			Performance.Start(out string reference);

			if (oldElement != null)
			{
				UnsubscribeFromEvents(oldElement);
			}

			Color currentColor = oldElement?.BackgroundColor ?? Color.Default;
			if (element.BackgroundColor != currentColor)
			{
				UpdateBackgroundColor();
			}

			if (MediaElement != null)
			{
				MediaElement.PropertyChanged += OnElementPropertyChanged;
				MediaElement.SeekRequested += SeekRequested;
				MediaElement.StateRequested += StateRequested;
				MediaElement.PositionRequested += OnPositionRequested;
			}

			if (_tracker is null)
			{
				// Can't set up the tracker in the constructor because it access the Element (for now)
				SetTracker(new VisualElementTracker(this));
			}

			OnElementChanged(new ElementChangedEventArgs<MediaElement>(oldElement as MediaElement, MediaElement));

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);

			Performance.Stop(reference);
		}

		void StateRequested(object sender, StateRequested e)
		{
			if (_view == null)
				return;

			switch (e.State)
			{
				case MediaElementState.Playing:
					_view.Start();
					Controller.CurrentState = _view.IsPlaying ? MediaElementState.Playing : MediaElementState.Stopped;
					break;

				case MediaElementState.Paused:
					if (_view.CanPause())
					{
						_view.Pause();
						Controller.CurrentState = MediaElementState.Paused;
					}
					break;

				case MediaElementState.Stopped:
					_view.Pause();
					_view.SeekTo(0);

					Controller.CurrentState = _view.IsPlaying ? MediaElementState.Playing : MediaElementState.Stopped;
					break;
			}

			UpdateLayoutParameters();
			Controller.Position = _view.Position;
		}

		void OnPositionRequested(object sender, EventArgs e)
		{
			if (_view == null)
				return;

			Controller.Position = _view.Position;
		}

		void SeekRequested(object sender, SeekRequested e)
		{
			if (_view == null)
				return;

			Controller.Position = _view.Position;
		}

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor is null)
			{
				_defaultLabelFor = LabelFor;
			}

			LabelFor = (int)(id ?? _defaultLabelFor);
		}

		void SetTracker(VisualElementTracker tracker)
		{
			_tracker = tracker;
		}

		void UpdateBackgroundColor()
		{
			SetBackgroundColor(Element.BackgroundColor.ToAndroid());
		}

		void IVisualElementRenderer.UpdateLayout() => _tracker?.UpdateLayout();

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
			{
				return;
			}

			_isDisposed = true;

			ReleaseControl();

			if (disposing)
			{
				SetOnClickListener(null);
				SetOnTouchListener(null);

				_automationPropertiesProvider?.Dispose();
				_tracker?.Dispose();

				if (Element != null)
				{
					UnsubscribeFromEvents(Element as MediaElement);

					if (Platform.GetRenderer(Element) == this)
						Element.ClearValue(Platform.RendererProperty);
				}
			}

			base.Dispose(disposing);
		}

		//todo: make virtual when unsealed
		void OnElementChanged(ElementChangedEventArgs<MediaElement> e)
		{
			if (e.OldElement != null)
			{
				
			}

			if (e.NewElement != null)
			{
				this.EnsureId();

				UpdateKeepScreenOn();
				UpdateLayoutParameters();
				UpdateShowPlaybackControls();
				UpdateSource();
				UpdateBackgroundColor();

				ElevationHelper.SetElevation(this, e.NewElement);
			}

			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));
		}

		void MetadataRetrieved(object sender, EventArgs e)
		{
			if (_view == null)
				return;

			Controller.Duration = _view.DurationTimeSpan;
			Controller.VideoHeight = _view.VideoHeight;
			Controller.VideoWidth = _view.VideoWidth;

			Device.BeginInvokeOnMainThread(UpdateLayoutParameters);
		}

		//todo: make virtual when unsealed
		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(MediaElement.Aspect):
					UpdateLayoutParameters();
					break;

				case nameof(MediaElement.IsLooping):
					if (_mediaPlayer != null)
					{
						_mediaPlayer.Looping = MediaElement.IsLooping;
					}
					break;

				case nameof(MediaElement.KeepScreenOn):
					UpdateKeepScreenOn();
					break;

				case nameof(MediaElement.ShowsPlaybackControls):
					UpdateShowPlaybackControls();
					break;

				case nameof(MediaElement.Source):
					UpdateSource();
					break;

				case nameof(MediaElement.Volume):
					_mediaPlayer?.SetVolume((float)MediaElement.Volume, (float)MediaElement.Volume);
					break;
			}

			ElementPropertyChanged?.Invoke(this, e);
		}

		public void RegisterEffect(Effect effect)
		{
			_effectControlProvider.RegisterEffect(effect);
		}

		void UpdateKeepScreenOn()
		{
			if (_view == null)
				return;

			_view.KeepScreenOn = MediaElement.KeepScreenOn;
		}

		void UpdateShowPlaybackControls()
		{
			if (_controller == null)
				return;

			_controller.Visibility = MediaElement.ShowsPlaybackControls ? ViewStates.Visible : ViewStates.Gone;
		}

		void UpdateSource()
		{
			if (_view == null)
				return;

			if (MediaElement.Source != null)
			{
				if (MediaElement.Source is UriMediaSource uriSource)
				{
					if (uriSource.Uri.Scheme == "ms-appx")
					{
						if (uriSource.Uri.LocalPath.Length <= 1)
							return;

						// video resources should be in the raw folder with Build Action set to AndroidResource
						string uri = "android.resource://" + Context.PackageName + "/raw/" + uriSource.Uri.LocalPath.Substring(1, uriSource.Uri.LocalPath.LastIndexOf('.') - 1).ToLower();
						_view.SetVideoURI(global::Android.Net.Uri.Parse(uri));
					}
					else if (uriSource.Uri.Scheme == "ms-appdata")
					{
						string filePath = Platform.ResolveMsAppDataUri(uriSource.Uri);

						if (string.IsNullOrEmpty(filePath))
							throw new ArgumentException("Invalid Uri", "Source");

						_view.SetVideoPath(filePath);

					}
					else
					{
						if (uriSource.Uri.IsFile)
						{
							_view.SetVideoPath(uriSource.Uri.AbsolutePath);
						}
						else
						{
							_view.SetVideoURI(global::Android.Net.Uri.Parse(uriSource.Uri.AbsoluteUri));
						}
					}
				}
				else if (MediaElement.Source is FileMediaSource fileSource)
				{
					_view.SetVideoPath(fileSource.File);
				}

				if (MediaElement.AutoPlay)
				{
					_view.Start();
					Controller.CurrentState = _view.IsPlaying ? MediaElementState.Playing : MediaElementState.Stopped;
				}

			}
			else if (_view.IsPlaying)
			{
				_view.StopPlayback();
				Controller.CurrentState = MediaElementState.Stopped;
			}
		}

		void MediaPlayer.IOnCompletionListener.OnCompletion(MediaPlayer mp)
		{
			if (Controller == null)
				return;

			Controller.Position = TimeSpan.FromMilliseconds(_mediaPlayer.CurrentPosition);
			Controller.OnMediaEnded();
		}

		void MediaPlayer.IOnPreparedListener.OnPrepared(MediaPlayer mp)
		{
			if (Controller == null)
				return;

			Controller.OnMediaOpened();

			UpdateLayoutParameters();

			_mediaPlayer = mp;
			mp.Looping = MediaElement.IsLooping;
			mp.SeekTo(0);

			if (MediaElement.AutoPlay)
			{
				_mediaPlayer.Start();
				Controller.CurrentState = MediaElementState.Playing;
			}
			else
			{
				Controller.CurrentState = MediaElementState.Paused;
			}
		}

		void UpdateLayoutParameters()
		{
			if (_view == null)
				return;

			if (_view.VideoWidth == 0 || _view.VideoHeight == 0)
			{
				_view.LayoutParameters = new FrameLayout.LayoutParams(Width, Height, GravityFlags.Fill);
				return;
			}

			float ratio = (float)_view.VideoWidth / (float)_view.VideoHeight;
			float controlRatio = (float)Width / Height;

			switch (MediaElement.Aspect)
			{
				case Aspect.Fill:
					// TODO: this doesn't stretch like other platforms...
					_view.LayoutParameters = new FrameLayout.LayoutParams(Width, Height, GravityFlags.Fill) { LeftMargin = 0, RightMargin = 0, TopMargin = 0, BottomMargin = 0 };
					break;

				case Aspect.AspectFit:
					if (ratio > controlRatio)
					{
						int requiredHeight = (int)(Width / ratio);
						int vertMargin = (Height - requiredHeight) / 2;
						_view.LayoutParameters = new FrameLayout.LayoutParams(Width, requiredHeight, GravityFlags.FillHorizontal | GravityFlags.CenterVertical) { LeftMargin = 0, RightMargin = 0, TopMargin = vertMargin, BottomMargin = vertMargin };
					}
					else
					{
						int requiredWidth = (int)(Height * ratio);
						int horizMargin = (Width - requiredWidth) / 2;
						_view.LayoutParameters = new FrameLayout.LayoutParams(requiredWidth, Height, GravityFlags.CenterHorizontal | GravityFlags.FillVertical) { LeftMargin = horizMargin, RightMargin = horizMargin, TopMargin = 0, BottomMargin = 0 };
					}
					break;

				case Aspect.AspectFill:
					if (ratio > controlRatio)
					{
						int requiredWidth = (int)(Height * ratio);
						int horizMargin = (Width - requiredWidth) / 2;
						_view.LayoutParameters = new FrameLayout.LayoutParams((int)(Height * ratio), Height, GravityFlags.CenterHorizontal | GravityFlags.FillVertical) { TopMargin = 0, BottomMargin = 0, LeftMargin = horizMargin, RightMargin = horizMargin };
					}
					else
					{
						int requiredHeight = (int)(Width / ratio);
						int vertMargin = (Height - requiredHeight) / 2;
						_view.LayoutParameters = new FrameLayout.LayoutParams(Width, requiredHeight, GravityFlags.FillHorizontal | GravityFlags.CenterVertical) { LeftMargin = 0, RightMargin = 0, TopMargin = vertMargin, BottomMargin = vertMargin };
					}

					break;
			}
		}

		void ReleaseControl()
		{
			if (_view != null)
			{
				_view.MetadataRetrieved -= MetadataRetrieved;
				RemoveView(_view);
				_view.SetOnPreparedListener(null);
				_view.SetOnCompletionListener(null);
				_view.Dispose();
				_view = null;
			}

			if (_controller != null)
			{
				_controller.Dispose();
				_controller = null;
			}

			if (_mediaPlayer != null)
			{
				_mediaPlayer.Dispose();
				_mediaPlayer = null;
			}
		}

		bool MediaPlayer.IOnErrorListener.OnError(MediaPlayer mp, MediaError what, int extra)
		{
			if (Controller == null)
				return false;

			Controller.OnMediaFailed();
			return false;
		}
		
		bool MediaPlayer.IOnInfoListener.OnInfo(MediaPlayer mp, MediaInfo what, int extra)
		{
			if (_view == null)
				return false;

			switch (what)
			{
				case MediaInfo.BufferingStart:
					Controller.CurrentState = MediaElementState.Buffering;
					mp.BufferingUpdate += Mp_BufferingUpdate;
					break;

				case MediaInfo.BufferingEnd:
					mp.BufferingUpdate -= Mp_BufferingUpdate;
					Controller.CurrentState = MediaElementState.Paused;
					break;

				case MediaInfo.VideoRenderingStart:
					_view.SetBackground(null);
					Controller.CurrentState = MediaElementState.Playing;
					break;
			}

			_mediaPlayer = mp;
			
			return true;
		}

		void Mp_BufferingUpdate(object sender, MediaPlayer.BufferingUpdateEventArgs e)
		{
			Controller.BufferingProgress = e.Percent / 100f;
		}
	}
}
