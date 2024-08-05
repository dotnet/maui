package com.microsoft.maui.glide;

import android.graphics.drawable.Drawable;
import android.os.Handler;
import android.os.Looper;
import android.widget.ImageView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.bumptech.glide.Glide;
import com.bumptech.glide.request.target.CustomViewTarget;
import com.bumptech.glide.request.transition.Transition;

import com.microsoft.maui.ImageLoaderCallback;
import com.microsoft.maui.PlatformInterop;
import com.microsoft.maui.glide.GlideLogging;

public class MauiCustomViewTarget extends CustomViewTarget<ImageView, Drawable> {
    private final ImageLoaderCallback callback;
    private boolean destroyed;
    private boolean failed;

    public MauiCustomViewTarget(@NonNull ImageView view, ImageLoaderCallback callback) {
        super(view);

        this.callback = callback;
    }

    @Override
    protected void onResourceCleared(@Nullable Drawable placeholder) {
        GlideLogging.v("onResourceCleared: setImageDrawable(placeholder)");
        view.setImageDrawable(placeholder);
        
        if (this.destroyed && !this.failed) {
            GlideLogging.v("onResourceCleared: setIsImageRecycled(true)");
            PlatformInterop.setGlideClearedTag(this.view, true);
        }
    }
    
    @Override
    public void onDestroy() {
        this.destroyed = true;
    }

    @Override
    public void onLoadFailed(@Nullable Drawable errorDrawable) {
        GlideLogging.v("onLoadFailed: errorDrawable");
        this.failed = true;
        view.setImageDrawable(errorDrawable);
        PlatformInterop.setGlideClearedTag(view, false);

        // trigger the callback out of this target
        post(() -> {
            GlideLogging.v("onLoadFailed: callback.onComplete(false)");
            callback.onComplete(false, errorDrawable, this::clear);
        });
    }

    @Override
    public void onResourceReady(@NonNull Drawable resource, @Nullable Transition<? super Drawable> transition) {
        GlideLogging.v("onResourceReady(resource)");

        // set the image
        view.setImageDrawable(resource);
        PlatformInterop.setGlideClearedTag(this.view, false);

        post(() -> {
            GlideLogging.v("onResourceReady: callback.onComplete(true)");
            callback.onComplete(true, resource, this::clear);
        });
    }

    private void post(Runnable runnable) {
        view.post(runnable);
    }

    private void clear() {
        //post(() -> {
            // TODO: Explicitly release image
            // https://github.com/dotnet/maui/issues/6464
            // https://github.com/dotnet/maui/pull/6543
        //});
    }
}