using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	// All this animation code will go away if/once we pull in GlideX
	public class FormsAnimationDrawableStateEventArgs : EventArgs
	{
		public FormsAnimationDrawableStateEventArgs(bool finished)
		{
			Finished = finished;
		}

		public bool Finished { get; set; }
	}

	public interface IFormsAnimationDrawable : IDisposable
	{
		event EventHandler AnimationStarted;
		event EventHandler<FormsAnimationDrawableStateEventArgs> AnimationStopped;

		int RepeatCount { get; set; }

		bool IsRunning { get; }

		Drawable ImageDrawable { get; }

		void Start();
		void Stop();
	}

	public class FormsAnimationDrawable : AnimationDrawable, IFormsAnimationDrawable
	{
		const int DefaultBufferSize = 4096;

		int _repeatCounter = 0;
		int _frameCount = 0;
		bool _finished = false;
		bool _isRunning = false;

		public FormsAnimationDrawable()
		{
			RepeatCount = 1;

			if (!Forms.IsLollipopOrNewer)
				base.SetVisible(false, true);
		}

		public int RepeatCount { get; set; }

		public event EventHandler AnimationStarted;
		public event EventHandler<FormsAnimationDrawableStateEventArgs> AnimationStopped;

		public override bool IsRunning
		{
			get { return _isRunning; }
		}

		public Drawable ImageDrawable
		{
			get { return this; }
		}

		public override void Start()
		{
			_repeatCounter = 0;
			_frameCount = NumberOfFrames;
			_finished = false;

			base.OneShot = RepeatCount == 1;

			base.Start();

			if (!Forms.IsLollipopOrNewer)
				base.SetVisible(true, true);

			_isRunning = true;
			AnimationStarted?.Invoke(this, null);
		}

		public override void Stop()
		{
			base.Stop();

			if (!Forms.IsLollipopOrNewer)
				base.SetVisible(false, true);

			_isRunning = false;
			AnimationStopped?.Invoke(this, new FormsAnimationDrawableStateEventArgs(_finished));
		}

		public override bool SelectDrawable(int index)
		{
			if (!_isRunning)
				return base.SelectDrawable(0);

			// Hitting last frame?
			if (index != 0 && index == _frameCount - 1)
				_repeatCounter++;

			// Restarted animation, reached max number of repeats?
			if (_repeatCounter >= RepeatCount)
			{
				_finished = true;

				// Stop can't be done from within this method.
				new Handler(Looper.MainLooper).Post(() =>
				{
					if (this.IsRunning)
						this.Stop();
				});

				// Until stopped, show first image.
				return base.SelectDrawable(0);
			}


			return base.SelectDrawable(index);
		}

		public static Task<IFormsAnimationDrawable> LoadImageAnimationAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default(CancellationToken))
		{
			switch (imagesource)
			{
				case FileImageSource fis:
					return LoadImageAnimationAsync(fis, context, cancelationToken);
				case StreamImageSource sis:
					return LoadImageAnimationAsync(sis, context, cancelationToken);
				case UriImageSource uis:
					return LoadImageAnimationAsync(uis, context, cancelationToken);
			}

			return Task.FromResult<IFormsAnimationDrawable>(null);
		}

		public async Task<IFormsAnimationDrawable> LoadImageAnimationAsync(StreamImageSource imagesource, Context context, CancellationToken cancelationToken = default(CancellationToken))
		{
			var streamSource = imagesource as StreamImageSource;
			FormsAnimationDrawable animation = null;
			if (streamSource?.Stream != null)
			{
				using (Stream stream = await ((IStreamImageSource)streamSource).GetStreamAsync(cancelationToken).ConfigureAwait(false))
				{
					int sourceDensity = 1;
					int targetDensity = 1;

					if (stream.CanSeek)
					{
						BitmapFactory.Options options = new BitmapFactory.Options();
						options.InJustDecodeBounds = true;
						await BitmapFactory.DecodeStreamAsync(stream, null, options);
						sourceDensity = options.InDensity;
						targetDensity = options.InTargetDensity;
						stream.Seek(0, SeekOrigin.Begin);
					}

					using (var decoder = new AndroidGIFImageParser(context, sourceDensity, targetDensity))
					{
						try
						{
							await decoder.ParseAsync(stream).ConfigureAwait(false);
							animation = decoder.Animation;
						}
						catch (GIFDecoderFormatException)
						{
							animation = null;
						}
					}
				}
			}

			if (animation == null)
			{
				Internals.Log.Warning(nameof(ImageLoaderSourceHandler), "Image data was invalid: {0}", streamSource);
			}

			return animation;
		}

		public static async Task<IFormsAnimationDrawable> LoadImageAnimationAsync(FileImageSource imagesource, Context context, CancellationToken cancelationToken = default(CancellationToken))
		{
			string file = ((FileImageSource)imagesource).File;
			FormsAnimationDrawable animation = null;

			BitmapFactory.Options options = new BitmapFactory.Options
			{
				InJustDecodeBounds = true
			};

			int drawableIdentifier = ResourceManager.GetDrawableByName(file);

			if (drawableIdentifier != 0)
			{
				if (!FileImageSourceHandler.DecodeSynchronously)
					await BitmapFactory.DecodeResourceAsync(context.Resources, drawableIdentifier, options);
				else
					BitmapFactory.DecodeResource(context.Resources, drawableIdentifier, options);

				animation = await GetFormsAnimationDrawableFromResource(drawableIdentifier, context, options);
			}
			else
				animation = await GetFormsAnimationDrawableFromFile(file, context, options);

			if (animation == null)
			{
				Internals.Log.Warning(nameof(FileImageSourceHandler), "Could not retrieve image or image data was invalid: {0}", imagesource);
			}

			return animation;
		}

		public static async Task<IFormsAnimationDrawable> LoadImageAnimationAsync(UriImageSource imagesource, Context context, CancellationToken cancelationToken = default(CancellationToken))
		{
			Uri uri = imagesource?.Uri;
			FormsAnimationDrawable animation = null;

			if (uri != null)
			{
				var options = new BitmapFactory.Options
				{
					InJustDecodeBounds = true
				};

				using (Stream stream = await imagesource.GetStreamAsync(cancelationToken).ConfigureAwait(false))
				{
					using (var decoder = new AndroidGIFImageParser(context, options.InDensity, options.InTargetDensity))
					{
						try
						{
							if (!FileImageSourceHandler.DecodeSynchronously)
								await decoder.ParseAsync(stream).ConfigureAwait(false);
							else
								decoder.ParseAsync(stream).Wait();

							animation = decoder.Animation;
						}
						catch (GIFDecoderFormatException ex)
						{
							System.Diagnostics.Debug.WriteLine(ex.Message);
							animation = null;
						}
					}
				}

				if (animation == null)
				{
					Log.Warning(nameof(FileImageSourceHandler), "Could not retrieve image or image data was invalid: {0}", imagesource);
				}
			}

			return animation;
		}

		internal static async Task<FormsAnimationDrawable> GetFormsAnimationDrawableFromResource(int resourceId, Context context, BitmapFactory.Options options)
		{
			FormsAnimationDrawable animation = null;

			using (var stream = context.Resources.OpenRawResource(resourceId))
				animation = await GetFormsAnimationDrawableFromStream(stream, context, options);

			return animation;
		}

		internal static async Task<FormsAnimationDrawable> GetFormsAnimationDrawableFromFile(string file, Context context, BitmapFactory.Options options)
		{
			FormsAnimationDrawable animation = null;

			using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, true))
				animation = await GetFormsAnimationDrawableFromStream(stream, context, options);

			return animation;
		}

		internal static async Task<FormsAnimationDrawable> GetFormsAnimationDrawableFromStream(Stream stream, Context context, BitmapFactory.Options options)
		{
			FormsAnimationDrawable animation = null;

			using (var decoder = new AndroidGIFImageParser(context, options.InDensity, options.InTargetDensity))
			{
				try
				{
					if (!FileImageSourceHandler.DecodeSynchronously)
						await decoder.ParseAsync(stream).ConfigureAwait(false);
					else
						decoder.ParseAsync(stream).Wait();

					animation = decoder.Animation;
				}
				catch (GIFDecoderFormatException)
				{
					animation = null;
				}
			}

			return animation;
		}
	}

	class AndroidGIFImageParser : GIFImageParser, IDisposable
	{
		readonly Context _context;
		readonly int _sourceDensity;
		readonly int _targetDensity;
		Bitmap _currentBitmap;
		bool _disposed;

		public AndroidGIFImageParser(Context context, int sourceDensity, int targetDensity)
		{
			_context = context;
			_sourceDensity = sourceDensity;
			_targetDensity = targetDensity;
			Animation = new FormsAnimationDrawable();
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public FormsAnimationDrawable Animation { get; private set; }

		protected override void StartParsing()
		{
			System.Diagnostics.Debug.Assert(!Animation.IsRunning);
			System.Diagnostics.Debug.Assert(Animation.NumberOfFrames == 0);
			System.Diagnostics.Debug.Assert(_currentBitmap == null);
		}

		protected override void AddBitmap(GIFHeader header, GIFBitmap gifBitmap, bool ignoreImageData)
		{
			if (!ignoreImageData)
			{
				Bitmap bitmap;

				if (_sourceDensity < _targetDensity)
				{
					if (_currentBitmap == null)
						_currentBitmap = Bitmap.CreateBitmap(header.Width, header.Height, Bitmap.Config.Argb8888);

					System.Diagnostics.Debug.Assert(_currentBitmap.Width == header.Width);
					System.Diagnostics.Debug.Assert(_currentBitmap.Height == header.Height);

					_currentBitmap.SetPixels(gifBitmap.Data, 0, header.Width, 0, 0, header.Width, header.Height);

					float scaleFactor = (float)_targetDensity / (float)_sourceDensity;
					int scaledWidth = (int)(scaleFactor * header.Width);
					int scaledHeight = (int)(scaleFactor * header.Height);

					bitmap = Bitmap.CreateScaledBitmap(_currentBitmap, scaledWidth, scaledHeight, true);

					System.Diagnostics.Debug.Assert(!_currentBitmap.Equals(bitmap));
				}
				else
				{
					bitmap = Bitmap.CreateBitmap(gifBitmap.Data, header.Width, header.Height, Bitmap.Config.Argb8888);
				}

				// Frame delay compability adjustment in milliseconds.
				int delay = gifBitmap.Delay;
				if (delay <= 20)
					delay = 100;

				Animation.AddFrame(new BitmapDrawable(_context.Resources, bitmap), delay);

				if (gifBitmap.LoopCount != 0)
					Animation.RepeatCount = gifBitmap.LoopCount;
			}
		}

		protected override void FinishedParsing()
		{
			if (_currentBitmap != null)
			{
				_currentBitmap.Recycle();
				_currentBitmap.Dispose();
				_currentBitmap = null;
			}

			System.Diagnostics.Debug.Assert(!Animation.IsRunning);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (!disposing)
				return;

			if (_currentBitmap != null)
			{
				_currentBitmap.Recycle();
				_currentBitmap.Dispose();
				_currentBitmap = null;
			}

			_disposed = true;
		}
	}
}