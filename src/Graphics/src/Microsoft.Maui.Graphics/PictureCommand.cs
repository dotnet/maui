namespace Microsoft.Maui.Graphics
{
    public enum PictureCommand
    {
        DrawLine = 0,
        DrawRectangle = 1,
        DrawRoundedRectangle = 2,
        DrawEllipse = 3,
        DrawPath = 4,
        DrawImage = 5,
        DrawArc = 6,
        DrawPdfPage = 7,

        FillRectangle = 10,
        FillRoundedRectangle = 11,
        FillEllipse = 12,
        FillPath = 13,
        FillArc = 14,
        FillPath2 = 15,

        DrawStringAtPoint = 20,
        DrawStringInRect = 21,
        DrawStringInPath = 22,
        DrawTextInRect = 25,

        StrokeSize = 30,
        StrokeColor = 31,
        StrokeDashPattern = 32,
        StrokeLineCap = 33,
        StrokeLineJoin = 34,
        StrokeLocation = 35,
        StrokeMiterLimit = 36,
        LimitStrokeScaling = 37,
        StrokeLimit = 38,
        StrokeBrush = 39,

        FillColor = 40,
        FillPaint = 41,

        FontColor = 50,
        FontName = 51,
        FontSize = 52,

        Scale = 60,
        Translate = 61,
        Rotate = 62,
        RotateAtPoint = 63,
        ConcatenateTransform = 64,

        Shadow = 70,
        Alpha = 71,
        BlendMode = 72,

        SubtractFromClip = 80,
        ClipPath = 81,
        ClipRectangle = 82,
        SubtractPathFromClip = 83,

        SaveState = 100,
        RestoreState = 101,
        ResetState = 102,

        SystemFont = 110,
        BoldSystemFont = 111
    }
}