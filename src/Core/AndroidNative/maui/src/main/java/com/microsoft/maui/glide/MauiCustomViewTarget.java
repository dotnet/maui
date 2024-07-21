package com.microsoft.maui.glide;

import android.graphics.drawable.Drawable;
import android.os.Handler;
import android.os.Looper;
import android.widget.ImageView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.bumptech.glide.Glide;
import com.bumptech.glide.request.target.DrawableImageViewTarget;
import com.bumptech.glide.request.transition.Transition;

import com.microsoft.maui.ImageLoaderCallback;

public class MauiCustomViewTarget extends DrawableImageViewTarget {
    private final ImageLoaderCallback callback;

    public MauiCustomViewTarget(@NonNull ImageView view, ImageLoaderCallback callback) {
        super(view);

        this.callback = callback;
    }

    @Override
    public void onLoadFailed(@Nullable Drawable errorDrawable) {
        super.onLoadFailed(errorDrawable);
        
        // trigger the callback out of this target
        post(() -> callback.onComplete(false, errorDrawable, this::clear));
    }

    @Override
    public void onResourceReady(@NonNull Drawable resource, @Nullable Transition<? super Drawable> transition) {
        super.onResourceReady(resource, transition);

        post(() -> {
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