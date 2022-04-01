package com.microsoft.maui.glide.stream;

import androidx.annotation.NonNull;

import com.bumptech.glide.load.model.ModelLoader;
import com.bumptech.glide.load.model.ModelLoaderFactory;
import com.bumptech.glide.load.model.MultiModelLoaderFactory;

import java.io.InputStream;

public class GlideInputStreamModelLoaderFactory implements ModelLoaderFactory<InputStream, InputStream> {
    @NonNull
    @Override
    public ModelLoader<InputStream, InputStream> build(@NonNull MultiModelLoaderFactory multiFactory) {
        return new GlideInputStreamModelLoader();
    }

    @Override
    public void teardown() {

    }
}
