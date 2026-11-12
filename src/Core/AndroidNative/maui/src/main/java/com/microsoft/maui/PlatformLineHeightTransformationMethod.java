package com.microsoft.maui;

import android.graphics.Paint;
import android.graphics.Rect;
import android.text.SpannableString;
import android.text.Spanned;
import android.text.method.TransformationMethod;
import android.text.style.LineHeightSpan;
import android.view.View;

/**
 * Applies a Label's LineHeight to a TextView's text.
 * TextView's line spacing multiplier also scales the font padding that TextView adds to the first
 * line, which made the gap below the first line larger than the others. Instead, every line but the
 * first gets extra space above it, from that line's own height, each time the text is laid out.
 * That follows font changes and lines enlarged by spans (for example HTML headings).
 */
public class PlatformLineHeightTransformationMethod implements TransformationMethod {
    private final float lineHeight;
    private final LineHeightSpan span = new LineHeightSpan() {
        @Override
        public void chooseHeight(CharSequence text, int start, int end, int spanstartv, int lineTop, Paint.FontMetricsInt fm) {
            if (start == 0) {
                return;
            }
            int extra = Math.round((fm.descent - fm.ascent) * (lineHeight - 1));
            fm.ascent -= extra;
            fm.top = Math.min(fm.top, fm.ascent);
        }
    };

    public PlatformLineHeightTransformationMethod(float lineHeight) {
        this.lineHeight = lineHeight;
    }

    public float getLineHeight() {
        return lineHeight;
    }

    @Override
    public CharSequence getTransformation(CharSequence source, View view) {
        if (source == null) {
            return null;
        }
        SpannableString text = new SpannableString(source);
        text.setSpan(span, 0, text.length(), Spanned.SPAN_INCLUSIVE_INCLUSIVE);
        return text;
    }

    @Override
    public void onFocusChanged(View view, CharSequence sourceText, boolean focused, int direction, Rect previouslyFocusedRect) {
    }
}
