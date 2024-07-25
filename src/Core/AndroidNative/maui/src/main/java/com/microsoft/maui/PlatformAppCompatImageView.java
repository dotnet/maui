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
    private boolean invalidatedWhileFrozen;
    
    @Override
    public void invalidate() {
        if (frozen) {
            // When switching between images, the image view is invalidated which causes `onDraw` being called.
            // At this point, the previous image `Drawable` is already removed from the Glide `BitmapPool` therefore the app will potentially crash.
            // To avoid this, we need to skip the invalidation when the image is loading.
            // See more: https://github.com/dotnet/maui/pull/12310
            invalidatedWhileFrozen = true;
            return;
        }

        super.invalidate();
    }
    
    @Override
    public void setImageDrawable(Drawable drawable) {
        frozen = false;

        if (invalidatedWhileFrozen) {
            // If the view was invalidated while frozen, make sure it is invalidated now that we have a new image.
            invalidatedWhileFrozen = false;
            invalidate();
        }

        super.setImageDrawable(drawable);
    }

    /** Freezes the view to avoid invalidating it while the image is loading. */
    public void freeze() {
        frozen = true;
    }
}
