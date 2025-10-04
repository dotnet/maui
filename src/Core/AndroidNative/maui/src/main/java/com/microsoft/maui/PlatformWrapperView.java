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

    private int paintType = PlatformPaintType.NONE;
    private float offsetX = 0;
    private float offsetY = 0;
    private float radius = 0;
    private int[] colors = new int[0];
    private float[] positions = new float[0];
    private float[] bounds = new float[0];

    @Override
    protected void setHasClip(boolean hasClip) {
        super.setHasClip(hasClip);
        this.hasClip = hasClip;
        shadowInvalidated = true;
    }

    protected final void updateShadow(int paintType, float radius, float offsetX, float offsetY, int[] colors, float[] positions, float[] bounds) {
        this.paintType = paintType;
        this.radius = radius;
        this.offsetX = offsetX;
        this.offsetY = offsetY;
        this.colors = colors;
        this.positions = positions;
        this.bounds = bounds;

        if (paintType == PlatformPaintType.NONE) {
            shadowPaint = null;
            shadowCanvas = null;
            if (shadowBitmap != null) {
                bitmapPool.put(shadowBitmap);
                shadowBitmap = null;
            }
        } else {
            shadowCanvas = new Canvas();
            shadowPaint = new Paint();
            shadowPaint.setAntiAlias(true);
            shadowPaint.setDither(true);
            shadowPaint.setFilterBitmap(true);
            shadowPaint.setStyle(Paint.Style.FILL_AND_STROKE);

            if (radius > 0) {
                shadowPaint.setMaskFilter(new BlurMaskFilter(radius, BlurMaskFilter.Blur.NORMAL));
            }

            if (paintType == PlatformPaintType.SOLID) {
                shadowPaint.setColor(colors.length > 0 ? colors[0] : android.graphics.Color.BLACK);
            }
        }

        shadowInvalidated = true;
        invalidate();
    }

    @Override
    protected void onDetachedFromWindow() {
        super.onDetachedFromWindow();
        shadowInvalidated = true;
        if (shadowBitmap != null) {
            bitmapPool.put(shadowBitmap);
            shadowBitmap = null;
        }
    }

    @Override
    protected void onLayout(boolean changed, int left, int top, int right, int bottom) {
        shadowInvalidated = true;
    }

    @Override
    public void requestLayout() {
        super.requestLayout();
        shadowInvalidated = true;
    }

    @Override
    public void invalidate() {
        super.invalidate();
        shadowInvalidated = true;
    }

    @Override
    protected void doMeasure(int widthMeasureSpec, int heightMeasureSpec) {
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
    public long measureAndGetWidthAndHeight(int widthMeasureSpec, int heightMeasureSpec) {
        if (getChildCount() == 0 || !(getChildAt(0) instanceof PlatformViewGroup)) {
            this.measure(widthMeasureSpec, heightMeasureSpec);
            int width = this.getMeasuredWidth();
            int height = this.getMeasuredHeight();
            long measure = ((long)width << 32) | (height & 0xffffffffL);
            return measure;
        } else {
            PlatformViewGroup child = (PlatformViewGroup)getChildAt(0);
            long measure = super.measureAndGetWidthAndHeight(widthMeasureSpec, heightMeasureSpec);
            child.measureAndGetWidthAndHeight(widthMeasureSpec, heightMeasureSpec);
            return measure;
        }
    }

    @Override
    public void overrideMeasuredDimension(int width, int height) {
        setMeasuredDimension(width, height);

        if (getChildCount() == 0 || !(getChildAt(0) instanceof PlatformViewGroup)) {
            return;
        }

        PlatformViewGroup child = (PlatformViewGroup)getChildAt(0);
        child.overrideMeasuredDimension(width, height);
    }

    @Override
    protected void dispatchDraw(Canvas canvas) {
        if (paintType != PlatformPaintType.NONE) {
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

        // Apply shader if needed
        updateShadowShader(bitmapWidth, bitmapHeight);

        Path clipPath = hasClip ? getClipPath(viewWidth, viewHeight) : null;

        canvas.save();
        canvas.translate(offsetX, offsetY);
        drawable.drawShadow(canvas, shadowPaint, clipPath);
        canvas.restore();
    }

    private void drawShadowViaDispatchDraw(@NonNull Canvas canvas, int viewWidth, int viewHeight) {
        if (shadowInvalidated) {
            shadowInvalidated = false;

            int radiusSafeSpace = getRadiusSafeSpace();
            int bitmapWidth = normalizeForPool(viewWidth + radiusSafeSpace);
            int bitmapHeight = normalizeForPool(viewHeight + radiusSafeSpace);
            int drawOriginX = (bitmapWidth - viewWidth) / 2;
            int drawOriginY = (bitmapHeight - viewHeight) / 2;

            if (shadowBitmap != null) {
                if (shadowBitmap.getWidth() == bitmapWidth && shadowBitmap.getHeight() == bitmapHeight) {
                    shadowBitmap.eraseColor(Color.TRANSPARENT);
                } else {
                    bitmapPool.put(shadowBitmap);
                    shadowBitmap = bitmapPool.get(bitmapWidth, bitmapHeight, Bitmap.Config.ARGB_8888);
                }
            } else {
                shadowBitmap = bitmapPool.get(bitmapWidth, bitmapHeight, Bitmap.Config.ARGB_8888);
            }

            shadowCanvas.setBitmap(shadowBitmap);

            // Create the local copy of all content to draw bitmap as a bottom layer of natural canvas.
            Bitmap extractAlpha = bitmapPool.get(normalizeForPool(viewWidth), normalizeForPool(viewHeight), Bitmap.Config.ALPHA_8);
            Canvas alphaCanvas = new Canvas(extractAlpha);
            super.dispatchDraw(alphaCanvas);

            // Apply shader if needed
            updateShadowShader(bitmapWidth, bitmapHeight);

            // Why don't we simply draw the alpha bitmap directly on the view canvas?
            // Reason: setMaskFilter (used by shadowPaint) is *not* supported in hardware accelerated mode
            // https://developer.android.com/develop/ui/views/graphics/hardware-accel
            // If we use `SOFTWARE` layer, than we fall into a view-clipped `Canvas` where we can't draw the outer shadow.
            shadowCanvas.drawBitmap(extractAlpha, drawOriginX, drawOriginY, shadowPaint);

            bitmapPool.put(extractAlpha);

            shadowBitmapX = offsetX - drawOriginX;
            shadowBitmapY = offsetY - drawOriginY;
        }
    
        // Draw shadow rectangle
        canvas.drawBitmap(shadowBitmap, shadowBitmapX, shadowBitmapY, null);
    }

    private int getRadiusSafeSpace() {
        // Account for potentially different blurring algorithms
        return (int)(radius * 3);
    }

    private static int normalizeForPool(int pixels) {
        // We want to reuse memory as much as possible so let's normalize bitmaps to the nearest 48px grid.
        return (int)(Math.ceil(((double)pixels) / 48.0) * 48.0);
    }

    private void updateShadowShader(int bitmapWidth, int bitmapHeight) {
        Shader shader = null;

        if (paintType == PlatformPaintType.LINEAR) {
            shader = new android.graphics.LinearGradient(
                bounds[0] * bitmapWidth, bounds[1] * bitmapHeight,  // Start point
                bounds[2] * bitmapWidth, bounds[3] * bitmapHeight,  // End point
                colors,
                positions,
                android.graphics.Shader.TileMode.CLAMP
            );
        } else if (paintType == PlatformPaintType.RADIAL) {
            shader = new android.graphics.RadialGradient(
                bounds[0] * bitmapWidth, bounds[1] * bitmapHeight,  // Center point
                bounds[2] * Math.max(bitmapWidth, bitmapHeight),   // Radius
                colors,
                positions,
                android.graphics.Shader.TileMode.CLAMP
            );
        }

        if (shader != null) {
            shadowPaint.setShader(shader);
        }
    }
}
