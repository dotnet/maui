package com.microsoft.maui.glide.fontimagesource;

import com.bumptech.glide.load.model.ModelLoader;
import com.bumptech.glide.load.model.ModelLoaderFactory;
import com.bumptech.glide.load.model.MultiModelLoaderFactory;

public class FontModelLoaderFactory implements ModelLoaderFactory<FontModel, FontModel> {
    @Override
    public ModelLoader<FontModel, FontModel> build(MultiModelLoaderFactory multiFactory) {
        return new FontModelLoader();
    }

    @Override
    public void teardown() {
        // Do nothing.
    }
}
