package com.microsoft.maui;

import android.content.Context;
import android.graphics.Bitmap;
import android.util.DisplayMetrics;
import com.bumptech.glide.load.engine.bitmap_recycle.BitmapPool;
import com.bumptech.glide.load.resource.bitmap.BitmapTransformation;
import com.bumptech.glide.load.Key;
import java.nio.ByteBuffer;
import java.security.MessageDigest;

class ClampBoundsToDisplayMetricsTransformation extends BitmapTransformation {
    private static final String ID = "com.microsoft.maui.ClampBoundsToDisplayMetricsTransformation";
    private DisplayMetrics displayMetrics;
    private String DisplayMetricsID;

    public ClampBoundsToDisplayMetricsTransformation(DisplayMetrics display) {
        this.displayMetrics = display;
        DisplayMetricsID = displayMetrics.widthPixels + "x" + displayMetrics.heightPixels;
    }

    @Override
    protected Bitmap transform(BitmapPool pool, Bitmap toTransform, int outWidth, int outHeight) {
        int width = toTransform.getWidth();
        int height = toTransform.getHeight();

        if (width <= displayMetrics.widthPixels && height <= displayMetrics.heightPixels) {
            return toTransform;
        }

        float aspectRatio = (float) width / (float) height;

        if (width > height) {
            outWidth = displayMetrics.widthPixels;
            outHeight = Math.round(displayMetrics.widthPixels / aspectRatio);
        } else {
            outHeight = displayMetrics.heightPixels;
            outWidth = Math.round(displayMetrics.heightPixels * aspectRatio);
        }

        return Bitmap.createScaledBitmap(toTransform, outWidth, outHeight, false);
    }

    @Override
     public boolean equals(Object o) {
       return o instanceof ClampBoundsToDisplayMetricsTransformation;
     }

      @Override
     public int hashCode() {
       return ID.hashCode();
     }

      @Override
     public void updateDiskCacheKey(MessageDigest messageDigest) {
       byte[] bytes = (ID + DisplayMetricsID).getBytes(Key.CHARSET);
       messageDigest.update(bytes);
     }
}
