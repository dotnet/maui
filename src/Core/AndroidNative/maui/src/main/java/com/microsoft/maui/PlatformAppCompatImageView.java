package com.microsoft.maui;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.drawable.Drawable;
import android.util.Log;

import androidx.annotation.NonNull;
import androidx.appcompat.widget.AppCompatImageView;

public class PlatformAppCompatImageView extends AppCompatImageView {
    public PlatformAppCompatImageView(@NonNull Context context) {
        super(context);
    }

    private boolean frozen;
    
    /** Freezes the view to avoid redrawing it while the image is loading. */
    public void freeze() {
        // When switching between images, the image view is invalidated which causes `onDraw` being called.
        // At this point, the previous image `Drawable` is already removed from the Glide `BitmapPool` therefore the app will potentially crash.
        // To avoid this, we need to skip redrawing while the new image is loading.
        // See more: https://github.com/dotnet/maui/pull/12310
        
        frozen = true;
    }
    
    @Override
    public void requestLayout () {
        if (frozen) {
            // Prevent layout requests while frozen to avoid redrawing the view.
            return;
        }

        super.requestLayout();
    }
    
    @Override
    public void invalidate() {
        if (frozen) {
            // Prevent invalidations while frozen to avoid redrawing the view.
            return;
        }

        super.invalidate();
    }
    
    @Override
    public void setImageDrawable(Drawable drawable) {
        frozen = false;
        super.setImageDrawable(drawable);
    }
    
    @Override
    protected void onDraw(Canvas canvas) {
        if (frozen) {
            // If for some weird reason this is still happening even when frozen
            // we must skip the drawing to avoid reading a recycled bitmap drawable.
            // See more: https://bumptech.github.io/glide/javadocs/4140/library/com.bumptech.glide.request.target/-target/on-load-cleared.html
            // This will cause a white flicker but that's acceptable all considering.
            return;
        }

        super.onDraw(canvas);
    }
}
