package com.microsoft.maui.glide;

import android.content.Context;
import android.graphics.drawable.Drawable;
import android.os.Handler;
import android.os.Looper;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.bumptech.glide.Glide;
import com.bumptech.glide.RequestBuilder;
import com.bumptech.glide.request.target.CustomTarget;
import com.bumptech.glide.request.transition.Transition;

import com.microsoft.maui.ImageLoaderCallback;
import com.microsoft.maui.PlatformLogger;
import com.microsoft.maui.glide.MauiTarget;

public class MauiCustomTarget extends CustomTarget<Drawable> implements MauiTarget {
    private static final PlatformLogger logger = new PlatformLogger("MauiCustomTarget");

    private final Context context;
    private final ImageLoaderCallback callback;
    private final String resourceLogIdentifier;
    private boolean completed;

    public MauiCustomTarget(Context context, ImageLoaderCallback callback, Object model) {
        this.completed = false;
        this.context = context;
        this.callback = callback;

        if (logger.isVerboseLoggable && model != null) {
            this.resourceLogIdentifier = model.toString();
        } else {
            this.resourceLogIdentifier = null;
        }
    }

    public void load(RequestBuilder<Drawable> builder) {
        builder.into(this);
    }

    @Override
    public void onLoadFailed(@Nullable Drawable errorDrawable) {
        if (completed)
            return;

        this.completed = true;

        if (logger.isVerboseLoggable) logger.v("onLoadFailed: " + resourceLogIdentifier);

        callback.onComplete(false, errorDrawable, null);
    }

    @Override
    public void onResourceReady(@NonNull Drawable resource, @Nullable Transition<? super Drawable> transition) {
        if (completed)
            return;
        this.completed = true;

        if (logger.isVerboseLoggable) logger.v("onResourceReady: " + resourceLogIdentifier);

        callback.onComplete(true, resource, this::clear);
    }

    @Override
    public void onLoadCleared(@Nullable Drawable placeholder) {
        if (logger.isVerboseLoggable) logger.v("onLoadCleared: " + resourceLogIdentifier);
    }

    private void post(Runnable runnable) {
        Looper looper = Looper.getMainLooper();
        if (looper.isCurrentThread()) {
            runnable.run();
            return;
        }

        Handler handler = new Handler(looper);
        handler.post(runnable);
    }

    private void clear() {
        // TODO: it looks like no one is really disposing the result on C# side
        // we must fix it there to release the Glide cache entry properly
        post(() -> {
            Glide
                .with(context)
                .clear(this);
        });
    }
}