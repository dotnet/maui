package com.microsoft.maui.glide;

import android.content.Context;
import android.graphics.drawable.Drawable;
import android.os.Handler;
import android.os.Looper;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.bumptech.glide.Glide;
import com.bumptech.glide.request.target.CustomTarget;
import com.bumptech.glide.request.transition.Transition;

import com.microsoft.maui.ImageLoaderCallback;

public class MauiCustomTarget extends CustomTarget<Drawable> {
    private final Context context;
    private final ImageLoaderCallback callback;
    private boolean completed;

    public MauiCustomTarget(Context context, ImageLoaderCallback callback) {
        this.completed = false;
        this.context = context;
        this.callback = callback;
    }

    @Override
    public void onLoadFailed(@Nullable Drawable errorDrawable) {
        if (completed)
            return;
        this.completed = true;

        // trigger the callback out of this target
        post(() -> callback.onComplete(false, errorDrawable, null));
    }

    @Override
    public void onResourceReady(@NonNull Drawable resource, @Nullable Transition<? super Drawable> transition) {
        if (completed)
            return;
        this.completed = true;

        // trigger the callback out of this target
        post(() -> callback.onComplete(true, resource, this::clear));
    }

    @Override
    public void onLoadCleared(@Nullable Drawable placeholder) {
    }

    private void post(Runnable runnable) {
        Looper looper = Looper.getMainLooper();
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