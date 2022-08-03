package com.microsoft.maui.glide.fallback;

import androidx.annotation.NonNull;

import com.bumptech.glide.load.model.ModelLoader;
import com.bumptech.glide.load.model.ModelLoaderFactory;
import com.bumptech.glide.load.model.MultiModelLoaderFactory;
import com.microsoft.maui.ImageLoaderCallback;

// Custom, no-op model loader factory to workaround https://github.com/dotnet/maui/issues/6783
public class ImageLoaderCallbackModelLoaderFactory implements ModelLoaderFactory<ImageLoaderCallback, ImageLoaderCallback> {
    @NonNull
    @Override
    public ModelLoader<ImageLoaderCallback, ImageLoaderCallback> build(@NonNull MultiModelLoaderFactory multiFactory) {
        return new ImageLoaderCallbackModelLoader();
    }

    @Override
    public void teardown() {
    }
}
