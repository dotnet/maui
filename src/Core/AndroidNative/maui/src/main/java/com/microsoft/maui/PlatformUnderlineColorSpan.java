package com.microsoft.maui;

import android.os.Build;
import android.text.TextPaint;
import android.text.style.CharacterStyle;
import android.text.style.UpdateAppearance;

/**
 * Class for drawing a Span's underline in its own color (TextPaint.underlineColor, API 29+).
 * Below API 29 the underline is drawn in the text color, like UnderlineSpan.
 */
public class PlatformUnderlineColorSpan extends CharacterStyle implements UpdateAppearance {
    private final int color;

    /**
     * Constructor
     * @param color the underline color (ARGB)
     */
    public PlatformUnderlineColorSpan(int color) {
        this.color = color;
    }

    @Override
    public void updateDrawState(TextPaint textPaint) {
        if (textPaint == null) {
            return;
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            // The text-colored underline gives way to one in underlineColor, at the font's own thickness.
            textPaint.setUnderlineText(false);
            textPaint.underlineColor = color;
            textPaint.underlineThickness = textPaint.getUnderlineThickness();
        } else {
            textPaint.setUnderlineText(true);
        }
    }
}
