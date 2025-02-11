package com.microsoft.maui.glide;

import android.content.ComponentCallbacks2;
import android.content.Context;
import android.content.res.Configuration;
import android.graphics.Bitmap;
import android.util.LruCache;
import com.bumptech.glide.load.engine.bitmap_recycle.BitmapPool;
import com.bumptech.glide.load.engine.bitmap_recycle.LruBitmapPool;
import com.bumptech.glide.load.engine.cache.MemorySizeCalculator;

public class ShadowBitmapPool extends LruBitmapPool implements ComponentCallbacks2 {
    private static ShadowBitmapPool bitmapPool;

    public static BitmapPool get(Context context) {
        if (bitmapPool == null) {
            synchronized (ShadowBitmapPool.class) {
                if (bitmapPool == null) {
                    // Use application context to avoid memory leaks
                    Context applicationContext = context.getApplicationContext();
                    bitmapPool = createBitmapPool(applicationContext);
                    applicationContext.registerComponentCallbacks(bitmapPool);
                }
            }
        }
        return bitmapPool;
    }

    private static ShadowBitmapPool createBitmapPool(Context context) {
        MemorySizeCalculator memorySizeCalculator = new MemorySizeCalculator.Builder(context).build();
        int poolSize = memorySizeCalculator.getBitmapPoolSize();
        return new ShadowBitmapPool(poolSize);
    }

    private ShadowBitmapPool(int maxSize) {
        super(maxSize);
    }

    @Override
    public void onTrimMemory(int level) {
        trimMemory(level);
    }

    @Override
    public void onLowMemory() {
        trimMemory(TRIM_MEMORY_UI_HIDDEN);
    }

    @Override
    public void onConfigurationChanged(Configuration newConfig) {
        // Do nothing.
    }
}
