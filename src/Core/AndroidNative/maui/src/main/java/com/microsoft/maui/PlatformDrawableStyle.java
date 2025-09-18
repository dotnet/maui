package com.microsoft.maui;

import android.content.Context;
import android.content.res.Resources;
import android.graphics.Shader;
import android.graphics.Paint;
import android.graphics.Color;
import android.graphics.LinearGradient;
import android.graphics.RadialGradient;
import android.os.Build;
import android.util.TypedValue;

public class PlatformDrawableStyle {
    // Style properties
    private boolean isSolid;
    private boolean isSolidInvalidated = true;
    private int paintType = PlatformPaintType.NONE;
    private int solidColor;
    private int[] gradientColors;
    private float[] gradientPositions;
    private float[] gradientBounds;
    private Shader shader;
    private int shaderWidth;
    private int shaderHeight;
    private int noneColor = 0;
    private int hasNoneColor = -1;
    private Context context;

    public PlatformDrawableStyle(Context context) {
        this.context = context;
    }

    // Getters and setters
    public boolean getIsSolid() {
        if (this.isSolidInvalidated) {
            this.isSolid = computeIsSolid();
            this.isSolidInvalidated = false;
        }

        return this.isSolid;
    }

    private boolean computeIsSolid() {
        if (this.gradientColors != null) {
            for (int i = 0; i < this.gradientColors.length; i++) {
                if (Color.alpha(this.gradientColors[i]) != 255) {
                    return false;
                }
            }
            
            return true;
        }

        if (this.paintType == PlatformPaintType.NONE) {
            if (this.hasNoneColor < 0) {
                applyTheme();
            }

            if (this.hasNoneColor == 1) {
                return Color.alpha(this.noneColor) == 255;
            }

            // None means no background, so that's = to transparent
            return false;
        }

        return Color.alpha(this.solidColor) == 255;
    } 

    public int getPaintType() {
        return this.paintType;
    }

    private Shader getShader(int width, int height) {
        if (this.paintType == PlatformPaintType.NONE || this.paintType == PlatformPaintType.SOLID) {
            return null;
        }

        if (width != this.shaderWidth || height != this.shaderHeight) {
            this.shaderWidth = width;
            this.shaderHeight = height;
            this.shader = null;
        }

        if (this.shader == null) {
            if (this.paintType == PlatformPaintType.LINEAR) {
                this.shader = new LinearGradient(
                    this.gradientBounds[0] * width, this.gradientBounds[1] * height,  // Start point
                    this.gradientBounds[2] * width, this.gradientBounds[3] * height,  // End point
                    this.gradientColors,
                    this.gradientPositions,
                    Shader.TileMode.CLAMP
                );
            }
            else if (this.paintType == PlatformPaintType.RADIAL) {
                this.shader = new RadialGradient(
                    this.gradientBounds[0] * width, this.gradientBounds[1] * height,  // Center point
                    this.gradientBounds[2] * Math.max(width, height),            // Radius
                    this.gradientColors,
                    this.gradientPositions,
                    Shader.TileMode.CLAMP
                );
            }
        }

        return this.shader;
    }

    // Convenience method to apply all style properties at once
    public void setStyle(int paintType, int solidColor, int[] colors, float[] positions, float[] bounds) {
        this.paintType = paintType;
        this.solidColor = solidColor;
        this.gradientColors = colors;
        this.gradientPositions = positions;
        this.gradientBounds = bounds;
        this.shader = null; // Reset shader when style changes
        this.isSolidInvalidated = true; // isSolid needs to be re-evaluated
    }

    public void applyStyle(Paint paint, int width, int height, PlatformDrawableStyle fallbackStyle) {
        if (this.paintType == PlatformPaintType.SOLID) {
            paint.setShader(null);
            paint.setColor(this.solidColor);
        } else if (this.paintType == PlatformPaintType.LINEAR || this.paintType == PlatformPaintType.RADIAL) {
            // Reset the color to its default value so that a shader can be applied on top of it
            paint.setColor(Color.BLACK);
            paint.setShader(getShader(width, height));
        } else if (fallbackStyle != null) {
            fallbackStyle.applyStyle(paint, width, height, null);
        } else {
            if (this.hasNoneColor < 0) {
                applyTheme();
            }

            paint.setShader(null);

            if (this.hasNoneColor > 0) {
                paint.setColor(this.noneColor);
            } else {
                paint.setColor(Color.TRANSPARENT);
            }
        }
    }

    private void applyTheme() {
        TypedValue value = new TypedValue();
        if (this.context != null && this.context.getTheme().resolveAttribute(android.R.attr.windowBackground, value, true) && isColorType(value)) {
            this.hasNoneColor = 1;
            this.noneColor = value.data;
        }

        this.hasNoneColor = 0;
        this.noneColor = 0;
    }

    private static boolean isColorType(TypedValue value)
    {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            return value.isColorType();
        } else {
            // Implementation from AOSP
            return (value.type >= TypedValue.TYPE_FIRST_COLOR_INT && value.type <= TypedValue.TYPE_LAST_COLOR_INT);
        }
    }
}
