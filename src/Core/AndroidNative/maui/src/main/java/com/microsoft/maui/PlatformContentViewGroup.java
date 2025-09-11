package com.microsoft.maui;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Path;
import android.util.AttributeSet;
import android.view.ViewGroup;
import android.view.ViewParent;

import com.microsoft.maui.PlatformInterop;

public abstract class PlatformContentViewGroup extends ViewGroup {

    public PlatformContentViewGroup(Context context) {
        super(context);
    }

    public PlatformContentViewGroup(Context context, AttributeSet attrs) {
        super(context, attrs);
    }

    public PlatformContentViewGroup(Context context, AttributeSet attrs, int defStyle) {
        super(context, attrs, defStyle);
    }

    public PlatformContentViewGroup(Context context, AttributeSet attrs, int defStyle, int defStyleRes) {
        super(context, attrs, defStyle, defStyleRes);
    }

    private boolean hasClip = false;

    /**
     * Set by C#, determining if we need to call getClipPath()
     * Intentionally invalidates the view in case clip changed
     * @param hasClip
     */
    protected void setHasClip(boolean hasClip) {
        this.hasClip = hasClip;
        invalidate();
    }

    @Override
    protected void dispatchDraw(Canvas canvas) {
        // Only call into C# if there is a Clip
        if (hasClip) {
            Path path = getClipPath(canvas.getWidth(), canvas.getHeight());
            if (path != null) {
                canvas.clipPath(path);
            }
        }
        super.dispatchDraw(canvas);
    }

    /**
     * Available for use from C# to call ViewGroup's dispatchDraw()
     * @param canvas
     */
    protected final void viewGroupDispatchDraw(Canvas canvas) {
        super.dispatchDraw(canvas);
    }

    /**
     * Implemented in C# for Clip/Geometry logic
     * @param width
     * @param height
     * @return a Path to pass to canvas.clipPath() or null if not needed
     */
    protected abstract Path getClipPath(int width, int height);
}
