package com.microsoft.maui.glide.fallback;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.bumptech.glide.load.Options;
import com.bumptech.glide.load.model.ModelLoader;
import com.microsoft.maui.ImageLoaderCallback;

// Custom, no-op model loader to workaround https://github.com/dotnet/maui/issues/6783
public class ImageLoaderCallbackModelLoader implements ModelLoader<ImageLoaderCallback, ImageLoaderCallback> {
    @Nullable
    @Override
    public LoadData<ImageLoaderCallback> buildLoadData(@NonNull ImageLoaderCallback imageLoaderCallback, int width, int height, @NonNull Options options) {
        return null;
    }

    @Override
    public boolean handles(@NonNull ImageLoaderCallback imageLoaderCallback) {
        return true;
    }
}
