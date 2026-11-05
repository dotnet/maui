using System;
using System.Linq;
using ImageMagick;

namespace VisualTestUtils.MagickNet
{
	/// <summary>
	/// Verify images using ImageMagick.
	/// </summary>
	public class MagickNetVisualComparer : IVisualComparer
	{
		private ErrorMetric _errorMetric;
		private double _differenceThreshold;

		/// <summary>
		/// Initializes a new instance of the <see cref="MagickNetVisualComparer"/> class.
		/// </summary>
		/// <param name="errorMetric">Error metric.</param>
		/// <param name="differenceThreshold">The maximum normalized error allowed between the baseline and actual snapshot images. Default value is .005. The default RMSE metric retains the channel normalization used before Magick.NET 14.16.</param>
		public MagickNetVisualComparer(ErrorMetric errorMetric = ErrorMetric.RootMeanSquared, double differenceThreshold = 0.005)
		{
			_errorMetric = errorMetric;
			_differenceThreshold = differenceThreshold;
		}

		/// <inheritdoc/>
		public ImageDifference Compare(ImageSnapshot baselineImage, ImageSnapshot actualImage)
		{
			using var magickBaselineImage = new MagickImage(baselineImage.Data);
			using var magickActualImage = new MagickImage(actualImage.Data);

			ImageSizeDifference imageSizeDifference = ImageSizeDifference.Compare((int)magickBaselineImage.Width, (int)magickBaselineImage.Height, (int)magickActualImage.Width, (int)magickActualImage.Height);
			if (imageSizeDifference != null)
				return imageSizeDifference;

			double distortionDifference = magickBaselineImage.Compare(magickActualImage, _errorMetric, Channels.Red);
			if (_errorMetric == ErrorMetric.RootMeanSquared)
			{
				// Magick.NET 14.16 normalizes by compared channels; retain the historical color/alpha-channel normalization.
				int channelCount = magickBaselineImage.Channels.Count(channel => channel != PixelChannel.Index);
				distortionDifference /= Math.Sqrt(channelCount);
			}

			if (distortionDifference > this._differenceThreshold)
				return new ImagePercentageDifference(distortionDifference);

			return null;
		}
	}
}
