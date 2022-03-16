package com.microsoft.maui;

import android.content.Context;
import android.graphics.Typeface;
import android.graphics.drawable.Drawable;
import android.net.Uri;
import android.widget.ImageView;

import androidx.annotation.ColorInt;
import androidx.annotation.Nullable;

import com.bumptech.glide.Glide;
import com.bumptech.glide.RequestBuilder;
import com.bumptech.glide.RequestManager;
import com.bumptech.glide.load.DataSource;
import com.bumptech.glide.load.engine.DiskCacheStrategy;
import com.bumptech.glide.load.engine.GlideException;
import com.bumptech.glide.request.RequestListener;
import com.bumptech.glide.request.target.Target;
import com.microsoft.maui.glide.fontimagesource.FontModel;

import java.io.File;
import java.io.InputStream;

public class ImageLoader {
    public static void loadFromResourceId(ImageView imageView, int resourceId, ImageLoaderCallback callback)
    {
        imageView.setImageResource(resourceId);
        callback.onComplete(true, null);
    }

    public static void loadFromFile(ImageView imageView, String file, ImageLoaderCallback callback)
    {
        RequestManager glide = Glide.with(imageView);

        glide
            .load(new File(file))
            .addListener(new CallbackListener(callback, glide))
            .into(imageView);
    }

    public static void loadFromUri(ImageView imageView, String uri, Boolean cachingEnabled, ImageLoaderCallback callback)
    {
        Uri androidUri = Uri.parse(uri);

        if (androidUri == null) {
            callback.onComplete(false, null);
            return;
        }

        RequestManager glide = Glide.with(imageView);

        RequestBuilder<Drawable> builder = glide
            .load(androidUri);

        if (!cachingEnabled)
        {
            builder = builder
                .diskCacheStrategy(DiskCacheStrategy.NONE)
                .skipMemoryCache(true);
        }

        builder
            .addListener(new CallbackListener(callback, glide))
            .into(imageView);
    }

    public static void loadFromStream(ImageView imageView, InputStream inputStream, ImageLoaderCallback callback)
    {
        RequestManager glide = Glide.with(imageView);

        glide
            .load(inputStream)
            .addListener(new CallbackListener(callback, glide))
            .into(imageView);
    }

    public static void loadFromFont(ImageView imageView, @ColorInt int color, String glyph, Typeface typeface, float textSize, ImageLoaderCallback callback)
    {
        RequestManager glide = Glide.with(imageView);

        FontModel fontModel = new FontModel(color, glyph, textSize, typeface);

        glide
            .load(fontModel)
            .override(Target.SIZE_ORIGINAL, Target.SIZE_ORIGINAL)
            .addListener(new CallbackListener(callback, glide))
            .into(imageView);
    }

    public static void loadFromResourceId(Context context, int resourceId, ImageLoaderDrawableCallback callback)
    {
        RequestManager glide = Glide.with(context);

        glide
            .load(resourceId)
            .addListener(new CallbackListener(callback, glide));
    }

    public static void loadFromFile(Context context, String file, ImageLoaderDrawableCallback callback)
    {
        RequestManager glide = Glide.with(context);

        glide
            .load(new File(file))
            .addListener(new CallbackListener(callback, glide));
    }

    public static void loadFromUri(Context context, String uri, Boolean cachingEnabled, ImageLoaderDrawableCallback callback)
    {
        Uri androidUri = Uri.parse(uri);

        if (androidUri == null) {
            callback.onComplete(null, null);
            return;
        }

        RequestManager glide = Glide.with(context);

        RequestBuilder<Drawable> builder = glide
            .load(androidUri);

        if (!cachingEnabled)
        {
            builder = builder
                .diskCacheStrategy(DiskCacheStrategy.NONE)
                .skipMemoryCache(true);
        }

        builder
            .addListener(new CallbackListener(callback, glide));
    }


    public static void loadFromStream(Context context, InputStream inputStream, ImageLoaderDrawableCallback callback)
    {
        RequestManager glide = Glide.with(context);

        glide
            .load(inputStream)
            .addListener(new CallbackListener(callback, glide));
    }


    public static void loadFromFont(Context context, @ColorInt int color, String glyph, Typeface typeface, float textSize, ImageLoaderDrawableCallback callback)
    {
        RequestManager glide = Glide.with(context);

        FontModel fontModel = new FontModel(color, glyph, textSize, typeface);

        glide
            .load(fontModel)
            .override(Target.SIZE_ORIGINAL, Target.SIZE_ORIGINAL)
            .addListener(new CallbackListener(callback, glide));
    }
}

class CallbackListener implements RequestListener<Drawable>
{
    private final ImageLoaderCallback callback;
    private final ImageLoaderDrawableCallback callbackDrawable;
    private final RequestManager requestManager;


    CallbackListener(ImageLoaderCallback callback, RequestManager requestManager)
    {
        this.callback = callback;
        this.callbackDrawable = null;
        this.requestManager = requestManager;
    }

    CallbackListener(ImageLoaderDrawableCallback callback, RequestManager requestManager)
    {
        this.callback = null;
        this.callbackDrawable = callback;
        this.requestManager = requestManager;
    }

    @Override
    public boolean onLoadFailed(@Nullable GlideException e, Object model, Target<Drawable> target, boolean isFirstResource) {
        if (callbackDrawable != null)
            callbackDrawable.onComplete(null, null);
        else if (callback != null)
            callback.onComplete(false, null);
        return false;
    }

    @Override
    public boolean onResourceReady(Drawable resource, Object model, Target<Drawable> target, DataSource dataSource, boolean isFirstResource) {
        if (callbackDrawable != null) {
            callbackDrawable.onComplete(resource, new Runnable() {
                @Override
                public void run() {
                    requestManager.clear(target);
                }
            });

        } else if (callback != null) {
            callback.onComplete(true, new Runnable() {
                @Override
                public void run() {
                    requestManager.clear(target);
                }
            });
        }
        return false;
    }
}
