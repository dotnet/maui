package com.microsoft.maui.glide;

import android.util.Log;

public class GlideLogging {
    private static final String TAG = "Glide";
    private static final boolean IS_VERBOSE_LOGGABLE = Log.isLoggable(TAG, Log.VERBOSE);

    public static boolean isVerboseLoggable() {
        return IS_VERBOSE_LOGGABLE;
    }

    public static void v(String message) {
        if (IS_VERBOSE_LOGGABLE) {
            Log.v(TAG, message);
        }
    }
}
