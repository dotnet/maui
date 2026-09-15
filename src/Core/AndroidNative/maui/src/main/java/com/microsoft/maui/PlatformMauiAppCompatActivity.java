package com.microsoft.maui;

import android.content.pm.ApplicationInfo;
import android.content.res.TypedArray;
import android.os.Bundle;
import android.util.Log;

import androidx.activity.ComponentActivity;
import androidx.appcompat.app.AppCompatActivity;

import java.lang.reflect.Field;

/**
 * Class for batching native method calls within the MauiAppCompatActivity implementation
 */
public class PlatformMauiAppCompatActivity {
    private static final String TAG = "MauiAppCompat";

    // These are AndroidX saved-instance-state keys. MAUI does not create the bundles stored under
    // these keys; it only removes or preserves them before AppCompat restores saved state. AndroidX
    // does not expose public constants for these values.
    //
    // ComponentActivity saves pending ActivityResultRegistry state here. Preserving this bundle
    // lets AndroidX replay pending activity results after activity or process recreation.
    private static final String ACTIVITY_RESULT_REGISTRY_KEY = "android:support:activity-result";

    // AndroidX FragmentManager saved-state key. MAUI removes this when fragment restore is
    // disabled because restoring old fragments can conflict with MAUI's own navigation/window
    // reconstruction.
    private static final String SUPPORT_FRAGMENTS_KEY = "android:support:fragments";

    // SavedStateRegistry's top-level bundle key. Older MAUI behavior removed this whole bundle to
    // suppress fragment restore side effects, but that also discarded ActivityResultRegistry state.
    private static final String SAVED_STATE_REGISTRY_KEY = "androidx.lifecycle.BundlableSavedStateRegistry.key";

    private static boolean activityResultRegistryKeyChecked;

    public static void onCreate(AppCompatActivity activity, Bundle savedInstanceState, boolean allowFragmentRestore, int splashAttr, int mauiTheme)
    {
        if (!allowFragmentRestore && savedInstanceState != null) {
            warnIfActivityResultRegistryKeyChanged(activity);
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
        // First remove the direct fragment entry that may be present in the activity state.
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

    private static void warnIfActivityResultRegistryKeyChanged(AppCompatActivity activity)
    {
        if (activityResultRegistryKeyChecked) {
            return;
        }

        activityResultRegistryKeyChecked = true;

        if ((activity.getApplicationInfo().flags & ApplicationInfo.FLAG_DEBUGGABLE) == 0) {
            return;
        }

        try {
            Field field = ComponentActivity.class.getDeclaredField("ACTIVITY_RESULT_TAG");
            field.setAccessible(true);
            Object value = field.get(null);

            if (!ACTIVITY_RESULT_REGISTRY_KEY.equals(value)) {
                Log.w(TAG, "AndroidX ActivityResultRegistry saved-state key changed; MediaPicker recovery may be affected.");
            }
        } catch (Throwable ex) {
            Log.w(TAG, "Unable to verify AndroidX ActivityResultRegistry saved-state key.", ex);
        }
    }
}
