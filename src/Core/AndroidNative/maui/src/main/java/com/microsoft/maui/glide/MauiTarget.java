package com.microsoft.maui.glide;

import android.graphics.drawable.Drawable;

import com.bumptech.glide.RequestBuilder;
import com.bumptech.glide.request.target.Target;

public interface MauiTarget extends Target<Drawable> {
    void load(RequestBuilder<Drawable> builder);
}
