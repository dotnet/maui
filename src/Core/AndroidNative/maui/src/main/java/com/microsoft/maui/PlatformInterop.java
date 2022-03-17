package com.microsoft.maui;

import android.content.Context;
import android.graphics.Typeface;
import android.graphics.drawable.Drawable;
import android.net.Uri;
import android.view.View;
import android.view.ViewGroup;
import android.view.ViewParent;
import android.widget.ImageView;

import androidx.annotation.ColorInt;
import androidx.appcompat.widget.SearchView;

import com.bumptech.glide.Glide;
import com.bumptech.glide.RequestBuilder;
import com.bumptech.glide.RequestManager;
import com.bumptech.glide.load.engine.DiskCacheStrategy;
import com.bumptech.glide.request.target.Target;
import com.microsoft.maui.glide.MauiCustomTarget;
import com.microsoft.maui.glide.MauiCustomViewTarget;
import com.microsoft.maui.glide.font.FontModel;

import java.io.File;
import java.io.InputStream;

public class PlatformInterop {
    public static void requestLayoutIfNeeded(View view)
    {
        if (!view.isInLayout())
            view.requestLayout();
    }

    public static void removeFromParent(View view)
    {
        ViewParent parent = view.getParent();
        if (parent == null)
            return;
        ((ViewGroup)parent).removeView(view);
    }

    public static void setPivotXIfNeeded(View view, float pivotX)
    {
        if (view.getPivotX() != pivotX)
            view.setPivotX(pivotX);
    }

    public static void setPivotYIfNeeded(View view, float pivotY)
    {
        if (view.getPivotY() != pivotY)
            view.setPivotY(pivotY);
    }

    public static void setContentDescriptionForAutomationId(View view, String description)
    {
        view = getSemanticPlatformElement(view);

        // Android automatically sets ImportantForAccessibility to "Yes" when you set the ContentDescription.
		// We are only setting the ContentDescription here for Automation testing purposes so
		// we don't want Layouts/images/Other controls to automatically toggle to Yes.
		// Unfortunately Android (AFAICT) doesn't have an obvious way of calculating what "Auto" will be interpreted as
		// iOS is kind of enough to indicate that anything inheriting from "UIControl" but the Android documentation
		// just says "Android uses heuristics to figure out what Auto will mean"
		// It seems like if we just toggle this back to "Auto" that everything just works.
        int importantForAccessibility = view.getImportantForAccessibility();
        view.setContentDescription(description);
        if (importantForAccessibility == View.IMPORTANT_FOR_ACCESSIBILITY_AUTO) {
            view.setImportantForAccessibility(View.IMPORTANT_FOR_ACCESSIBILITY_AUTO);
        }
    }

    public static View getSemanticPlatformElement(View view)
	{
		if (view instanceof SearchView) {
            view = view.findViewById(androidx.appcompat.R.id.search_button);
        }

        return view;
	}

    public static void set(
        View view,
        int visibility,
        int layoutDirection,
        int minimumHeight,
        int minimumWidth,
        boolean enabled,
        float alpha,
        float translationX,
        float translationY,
        float scaleX,
        float scaleY,
        float rotation,
        float rotationX,
        float rotationY,
        float pivotX,
        float pivotY)
    {
        requestLayoutIfNeeded(view);
        view.setVisibility(visibility);
        view.setLayoutDirection(layoutDirection);
        view.setMinimumHeight(minimumHeight);
        view.setMinimumWidth(minimumWidth);
        view.setEnabled(enabled);
        view.setAlpha(alpha);
        view.setTranslationX(translationX);
        view.setTranslationY(translationY);
        view.setScaleX(scaleX);
        view.setScaleY(scaleY);
        view.setRotation(rotation);
        view.setRotationX(rotationX);
        view.setRotationY(rotationY);
        setPivotXIfNeeded(view, pivotX);
        setPivotYIfNeeded(view, pivotY);
    }

    public static void loadImageFromFile(ImageView imageView, String file, ImageLoaderCallback callback)
    {
        RequestManager glide = Glide.with(imageView);

        MauiCustomViewTarget target = new MauiCustomViewTarget(imageView, callback, glide);

        glide
            .load(new File(file))
            .into(target);
    }

    public static void loadImageFromUri(ImageView imageView, String uri, Boolean cachingEnabled, ImageLoaderCallback callback)
    {
        Uri androidUri = Uri.parse(uri);

        if (androidUri == null) {
            callback.onComplete(false, null, null);
            return;
        }

        RequestManager glide = Glide.with(imageView);

        MauiCustomViewTarget target = new MauiCustomViewTarget(imageView, callback, glide);

        RequestBuilder<Drawable> builder = glide
            .load(androidUri);

        if (!cachingEnabled)
        {
            builder = builder
                .diskCacheStrategy(DiskCacheStrategy.NONE)
                .skipMemoryCache(true);
        }

        builder
            .into(target);
    }

    public static void loadImageFromStream(ImageView imageView, InputStream inputStream, ImageLoaderCallback callback)
    {
        RequestManager glide = Glide.with(imageView);

        MauiCustomViewTarget target = new MauiCustomViewTarget(imageView, callback, glide);

        glide
            .load(inputStream)
            .into(target);
    }

    public static void loadImageFromFont(ImageView imageView, @ColorInt int color, String glyph, Typeface typeface, float textSize, ImageLoaderCallback callback)
    {
        RequestManager glide = Glide.with(imageView);

        MauiCustomViewTarget target = new MauiCustomViewTarget(imageView, callback, glide);

        FontModel fontModel = new FontModel(color, glyph, textSize, typeface);

        glide
            .load(fontModel)
            .override(Target.SIZE_ORIGINAL, Target.SIZE_ORIGINAL)
            .into(target);
    }

    public static void loadImageFromFile(Context context, String file, ImageLoaderCallback callback)
    {
        RequestManager glide = Glide.with(context);

        MauiCustomTarget target = new MauiCustomTarget(callback, glide);

        glide
            .load(new File(file))
            .into(target);
    }

    public static void loadImageFromUri(Context context, String uri, Boolean cachingEnabled, ImageLoaderCallback callback)
    {
        Uri androidUri = Uri.parse(uri);

        if (androidUri == null) {
            callback.onComplete(false, null, null);
            return;
        }

        RequestManager glide = Glide.with(context);

        MauiCustomTarget target = new MauiCustomTarget(callback, glide);

        RequestBuilder<Drawable> builder = glide
            .load(androidUri);

        if (!cachingEnabled)
        {
            builder = builder
                .diskCacheStrategy(DiskCacheStrategy.NONE)
                .skipMemoryCache(true);
        }

        builder.into(target);
    }


    public static void loadImageFromStream(Context context, InputStream inputStream, ImageLoaderCallback callback)
    {
        RequestManager glide = Glide.with(context);

        MauiCustomTarget target = new MauiCustomTarget(callback, glide);

        glide
            .load(inputStream)
            .into(target);
    }


    public static void loadImageFromFont(Context context, @ColorInt int color, String glyph, Typeface typeface, float textSize, ImageLoaderCallback callback)
    {
        RequestManager glide = Glide.with(context);

        FontModel fontModel = new FontModel(color, glyph, textSize, typeface);

        MauiCustomTarget target = new MauiCustomTarget(callback, glide);

        glide
            .load(fontModel)
            .override(Target.SIZE_ORIGINAL, Target.SIZE_ORIGINAL)
            .into(target);
    }
}
