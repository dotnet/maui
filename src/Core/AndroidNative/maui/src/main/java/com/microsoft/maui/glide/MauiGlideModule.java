package com.microsoft.maui.glide;

import android.content.Context;
import android.graphics.Bitmap;
import android.util.Log;

import com.bumptech.glide.Glide;
import com.bumptech.glide.Registry;
import com.bumptech.glide.annotation.GlideModule;
import com.bumptech.glide.module.AppGlideModule;
import com.bumptech.glide.module.LibraryGlideModule;
import com.microsoft.maui.glide.fontimagesource.FontModel;
import com.microsoft.maui.glide.fontimagesource.FontModelLoaderFactory;
import com.microsoft.maui.glide.fontimagesource.FontModelResourceDecoder;

@GlideModule
public class MauiGlideModule extends AppGlideModule {
    @Override
    public void registerComponents(Context context, Glide glide, Registry registry) {
        registry.prepend(FontModel.class, FontModel.class, new FontModelLoaderFactory());
        registry.prepend(FontModel.class, Bitmap.class, new FontModelResourceDecoder());
    }

    @Override
    public boolean isManifestParsingEnabled() {
        return false;
    }
}
