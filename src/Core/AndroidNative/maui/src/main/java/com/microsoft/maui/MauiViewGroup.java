package com.microsoft.maui;


import android.content.Context;
import android.content.res.ColorStateList;
import android.graphics.Color;
import android.graphics.Typeface;
import android.os.Build;
import android.text.TextUtils;
import android.util.AttributeSet;
import android.util.TypedValue;
import android.view.ViewGroup;
import android.view.*;
import android.widget.TextView;
import android.view.ViewParent;

import static android.R.attr.pivotX;

public class MauiViewGroup extends ViewGroup {

    public MauiViewGroup(Context context) {
        super(context);
        // TODO Auto-generated constructor stub
    }

    public MauiViewGroup(Context context, AttributeSet attrs) {
        super(context, attrs);
        // TODO Auto-generated constructor stub
    }

    public MauiViewGroup(Context context, AttributeSet attrs, int defStyle) {
        super(context, attrs, defStyle);
        // TODO Auto-generated constructor stub
    }

    public void measureAndLayout (int widthMeasureSpec, int heightMeasureSpec, int l, int t, int r, int b)
    {
        measure (widthMeasureSpec, heightMeasureSpec);
        layout (l, t, r, b);
    }

    @Override
    protected void onLayout(boolean changed, int l, int t, int r, int b) {
    }

    boolean inputTransparent;

    protected void setInputTransparent (boolean value)
    {
        inputTransparent = value;
    }

    protected boolean getInputTransparent ()
    {
        return inputTransparent;
    }

    @Override
    public boolean onInterceptTouchEvent (MotionEvent ev)
    {
        if (inputTransparent)
            return false;

        return super.onInterceptTouchEvent(ev);
    }

    @Override
    public boolean onTouchEvent (MotionEvent ev)
    {
        if (inputTransparent)
            return false;

        return super.onTouchEvent(ev);
    }

    public void sendBatchUpdate (
        float pivotX,
        float pivotY,
        int visibility,
        boolean enabled,
        float opacity,
        float rotation,
        float rotationX,
        float rotationY,
        float scaleX,
        float scaleY,
        float translationX,
        float translationY){
        setPivotX (pivotX);
        setPivotY (pivotY);

        if (getVisibility () != visibility)
            setVisibility (visibility);

        if (isEnabled () != enabled)
            setEnabled (enabled);

        setAlpha (opacity);
        setRotation (rotation);
        setRotationX (rotationX);
        setRotationY (rotationY);
        setScaleX (scaleX);
        setScaleY (scaleY);
        setTranslationX (translationX);
        setTranslationY (translationY);
    }

    public static void sendViewBatchUpdate (
        View view,
        float pivotX,
        float pivotY,
        int visibility,
        boolean enabled,
        float opacity,
        float rotation,
        float rotationX,
        float rotationY,
        float scaleX,
        float scaleY,
        float translationX,
        float translationY){
        view.setPivotX (pivotX);
        view.setPivotY (pivotY);

        if (view.getVisibility () != visibility)
            view.setVisibility (visibility);

        if (view.isEnabled () != enabled)
            view.setEnabled (enabled);

        view.setAlpha (opacity);
        view.setRotation (rotation);
        view.setRotationX (rotationX);
        view.setRotationY (rotationY);
        view.setScaleX (scaleX);
        view.setScaleY (scaleY);
        view.setTranslationX (translationX);
        view.setTranslationY (translationY);
    }
}
