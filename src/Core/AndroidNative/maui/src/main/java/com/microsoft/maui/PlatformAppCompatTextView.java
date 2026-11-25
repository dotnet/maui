package com.microsoft.maui;

import android.content.Context;
import android.graphics.Canvas;

import androidx.annotation.NonNull;
import androidx.appcompat.widget.AppCompatTextView;

public class PlatformAppCompatTextView extends AppCompatTextView implements ItalicOverhang.Host {
    public PlatformAppCompatTextView(@NonNull Context context) {
        super(context);
        italicOverhang = new ItalicOverhang(this, this);
    }

    private boolean isFormatted;
    private ItalicOverhang italicOverhang;

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

    @Override
    public int getCompoundPaddingRight() {
        return super.getCompoundPaddingRight() + (italicOverhang == null ? 0 : italicOverhang.getPadding());
    }

    @Override
    protected void onMeasure(int widthMeasureSpec, int heightMeasureSpec) {
        italicOverhang.measure(widthMeasureSpec, heightMeasureSpec);
    }

    @Override
    public void measureText(int widthMeasureSpec, int heightMeasureSpec) {
        super.onMeasure(widthMeasureSpec, heightMeasureSpec);
    }

    @Override
    protected void onDraw(Canvas canvas) {
        italicOverhang.setDrawing(true);
        try {
            super.onDraw(canvas);
        } finally {
            italicOverhang.setDrawing(false);
        }
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
