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
import com.microsoft.maui.PlatformAppCompatImageView;
import com.microsoft.maui.PlatformInterop;
import com.microsoft.maui.glide.GlideLogging;

public class MauiCustomViewTarget extends CustomViewTarget<ImageView, Drawable> {
    private final ImageLoaderCallback callback;
    private final PlatformAppCompatImageView platformView;
    private boolean destroyed;
    private boolean failed;
    private GlideLogging logger;

    public MauiCustomViewTarget(@NonNull ImageView view, ImageLoaderCallback callback) {
        super(view);

        this.logger = new GlideLogging("MauiCustomViewTarget");
        this.callback = callback;
        this.platformView = view instanceof PlatformAppCompatImageView ? (PlatformAppCompatImageView) view : null;
    }

    @Override
    protected void onResourceCleared(@Nullable Drawable placeholder) {
        if (destroyed || platformView == null) {
            logger.v("onResourceCleared: setImageDrawable(placeholder)");
            view.setImageDrawable(placeholder);
        } else if (platformView != null) {
            logger.v("onResourceCleared: freeze()");
            // if we're switching the image with another one, don't set the empty placeholder (null)
            // and simply freeze the view to prevent it from accessing the bitmap which is about to be recycled
            // See more: https://bumptech.github.io/glide/javadocs/4140/library/com.bumptech.glide.request.target/-target/on-load-cleared.html
            this.platformView.freeze();
        }
        
        if (this.destroyed && !this.failed) {
            logger.v("onResourceCleared: setIsImageRecycled(true)");
            PlatformInterop.setGlideClearedTag(this.view, true);
        }
    }
    
    @Override
    public void onDestroy() {
        this.destroyed = true;
    }

    @Override
    public void onLoadFailed(@Nullable Drawable errorDrawable) {
        logger.v("onLoadFailed: errorDrawable");
        this.failed = true;
        view.setImageDrawable(errorDrawable);
        PlatformInterop.setGlideClearedTag(view, false);

        // trigger the callback out of this target
        post(() -> {
            logger.v("onLoadFailed: callback.onComplete(false)");
            callback.onComplete(false, errorDrawable, this::clear);
        });
    }

    @Override
    public void onResourceReady(@NonNull Drawable resource, @Nullable Transition<? super Drawable> transition) {
        logger.v("onResourceReady: " + resource);

        // set the image
        view.setImageDrawable(resource);
        PlatformInterop.setGlideClearedTag(this.view, false);

        post(() -> {
            logger.v("onResourceReady: callback.onComplete(true)");
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