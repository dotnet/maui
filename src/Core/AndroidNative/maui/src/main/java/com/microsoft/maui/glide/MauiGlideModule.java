package com.microsoft.maui.glide;

import android.content.Context;
import android.graphics.Bitmap;

import com.bumptech.glide.Glide;
import com.bumptech.glide.Registry;
import com.bumptech.glide.annotation.GlideModule;
import com.bumptech.glide.module.AppGlideModule;

import com.microsoft.maui.ImageLoaderCallback;
import com.microsoft.maui.glide.fallback.ImageLoaderCallbackModelLoaderFactory;
import com.microsoft.maui.glide.font.FontModel;
import com.microsoft.maui.glide.font.FontModelLoaderFactory;
import com.microsoft.maui.glide.font.FontModelResourceDecoder;
import com.microsoft.maui.glide.stream.GlideInputStreamModelLoaderFactory;

import java.io.InputStream;

@GlideModule
public class MauiGlideModule extends AppGlideModule {
    @Override
    public void registerComponents(Context context, Glide glide, Registry registry) {
        // add custom loaders
        registry.prepend(FontModel.class, FontModel.class, new FontModelLoaderFactory());
        registry.prepend(FontModel.class, Bitmap.class, new FontModelResourceDecoder());
        registry.prepend(InputStream.class, InputStream.class, new GlideInputStreamModelLoaderFactory());
        // add workaround loader for https://github.com/dotnet/maui/issues/6783
        registry.prepend(ImageLoaderCallback.class, ImageLoaderCallback.class, new ImageLoaderCallbackModelLoaderFactory());
    }

    @Override
    public boolean isManifestParsingEnabled() {
        return false;
    }
}