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
import com.microsoft.maui.PlatformLogger;

public class MauiCustomViewTarget extends CustomViewTarget<ImageView, Drawable> {
    private static final PlatformLogger logger = new PlatformLogger("MauiCustomViewTarget");

    private final ImageLoaderCallback callback;
    private final String resourceLogIdentifier;
    private boolean completed;

    public MauiCustomViewTarget(@NonNull ImageView view, ImageLoaderCallback callback, Object model) {
        super(view);

        this.completed = false;
        this.callback = callback;

        if (logger.isVerboseLoggable && model != null) {
            this.resourceLogIdentifier = model.toString();
        } else {
            this.resourceLogIdentifier = null;
        }
    }

    @Override
    protected void onResourceCleared(@Nullable Drawable placeholder) {
        if (logger.isVerboseLoggable) logger.v("onResourceCleared: " + resourceLogIdentifier);

		this.view.setImageDrawable(placeholder);
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

        // set the image
        this.view.setImageDrawable(resource);

        callback.onComplete(true, resource, null);
    }
}