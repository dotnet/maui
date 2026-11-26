package com.microsoft.maui;

import android.content.Context;
import android.graphics.Canvas;

import androidx.annotation.NonNull;

import com.google.android.material.textview.MaterialTextView;

public class PlatformMaterialTextView extends MaterialTextView implements ItalicOverhang.Host {
    private ItalicOverhang italicOverhang;

    public PlatformMaterialTextView(@NonNull Context context) {
        super(context);
        italicOverhang = new ItalicOverhang(this, this);
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
}
