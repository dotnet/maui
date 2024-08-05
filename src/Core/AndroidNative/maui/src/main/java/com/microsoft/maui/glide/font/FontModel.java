package com.microsoft.maui.glide.font;

import android.graphics.Typeface;

import androidx.annotation.ColorInt;

import com.bumptech.glide.signature.ObjectKey;

public class FontModel {
    private final int color;
    private final String glyph;
    private final float textSize;
    private final Typeface typeface;

    public FontModel(@ColorInt int color, String glyph, float textSize, Typeface typeface)
    {
        this.color = color;
        this.glyph = glyph;
        this.textSize = textSize;
        this.typeface = typeface;
    }

    public int getColor() {
        return color;
    }

    public String getGlyph() {
        return glyph;
    }

    public float getTextSize() {
        return textSize;
    }

    public Typeface getTypeface() {
        return typeface;
    }

    public ObjectKey getCacheKey()
    {
        return new ObjectKey(
            Integer.toString(color)
                + glyph
                + String.valueOf(textSize)
                + typeface.toString());
    }
}
