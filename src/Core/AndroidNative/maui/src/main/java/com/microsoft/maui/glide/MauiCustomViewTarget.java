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

public class MauiCustomViewTarget extends CustomViewTarget<ImageView, Drawable> {
    private final ImageLoaderCallback callback;
    private boolean completed;

    public MauiCustomViewTarget(@NonNull ImageView view, ImageLoaderCallback callback) {
        super(view);

        this.completed = false;
        this.callback = callback;
    }

    @Override
    protected void onResourceCleared(@Nullable Drawable placeholder) {
		post(() -> this.view.setImageDrawable(placeholder));
    }

    @Override
    public void onLoadFailed(@Nullable Drawable errorDrawable) {
        if (completed)
            return;
        this.completed = true;

        // trigger the callback out of this target
        post(() -> callback.onComplete(false, errorDrawable, this::clear));
    }

    @Override
    public void onResourceReady(@NonNull Drawable resource, @Nullable Transition<? super Drawable> transition) {
        if (completed)
            return;
        this.completed = true;

        post(() -> {
            // set the image
            this.view.setImageDrawable(resource);

            // trigger the callback out of this target
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