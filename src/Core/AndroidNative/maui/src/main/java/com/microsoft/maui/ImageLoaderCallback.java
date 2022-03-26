package com.microsoft.maui;

import android.graphics.drawable.Drawable;

import androidx.annotation.Nullable;

public interface ImageLoaderCallback {
    void onComplete(Boolean success, @Nullable Drawable drawable, @Nullable Runnable dispose);
}

