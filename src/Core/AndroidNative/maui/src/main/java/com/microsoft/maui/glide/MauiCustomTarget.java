package com.microsoft.maui.glide;

import android.app.Activity;
import android.content.Context;
import android.content.ContextWrapper;
import android.graphics.drawable.Drawable;
import android.os.Handler;
import android.os.Looper;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.FragmentActivity;
import androidx.lifecycle.Lifecycle;

import com.bumptech.glide.Glide;
import com.bumptech.glide.RequestBuilder;
import com.bumptech.glide.request.target.CustomTarget;
import com.bumptech.glide.request.transition.Transition;

import com.microsoft.maui.ImageLoaderCallback;
import com.microsoft.maui.PlatformLogger;
import com.microsoft.maui.glide.MauiTarget;

public class MauiCustomTarget extends CustomTarget<Drawable> implements MauiTarget {
    private static final PlatformLogger logger = new PlatformLogger("MauiCustomTarget");

    private final Context context;
    private final ImageLoaderCallback callback;
    private final String resourceLogIdentifier;
    private boolean completed;

    /**
     * Checks if the given context is destroyed.
     * @param context The context to check
     * @return true if the context is destroyed, false otherwise
     */
    private static boolean isContextDestroyed(Context context) {
        if (context == null) {
            return true;
        }

        Activity activity = getActivity(context);
        if (activity instanceof FragmentActivity) {
            FragmentActivity fragmentActivity = (FragmentActivity) activity;
            
            // Check if activity is finishing or destroyed
            if (fragmentActivity.isFinishing() || fragmentActivity.isDestroyed()) {
                return true;
            }

            // Check lifecycle state
            try {
                if (fragmentActivity.getLifecycle().getCurrentState() == Lifecycle.State.DESTROYED) {
                    return true;
                }
            } catch (Exception e) {
                // If there's an exception getting the lifecycle state, consider it destroyed
                return true;
            }
        } else if (activity != null) {
            // For regular Activity, check if finishing or destroyed
            if (activity.isFinishing() || activity.isDestroyed()) {
                return true;
            }
        }

        return false;
    }

    /**
     * Gets the Activity from a Context.
     * @param context The context to get the activity from
     * @return The activity if found, null otherwise
     */
    private static Activity getActivity(Context context) {
        if (context == null) {
            return null;
        }

        if (context instanceof Activity) {
            return (Activity) context;
        }

        if (context instanceof ContextWrapper) {
            Context baseContext = ((ContextWrapper) context).getBaseContext();
            return getActivity(baseContext);
        }

        return null;
    }

    private final Context context;
    private final ImageLoaderCallback callback;
    private final String resourceLogIdentifier;
    private boolean completed;

    public MauiCustomTarget(Context context, ImageLoaderCallback callback, Object model) {
        this.completed = false;
        this.context = context;
        this.callback = callback;

        if (logger.isVerboseLoggable && model != null) {
            this.resourceLogIdentifier = model.toString();
        } else {
            this.resourceLogIdentifier = null;
        }
    }

    public void load(RequestBuilder<Drawable> builder) {
        builder.into(this);
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

        callback.onComplete(true, resource, this::clear);
    }

    @Override
    public void onLoadCleared(@Nullable Drawable placeholder) {
        if (logger.isVerboseLoggable) logger.v("onLoadCleared: " + resourceLogIdentifier);
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
        // TODO: it looks like no one is really disposing the result on C# side
        // we must fix it there to release the Glide cache entry properly
        post(() -> {
            if (!isContextDestroyed(context)) {
                Glide
                    .with(context)
                    .clear(this);
            }
        });
    }
}