package com.microsoft.maui;

import android.graphics.Paint;
import android.graphics.Path;
import android.graphics.RectF;
import android.graphics.Typeface;
import android.os.Build;
import android.text.Layout;
import android.widget.TextView;

import java.util.Arrays;

/**
 * Before Android 15 TextView measures text by its advance width, so the slant of an italic last
 * glyph can be drawn outside the view and clipped. The missing space is reserved as right padding
 * while the text is measured and laid out, but not while drawing, which keeps it inside the clip
 * bounds. Nothing is reserved when the glyphs already fit, or when reserving it would change how
 * the text wraps or truncates.
 */
final class ItalicOverhang {

    interface Host {
        void measureText(int widthMeasureSpec, int heightMeasureSpec);
    }

    private final TextView view;
    private final Host host;
    private final Path path = new Path();
    private final RectF bounds = new RectF();
    private int reserved;
    private boolean drawing;

    ItalicOverhang(TextView view, Host host) {
        this.view = view;
        this.host = host;
    }

    int getPadding() {
        return drawing ? 0 : reserved;
    }

    void setDrawing(boolean drawing) {
        this.drawing = drawing;
    }

    void measure(int widthMeasureSpec, int heightMeasureSpec) {
        reserved = 0;
        host.measureText(widthMeasureSpec, heightMeasureSpec);

        if (Build.VERSION.SDK_INT >= 35) {
            return;
        }

        int excess = computeExcess();
        if (excess <= 0) {
            return;
        }

        int[] lines = getLines();
        reserved = excess;
        host.measureText(widthMeasureSpec, heightMeasureSpec);

        int remaining = computeExcess();
        if (remaining > 0 && Arrays.equals(lines, getLines())) {
            reserved += 2 * remaining;
            host.measureText(widthMeasureSpec, heightMeasureSpec);
        }

        if (!Arrays.equals(lines, getLines())) {
            reserved = 0;
            host.measureText(widthMeasureSpec, heightMeasureSpec);
        }
    }

    private int[] getLines() {
        Layout layout = view.getLayout();
        if (layout == null) {
            return new int[0];
        }

        int[] lines = new int[layout.getLineCount() * 2];
        for (int line = 0; line < layout.getLineCount(); line++) {
            lines[line * 2] = layout.getLineStart(line);
            lines[line * 2 + 1] = layout.getEllipsisCount(line) > 0 ? 1 : 0;
        }

        return lines;
    }

    private int computeExcess() {
        Layout layout = view.getLayout();
        Typeface typeface = view.getTypeface();
        Paint paint = view.getPaint();
        boolean isItalic = (typeface != null && typeface.isItalic()) || paint.getTextSkewX() < 0;
        if (layout == null || !isItalic) {
            return 0;
        }

        int contentWidth = view.getMeasuredWidth() - view.getCompoundPaddingLeft() - (view.getCompoundPaddingRight() - getPadding());
        float excess = 0;
        for (int line = 0; line < layout.getLineCount(); line++) {
            if (layout.getLineVisibleEnd(line) <= layout.getLineStart(line)
                    || layout.getParagraphDirection(line) != Layout.DIR_LEFT_TO_RIGHT) {
                continue;
            }

            float lineRight = getLineRight(layout, line, contentWidth);
            if (lineRight + paint.getTextSize() / 2 <= contentWidth) {
                continue;
            }

            String text = getDisplayedText(layout, line);
            path.reset();
            paint.getTextPath(text, 0, text.length(), 0, 0, path);
            path.computeBounds(bounds, true);

            float lineLeft = layout.getWidth() <= contentWidth ? layout.getLineLeft(line) : lineRight - layout.getLineWidth(line);
            excess = Math.max(excess, lineLeft + bounds.right - contentWidth);
        }

        return (int) Math.ceil(excess);
    }

    private static String getDisplayedText(Layout layout, int line) {
        CharSequence text = layout.getText();
        int start = layout.getLineStart(line);
        int end = layout.getLineVisibleEnd(line);
        int ellipsisCount = layout.getEllipsisCount(line);
        if (ellipsisCount == 0) {
            return text.subSequence(start, end).toString();
        }

        int ellipsisStart = start + layout.getEllipsisStart(line);
        int ellipsisEnd = Math.min(ellipsisStart + ellipsisCount, end);
        return text.subSequence(start, ellipsisStart) + "\u2026" + text.subSequence(ellipsisEnd, end);
    }

    private static float getLineRight(Layout layout, int line, int contentWidth) {
        if (layout.getWidth() <= contentWidth) {
            return layout.getLineRight(line);
        }

        switch (layout.getParagraphAlignment(line)) {
            case ALIGN_OPPOSITE:
                return contentWidth;
            case ALIGN_CENTER:
                return (contentWidth + layout.getLineWidth(line)) / 2;
            default:
                return layout.getLineWidth(line);
        }
    }
}
