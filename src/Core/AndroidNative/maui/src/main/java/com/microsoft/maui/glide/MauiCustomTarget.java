package com.microsoft.maui.glide;

import android.content.Context;
import android.graphics.drawable.Drawable;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.bumptech.glide.Glide;
import com.bumptech.glide.request.target.CustomTarget;
import com.bumptech.glide.request.transition.Transition;

import com.microsoft.maui.ImageLoaderCallback;

public class MauiCustomTarget extends CustomTarget<Drawable> {
    private static final String TAG = "MauiCustomTarget";
    private static final boolean IS_VERBOSE_LOGGABLE = Log.isLoggable(TAG, Log.VERBOSE);

    private final Context context;
    private final ImageLoaderCallback callback;
    private final String resourceLogIdentifier;
    private boolean completed;

    public MauiCustomTarget(Context context, ImageLoaderCallback callback, Object model) {
        this.completed = false;
        this.context = context;
        this.callback = callback;

        if (IS_VERBOSE_LOGGABLE && model != null) {
            this.resourceLogIdentifier = model.toString();
        } else {
            this.resourceLogIdentifier = null;
        }
    }

    @Override
    public void onLoadFailed(@Nullable Drawable errorDrawable) {
        if (completed)
            return;

        this.completed = true;

        if (IS_VERBOSE_LOGGABLE) Log.v(TAG, "onLoadFailed: " + resourceLogIdentifier);

        // trigger the callback out of this target
        callback.onComplete(false, errorDrawable, null);
    }

    @Override
    public void onResourceReady(@NonNull Drawable resource, @Nullable Transition<? super Drawable> transition) {
        if (completed)
            return;
        this.completed = true;

        if (IS_VERBOSE_LOGGABLE) Log.v(TAG, "onResourceReady: " + resourceLogIdentifier);

        // trigger the callback out of this target
        callback.onComplete(true, resource, this::clear);
    }

    @Override
    public void onLoadCleared(@Nullable Drawable placeholder) {
        if (IS_VERBOSE_LOGGABLE) Log.v(TAG, "onLoadCleared: " + resourceLogIdentifier);
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
        post(() -> {
            Glide
                .with(context)
                .clear(this);
        });
    }
}