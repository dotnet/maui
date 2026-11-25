package com.microsoft.maui;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.Rect;
import android.graphics.Typeface;
import android.os.Build;
import android.text.Layout;

import androidx.annotation.NonNull;
import androidx.appcompat.widget.AppCompatTextView;

public class PlatformAppCompatTextView extends AppCompatTextView {
    public PlatformAppCompatTextView(@NonNull Context context) {
        super(context);
    }

    private boolean isFormatted;
    private boolean isDrawing;
    private int italicOverhang;

    /**
     * Sets isFormatted=true, used for FormattedString content
     * @param text
     * @param type
     */
    @Override
    public void setText(CharSequence text, BufferType type) {
        isFormatted = !(text instanceof String);
        super.setText(text, type);
    }

    /**
     * Before Android 15 TextView measures text by its advance width, so the slant of an italic last
     * glyph is drawn outside the view and clipped. The overhang is reserved here while the text is
     * measured and laid out, but not while drawing, which keeps it inside the clip bounds.
     */
    @Override
    public int getCompoundPaddingRight() {
        return super.getCompoundPaddingRight() + (isDrawing ? 0 : italicOverhang);
    }

    @Override
    protected void onMeasure(int widthMeasureSpec, int heightMeasureSpec) {
        italicOverhang = 0;
        super.onMeasure(widthMeasureSpec, heightMeasureSpec);

        if (Build.VERSION.SDK_INT >= 35) {
            return;
        }

        int overhang = computeItalicOverhang();
        if (overhang > 0) {
            italicOverhang = overhang;
            super.onMeasure(widthMeasureSpec, heightMeasureSpec);
        }
    }

    @Override
    protected void onDraw(Canvas canvas) {
        isDrawing = true;
        try {
            super.onDraw(canvas);
        } finally {
            isDrawing = false;
        }
    }

    private int computeItalicOverhang() {
        Layout layout = getLayout();
        Typeface typeface = getTypeface();
        Paint paint = getPaint();
        boolean isItalic = (typeface != null && typeface.isItalic()) || paint.getTextSkewX() < 0;
        if (layout == null || !isItalic) {
            return 0;
        }

        CharSequence text = layout.getText();
        Rect bounds = new Rect();
        int overhang = 0;
        for (int line = 0; line < layout.getLineCount(); line++) {
            int start = layout.getLineStart(line);
            int end = layout.getLineVisibleEnd(line);
            if (end <= start || layout.getParagraphDirection(line) != Layout.DIR_LEFT_TO_RIGHT) {
                continue;
            }

            String lastGlyph = layout.getEllipsisCount(line) > 0
                ? "\u2026"
                : text.subSequence(Character.offsetByCodePoints(text, end, -1), end).toString();
            paint.getTextBounds(lastGlyph, 0, lastGlyph.length(), bounds);
            overhang = Math.max(overhang, (int) Math.ceil(bounds.right - paint.measureText(lastGlyph)));
        }

        return overhang;
    }

    @Override
    protected void onLayout(boolean changed, int left, int top, int right, int bottom) {
        super.onLayout(changed, left, top, right, bottom);

        if (isFormatted) {
            onLayoutFormatted(changed, left, top, right, bottom);
        }
    }

    /**
     * To be overridden from C#
     * @param changed
     * @param left
     * @param top
     * @param right
     * @param bottom
     */
    protected void onLayoutFormatted(boolean changed, int left, int top, int right, int bottom) { }
}
