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
import com.bumptech.glide.request.Request;
import com.bumptech.glide.request.target.CustomViewTarget;
import com.bumptech.glide.request.transition.Transition;

import microsoft.maui.R;
import com.microsoft.maui.ImageLoaderCallback;
import com.microsoft.maui.PlatformInterop;
import com.microsoft.maui.PlatformLogger;

import java.lang.ref.WeakReference;
import com.microsoft.maui.glide.MauiTarget;

public class MauiCustomViewTarget extends CustomViewTarget<ImageView, Drawable> implements MauiTarget {
    private static final PlatformLogger logger = new PlatformLogger("MauiCustomViewTarget");

    @IdRes private static final int RUNNING_CALLBACKS_TAG = R.id.maui_custom_view_target_running_callbacks_tag;
    @IdRes private static final int CHANGED_TAG = R.id.maui_custom_view_target_changed_tag;
    @IdRes private static final int CURRENT_TAG = R.id.maui_custom_view_target_current_tag;
    // Unlike View.post, also runs while the view is not attached to a window.
    private static final Handler MAIN_HANDLER = new Handler(Looper.getMainLooper());

    private final ImageLoaderCallback callback;
    private final String resourceLogIdentifier;
    private boolean completed;
    // The request this target started, so that it is only cleared while it is still the view's request.
    private Request startedRequest;
    // A failed request never set an image on the view, so clearing it (for example when Glide restarts it after the
    // network reconnects) must not change the image shown instead, such as an error image.
    private boolean failed;

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
        start(builder, this.view.getTag(CHANGED_TAG), false);
    }

    /**
     * Records that the view was cleared or a load into it was started, so that loads into it
     * that were requested earlier but whose start was deferred are not started anymore.
     */
    public static void onViewChanged(@NonNull ImageView view) {
        view.setTag(CHANGED_TAG, new Object());
    }

    /**
     * Stops a load into the view that has not finished yet (including one that Glide paused and would restart),
     * and loads into it whose start was deferred, so that they cannot replace an image that is set on the view later.
     * The stopped load is reported as cancelled. Like starting a new load into the view, stopping an unfinished
     * load clears the view. A load that finished, and the image it shows, are left alone.
     */
    public static void stopUnfinishedLoad(@NonNull ImageView view) {
        onViewChanged(view);

        MauiCustomViewTarget target = getCurrent(view);
        if (target == null) {
            return;
        }
        if (target.completed) {
            forgetCurrent(view);
            return;
        }
        // Glide does not allow clearing from inside one of its target callbacks (and then the load has finished).
        Object invokingCallbacks = view.getTag(RUNNING_CALLBACKS_TAG);
        if (invokingCallbacks instanceof Boolean && (Boolean) invokingCallbacks) {
            return;
        }
        forgetCurrent(view);
        target.completed = true;
        // Only while the view's request is still the one this target started (another load may have replaced it).
        if (target.isStillViewRequest() && !PlatformInterop.isContextDestroyed(view.getContext())) {
            Glide.with(view).clear(target);
        }
        // Like a deferred load that is dropped; posted so that the caller is not reentered.
        MAIN_HANDLER.post(() -> target.callback.onComplete(null, null, null));
    }

    /**
     * Forgets the load into the view that {@link #stopUnfinishedLoad} would stop, for example when the view is cleared.
     */
    public static void forgetCurrent(@NonNull ImageView view) {
        view.setTag(CURRENT_TAG, null);
    }

    @Nullable
    private static MauiCustomViewTarget getCurrent(@NonNull ImageView view) {
        // Weak, so that a load that Glide stopped by itself (for example cleared by other code) is not retained.
        Object current = view.getTag(CURRENT_TAG);
        return current instanceof WeakReference ? (MauiCustomViewTarget) ((WeakReference<?>) current).get() : null;
    }

    private boolean isStillViewRequest() {
        Request request = getRequest();
        return request != null && request == startedRequest;
    }

    private void start(RequestBuilder<Drawable> builder, Object changedToken, boolean deferred) {
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
            // Not View.post, which waits until the view is attached to a window.
            MAIN_HANDLER.post(() -> {
                // The view was cleared (for example its image source removed), or a newer load was started into it,
                // before this load could start: report it as cancelled instead of replacing the newer state.
                // (Also when its activity was destroyed in the meantime, when Glide cannot load anymore.)
                // Deferred loads run in the order they were requested, so they do not cancel each other.
                if (this.view.getTag(CHANGED_TAG) != changedToken || PlatformInterop.isContextDestroyed(this.view.getContext())) {
                    if (logger.isVerboseLoggable) logger.v("cancel deferred load: " + resourceLogIdentifier);
                    if (!completed) {
                        completed = true;
                        callback.onComplete(null, null, null);
                    }
                    return;
                }
                start(builder, changedToken, true);
            });
            return;
        }

        if (!deferred) {
            onViewChanged(this.view);
        }
        this.view.setTag(CURRENT_TAG, new WeakReference<>(this));
        builder.into(this);
        this.startedRequest = getRequest();
    }

    @Override
    protected void onResourceCleared(@Nullable Drawable placeholder) {
        if (logger.isVerboseLoggable) logger.v("onResourceCleared: " + resourceLogIdentifier);

        if (failed)
            return;

		this.view.setImageDrawable(placeholder);
    }

    @Override
    public void onLoadFailed(@Nullable Drawable errorDrawable) {
        if (completed)
            return;

        this.completed = true;
        if (getCurrent(this.view) == this)
            forgetCurrent(this.view);

        if (logger.isVerboseLoggable) logger.v("onLoadFailed: " + resourceLogIdentifier);

        this.failed = true;
        // Glide restarts failed requests when the network reconnects. Nothing waits for this request anymore, so stop it.
        MAIN_HANDLER.post(this::forgetFailedRequest);

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
        if (getCurrent(this.view) == this)
            forgetCurrent(this.view);

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

    private void forgetFailedRequest() {
        if (!isStillViewRequest() || PlatformInterop.isContextDestroyed(this.view.getContext()))
            return;
        Glide.with(this.view).clear(this);
    }

    private void setIsInvokingCallbacks(boolean isInvokingCallbacks) {
        this.view.setTag(RUNNING_CALLBACKS_TAG, isInvokingCallbacks);
    }

    private boolean getIsInvokingCallbacks() {
        Object tag = this.view.getTag(RUNNING_CALLBACKS_TAG);
        return tag != null && tag instanceof Boolean && (Boolean) tag;
    }
}