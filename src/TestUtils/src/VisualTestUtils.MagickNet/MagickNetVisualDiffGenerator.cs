// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageMagick;

namespace VisualTestUtils.MagickNet
{
    /// <summary>
    /// Verify images using ImageMagick.
    /// </summary>
    public class MagickNetVisualDiffGenerator : IVisualDiffGenerator
    {
        private ErrorMetric _errorMetric;

        public MagickNetVisualDiffGenerator(ErrorMetric error = ErrorMetric.RootMeanSquared)
        {
            _errorMetric = error;
        }

        public ImageSnapshot GenerateDiff(ImageSnapshot baselineImage, ImageSnapshot actualImage)
        {
            using var magickBaselineImage = new MagickImage(baselineImage.Data);
            using var magickActualImage = new MagickImage(actualImage.Data);

            using var magickDiffImage = (MagickImage)magickBaselineImage.Compare(magickActualImage, _errorMetric, Channels.Red, out _);
            magickDiffImage.Format = MagickFormat.Png;

            return new ImageSnapshot(magickDiffImage.ToByteArray(), ImageSnapshotFormat.PNG);
        }
    }
}
