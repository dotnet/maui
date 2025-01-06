package com.microsoft.maui;

import android.util.Log;

public class PlatformLogger {

    private final String tag;
    public final boolean isVerboseLoggable;
    
    public PlatformLogger(String tag) {
        this.tag = tag;
        this.isVerboseLoggable = Log.isLoggable(tag, Log.VERBOSE);
    }

    public void v(String message) {
        Log.v(tag, message);
    }
}