package com.microsoft.maui;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Rect;
import android.view.View;

import androidx.annotation.NonNull;

public abstract class PlatformWrapperView extends PlatformContentViewGroup {
    public PlatformWrapperView(Context context) {
        super(context);
        this.viewBounds = new Rect();
        setClipChildren(false);
        setWillNotDraw(true);
    }

    private final Rect viewBounds;
    private boolean hasShadow;

    /**
     * Set by C#, determining if we need to call drawShadow()
     * Intentionally invalidates the view in case shadow definition changed
     * @param hasShadow
     */
    protected final void setHasShadow(boolean hasShadow) {
        this.hasShadow = hasShadow;
        invalidate();
    }

    @Override
    protected void onMeasure(int widthMeasureSpec, int heightMeasureSpec) {
        if (getChildCount() == 0) {
            super.onMeasure(widthMeasureSpec, heightMeasureSpec);
            return;
        }

        View child = getChildAt(0);
        viewBounds.set(0, 0, MeasureSpec.getSize(widthMeasureSpec), MeasureSpec.getSize(heightMeasureSpec));
        child.measure(widthMeasureSpec, heightMeasureSpec);
        setMeasuredDimension(child.getMeasuredWidth(), child.getMeasuredHeight());
    }

    @Override
    protected void dispatchDraw(Canvas canvas) {
        // Only call into C# if there is a Shadow
        if (hasShadow) {
            int viewWidth = viewBounds.width();
            int viewHeight = viewBounds.height();
            if (getChildCount() > 0)
            {
                View child = getChildAt(0);
                if (viewWidth == 0)
                    viewWidth = child.getMeasuredWidth();
                if (viewHeight == 0)
                    viewHeight = child.getMeasuredHeight();
            }
            drawShadow(canvas, viewWidth, viewHeight);
        }
        super.dispatchDraw(canvas);
    }

    /**
     * Overridden in C#, for custom logic around shadows
     * @param canvas
     * @param viewWidth
     * @param viewHeight
     * @return
     */
    protected abstract void drawShadow(@NonNull Canvas canvas, int viewWidth, int viewHeight);
}
