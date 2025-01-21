// Contains code from luberda-molinet/FFImageLoading
// https://github.com/luberda-molinet/FFImageLoading/blob/bb675c6011b39ddccecbe6125d1853de81e6396a/source/FFImageLoading.Shared.IosMac/Decoders/GifDecoder.cs

using System;
using CoreGraphics;
using Foundation;
using ImageIO;
using UIKit;

namespace Microsoft.Maui;

static class ImageAnimationHelper
{
	sealed class ImageDataHelper : IDisposable
	{
		readonly nfloat _scale;
		readonly CGImage[] _keyFrames;
		readonly int[] _delayTimes;
		readonly int _imageCount;
		int _totalAnimationTime;
		bool _disposed;

		public ImageDataHelper(nint imageCount, nfloat scale)
		{
			if (imageCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(imageCount), $"{nameof(imageCount)} is 0, no images to animate.");
			if (scale < 1)
				throw new ArgumentOutOfRangeException(nameof(scale), $"{nameof(scale)} is < 1, cannot scale up.");

			_scale = scale;
			_keyFrames = new CGImage[imageCount];
			_delayTimes = new int[imageCount];
			_imageCount = (int)imageCount;

			Width = 0;
			Height = 0;
		}

		public int Width { get; private set; }

		public int Height { get; private set; }

		public void AddFrameData(CGImageSource imageSource, int index)
		{
			if (index < 0 || index >= _imageCount || index >= imageSource.ImageCount)
				throw new ArgumentOutOfRangeException(nameof(index), $"Error adding frame data. {nameof(index)} is less than 0, or more than or equal to image count.");

			var imageProperties = imageSource.GetProperties(index, null);
			using var gifImageProperties = imageProperties?.Dictionary[ImageIO.CGImageProperties.GIFDictionary];
			using var unclampedDelayTimeValue = gifImageProperties?.ValueForKey(ImageIO.CGImageProperties.GIFUnclampedDelayTime);
			using var delayTimeValue = gifImageProperties?.ValueForKey(ImageIO.CGImageProperties.GIFDelayTime);

			var delayTime = 0.1;
			if (unclampedDelayTimeValue is NSNumber unclampedDelay)
				delayTime = unclampedDelay.DoubleValue;
			else if (delayTimeValue is NSNumber delay)
				delayTime = delay.DoubleValue;

			var image = imageSource.CreateImage(index, null!);
			if (image is null)
				throw new ArgumentException($"Image source did not contain an image at index {index}.");

			AddFrameData(image, index, delayTime);
		}

		public void AddFrameData(CGImage image, int index, double delayTime = 0.1)
		{
			if (index < 0 || index >= _imageCount)
				throw new ArgumentOutOfRangeException(nameof(index), $"Error adding frame data. {nameof(index)} is less than 0, or more than or equal to image count.");
			if (image is null)
				throw new ArgumentNullException(nameof(image));

			// Frame delay compability adjustment.
			if (delayTime <= 0.02)
				delayTime = 0.1;

			// GIF only has centiseconds data
			var centiseconds = (int)(delayTime * 100.0);

			Width = Math.Max(Width, (int)image.Width);
			Height = Math.Max(Height, (int)image.Height);

			_keyFrames[index]?.Dispose();
			_keyFrames[index] = image;
			_delayTimes[index] = centiseconds;
			_totalAnimationTime += centiseconds;
		}

		public UIImage? ToUIImage()
		{
			var frames = ToConsistentImageArray(out _, out var totalDuration);
			if (frames.Length == 0)
				return null;

			var seconds = totalDuration / 100.0;
			return UIImage.CreateAnimatedImage(frames, seconds);
		}

		// The GIF stores a separate duration for each frame, in units of centiseconds (hundredths of a second).
		// However, a `UIImage` only has a single, total `duration` property, which is a floating-point number.
		// To handle this mismatch, we add each source image (from the GIF) to `animation` a varying number of times to
		// match the ratios between the frame durations in the GIF.
		// For example, suppose the GIF contains three frames:
		//  - Frame 0 has duration 3.
		//  - Frame 1 has duration 9.
		//  - Frame 2 has duration 15.
		// We divide each duration by the greatest common denominator of all the durations, which is 3, and add each
		// frame the resulting number of times.
		// Thus `animation` will contain:
		//  - Frame 0  3/3 = 1 time.
		//  - Frame 1  9/3 = 3 times.
		//  - Frame 2 15/3 = 5 times.
		public UIImage[] ToConsistentImageArray(out int frameDuration, out int totalDuration)
		{
			var gcd = GetGreatestCommonDenominator(_delayTimes);

			var frameCount = _totalAnimationTime / gcd;
			var frames = new UIImage[frameCount];

			var f = 0;
			for (var i = 0; i < _imageCount; i++)
			{
				var frame = UIImage.FromImage(_keyFrames[i], _scale, UIImageOrientation.Up);
				for (var repeats = _delayTimes[i] / gcd; repeats > 0; --repeats)
					frames[f++] = frame;
			}

			frameDuration = gcd;
			totalDuration = _totalAnimationTime;
			return frames;
		}

		public static int GetGreatestCommonDenominator(int[] delays)
		{
			var gcd = delays[0];

			for (var i = 1; i < delays.Length; ++i)
			{
				gcd = CheckPair(delays[i], gcd);
				if (gcd == 1)
					break;
			}

			return gcd;

			static int CheckPair(int a, int b)
			{
				if (a is 0 or 1)
					return b;
				if (b is 0 or 1)
					return a;

				while (true)
				{
					var r = a % b;
					if (r == 0)
						return b;

					a = b;
					b = r;
				}
			}
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			for (int i = 0; i < _imageCount; i++)
			{
				_keyFrames[i]?.Dispose();
				_keyFrames[i] = null!;
			}

			_disposed = true;
		}
	}

	public static bool IsAnimated(this CGImageSource imageSource) =>
		imageSource.ImageCount > 1;

	public static UIImage? Create(CGImageSource imageSource, nfloat scale)
	{
		var imageCount = imageSource.ImageCount;
		if (imageCount <= 0)
			return null;

		// Load repeat data
		var repeatCount = 0.0;
		if (imageSource.TypeIdentifier == "com.compuserve.gif")
		{
			var imageProperties = imageSource.GetProperties(null);
			using var gifImageProperties = imageProperties?.Dictionary[ImageIO.CGImageProperties.GIFDictionary];
			using var repeatCountValue = gifImageProperties?.ValueForKey(ImageIO.CGImageProperties.GIFLoopCount);

			if (repeatCountValue is NSNumber number)
				repeatCount = number.DoubleValue;
			else if (repeatCountValue is not null)
				_ = double.TryParse(repeatCountValue.ToString(), out repeatCount);
		}

		// load image data
		using var helper = new ImageDataHelper(imageCount, scale);
		for (int i = 0; i < imageCount; i++)
		{
			helper.AddFrameData(imageSource, i);
		}

		var image = helper.ToUIImage();

		return image;
	}
}
