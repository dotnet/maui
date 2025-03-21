package com.microsoft.maui;

import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.Path;

public interface PlatformShadowDrawable {
    void drawShadow(Canvas canvas, Paint shadowPaint, Path outerClipPath);
    boolean canDrawShadow();
}
