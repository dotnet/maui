package com.microsoft.maui;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Path;
import android.util.AttributeSet;
import android.view.ViewGroup;

public abstract class PlatformViewGroup extends ViewGroup {
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
}
