package com.microsoft.maui;

import android.content.Context;
import android.view.View;

import androidx.annotation.NonNull;
import androidx.coordinatorlayout.widget.CoordinatorLayout;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.google.android.material.appbar.AppBarLayout;

/**
 * Used by Shell flyout menu
 */
public class ShellRecyclerView extends RecyclerView {
    public ShellRecyclerView(@NonNull Context context, Adapter adapter)
    {
        super(context);

        setClipToPadding(false);
        setLayoutManager(new LinearLayoutManager(context, RecyclerView.VERTICAL, false));
        setLayoutParameters(this, true);
        setAdapter(adapter);
    }

    /**
     * Configures a default, scrollable CoordinatorLayout.LayoutParams that matches its parent
     * @param view
     * @param scrollingEnabled
     */
    public static void setLayoutParameters(View view, boolean scrollingEnabled)
    {
        CoordinatorLayout.LayoutParams layoutParams = new CoordinatorLayout.LayoutParams(CoordinatorLayout.LayoutParams.MATCH_PARENT, CoordinatorLayout.LayoutParams.MATCH_PARENT);
        if (scrollingEnabled) {
            layoutParams.setBehavior(new AppBarLayout.ScrollingViewBehavior());
        }
        view.setLayoutParams(layoutParams);
    }
}
