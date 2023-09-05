using CoreAnimation;

namespace Microsoft.Maui.Platform;

public class MauiCAKeyFrameAnimation : CAKeyFrameAnimation
{
    public int Width { get; set; }

    public int Height { get; set; }
}