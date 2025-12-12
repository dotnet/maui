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
    public void setLinearGradientBackground(float x1, float y1, float x2, float y2, int[] colors, float[] positions) {
        float[] bounds = { x1, y1, x2, y2 };
        this.backgroundStyle.setStyle(PlatformPaintType.LINEAR, 0, colors, positions, bounds);
        invalidateSelf();
    }
    
    public void setRadialGradientBackground(float x, float y, float radius, int[] colors, float[] positions) {
        float[] bounds = { x, y, radius };
        this.backgroundStyle.setStyle(PlatformPaintType.RADIAL, 0, colors, positions, bounds);
        invalidateSelf();
    }
    
    public void setSolidBackground(int solidColor) {
        this.backgroundStyle.setStyle(PlatformPaintType.SOLID, solidColor, null, null, null);
        invalidateSelf();
    }

    public void setNoBackground() {
        this.backgroundStyle.setStyle(PlatformPaintType.NONE, 0, null, null, null);
        invalidateSelf();
    }

    public void setLinearGradientBorder(float x1, float y1, float x2, float y2, int[] colors, float[] positions) {
        float[] bounds = { x1, y1, x2, y2 };
        this.borderStyle.setStyle(PlatformPaintType.LINEAR, 0, colors, positions, bounds);
        invalidateSelf();
    }
    
    public void setRadialGradientBorder(float x, float y, float radius, int[] colors, float[] positions) {
        float[] bounds = { x, y, radius };
        this.borderStyle.setStyle(PlatformPaintType.RADIAL, 0, colors, positions, bounds);
        invalidateSelf();
    }
    
    public void setSolidBorder(int solidColor) {
        this.borderStyle.setStyle(PlatformPaintType.SOLID, solidColor, null, null, null);
        invalidateSelf();
    }

    public void setNoBorder() {
        this.borderStyle.setStyle(PlatformPaintType.NONE, 0, null, null, null);
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
        
        if (this.width != newWidth || this.height != newHeight) {
            this.width = newWidth;
            this.height = newHeight;
            this.invalidatePath = true;
        }

        super.onBoundsChange(bounds);
    }

    @Override
    protected void onDraw(Shape shape, Canvas canvas, Paint paint) {
        // Set background paint
        this.backgroundStyle.applyStyle(paint, this.width, this.height, null);

        if (this.hasShape) {

            // Configure border paint (when set it means there is a border - aka border thickness > 0)
            if (this.borderPaint != null) {
                Paint borderPaint = this.borderPaint;
                borderPaint.setStrokeWidth(strokeThickness);
                borderPaint.setStrokeJoin(strokeLineJoin);
                borderPaint.setStrokeCap(strokeLineCap);
                borderPaint.setStrokeMiter(this.strokeMiterLimit * 2);
                borderPaint.setPathEffect(this.borderPathEffect);
                this.borderStyle.applyStyle(borderPaint, this.width, this.height, this.backgroundStyle);
            }

            tryUpdateClipPath();

            if (this.clipPath != null) {
                boolean hasBackgroundPaint = this.backgroundStyle.getPaintType() != PlatformPaintType.NONE;
                if (hasBackgroundPaint) {
                    canvas.drawPath(this.clipPath, paint);
                }

                boolean hasBorderPaint = this.borderStyle.getPaintType() != PlatformPaintType.NONE;
                if (this.borderPaint != null && (hasBorderPaint || hasBackgroundPaint)) {
                    canvas.drawPath(this.clipPath, this.borderPaint);
                }

                return;
            }
            // else fallback to simple background drawing
        }

        // Simple background drawing (border is supported **only** with a shape)
        super.onDraw(shape, canvas, paint);
    }

    private void tryUpdateClipPath() {
        if (this.invalidatePath) {
            this.invalidatePath = false;
            
            if (this.hasShape) {
                updateClipPath(this.width, this.height);
            } else {
                this.fullClipPath = null;
                this.clipPath = null;
            }
        }
    }
    
    protected abstract void updateClipPath(int width, int height);

    // PlatformShadowDrawable implementation
    @Override
    public boolean canDrawShadow() {
        return this.backgroundStyle.getIsSolid() && (this.strokeThickness == 0 || this.borderStyle.getPaintType() == PlatformPaintType.NONE || this.borderStyle.getIsSolid());
    }

    @Override
    public void drawShadow(Canvas canvas, Paint shadowPaint, Path outerClipPath) {
        if (canvas == null || shadowPaint == null) {
            return;
        }

        Path contentPath;
        
        if (this.hasShape) {
            tryUpdateClipPath();
            if (this.fullClipPath == null) {
                return;
            }
            contentPath = this.fullClipPath;
        } else {
            contentPath = new Path();
            contentPath.addRect(0, 0, this.width, this.height, Path.Direction.CW);
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
