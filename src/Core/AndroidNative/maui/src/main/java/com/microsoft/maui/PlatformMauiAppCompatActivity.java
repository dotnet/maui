package com.microsoft.maui;

import android.content.Context;
import android.content.res.Resources;
import android.content.res.TypedArray;

import androidx.appcompat.app.AppCompatActivity;
import android.os.Bundle;


/**
 * Class for batching native method calls within the MauiAppCompatActivity implementation
 */
public class PlatformMauiAppCompatActivity {
    public static void onCreate(AppCompatActivity activity, Bundle savedInstanceState, boolean allowFragmentRestore, int splashAttr, int mauiTheme)
    {
        if (!allowFragmentRestore && savedInstanceState != null) {
            savedInstanceState.remove("android:support:fragments");
            Bundle bundle = savedInstanceState.getBundle("androidx.lifecycle.BundlableSavedStateRegistry.key");
            if (bundle != null) {
                bundle.remove("android:support:fragments");
            }
        }

        boolean mauiSplashAttrValue = false;
        TypedArray a = null;
        try {
            a = activity.obtainStyledAttributes(new int[]{splashAttr});
            mauiSplashAttrValue = a.getBoolean(0, false);
        } finally {
            if (a != null)
                a.recycle();
        }

        if (mauiSplashAttrValue) {
            activity.setTheme(mauiTheme);
        }
    }
}
