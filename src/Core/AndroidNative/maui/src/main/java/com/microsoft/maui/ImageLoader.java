package com.microsoft.maui;

import android.content.Context;
import android.graphics.Typeface;
import android.graphics.drawable.Drawable;
import android.net.Uri;
import android.widget.ImageView;

import androidx.annotation.ColorInt;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.bumptech.glide.Glide;
import com.bumptech.glide.RequestBuilder;
import com.bumptech.glide.RequestManager;
import com.bumptech.glide.load.DataSource;
import com.bumptech.glide.load.engine.DiskCacheStrategy;
import com.bumptech.glide.load.engine.GlideException;
import com.bumptech.glide.request.RequestListener;
import com.bumptech.glide.request.target.CustomTarget;
import com.bumptech.glide.request.target.CustomViewTarget;
import com.bumptech.glide.request.target.SimpleTarget;
import com.bumptech.glide.request.target.Target;
import com.bumptech.glide.request.transition.Transition;
import com.microsoft.maui.glide.fontimagesource.FontModel;

import java.io.File;
import java.io.InputStream;

public class ImageLoader {
    public static void loadFromFile(ImageView imageView, String file, ImageLoaderCallback callback)
    {
        RequestManager glide = Glide.with(imageView);

        MauiGlideViewTarget target = new MauiGlideViewTarget(imageView, callback, glide);

        glide
            .load(new File(file))
            .into(target);
    }

    public static void loadFromUri(ImageView imageView, String uri, Boolean cachingEnabled, ImageLoaderCallback callback)
    {
        Uri androidUri = Uri.parse(uri);

        if (androidUri == null) {
            callback.onComplete(false, null, null);
            return;
        }

        RequestManager glide = Glide.with(imageView);

        MauiGlideViewTarget target = new MauiGlideViewTarget(imageView, callback, glide);

        RequestBuilder<Drawable> builder = glide
            .load(androidUri);

        if (!cachingEnabled)
        {
            builder = builder
                .diskCacheStrategy(DiskCacheStrategy.NONE)
                .skipMemoryCache(true);
        }

        builder
            .into(target);
    }

    public static void loadFromStream(ImageView imageView, InputStream inputStream, ImageLoaderCallback callback)
    {
        RequestManager glide = Glide.with(imageView);

        MauiGlideViewTarget target = new MauiGlideViewTarget(imageView, callback, glide);

        glide
            .load(inputStream)
            .into(target);
    }

    public static void loadFromFont(ImageView imageView, @ColorInt int color, String glyph, Typeface typeface, float textSize, ImageLoaderCallback callback)
    {
        RequestManager glide = Glide.with(imageView);

        MauiGlideViewTarget target = new MauiGlideViewTarget(imageView, callback, glide);

        FontModel fontModel = new FontModel(color, glyph, textSize, typeface);

        glide
            .load(fontModel)
            .override(Target.SIZE_ORIGINAL, Target.SIZE_ORIGINAL)
            .into(target);
    }

    public static void loadFromFile(Context context, String file, ImageLoaderCallback callback)
    {
        RequestManager glide = Glide.with(context);

        MauiGlideTarget target = new MauiGlideTarget(callback, glide);

        glide
            .load(new File(file))
            .into(target);
    }

    public static void loadFromUri(Context context, String uri, Boolean cachingEnabled, ImageLoaderCallback callback)
    {
        Uri androidUri = Uri.parse(uri);

        if (androidUri == null) {
            callback.onComplete(false, null, null);
            return;
        }

        RequestManager glide = Glide.with(context);

        MauiGlideTarget target = new MauiGlideTarget(callback, glide);

        RequestBuilder<Drawable> builder = glide
            .load(androidUri);

        if (!cachingEnabled)
        {
            builder = builder
                .diskCacheStrategy(DiskCacheStrategy.NONE)
                .skipMemoryCache(true);
        }

        builder.into(target);
    }


    public static void loadFromStream(Context context, InputStream inputStream, ImageLoaderCallback callback)
    {
        RequestManager glide = Glide.with(context);

        MauiGlideTarget target = new MauiGlideTarget(callback, glide);

        glide
            .load(inputStream)
            .into(target);
    }


    public static void loadFromFont(Context context, @ColorInt int color, String glyph, Typeface typeface, float textSize, ImageLoaderCallback callback)
    {
        RequestManager glide = Glide.with(context);

        FontModel fontModel = new FontModel(color, glyph, textSize, typeface);

        MauiGlideTarget target = new MauiGlideTarget(callback, glide);

        glide
            .load(fontModel)
            .override(Target.SIZE_ORIGINAL, Target.SIZE_ORIGINAL)
            .into(target);
    }
}

class MauiGlideViewTarget extends CustomViewTarget<ImageView, Drawable>
{
    private final ImageLoaderCallback callback;
    private final RequestManager requestManager;

    public MauiGlideViewTarget(@NonNull ImageView view, ImageLoaderCallback callback, RequestManager requestManager) {
        super(view);

        this.callback = callback;
        this.requestManager = requestManager;
    }

    @Override
    protected void onResourceCleared(@Nullable Drawable placeholder) {

    }

    @Override
    public void onLoadFailed(@Nullable Drawable errorDrawable) {
        callback.onComplete(false, errorDrawable, new Runnable() {
            @Override
            public void run() {
                requestManager.clear(MauiGlideViewTarget.this);
            }
        });
    }

    @Override
    public void onResourceReady(@NonNull Drawable resource, @Nullable Transition<? super Drawable> transition) {
        callback.onComplete(true, resource, new Runnable() {
            @Override
            public void run() {
                requestManager.clear(MauiGlideViewTarget.this);
            }
        });
    }
}

class MauiGlideTarget extends CustomTarget<Drawable>
{
    private final ImageLoaderCallback callback;
    private final RequestManager requestManager;


    MauiGlideTarget(ImageLoaderCallback callback, RequestManager requestManager)
    {
        this.callback = callback;
        this.requestManager = requestManager;
    }

    @Override
    public void onLoadFailed(@Nullable Drawable errorDrawable) {
        super.onLoadFailed(errorDrawable);

        callback.onComplete(false, errorDrawable, new Runnable() {
            @Override
            public void run() {
                requestManager.clear(MauiGlideTarget.this);
            }
        });
    }

    @Override
    public void onResourceReady(@NonNull Drawable resource, @Nullable Transition<? super Drawable> transition) {
        callback.onComplete(true, resource, new Runnable() {
            @Override
            public void run() {
                requestManager.clear(MauiGlideTarget.this);
            }
        });
    }

    @Override
    public void onLoadCleared(@Nullable Drawable placeholder) {

    }
}
