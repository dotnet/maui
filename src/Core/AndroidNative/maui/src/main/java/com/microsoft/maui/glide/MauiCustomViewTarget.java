package com.microsoft.maui.glide;

import android.graphics.drawable.Drawable;
import android.widget.ImageView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.bumptech.glide.RequestManager;
import com.bumptech.glide.request.target.CustomViewTarget;
import com.bumptech.glide.request.transition.Transition;
import com.microsoft.maui.ImageLoaderCallback;

public class MauiCustomViewTarget extends CustomViewTarget<ImageView, Drawable> {
    private final ImageLoaderCallback callback;
    private final RequestManager requestManager;
    private boolean completed;

    public MauiCustomViewTarget(@NonNull ImageView view, ImageLoaderCallback callback, RequestManager requestManager) {
        super(view);

        this.completed = false;
        this.callback = callback;
        this.requestManager = requestManager;
    }

    @Override
    protected void onResourceCleared(@Nullable Drawable placeholder) {
    }

    @Override
    public void onLoadFailed(@Nullable Drawable errorDrawable) {
        if (completed)
            return;
        this.completed = true;

        callback.onComplete(false,
            errorDrawable,
            () -> { } // TODO: explicitly release image
        );
    }

    @Override
    public void onResourceReady(@NonNull Drawable resource, @Nullable Transition<? super Drawable> transition) {
        if (completed)
            return;
        this.completed = true;

        this.view.setImageDrawable(resource);
        callback.onComplete(
            true,
            resource,
            () -> { } // TODO: explicitly release image
        );
    }
}
