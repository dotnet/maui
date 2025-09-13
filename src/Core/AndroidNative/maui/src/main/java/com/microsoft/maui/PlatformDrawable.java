package com.microsoft.maui;

import android.content.Context;
import android.content.res.Resources;
import android.graphics.BlurMaskFilter;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Path;
import android.graphics.PathEffect;
import android.graphics.Rect;
import android.graphics.drawable.PaintDrawable;
import android.graphics.drawable.shapes.Shape;
import android.graphics.drawable.shapes.RectShape;

import androidx.annotation.NonNull;

public abstract class PlatformDrawable extends PaintDrawable implements PlatformShadowDrawable {
    private float strokeThickness;
    private Paint.Join strokeLineJoin;
    private Paint.Cap strokeLineCap;
    private float strokeMiterLimit;
    private PathEffect borderPathEffect;
    private Paint borderPaint;

    // Shape and clipping
    private Path clipPath;
    private Path fullClipPath;
    private boolean hasShape;
    
    // Style properties
    private PlatformDrawableStyle backgroundStyle;
    private PlatformDrawableStyle borderStyle;
    
    // Dimensions
    private int width;
    private int height;
    
    // State flags
    private boolean invalidatePath = true;

    public PlatformDrawable(Context context) {
        super();
        this.clipPath = new Path();
        this.fullClipPath = new Path();
        this.backgroundStyle = new PlatformDrawableStyle(context);
        this.borderStyle = new PlatformDrawableStyle(null);
        setShape(new RectShape());
    }

    public void setStrokeThickness(float thickness) {
        this.strokeThickness = thickness;
        if (thickness == 0) {
            this.borderPaint = null;
        } else if (this.borderPaint == null) {
            this.borderPaint = new Paint(Paint.ANTI_ALIAS_FLAG);
			this.borderPaint.setStyle(Paint.Style.STROKE);
        }

        this.invalidateShapePath();
    }

    public float getStrokeThickness() {
        return strokeThickness;
    }

    public void setStrokeLineJoin(Paint.Join join) {
        this.strokeLineJoin = join;
        this.invalidateSelf();
    }

    public Paint.Join getStrokeLineJoin() {
        return strokeLineJoin;
    }

    public void setStrokeLineCap(Paint.Cap cap) {
        this.strokeLineCap = cap;
        this.invalidateSelf();
    }

    public Paint.Cap getStrokeLineCap() {
        return strokeLineCap;
    }

    public void setStrokeMiterLimit(float limit) {
        this.strokeMiterLimit = limit;
        this.invalidateSelf();
    }

    public float getStrokeMiterLimit() {
        return strokeMiterLimit;
    }

    public void setBorderPathEffect(PathEffect effect) {
        this.borderPathEffect = effect;
        this.invalidateSelf();
    }

    public PathEffect getBorderPathEffect() {
        return borderPathEffect;
    }

    // Shape and clipping
    public void setClipPath(Path path) {
        if (this.clipPath != null) {
            this.clipPath.reset();
            this.clipPath.set(path);
        }
    }

    public Path getClipPath() {
        return clipPath;
    }

    public void setFullClipPath(Path path) {
        if (this.fullClipPath != null) {
            this.fullClipPath.reset();
            this.fullClipPath.set(path);
        }
    }

    public Path getFullClipPath() {
        return fullClipPath;
    }

    public void shapeChanged(boolean hasShape) {
        this.hasShape = hasShape;
        this.invalidateShapePath();
    }

    // Simplified style management
    public void setBackgroundStyle(int paintType, boolean isSolid, int[] colors, float[] positions, float[] bounds) {
        backgroundStyle.setStyle(paintType, isSolid, 0, colors, positions, bounds);
        invalidateSelf();
    }

    public void setBorderStyle(int paintType, boolean isSolid, int[] colors, float[] positions, float[] bounds) {
        borderStyle.setStyle(paintType, isSolid, 0, colors, positions, bounds);
        invalidateSelf();
    }

    public void setBackgroundColor(int paintType, boolean isSolid, int solidColor) {
        backgroundStyle.setStyle(paintType, isSolid, solidColor, null, null, null);
        invalidateSelf();
    }

    public void setBorderColor(int paintType, boolean isSolid, int solidColor) {
        borderStyle.setStyle(paintType, isSolid, solidColor, null, null, null);
        invalidateSelf();
    }

    // State management
    public void invalidateShapePath() {
        this.invalidatePath = true;
        invalidateSelf();
    }

    @Override
    protected void onBoundsChange(Rect bounds) {
        int newWidth = bounds.width();
        int newHeight = bounds.height();
        
        if (width != newWidth || height != newHeight) {
            this.width = newWidth;
            this.height = newHeight;
            this.invalidatePath = true;
        }

        super.onBoundsChange(bounds);
    }

    @Override
    protected void onDraw(Shape shape, Canvas canvas, Paint paint) {
        // Set background paint
        backgroundStyle.applyStyle(paint, width, height, null);

        if (hasShape) {

            // Configure border paint (when set it means there is a border - aka border thickness > 0)
            if (borderPaint != null) {
                PlatformInterop.setPaintValues(borderPaint, strokeThickness, strokeLineJoin, strokeLineCap, strokeMiterLimit * 2, borderPathEffect);
                borderStyle.applyStyle(borderPaint, width, height, backgroundStyle);
            }

            tryUpdateClipPath();

            if (clipPath != null) {
                boolean hasBackgroundPaint = backgroundStyle.getPaintType() != PlatformPaintType.NONE;
                if (hasBackgroundPaint) {
                    canvas.drawPath(clipPath, paint);
                }

                boolean hasBorderPaint = borderStyle.getPaintType() != PlatformPaintType.NONE;
                if (borderPaint != null && (hasBorderPaint || hasBackgroundPaint)) {
                    canvas.drawPath(clipPath, borderPaint);
                }

                return;
            }
            // else fallback to simple background drawing
        }

        // Simple background drawing (border is supported **only** with a shape)
        super.onDraw(shape, canvas, paint);
    }

    private void tryUpdateClipPath() {
        if (invalidatePath) {
            invalidatePath = false;
            
            if (hasShape) {
                updateClipPath(width, height);
            } else {
                fullClipPath = null;
                clipPath = null;
            }
        }
    }
    
    protected abstract void updateClipPath(int width, int height);

    // PlatformShadowDrawable implementation
    @Override
    public boolean canDrawShadow() {
        return backgroundStyle.getIsSolid() && (strokeThickness == 0 || borderStyle.getIsSolid());
    }

    @Override
    public void drawShadow(Canvas canvas, Paint shadowPaint, Path outerClipPath) {
        if (canvas == null || shadowPaint == null) {
            return;
        }

        Path contentPath;
        
        if (hasShape) {
            tryUpdateClipPath();
            if (fullClipPath == null) {
                return;
            }
            contentPath = fullClipPath;
        } else {
            contentPath = new Path();
            contentPath.addRect(0, 0, width, height, Path.Direction.CW);
        }

        if (outerClipPath != null) {
            Path clippedPath = new Path();
            clippedPath.op(contentPath, outerClipPath, Path.Op.INTERSECT);
            canvas.drawPath(clippedPath, shadowPaint);
        } else {
            canvas.drawPath(contentPath, shadowPaint);
        }
    }
}
