package com.microsoft.maui.glide;

import android.content.Context;
import android.graphics.Bitmap;
import android.util.Log;

import com.bumptech.glide.Glide;
import com.bumptech.glide.GlideBuilder;
import com.bumptech.glide.Registry;
import com.bumptech.glide.annotation.GlideModule;
import com.bumptech.glide.module.AppGlideModule;

import com.microsoft.maui.ImageLoaderCallback;
import com.microsoft.maui.PlatformLogger;
import com.microsoft.maui.glide.fallback.ImageLoaderCallbackModelLoaderFactory;
import com.microsoft.maui.glide.font.FontModel;
import com.microsoft.maui.glide.font.FontModelLoaderFactory;
import com.microsoft.maui.glide.font.FontModelResourceDecoder;
import com.microsoft.maui.glide.stream.GlideInputStreamModelLoaderFactory;

import java.io.InputStream;

@GlideModule
public class MauiGlideModule extends AppGlideModule {
    private static final PlatformLogger logger = new PlatformLogger("Glide");

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

    @Override
    public void applyOptions(Context context, GlideBuilder builder) {
        // Glide is checking for the log level only on some classes, so we have to do it ourselves here.
        // Command: adb shell setprop log.tag.Glide VERBOSE
        if (logger.isVerboseLoggable) {
            builder.setLogLevel(Log.VERBOSE);
        }
    }
}
