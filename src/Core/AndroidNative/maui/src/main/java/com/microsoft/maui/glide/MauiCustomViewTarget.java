package com.microsoft.maui.glide;

import android.graphics.drawable.Drawable;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import android.widget.ImageView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.bumptech.glide.Glide;
import com.bumptech.glide.request.target.CustomViewTarget;
import com.bumptech.glide.request.transition.Transition;

import com.microsoft.maui.ImageLoaderCallback;
import com.microsoft.maui.PlatformAppCompatImageView;
import com.microsoft.maui.PlatformInterop;

public class MauiCustomViewTarget extends CustomViewTarget<ImageView, Drawable> {
    private static final String TAG = "MauiCustomViewTarget";
    private static final boolean IS_VERBOSE_LOGGABLE = Log.isLoggable(TAG, Log.VERBOSE);

    private final ImageLoaderCallback callback;
    private final PlatformAppCompatImageView platformView;
    private boolean destroyed;
    private boolean failed;

    public MauiCustomViewTarget(@NonNull ImageView view, ImageLoaderCallback callback) {
        super(view);

        this.callback = callback;
        this.platformView = view instanceof PlatformAppCompatImageView ? (PlatformAppCompatImageView) view : null;
    }
    
    @Override
    protected void onResourceLoading(@Nullable Drawable placeholder) {
        if (platformView != null) {
            logV("onResourceLoading: freeze PlatformAppCompatImageView");
            platformView.freeze();
        }
    }

    @Override
    protected void onResourceCleared(@Nullable Drawable placeholder) {
        if (destroyed || platformView == null) {
            logV("onResourceCleared: setImageDrawable(placeholder)");
            view.setImageDrawable(placeholder);
        }
        
		if (this.destroyed && !this.failed) {
		    logV("setIsCleared: true");
            PlatformInterop.setGlideClearedTag(this.view, true);
        }
    }
    
    @Override
    public void onDestroy() {
        this.destroyed = true;
    }

    @Override
    public void onLoadFailed(@Nullable Drawable errorDrawable) {
        logV("onLoadFailed: errorDrawable");
        this.failed = true;
        view.setImageDrawable(errorDrawable);
        PlatformInterop.setGlideClearedTag(view, false);

        // trigger the callback out of this target
        post(() -> {
            logV("onLoadFailed: callback.onComplete");
            callback.onComplete(false, errorDrawable, this::clear);
        });
    }

    @Override
    public void onResourceReady(@NonNull Drawable resource, @Nullable Transition<? super Drawable> transition) {
        logV("onResourceReady: " + resource);

        // set the image
        view.setImageDrawable(resource);
        PlatformInterop.setGlideClearedTag(this.view, false);

        post(() -> {
            logV("onResourceReady: callback.onComplete");
            callback.onComplete(true, resource, this::clear);
        });
    }

    private void post(Runnable runnable) {
        view.post(runnable);
    }
    
    private static void logV(String message) {
        if (IS_VERBOSE_LOGGABLE) {
            Log.v(TAG, message);
        }
    }
    
    private boolean isFinished() {
        return !this.getRequest().isRunning();
    }

    private void clear() {
        //post(() -> {
            // TODO: Explicitly release image
            // https://github.com/dotnet/maui/issues/6464
            // https://github.com/dotnet/maui/pull/6543
        //});
    }
}