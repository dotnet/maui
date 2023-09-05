#nullable disable

using System;
using System.IO;
using ImageIO;
using CoreAnimation;
using Foundation;
using System.Threading.Tasks;
using System.Threading;
using UIKit;

namespace Microsoft.Maui;

internal class ImageAnimationHelper
{
	class ImageDataHelper : IDisposable
	{
		readonly NSObject[] _keyFrames;
		readonly NSNumber[] _keyTimes;
		readonly double[] _delayTimes;
		readonly int _imageCount;
		double _totalAnimationTime;
		bool _disposed;

		public ImageDataHelper(nint imageCount)
		{
			if (imageCount <= 0)
				throw new ArgumentException($"{nameof(imageCount)} is 0, no images to animate.");

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
				throw new ArgumentException($"Error adding frame data. {nameof(index)} is less than 0, or more than or equal to image count.");

			double delayTime = 0.1f;

			var imageProperties = imageSource.GetProperties(index, null);

			using var gifImageProperties = imageProperties?.Dictionary[ImageIO.CGImageProperties.GIFDictionary];
			using var unclampedDelayTimeValue = gifImageProperties?.ValueForKey(ImageIO.CGImageProperties.GIFUnclampedDelayTime);
			using var delayTimeValue = gifImageProperties?.ValueForKey(ImageIO.CGImageProperties.GIFDelayTime);

			if (unclampedDelayTimeValue is NSNumber unclampedDelay)
			{
				delayTime = unclampedDelay.DoubleValue;
			}
			else if (delayTimeValue is NSNumber delay)
			{
				delayTime = delay.DoubleValue;
			}

			// Frame delay compability adjustment.
			if (delayTime <= 0.02f)
				delayTime = 0.1f;

			using var image = imageSource.CreateImage(index, null!);
			if (image != null)
			{
				Width = Math.Max(Width, (int)image.Width);
				Height = Math.Max(Height, (int)image.Height);

				_keyFrames[index]?.Dispose();
				_keyFrames[index] = null!;

				_keyFrames[index] = UIImage.FromImage(image);
				_delayTimes[index] = delayTime;
				_totalAnimationTime += delayTime;
			}
		}

		public MauiCAKeyFrameAnimation CreateKeyFrameAnimation(CGImageSource imageSource)
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

			return new MauiCAKeyFrameAnimation
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

	public static MauiCAKeyFrameAnimation CreateAnimationFromCGImageSource(CGImageSource imageSource)
	{
		float repeatCount = float.MaxValue;
		var imageCount = imageSource.ImageCount;

		if (imageCount <= 0)
			return null;

		using var imageData = new ImageDataHelper(imageCount);
		if (imageSource.TypeIdentifier == "com.compuserve.gif")
		{
			var imageProperties = imageSource.GetProperties(null);
			using var gifImageProperties = imageProperties?.Dictionary[ImageIO.CGImageProperties.GIFDictionary];
			using var repeatCountValue = gifImageProperties?.ValueForKey(ImageIO.CGImageProperties.GIFLoopCount);

			if (repeatCountValue != null)
				_ = float.TryParse(repeatCountValue.ToString(), out repeatCount);
			else
				repeatCount = 1;

			if (repeatCount == 0)
				repeatCount = float.MaxValue;
		}

		for (int i = 0; i < imageCount; i++)
		{
			imageData.AddFrameData(i, imageSource);
		}

		MauiCAKeyFrameAnimation animation = imageData.CreateKeyFrameAnimation(imageSource);

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

		return animation;
	}

	public static Task<MauiCAKeyFrameAnimation> CreateAnimationFromImageSource(IImageSource imageSource, CancellationToken cancellationToken = default)
	{
		if (imageSource is IStreamImageSource streamImageSource)
		{
			return CreateAnimationFromStreamImageSourceAsync(streamImageSource, cancellationToken);
		}
		else if (imageSource is IFileImageSource fileImageSource)
		{
			return Task.FromResult(CreateAnimationFromFileImageSource(fileImageSource));
		}
		else if (imageSource is IUriImageSource uriImageSource)
		{
			return CreateAnimationFromUriImageSourceAsync(uriImageSource, cancellationToken);
		}

		throw new ArgumentException($"Type of {nameof(imageSource)} is not supported for loading animations.");
	}

	public static async Task<MauiCAKeyFrameAnimation> CreateAnimationFromStreamImageSourceAsync(IStreamImageSource imageSource, CancellationToken cancellationToken = default)
	{
		MauiCAKeyFrameAnimation animation = null;

        using var streamImage = await imageSource.GetStreamAsync(cancellationToken).ConfigureAwait(false);
		if (streamImage != null)
		{
			using var parsedImageSource = CGImageSource.FromData(NSData.FromStream(streamImage));
			animation = CreateAnimationFromCGImageSource(parsedImageSource);
		}

		return animation;
	}

	public static async Task<MauiCAKeyFrameAnimation> CreateAnimationFromUriImageSourceAsync(IUriImageSource imageSource, CancellationToken cancellationToken = default)
	{
		MauiCAKeyFrameAnimation animation = null;

		if (imageSource?.Uri != null)
		{
			using var streamImage = await ((IStreamImageSource)imageSource).GetStreamAsync(cancellationToken).ConfigureAwait(false);
			if (streamImage != null)
			{
				using (var parsedImageSource = CGImageSource.FromData(NSData.FromStream(streamImage)))
				{
					animation = CreateAnimationFromCGImageSource(parsedImageSource);
				}
			}
		}

		return animation;
	}

	public static MauiCAKeyFrameAnimation CreateAnimationFromFileImageSource(IFileImageSource imageSource)
	{
		MauiCAKeyFrameAnimation animation = null;
		string file = imageSource?.File;

		if (!string.IsNullOrEmpty(file))
		{

			var root = NSBundle.MainBundle.BundlePath;
#if MACCATALYST
			root = Path.Combine(root, "Contents", "Resources");
#endif
			using var parsedImageSource = CGImageSource.FromUrl(NSUrl.CreateFileUrl(Path.Combine(root, file), null));
			animation = CreateAnimationFromCGImageSource(parsedImageSource);
		}

		return animation;
	}
}