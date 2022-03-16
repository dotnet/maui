package com.microsoft.maui;

import android.graphics.drawable.Drawable;

import androidx.annotation.Nullable;

public interface ImageLoaderDrawableCallback {
    void onComplete(Drawable drawable, @Nullable Runnable dispose);
}
