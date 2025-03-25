package com.microsoft.maui;

import android.content.Context;
import android.graphics.Paint;
import android.text.style.LineHeightSpan;

/**
 * Class for setting a relativeLineHeight on a Span
 */
public class PlatformLineHeightSpan implements LineHeightSpan {
    private final float relativeLineHeight;
    private final Float top; //NOTE: nullable float

    public PlatformLineHeightSpan(Context context, float relativeLineHeight, float defaultFontSize) {
        this.relativeLineHeight = relativeLineHeight;
        Paint.FontMetrics fontMetrics = PlatformInterop.getFontMetrics(context, defaultFontSize);
        this.top = fontMetrics != null ? fontMetrics.top : null;
    }

    @Override
    public void chooseHeight(CharSequence charSequence, int i, int i1, int i2, int i3, Paint.FontMetricsInt fontMetricsInt) {
        if (fontMetricsInt != null) {
            float top = this.top != null ? this.top : fontMetricsInt.top;
            fontMetricsInt.ascent = (int)(top * relativeLineHeight);
        }
    }
}
