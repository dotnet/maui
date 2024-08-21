package com.microsoft.maui;

import android.content.Context;
import android.graphics.Bitmap;
import android.util.DisplayMetrics;
import com.bumptech.glide.load.engine.bitmap_recycle.BitmapPool;
import com.bumptech.glide.load.resource.bitmap.BitmapTransformation;
import com.bumptech.glide.load.Key;
import java.nio.ByteBuffer;
import java.security.MessageDigest;

class ResizeTransformation extends BitmapTransformation {
    private static final String ID = "com.microsoft.maui.ResizeTransformation";
    private static final byte[] ID_BYTES = ID.getBytes(Key.CHARSET);
    private DisplayMetrics displayMetrixs;

    public ResizeTransformation(DisplayMetrics display) {
        this.displayMetrixs = display;
    }

    @Override
    protected Bitmap transform(BitmapPool pool, Bitmap toTransform, int outWidth, int outHeight) {
        int width = toTransform.getWidth();
        int height = toTransform.getHeight();

        if (width <= display.widthPixels && height <= display.heightPixels) {
            return toTransform;
        }

        float aspectRatio = (float) width / (float) height;

        if (width > height) {
            outWidth = display.widthPixels;
            outHeight = Math.round(display.widthPixels / aspectRatio);
        } else {
            outHeight = display.heightPixels;
            outWidth = Math.round(display.heightPixels * aspectRatio);
        }

        return Bitmap.createScaledBitmap(toTransform, outWidth, outHeight, false);
    }

    @Override
    public boolean equals(Object o) {
        if (o instanceof ResizeTransformation) {
            ResizeTransformation other = (ResizeTransformation) o;
            return maxWidth == other.maxWidth;
        }
        return false;
    }

    @Override
    public int hashCode() {
        return ID.hashCode() + maxWidth * 31;
    }

    @Override
    public void updateDiskCacheKey(MessageDigest messageDigest) {
        messageDigest.update(ID_BYTES);
        byte[] maxWidthData = ByteBuffer.allocate(4).putInt(maxWidth).array();
        messageDigest.update(maxWidthData);
    }
}
