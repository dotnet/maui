package com.microsoft.maui;

import android.content.Context;
import android.graphics.Typeface;
import android.text.TextPaint;
import android.text.style.MetricAffectingSpan;
import android.util.TypedValue;

import androidx.annotation.NonNull;

/**
 * Class for setting letterSpacing, textSize, or typeface on a Span
 */
public class PlatformFontSpan extends MetricAffectingSpan {
    // NOTE: java.lang.Float is a "nullable" float
    private Float letterSpacing;
    private Float textSize;
    private Typeface typeface;

    /**
     * Constructor for setting letterSpacing-only
     * @param letterSpacing
     */
    public PlatformFontSpan(float letterSpacing) {
        this.letterSpacing = letterSpacing;
    }

    /**
     * Constructor for setting typeface and computing textSize
     * @param context
     * @param typeface
     * @param autoScalingEnabled
     * @param fontSize
     */
    public PlatformFontSpan(@NonNull Context context, Typeface typeface, boolean autoScalingEnabled, float fontSize) {
        this.typeface = typeface;
        textSize = TypedValue.applyDimension(
            autoScalingEnabled ? TypedValue.COMPLEX_UNIT_SP : TypedValue.COMPLEX_UNIT_DIP,
            fontSize,
            context.getResources().getDisplayMetrics()
        );
    }

    @Override
    public void updateDrawState(TextPaint textPaint) {
        if (textPaint != null) {
            apply(textPaint);
        }
    }

    @Override
    public void updateMeasureState(@NonNull TextPaint textPaint) {
        apply(textPaint);
    }

    void apply(TextPaint textPaint)
    {
        if (typeface != null) {
            textPaint.setTypeface(typeface);
        }
        if (textSize != null) {
            textPaint.setTextSize(textSize);
        }
        if (letterSpacing != null) {
            textPaint.setLetterSpacing(letterSpacing);
        }
    }
}
