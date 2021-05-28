using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoreAnimation;
using Foundation;
using ImageIO;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	internal class ImageAnimationHelper
	{
		class ImageDataHelper : IDisposable
		{
			NSObject[] _keyFrames = null;
			NSNumber[] _keyTimes = null;
			double[] _delayTimes = null;
			int _imageCount = 0;
			double _totalAnimationTime = 0.0f;
			bool _disposed = false;

			public ImageDataHelper(nint imageCount)
			{
				if (imageCount <= 0)
					throw new ArgumentException();

				_keyFrames = new NSObject[imageCount];
				_keyTimes = new NSNumber[imageCount + 1];
				_delayTimes = new double[imageCount];
				_imageCount = (int)imageCount;
				Width = 0;
				Height = 0;
			}

			public int Width { get; set; }
			public int Height { get; set; }

			public void AddFrameData(int index, CGImageSource imageSource)
			{
				if (index < 0 || index >= _imageCount || index >= imageSource.ImageCount)
					throw new ArgumentException();

				double delayTime = 0.1f;

				var imageProperties = imageSource.GetProperties(index, null);
				using (var gifImageProperties = imageProperties?.Dictionary[ImageIO.CGImageProperties.GIFDictionary])
				using (var unclampedDelayTimeValue = gifImageProperties?.ValueForKey(ImageIO.CGImageProperties.GIFUnclampedDelayTime))
				using (var delayTimeValue = gifImageProperties?.ValueForKey(ImageIO.CGImageProperties.GIFDelayTime))
				{
					if (unclampedDelayTimeValue != null)
						double.TryParse(unclampedDelayTimeValue.ToString(), out delayTime);
					else if (delayTimeValue != null)
						double.TryParse(delayTimeValue.ToString(), out delayTime);

					// Frame delay compability adjustment.
					if (delayTime <= 0.02f)
						delayTime = 0.1f;

					using (var image = imageSource.CreateImage(index, null))
					{
						if (image != null)
						{
							Width = Math.Max(Width, (int)image.Width);
							Height = Math.Max(Height, (int)image.Height);

							_keyFrames[index]?.Dispose();
							_keyFrames[index] = null;

							_keyFrames[index] = NSObject.FromObject(image);
							_delayTimes[index] = delayTime;
							_totalAnimationTime += delayTime;
						}
					}
				}
			}

			public FormsCAKeyFrameAnimation CreateKeyFrameAnimation()
			{
				if (_totalAnimationTime <= 0.0f)
					return null;

				double currentTime = 0.0f;
				for (int i = 0; i < _imageCount; i++)
				{
					_keyTimes[i]?.Dispose();
					_keyTimes[i] = null;

					_keyTimes[i] = new NSNumber(currentTime);
					currentTime += _delayTimes[i] / _totalAnimationTime;
				}

				// When using discrete animation there should be one more keytime
				// than values, with 1.0f as value.
				_keyTimes[_imageCount] = new NSNumber(1.0f);

				return new FormsCAKeyFrameAnimation
				{
					Values = _keyFrames,
					KeyTimes = _keyTimes,
					Duration = _totalAnimationTime,
					CalculationMode = CAAnimation.AnimationDiscrete
				};
			}

			protected virtual void Dispose(bool disposing)
			{
				if (_disposed)
					return;

				for (int i = 0; i < _imageCount; i++)
				{
					_keyFrames[i]?.Dispose();
					_keyFrames[i] = null;

					_keyTimes[i]?.Dispose();
					_keyTimes[i] = null;
				}

				_keyTimes[_imageCount]?.Dispose();
				_keyTimes[_imageCount] = null;

				_disposed = true;
			}

			public void Dispose()
			{
				Dispose(true);
			}
		}

		static public FormsCAKeyFrameAnimation CreateAnimationFromCGImageSource(CGImageSource imageSource)
		{
			FormsCAKeyFrameAnimation animation = null;
			float repeatCount = float.MaxValue;
			var imageCount = imageSource.ImageCount;

			if (imageCount <= 0)
				return null;

			using (var imageData = new ImageDataHelper(imageCount))
			{
				if (imageSource.TypeIdentifier == "com.compuserve.gif")
				{
					var imageProperties = imageSource.GetProperties(null);
					using (var gifImageProperties = imageProperties?.Dictionary[ImageIO.CGImageProperties.GIFDictionary])
					using (var repeatCountValue = gifImageProperties?.ValueForKey(ImageIO.CGImageProperties.GIFLoopCount))
					{
						if (repeatCountValue != null)
							float.TryParse(repeatCountValue.ToString(), out repeatCount);
						else
							repeatCount = 1;

						if (repeatCount == 0)
							repeatCount = float.MaxValue;
					}
				}

				for (int i = 0; i < imageCount; i++)
				{
					imageData.AddFrameData(i, imageSource);
				}

				animation = imageData.CreateKeyFrameAnimation();
				if (animation != null)
				{
					animation.RemovedOnCompletion = false;
					animation.KeyPath = "contents";
					animation.RepeatCount = repeatCount;
					animation.Width = imageData.Width;
					animation.Height = imageData.Height;

					if (imageCount == 1)
					{
						animation.Duration = double.MaxValue;
						animation.KeyTimes = null;
					}
				}
			}

			return animation;
		}

		static public async Task<FormsCAKeyFrameAnimation> CreateAnimationFromStreamImageSourceAsync(StreamImageSource imageSource, CancellationToken cancelationToken = default(CancellationToken))
		{
			FormsCAKeyFrameAnimation animation = null;

			if (imageSource?.Stream != null)
			{
				using (var streamImage = await ((IStreamImageSource)imageSource).GetStreamAsync(cancelationToken).ConfigureAwait(false))
				{
					if (streamImage != null)
					{
						using (var parsedImageSource = CGImageSource.FromData(NSData.FromStream(streamImage)))
						{
							animation = ImageAnimationHelper.CreateAnimationFromCGImageSource(parsedImageSource);
						}
					}
				}
			}

			return animation;
		}

		static public async Task<FormsCAKeyFrameAnimation> CreateAnimationFromUriImageSourceAsync(UriImageSource imageSource, CancellationToken cancelationToken = default(CancellationToken))
		{
			FormsCAKeyFrameAnimation animation = null;

			if (imageSource is IStreamImageSource streamImageSource)
			{
				using (var streamImage = await streamImageSource.GetStreamAsync(cancelationToken).ConfigureAwait(false))
				{
					if (streamImage != null)
					{
						using (var parsedImageSource = CGImageSource.FromData(NSData.FromStream(streamImage)))
						{
							animation = ImageAnimationHelper.CreateAnimationFromCGImageSource(parsedImageSource);
						}
					}
				}
			}

			return animation;
		}

		static public FormsCAKeyFrameAnimation CreateAnimationFromFileImageSource(FileImageSource imageSource)
		{
			FormsCAKeyFrameAnimation animation = null;
			string file = imageSource?.File;
			if (!string.IsNullOrEmpty(file))
			{
				using (var parsedImageSource = CGImageSource.FromUrl(NSUrl.CreateFileUrl(file, null)))
				{
					animation = ImageAnimationHelper.CreateAnimationFromCGImageSource(parsedImageSource);
				}
			}

			return animation;
		}
	}
}