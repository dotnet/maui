package com.microsoft.maui;

import android.os.Handler;
import android.os.Looper;
import android.view.View;

import java.util.HashSet;
import java.util.List;
import java.util.ArrayList;
import java.util.Set;

public final class FixedLayoutScheduler {
    private static final Set<View> pending = new HashSet<>();
    private static final Handler handler = new Handler(Looper.getMainLooper());
    private static boolean runnablePosted = false;

    private static final Runnable relayoutRunnable = new Runnable() {
        @Override
        public void run() {
            runnablePosted = false;

            // Snapshot & clear to avoid concurrent modification
            List<View> toProcess = new ArrayList<>(pending);
            pending.clear();

            for (View v : toProcess) {
                if (v == null) continue;

                int width = v.getWidth();
                int height = v.getHeight();

                if (width == 0 || height == 0) {
                    // Skip views with no size (not laid out yet)
                    continue;
                }

                int widthSpec = View.MeasureSpec.makeMeasureSpec(width, View.MeasureSpec.EXACTLY);
                int heightSpec = View.MeasureSpec.makeMeasureSpec(height, View.MeasureSpec.EXACTLY);

                v.measure(widthSpec, heightSpec);
                v.layout(v.getLeft(), v.getTop(), v.getRight(), v.getBottom());
                v.invalidate();
            }
        }
    };

    private FixedLayoutScheduler() {
        // no instances
    }

    /** Schedules a fixed-size layout pass for this view. */
    public static void scheduleLayoutPass(View v) {
        if (v == null) return;

        pending.add(v);
        v.forceLayout();

        if (!runnablePosted) {
            runnablePosted = true;
            handler.post(relayoutRunnable);
        }
    }
}