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
        return isSolid;
    }

    public int getPaintType() {
        return paintType;
    }

    public int getSolidColor() {
        return solidColor;
    }

    public float[] getGradientBounds() {
        return gradientBounds;
    }

    private Shader getShader(int width, int height) {
        if (paintType == PlatformPaintType.NONE || paintType == PlatformPaintType.SOLID) {
            return null;
        }

        if (width != shaderWidth || height != shaderHeight) {
            shaderWidth = width;
            shaderHeight = height;
            shader = null;
        }

        if (shader == null) {
            if (paintType == PlatformPaintType.LINEAR) {
                shader = new LinearGradient(
                    gradientBounds[0] * width, gradientBounds[1] * height,  // Start point
                    gradientBounds[2] * width, gradientBounds[3] * height,  // End point
                    gradientColors,
                    gradientPositions,
                    Shader.TileMode.CLAMP
                );
            }
            else if (paintType == PlatformPaintType.RADIAL) {
                shader = new RadialGradient(
                    gradientBounds[0] * width, gradientBounds[1] * height,  // Center point
                    gradientBounds[2] * Math.max(width, height),            // Radius
                    gradientColors,
                    gradientPositions,
                    Shader.TileMode.CLAMP
                );
            }
        }

        return shader;
    }

    // Convenience method to apply all style properties at once
    public void setStyle(int paintType, boolean isSolid, int solidColor, int[] colors, float[] positions, float[] bounds) {
        this.paintType = paintType;
        this.isSolid = isSolid;
        this.solidColor = solidColor;
        this.gradientColors = colors;
        this.gradientPositions = positions;
        this.gradientBounds = bounds;
        this.shader = null; // Reset shader when style changes
    }

    public void applyStyle(Paint paint, int width, int height, PlatformDrawableStyle fallbackStyle) {
        if (paintType == PlatformPaintType.SOLID) {
            paint.setShader(null);
            paint.setColor(solidColor);
        } else if (paintType == PlatformPaintType.LINEAR || paintType == PlatformPaintType.RADIAL) {
            paint.setColor(Color.TRANSPARENT);
            paint.setShader(getShader(width, height));
        } else if (fallbackStyle != null) {
            fallbackStyle.applyStyle(paint, width, height, null);
        } else {
            if (hasNoneColor < 0) {
                applyTheme(context.getTheme());
            }

            paint.setShader(null);

            if (hasNoneColor > 0) {
                paint.setColor(noneColor);
            } else {
                paint.setColor(Color.TRANSPARENT);
            }
        }
    }

    private void applyTheme(Resources.Theme theme) {
        TypedValue value = new TypedValue();
        if (theme.resolveAttribute(android.R.attr.windowBackground, value, true) && isColorType(value)) {
            hasNoneColor = 1;
            noneColor = value.data;
        }

        hasNoneColor = 0;
        noneColor = 0;
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
