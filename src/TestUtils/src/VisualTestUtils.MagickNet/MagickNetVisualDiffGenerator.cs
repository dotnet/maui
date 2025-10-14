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

        public MagickNetVisualDiffGenerator(ErrorMetric error = ErrorMetric.Fuzz)
        {
            _errorMetric = error;
        }

        public ImageSnapshot GenerateDiff(ImageSnapshot baselineImage, ImageSnapshot actualImage)
        {
            var magickBaselineImage = new MagickImage(baselineImage.Data);
            var magickActualImage = new MagickImage(actualImage.Data);

            MagickImage magickDiffImage = new MagickImage();
            magickDiffImage.Format = MagickFormat.Png;

            double percentageDifference = magickBaselineImage.Compare(magickActualImage, _errorMetric, magickDiffImage, Channels.Red);

            return new ImageSnapshot(magickDiffImage.ToByteArray(), ImageSnapshotFormat.PNG);
        }
    }
}
