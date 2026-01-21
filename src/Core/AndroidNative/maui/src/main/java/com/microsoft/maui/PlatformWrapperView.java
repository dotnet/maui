package com.microsoft.maui;

import android.app.Application;
import android.content.Context;
import android.os.Build;

import android.graphics.BlurMaskFilter;
import android.graphics.Color;
import android.graphics.Bitmap;
import android.graphics.drawable.Drawable;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.Path;
import android.graphics.PorterDuff;
import android.graphics.Rect;
import android.graphics.Shader;


import android.view.View;

import androidx.annotation.NonNull;

import com.microsoft.maui.PlatformPaintType;
import com.microsoft.maui.PlatformShadowDrawable;
import com.microsoft.maui.glide.ShadowBitmapPool;

import com.bumptech.glide.load.engine.bitmap_recycle.BitmapPool;
import com.bumptech.glide.load.engine.cache.MemorySizeCalculator;

public abstract class PlatformWrapperView extends PlatformContentViewGroup {

    public PlatformWrapperView(Context context) {
        super(context);
        this.viewBounds = new Rect();
        this.bitmapPool = ShadowBitmapPool.get(context);
        setClipChildren(false);
        setWillNotDraw(true);
    }

    private final BitmapPool bitmapPool;
    private final Rect viewBounds;

    private Paint shadowPaint;
    private Bitmap shadowBitmap;
    private float shadowBitmapX;
    private float shadowBitmapY;
    private Canvas shadowCanvas;
    private Shader shadowShader;
    private boolean shadowInvalidated = true;
    private boolean hasClip = false;

    private float offsetX = 0;
    private float offsetY = 0;
    private float radius = 0;

    private PlatformDrawableStyle shadowStyle = new PlatformDrawableStyle(null);

    @Override
    protected void setHasClip(boolean hasClip) {
        super.setHasClip(hasClip);
        this.hasClip = hasClip;
        this.shadowInvalidated = true;
    }

    protected final void setLinearGradientShadow(float radius, float offsetX, float offsetY, float x1, float y1, float x2, float y2, int[] colors, float[] positions) {
        this.radius = radius;
        this.offsetX = offsetX;
        this.offsetY = offsetY;
        float[] bounds = { x1, y1, x2, y2 };
        this.shadowStyle.setStyle(PlatformPaintType.LINEAR, 0, colors, positions, bounds);
        onShadowStyleChanged();
    }
    
    protected final void setRadialGradientShadow(float radius, float offsetX, float offsetY, float x, float y, float gradientRadius, int[] colors, float[] positions) {
        this.radius = radius;
        this.offsetX = offsetX;
        this.offsetY = offsetY;
        float[] bounds = { x, y, gradientRadius };
        this.shadowStyle.setStyle(PlatformPaintType.RADIAL, 0, colors, positions, bounds);
        onShadowStyleChanged();
    }
    
    protected final void setSolidShadow(float radius, float offsetX, float offsetY, int solidColor) {
        this.radius = radius;
        this.offsetX = offsetX;
        this.offsetY = offsetY;
        this.shadowStyle.setStyle(PlatformPaintType.SOLID, solidColor, null, null, null);
        onShadowStyleChanged();
    }

    protected final void setNoShadow() {
        this.radius = 0;
        this.offsetX = 0;
        this.offsetY = 0;
        this.shadowStyle.setStyle(PlatformPaintType.NONE, 0, null, null, null);
        onShadowStyleChanged();
    }

    private void onShadowStyleChanged() {
        if (this.shadowStyle.getPaintType() == PlatformPaintType.NONE) {
            this.shadowPaint = null;
            this.shadowCanvas = null;
            if (this.shadowBitmap != null) {
                this.bitmapPool.put(this.shadowBitmap);
                this.shadowBitmap = null;
            }
        } else {
            this.shadowCanvas = new Canvas();
            this.shadowPaint = new Paint();
            this.shadowPaint.setAntiAlias(true);
            this.shadowPaint.setDither(true);
            this.shadowPaint.setFilterBitmap(true);
            this.shadowPaint.setStyle(Paint.Style.FILL_AND_STROKE);

            if (this.radius > 0) {
                this.shadowPaint.setMaskFilter(new BlurMaskFilter(this.radius, BlurMaskFilter.Blur.NORMAL));
            }
        }

        this.shadowInvalidated = true;
        invalidate();
    }

    @Override
    protected void onDetachedFromWindow() {
        super.onDetachedFromWindow();
        this.shadowInvalidated = true;
        if (this.shadowBitmap != null) {
            this.bitmapPool.put(this.shadowBitmap);
            this.shadowBitmap = null;
        }
    }

    @Override
    protected void onLayout(boolean changed, int left, int top, int right, int bottom) {
        this.shadowInvalidated = true;
    }

    @Override
    public void requestLayout() {
        super.requestLayout();
        this.shadowInvalidated = true;
    }

    @Override
    public void invalidate() {
        super.invalidate();
        this.shadowInvalidated = true;
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
        if (this.shadowStyle.getPaintType() != PlatformPaintType.NONE) {
            int viewWidth = this.viewBounds.width();
            int viewHeight = this.viewBounds.height();
            if (getChildCount() > 0)
            {
                View child = getChildAt(0);
                if (viewWidth == 0)
                    viewWidth = child.getMeasuredWidth();
                if (viewHeight == 0)
                    viewHeight = child.getMeasuredHeight();
            }

            if (viewWidth > 0 && viewHeight > 0) {
                drawShadow(canvas, viewWidth, viewHeight);
            }
        }

        super.dispatchDraw(canvas);
    }

    protected void drawShadow(@NonNull Canvas canvas, int viewWidth, int viewHeight) {
        if (getChildCount() > 0)
        {
            View child = getChildAt(0);
            Drawable background = child.getBackground();
            // See if we can quickly draw shadow through Canvas API thanks to the fact we have a solid content
            if (background != null && background instanceof PlatformShadowDrawable && ((PlatformShadowDrawable)background).canDrawShadow()) {
                // Layout has already happened on the child view, but not on its drawable, so we need to set the bounds manually
                int left = child.getLeft();
                int top = child.getTop();
                int right = child.getRight();
                int bottom = child.getBottom();
                background.setBounds(0, 0, right - left, bottom - top);
                // Draw shadow through the drawable
                drawShadowViaPlatformShadowDrawable(canvas, (PlatformShadowDrawable)background, viewWidth, viewHeight);
                return;
            }

            // Otherwise, draw shadow through dispatchDraw / bitmap generation / .. (very expensive)
            drawShadowViaDispatchDraw(canvas, viewWidth, viewHeight);
        }
    }

    private void drawShadowViaPlatformShadowDrawable(@NonNull Canvas canvas, @NonNull PlatformShadowDrawable drawable, int viewWidth, int viewHeight) {
        int radiusSafeSpace = getRadiusSafeSpace();
        int bitmapWidth = viewWidth + radiusSafeSpace;
        int bitmapHeight = viewHeight + radiusSafeSpace;

        // Apply shadow style
        this.shadowStyle.applyStyle(this.shadowPaint, bitmapWidth, bitmapHeight, null);

        Path clipPath = this.hasClip ? getClipPath(viewWidth, viewHeight) : null;

        canvas.save();
        canvas.translate(this.offsetX, this.offsetY);
        drawable.drawShadow(canvas, this.shadowPaint, clipPath);
        canvas.restore();
    }

    private void drawShadowViaDispatchDraw(@NonNull Canvas canvas, int viewWidth, int viewHeight) {
        if (this.shadowInvalidated) {
            this.shadowInvalidated = false;

            int radiusSafeSpace = getRadiusSafeSpace();
            int bitmapWidth = normalizeForPool(viewWidth + radiusSafeSpace);
            int bitmapHeight = normalizeForPool(viewHeight + radiusSafeSpace);
            int drawOriginX = (bitmapWidth - viewWidth) / 2;
            int drawOriginY = (bitmapHeight - viewHeight) / 2;

            if (this.shadowBitmap != null) {
                if (this.shadowBitmap.getWidth() == bitmapWidth && this.shadowBitmap.getHeight() == bitmapHeight) {
                    this.shadowBitmap.eraseColor(Color.TRANSPARENT);
                } else {
                    this.bitmapPool.put(this.shadowBitmap);
                    this.shadowBitmap = this.bitmapPool.get(bitmapWidth, bitmapHeight, Bitmap.Config.ARGB_8888);
                }
            } else {
                this.shadowBitmap = this.bitmapPool.get(bitmapWidth, bitmapHeight, Bitmap.Config.ARGB_8888);
            }

            this.shadowCanvas.setBitmap(this.shadowBitmap);

            // Create the local copy of all content to draw bitmap as a bottom layer of natural canvas.
            Bitmap extractAlpha = this.bitmapPool.get(normalizeForPool(viewWidth), normalizeForPool(viewHeight), Bitmap.Config.ALPHA_8);
            Canvas alphaCanvas = new Canvas(extractAlpha);
            super.dispatchDraw(alphaCanvas);

            // Apply shadow style
            this.shadowStyle.applyStyle(this.shadowPaint, bitmapWidth, bitmapHeight, null);

            // Why don't we simply draw the alpha bitmap directly on the view canvas?
            // Reason: setMaskFilter (used by shadowPaint) is *not* supported in hardware accelerated mode
            // https://developer.android.com/develop/ui/views/graphics/hardware-accel
            // If we use `SOFTWARE` layer, than we fall into a view-clipped `Canvas` where we can't draw the outer shadow.
            this.shadowCanvas.drawBitmap(extractAlpha, drawOriginX, drawOriginY, this.shadowPaint);

            this.bitmapPool.put(extractAlpha);

            this.shadowBitmapX = this.offsetX - drawOriginX;
            this.shadowBitmapY = this.offsetY - drawOriginY;
        }
    
        // Draw shadow rectangle
        canvas.drawBitmap(this.shadowBitmap, this.shadowBitmapX, this.shadowBitmapY, null);
    }

    private int getRadiusSafeSpace() {
        // Account for potentially different blurring algorithms
        return (int)(this.radius * 3);
    }

    private static int normalizeForPool(int pixels) {
        // We want to reuse memory as much as possible so let's normalize bitmaps to the nearest 48px grid.
        return (int)(Math.ceil(((double)pixels) / 48.0) * 48.0);
    }
}
