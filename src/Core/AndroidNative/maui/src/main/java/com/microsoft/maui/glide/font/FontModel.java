package com.microsoft.maui.glide.font;

import android.graphics.Typeface;

import androidx.annotation.ColorInt;

import java.util.Objects;

import com.bumptech.glide.signature.ObjectKey;

import com.microsoft.maui.PlatformUtils;

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
    
    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        FontModel other = (FontModel) o;
        return color == other.color &&
            Objects.equals(glyph, other.glyph) &&
            textSize == other.textSize &&
            typeface == other.typeface;
    }

    @Override
    public int hashCode() {
        return Objects.hash(color, glyph, textSize, typeface);
    }

    @Override
    public String toString() {
        return "FontModel{" +
            "color=" + String.format("#%08X", (0xFFFFFFFF & color)) +
            ", glyph='" + PlatformUtils.getGlyphHex(glyph) + '\'' +
            ", textSize=" + textSize +
            ", typeface=" + typeface +
            '}';
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
