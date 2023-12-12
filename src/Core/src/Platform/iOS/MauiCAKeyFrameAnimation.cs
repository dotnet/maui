using CoreAnimation;

namespace Microsoft.Maui.Platform;

internal class MauiCAKeyFrameAnimation : CAKeyFrameAnimation
{
    public int Width { get; set; }

    public int Height { get; set; }
}