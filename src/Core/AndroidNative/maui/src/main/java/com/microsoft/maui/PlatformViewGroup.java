package com.microsoft.maui;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Path;
import android.util.AttributeSet;
import android.view.ViewGroup;

public abstract class PlatformViewGroup extends ViewGroup {
    boolean needsMeasure;
    boolean nativeMeasure = true;

    // We use a bit on each packed measured size to indicate the need for cross platform measure
    // See also https://developer.android.com/reference/android/view/View#MEASURED_SIZE_MASK
    public static final long NEEDS_MEASURE = 0x0800000008000000L;

    public PlatformViewGroup(Context context) {
        super(context);
    }

    public PlatformViewGroup(Context context, AttributeSet attrs) {
        super(context, attrs);
    }

    public PlatformViewGroup(Context context, AttributeSet attrs, int defStyle) {
        super(context, attrs, defStyle);
    }

    public PlatformViewGroup(Context context, AttributeSet attrs, int defStyle, int defStyleRes) {
        super(context, attrs, defStyle, defStyleRes);
    }

    public long measureAndGetWidthAndHeight(int widthMeasureSpec, int heightMeasureSpec) {
        this.needsMeasure = false;
        this.nativeMeasure = false;
        this.measure(widthMeasureSpec, heightMeasureSpec);
        this.nativeMeasure = true;

        if (this.needsMeasure) {
            return NEEDS_MEASURE;
        }

        int width = this.getMeasuredWidth();
        int height = this.getMeasuredHeight();
        long measure = ((long)width << 32) | (height & 0xffffffffL);
        return measure;
    }
    
    public void overrideMeasuredDimension(int width, int height) {
        setMeasuredDimension(width, height);
    }

    @Override
    protected void onMeasure(int widthMeasureSpec, int heightMeasureSpec) {
        if (nativeMeasure) {
            doMeasure(widthMeasureSpec, heightMeasureSpec);
        } else {
            this.needsMeasure = true;
            setMeasuredDimension(0, 0);
        }
    }

    protected void doMeasure(int widthMeasureSpec, int heightMeasureSpec) {
        super.onMeasure(widthMeasureSpec, heightMeasureSpec);
    }
}
