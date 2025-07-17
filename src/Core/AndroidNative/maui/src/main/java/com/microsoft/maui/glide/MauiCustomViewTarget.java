package com.microsoft.maui.glide;

import android.graphics.drawable.Drawable;
import android.os.Handler;
import android.os.Looper;
import android.widget.ImageView;

import androidx.annotation.IdRes;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.bumptech.glide.Glide;
import com.bumptech.glide.RequestBuilder;
import com.bumptech.glide.request.target.CustomViewTarget;
import com.bumptech.glide.request.transition.Transition;

import microsoft.maui.R;
import com.microsoft.maui.ImageLoaderCallback;
import com.microsoft.maui.PlatformLogger;
import com.microsoft.maui.glide.MauiTarget;

public class MauiCustomViewTarget extends CustomViewTarget<ImageView, Drawable> implements MauiTarget {
    private static final PlatformLogger logger = new PlatformLogger("MauiCustomViewTarget");

    @IdRes private static final int RUNNING_CALLBACKS_TAG = R.id.maui_custom_view_target_running_callbacks_tag;

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

    public void load(RequestBuilder<Drawable> builder) {
        // Glide does not support starting new loads from Target callbacks and throws an exception.
        // https://github.com/bumptech/glide/blob/9c9f56f29e63114c409184dd1e82b42b3f0063f7/library/src/main/java/com/bumptech/glide/request/SingleRequest.java#L301-L311
        //
        // > You can't start or clear loads in RequestListener or Target callbacks.
        // > If you're trying to start a fallback request when a load fails, use RequestBuilder#error(RequestBuilder).
        // > Otherwise consider posting your into() or clear() calls to the main thread using a Handler instead.
        //
        // This only happens when the target can load the previous request, and that is only possible within a `CustomViewTarget`
        // where the Request is being stored inside the target's view as a Tag.
        // https://github.com/bumptech/glide/blob/9c9f56f29e63114c409184dd1e82b42b3f0063f7/library/src/main/java/com/bumptech/glide/request/target/CustomViewTarget.java#L226-L239
        //
        // For this reason, if someone awaits the result of the image loading and then starts a new request,
        // we must ensure the new load happens in a new thread loop.
        //
        // Why don't we simply `post` all the callback invocations?
        // - performance: 99% of the times we simply map the image source once
        // - consistency: the callback assumes the load succeeded so it must run right away in the same loop
        //                because there's a change someone messes up with the drawable in the mean time
        //
        // Why don't we `post` `setImageDrawable + callback` together?
        // - performance: 99% of the times we simply map the image source once
        // - consistency: Glide is race-condition free but only if we set the image directly in the `onResourceReady`
        //
        // This use case is covered by `LoadDrawableAsyncReturnsWithSameImageAndDoesNotHang` core device test.
        if (getIsInvokingCallbacks()) {
            if (logger.isVerboseLoggable) logger.v("defer load: " + resourceLogIdentifier);
            this.view.post(() -> load(builder));
            return;
        }

        builder.into(this);
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

        try {
            setIsInvokingCallbacks(true);
            callback.onComplete(false, errorDrawable, null);
        }
        finally {
            setIsInvokingCallbacks(false);
        }
    }

    @Override
    public void onResourceReady(@NonNull Drawable resource, @Nullable Transition<? super Drawable> transition) {
        if (completed)
            return;
        this.completed = true;

        if (logger.isVerboseLoggable) logger.v("onResourceReady: " + resourceLogIdentifier);

        // set the image
        this.view.setImageDrawable(resource);

        try {
            setIsInvokingCallbacks(true);
            callback.onComplete(true, resource, null);
        }
        finally {
            setIsInvokingCallbacks(false);
        }
    }

    private void setIsInvokingCallbacks(boolean isInvokingCallbacks) {
        this.view.setTag(RUNNING_CALLBACKS_TAG, isInvokingCallbacks);
    }

    private boolean getIsInvokingCallbacks() {
        Object tag = this.view.getTag(RUNNING_CALLBACKS_TAG);
        return tag != null && tag instanceof Boolean && (Boolean) tag;
    }
}