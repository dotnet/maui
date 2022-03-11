package com.microsoft.maui;

import android.view.View;
import android.view.ViewGroup;
import android.view.ViewParent;
import androidx.appcompat.widget.SearchView;

public class ViewHelper {
    public static void requestLayoutIfNeeded(View view)
    {
        if (!view.isInLayout())
            view.requestLayout();
    }

    public static void removeFromParent(View view)
    {
        ViewParent parent = view.getParent();
        if (parent == null)
            return;
        ((ViewGroup)parent).removeView(view);
    }

    public static void setPivotXIfNeeded(View view, float pivotX)
    {
        if (view.getPivotX() != pivotX)
            view.setPivotX(pivotX);
    }

    public static void setPivotYIfNeeded(View view, float pivotY)
    {
        if (view.getPivotY() != pivotY)
            view.setPivotY(pivotY);
    }

    public static void setContentDescriptionForAutomationId(View view, String description)
    {
        view = getSemanticPlatformElement(view);

        // Android automatically sets ImportantForAccessibility to "Yes" when you set the ContentDescription.
		// We are only setting the ContentDescription here for Automation testing purposes so
		// we don't want Layouts/images/Other controls to automatically toggle to Yes.
		// Unfortunately Android (AFAICT) doesn't have an obvious way of calculating what "Auto" will be interpreted as
		// iOS is kind of enough to indicate that anything inheriting from "UIControl" but the Android documentation
		// just says "Android uses heuristics to figure out what Auto will mean"
		// It seems like if we just toggle this back to "Auto" that everything just works.
        int importantForAccessibility = view.getImportantForAccessibility();
        view.setContentDescription(description);
        if (importantForAccessibility == View.IMPORTANT_FOR_ACCESSIBILITY_AUTO) {
            view.setImportantForAccessibility(View.IMPORTANT_FOR_ACCESSIBILITY_AUTO);
        }
    }

    public static View getSemanticPlatformElement(View view)
	{
		if (view instanceof SearchView) {
            view = view.findViewById(androidx.appcompat.R.id.search_button);
        }

        return view;
	}

    public static void set(
        View view,
        int visibility,
        int layoutDirection,
        int minimumHeight,
        int minimumWidth,
        boolean enabled,
        float alpha,
        float translationX,
        float translationY,
        float scaleX,
        float scaleY,
        float rotation,
        float rotationX,
        float rotationY,
        float pivotX,
        float pivotY)
    {
        requestLayoutIfNeeded(view);
        view.setVisibility(visibility);
        view.setLayoutDirection(layoutDirection);
        view.setMinimumHeight(minimumHeight);
        view.setMinimumWidth(minimumWidth);
        view.setEnabled(enabled);
        view.setAlpha(alpha);
        view.setTranslationX(translationX);
        view.setTranslationY(translationY);
        view.setScaleX(scaleX);
        view.setScaleY(scaleY);
        view.setRotation(rotation);
        view.setRotationX(rotationX);
        view.setRotationY(rotationY);
        setPivotXIfNeeded(view, pivotX);
        setPivotYIfNeeded(view, pivotY);
    }
}
