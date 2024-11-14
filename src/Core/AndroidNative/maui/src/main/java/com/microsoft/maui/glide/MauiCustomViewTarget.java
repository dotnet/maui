package com.microsoft.maui.glide;

import android.graphics.drawable.Drawable;
import android.os.Handler;
import android.os.Looper;
import android.widget.ImageView;
import android.util.Log;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.bumptech.glide.Glide;
import com.bumptech.glide.request.target.CustomViewTarget;
import com.bumptech.glide.request.transition.Transition;

import com.microsoft.maui.ImageLoaderCallback;

public class MauiCustomViewTarget extends CustomViewTarget<ImageView, Drawable> {
    private static final String TAG = "MauiCustomViewTarget";
    private static final boolean IS_VERBOSE_LOGGABLE = Log.isLoggable(TAG, Log.VERBOSE);

    private final ImageLoaderCallback callback;
    private final String resourceLogIdentifier;
    private boolean completed;

    public MauiCustomViewTarget(@NonNull ImageView view, ImageLoaderCallback callback, Object model) {
        super(view);

        this.completed = false;
        this.callback = callback;

        if (IS_VERBOSE_LOGGABLE && model != null) {
            this.resourceLogIdentifier = model.toString();
        } else {
            this.resourceLogIdentifier = null;
        }
    }

    @Override
    protected void onResourceCleared(@Nullable Drawable placeholder) {
        if (IS_VERBOSE_LOGGABLE) Log.v(TAG, "onResourceCleared: " + resourceLogIdentifier);

		this.view.setImageDrawable(placeholder);
    }

    @Override
    public void onLoadFailed(@Nullable Drawable errorDrawable) {
        if (completed)
            return;

        this.completed = true;

        if (IS_VERBOSE_LOGGABLE) Log.v(TAG, "onLoadFailed: " + resourceLogIdentifier);

        // trigger the callback out of this target
        callback.onComplete(false, errorDrawable, this::clear);
    }

    @Override
    public void onResourceReady(@NonNull Drawable resource, @Nullable Transition<? super Drawable> transition) {
        if (completed)
            return;
        this.completed = true;

        if (IS_VERBOSE_LOGGABLE) Log.v(TAG, "onResourceReady: " + resourceLogIdentifier);

        // set the image
        this.view.setImageDrawable(resource);

        // trigger the callback out of this target
        callback.onComplete(true, resource, this::clear);
    }

    private void post(Runnable runnable) {
        Looper looper = Looper.getMainLooper();
        if (looper.isCurrentThread()) {
            runnable.run();
            return;
        }

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