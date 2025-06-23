package com.microsoft.maui.glide.font;

import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.Typeface;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.bumptech.glide.load.Options;
import com.bumptech.glide.load.ResourceDecoder;
import com.bumptech.glide.load.engine.Resource;
import com.bumptech.glide.load.engine.bitmap_recycle.BitmapPool;
import com.bumptech.glide.load.engine.bitmap_recycle.BitmapPoolAdapter;
import com.bumptech.glide.load.resource.bitmap.BitmapResource;

import com.microsoft.maui.PlatformLogger;

public class FontModelResourceDecoder implements ResourceDecoder<FontModel, Bitmap> {
    private static final PlatformLogger logger = new PlatformLogger("FontModelResDecoder");

    private final BitmapPool bitmapPool;

    public FontModelResourceDecoder()
    {
        bitmapPool = new BitmapPoolAdapter();
    }

    @Override
    public boolean handles(@NonNull FontModel model, @NonNull Options options) {
        return true;
    }

    @Nullable
    @Override
    public Resource<Bitmap> decode(@NonNull FontModel model, int width, int height, @NonNull Options options) {
        Paint paint = new Paint();
        paint.setTextSize(model.getTextSize());
        paint.setAntiAlias(true);
        paint.setColor(model.getColor());
        paint.setTextAlign(Paint.Align.LEFT);

        Typeface tf = model.getTypeface();
        if (tf != null)
        {
            paint.setTypeface(tf);
        }

        int paintWidth = (int)(paint.measureText(model.getGlyph()) + .5f);
        float baseline = (int)(-paint.ascent() + .5f);
        int paintHeight = (int)(baseline + paint.descent() + .5f);


        Bitmap bmp = bitmapPool.get(paintWidth, paintHeight, Bitmap.Config.ARGB_8888);

        Canvas canvas = new Canvas(bmp);
        canvas.drawText(model.getGlyph(), 0, baseline, paint);

        if (logger.isVerboseLoggable) logger.v(model.toString());

        return new BitmapResource(bmp, bitmapPool);
    }
}
