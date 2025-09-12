package com.microsoft.maui;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Path;
import android.util.AttributeSet;
import android.view.ViewGroup;

public abstract class PlatformViewGroup extends ViewGroup {

    private int lastWidthMeasureSpec;
    private int lastHeightMeasureSpec;
    private int measuredWidth;
    private int measuredHeight;
    private boolean hasValidMeasure;

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

    @Override
    public void requestLayout() {
        super.requestLayout();
        hasValidMeasure = false;
    }

    @Override
    protected void onMeasure(int widthMeasureSpec, int heightMeasureSpec) {
        if (hasValidMeasure && lastWidthMeasureSpec == widthMeasureSpec && lastHeightMeasureSpec == heightMeasureSpec) {
            setMeasuredDimension(measuredWidth, measuredHeight);
            return;
        }

        doMeasure(widthMeasureSpec, heightMeasureSpec);
        lastWidthMeasureSpec = widthMeasureSpec;
        lastHeightMeasureSpec = heightMeasureSpec;
        measuredWidth = getMeasuredWidth();
        measuredHeight = getMeasuredHeight();
        hasValidMeasure = true;
    }

    protected abstract void doMeasure(int widthMeasureSpec, int heightMeasureSpec);

    public void quickMeasure(int widthMeasureSpec, int heightMeasureSpec, int measuredWidth, int measuredHeight) {
        this.measuredWidth = measuredWidth;
        this.measuredHeight = measuredHeight;
        lastWidthMeasureSpec = widthMeasureSpec;
        lastHeightMeasureSpec = heightMeasureSpec;
        hasValidMeasure = true;
        measure(widthMeasureSpec, heightMeasureSpec);
    }

    public boolean needsMeasure(int widthMeasureSpec, int heightMeasureSpec) {
        return !hasValidMeasure || lastWidthMeasureSpec != widthMeasureSpec || lastHeightMeasureSpec != heightMeasureSpec;
    }
}
