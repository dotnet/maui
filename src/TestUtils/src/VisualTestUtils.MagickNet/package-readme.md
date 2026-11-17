This package extends VisualTestUtils, providing support for perceptual diffs
based on [Magick.NET](https://github.com/dlemstra/Magick.NET).

Support includes creating a visual representation of the image diffs.

The default comparer uses red-channel root mean squared error (RMSE), normalized
by the baseline's color and alpha channel count to preserve the scoring used
before Magick.NET 14.16. The default threshold remains `0.005`; it is a normalized
error score, not a count of differing pixels. Other explicitly selected metrics
use ImageMagick's native scores.
