package com.microsoft.maui;

import android.content.Context;
import android.graphics.Bitmap;
import com.bumptech.glide.load.engine.bitmap_recycle.BitmapPool;
import com.bumptech.glide.load.resource.bitmap.BitmapTransformation;
import com.bumptech.glide.load.Key;
import java.nio.ByteBuffer;
import java.security.MessageDigest;

class ResizeTransformation extends BitmapTransformation {
    private static final String ID = "com.microsoft.maui.ResizeTransformation";
    private static final byte[] ID_BYTES = ID.getBytes(Key.CHARSET);
    private int maxWidth;

    public ResizeTransformation(int maxWidth) {
        this.maxWidth = maxWidth;
    }

    @Override
    protected Bitmap transform(BitmapPool pool, Bitmap toTransform, int outWidth, int outHeight) {
        if (toTransform.getWidth() <= maxWidth) {
            return toTransform;
        }
        float aspectRatio = (float) toTransform.getHeight() / (float) toTransform.getWidth();
        int targetHeight = Math.round(maxWidth * aspectRatio);

        return Bitmap.createScaledBitmap(toTransform, maxWidth, targetHeight, false);
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
