package com.microsoft.maui;

import androidx.annotation.NonNull;

public abstract class HybridJavaScriptInterface {
    @android.webkit.JavascriptInterface
    public abstract void sendMessage(@NonNull String message);
}
