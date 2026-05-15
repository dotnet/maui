package com.microsoft.maui;

import android.content.res.TypedArray;

import androidx.appcompat.app.AppCompatActivity;
import android.os.Bundle;


/**
 * Class for batching native method calls within the MauiAppCompatActivity implementation
 */
public class PlatformMauiAppCompatActivity {
    // These are Android framework / AndroidX saved-instance-state keys. MAUI does not create
    // the bundles stored under these keys; it only removes or preserves them before AppCompat
    // restores saved state. AndroidX does not expose public constants for these values.
    //
    // ComponentActivity saves pending ActivityResultRegistry state here. Preserving this bundle
    // lets AndroidX replay pending activity results after activity or process recreation.
    private static final String ACTIVITY_RESULT_REGISTRY_KEY = "android:support:activity-result";

    // Framework FragmentManager and AndroidX FragmentManager saved-state keys. MAUI removes these
    // when fragment restore is disabled because restoring old platform fragments can conflict with
    // MAUI's own navigation/window reconstruction.
    private static final String ANDROID_FRAGMENTS_KEY = "android:fragments";
    private static final String SUPPORT_FRAGMENTS_KEY = "android:support:fragments";

    // SavedStateRegistry's top-level bundle key. Older MAUI behavior removed this whole bundle to
    // suppress fragment restore side effects, but that also discarded ActivityResultRegistry state.
    private static final String SAVED_STATE_REGISTRY_KEY = "androidx.lifecycle.BundlableSavedStateRegistry.key";

    public static void onCreate(AppCompatActivity activity, Bundle savedInstanceState, boolean allowFragmentRestore, int splashAttr, int mauiTheme)
    {
        if (!allowFragmentRestore && savedInstanceState != null) {
            removeFragmentRestoreState(savedInstanceState);
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

    private static void removeFragmentRestoreState(Bundle savedInstanceState)
    {
        // First remove the direct fragment entries that may be present in the activity state.
        savedInstanceState.remove(ANDROID_FRAGMENTS_KEY);
        savedInstanceState.remove(SUPPORT_FRAGMENTS_KEY);

        Bundle savedStateRegistry = savedInstanceState.getBundle(SAVED_STATE_REGISTRY_KEY);
        if (savedStateRegistry != null) {
            // The saved-state registry is a shared AndroidX container. Extract the activity-result
            // entry before removing the container so pending activity results are not lost with the
            // fragment-related providers.
            Bundle activityResultRegistryState = savedStateRegistry.getBundle(ACTIVITY_RESULT_REGISTRY_KEY);

            savedInstanceState.remove(SAVED_STATE_REGISTRY_KEY);

            if (activityResultRegistryState != null) {
                // Keep only the AndroidX ActivityResultRegistry state needed to replay pending
                // results after activity/process recreation. Other saved-state providers may
                // contain fragment state that MAUI cannot safely restore.
                Bundle prunedSavedStateRegistry = new Bundle();
                prunedSavedStateRegistry.putBundle(ACTIVITY_RESULT_REGISTRY_KEY, activityResultRegistryState);
                savedInstanceState.putBundle(SAVED_STATE_REGISTRY_KEY, prunedSavedStateRegistry);
            }
        }
    }
}
