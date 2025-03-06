package com.microsoft.maui;

import android.os.Handler;
import android.os.Looper;

/**
 * Java-side support for Dispatcher.Android.cs
 */
public class PlatformDispatcher extends Handler {
    private PlatformDispatcher(Looper looper) {
        super(looper);
    }

    public static PlatformDispatcher create() {
        Looper looper = Looper.myLooper();
        if (looper == null || looper != Looper.getMainLooper())
            return null;
        return new PlatformDispatcher(looper);
    }

    public boolean isDispatchRequired() {
        return Looper.myLooper() != getLooper();
    }
}
