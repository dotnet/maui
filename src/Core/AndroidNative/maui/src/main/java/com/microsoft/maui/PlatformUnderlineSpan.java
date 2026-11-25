package com.microsoft.maui;

import android.os.Build;
import android.text.TextPaint;
import android.text.style.CharacterStyle;
import android.text.style.UpdateAppearance;

/**
 * Class for drawing a Span's underline in its own color and / or thickness
 * (TextPaint.underlineColor / underlineThickness, API 29+).
 * Below API 29 the underline is drawn in the text color at the font's thickness, like UnderlineSpan.
 */
public class PlatformUnderlineSpan extends CharacterStyle implements UpdateAppearance {
    private final boolean hasColor;
    private final int color;
    private final float thickness;

    /**
     * Constructor
     * @param hasColor whether {@code color} is used; otherwise the underline takes the text color
     * @param color the underline color (ARGB)
     * @param thickness the underline thickness in pixels; zero or less uses the font's own thickness
     */
    public PlatformUnderlineSpan(boolean hasColor, int color, float thickness) {
        this.hasColor = hasColor;
        this.color = color;
        this.thickness = thickness;
    }

    @Override
    public void updateDrawState(TextPaint textPaint) {
        if (textPaint == null) {
            return;
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            // The standard underline gives way to one drawn in underlineColor at underlineThickness.
            textPaint.setUnderlineText(false);
            textPaint.underlineColor = hasColor ? color : textPaint.getColor();
            // Match Android's standard underline minimum when the font has no thickness metric.
            textPaint.underlineThickness = thickness > 0 ? thickness : Math.max(textPaint.getUnderlineThickness(), 1.0f);
        } else {
            textPaint.setUnderlineText(true);
        }
    }
}
