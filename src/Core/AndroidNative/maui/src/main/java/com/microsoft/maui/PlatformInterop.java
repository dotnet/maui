package com.microsoft.maui;

import android.app.Activity;
import android.content.Context;
import android.content.res.ColorStateList;
import android.graphics.BlendMode;
import android.graphics.BlendModeColorFilter;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Path;
import android.graphics.PathEffect;
import android.graphics.PorterDuff;
import android.graphics.Rect;
import android.graphics.Typeface;
import android.graphics.drawable.Drawable;
import android.graphics.drawable.PaintDrawable;
import android.net.Uri;
import android.os.Build;
import android.text.Editable;
import android.text.InputFilter;
import android.util.DisplayMetrics;
import android.util.TypedValue;
import android.view.Gravity;
import android.view.View;
import android.view.ViewGroup;
import android.view.ViewParent;
import android.widget.EditText;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.LinearLayout;

import androidx.annotation.ColorInt;
import androidx.annotation.NonNull;
import androidx.annotation.RequiresApi;
import androidx.appcompat.widget.SearchView;
import androidx.appcompat.widget.TintTypedArray;
import androidx.coordinatorlayout.widget.CoordinatorLayout;
import androidx.viewpager2.adapter.FragmentStateAdapter;
import androidx.viewpager2.widget.ViewPager2;
import androidx.window.layout.WindowMetricsCalculator;

import com.bumptech.glide.Glide;
import com.bumptech.glide.RequestBuilder;
import com.bumptech.glide.RequestManager;
import com.bumptech.glide.load.engine.DiskCacheStrategy;
import com.bumptech.glide.request.target.Target;

import com.google.android.material.appbar.AppBarLayout;
import com.google.android.material.appbar.MaterialToolbar;
import com.google.android.material.bottomnavigation.BottomNavigationView;
import com.google.android.material.tabs.TabLayout;
import com.google.android.material.tabs.TabLayoutMediator;

import com.microsoft.maui.glide.MauiCustomTarget;
import com.microsoft.maui.glide.MauiCustomViewTarget;
import com.microsoft.maui.glide.font.FontModel;

import java.io.InputStream;
import java.lang.reflect.Field;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

public class PlatformInterop {
     public static void requestLayoutIfNeeded(View view) {
        
        // If the view isn't currently in the layout process, then we simply request
        // that layout happen next time around
        if (!view.isInLayout())	{
            view.requestLayout();
			return;
		}
		
        /* 
            Something is requesting layout while the view is already in the middle of a layout pass. This is most 
            likely because a layout-affecting property has been data bound to another layout-affecting property, e.g. 
            binding the width of a child control to the ActualWidth of its parent.
            
            If we simply call `requestLayout()` again right now, it will set a flag which will be cleared at the end 
            of the current layout pass, and the view will not be laid out with the updated values.

            Instead, we post the layout request to the UI thread's queue, ensuring that it will happen after the current
            layout pass has finished. Layout will happen again with the updated values.
        */

		Runnable runnable = () -> { 
			if (!view.isInLayout())	{
				view.requestLayout();
			}
		};
		
		view.post(runnable);
    }

    public static void removeFromParent(View view) {
        ViewParent parent = view.getParent();
        if (parent == null)
            return;
        ((ViewGroup) parent).removeView(view);
    }

    public static void setPivotXIfNeeded(View view, float pivotX) {
        if (view.getPivotX() != pivotX)
            view.setPivotX(pivotX);
    }

    public static void setPivotYIfNeeded(View view, float pivotY) {
        if (view.getPivotY() != pivotY)
            view.setPivotY(pivotY);
    }

    public static void setContentDescriptionForAutomationId(View view, String description) {
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

    public static View getSemanticPlatformElement(View view) {
        if (view instanceof SearchView) {
            view = view.findViewById(androidx.appcompat.R.id.search_src_text);
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
        float pivotY) {
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

    @NonNull
    public static LinearLayout createNavigationBarOuterLayout(Context context) {
        LinearLayout linearLayout = new LinearLayout(context);
        linearLayout.setOrientation(LinearLayout.VERTICAL);
        linearLayout.setLayoutParams(new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT));
        return linearLayout;
    }

    @NonNull
    public static FrameLayout createNavigationBarArea(Context context, LinearLayout linearLayout) {
        FrameLayout frameLayout = new FrameLayout(context);
        frameLayout.setId(View.generateViewId());
        LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, 0);
        layoutParams.gravity = Gravity.FILL;
        layoutParams.weight = 1;
        frameLayout.setLayoutParams(layoutParams);
        linearLayout.addView(frameLayout);
        return frameLayout;
    }

    @NonNull
    public static BottomNavigationView createNavigationBar(Context context, int styleAttribute, LinearLayout linearLayout, BottomNavigationView.OnItemSelectedListener listener) {
        BottomNavigationView navigationView = new BottomNavigationView(context, null, styleAttribute);
        navigationView.setLayoutParams(new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT));
        navigationView.setBackgroundColor(Color.WHITE);
        navigationView.setOnItemSelectedListener(listener);
        linearLayout.addView(navigationView);
        return navigationView;
    }

    @NonNull
    public static MaterialToolbar createToolbar(Context context, int actionBarHeight, int popupTheme) {
        MaterialToolbar toolbar = new MaterialToolbar(context);
        AppBarLayout.LayoutParams layoutParams = new AppBarLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, actionBarHeight);
        layoutParams.setScrollFlags(0);
        toolbar.setLayoutParams(layoutParams);

        if (popupTheme > 0)
            toolbar.setPopupTheme(popupTheme);

        return toolbar;
    }

    @NonNull
    public static CoordinatorLayout createShellCoordinatorLayout(Context context) {
        CoordinatorLayout layout = new CoordinatorLayout(context);
        layout.setLayoutParams(new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT));
        return layout;
    }

    @NonNull
    public static AppBarLayout createShellAppBar(Context context, int appBarStyleAttribute, CoordinatorLayout layout) {
        AppBarLayout appbar = new AppBarLayout(context, null, appBarStyleAttribute);
        appbar.setLayoutParams(new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT));
        layout.addView(appbar);
        return appbar;
    }

    @NonNull
    public static TabLayout createShellTabLayout(Context context, AppBarLayout appbar, int actionBarHeight) {
        TabLayout layout = new TabLayout(context);
        AppBarLayout.LayoutParams layoutParams = new AppBarLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, actionBarHeight);
        layoutParams.gravity = Gravity.BOTTOM;
        layout.setLayoutParams(layoutParams);
        layout.setTabMode(TabLayout.MODE_SCROLLABLE);
        appbar.addView(layout);
        return layout;
    }

    @NonNull
    public static ViewPager2 createShellViewPager(Context context, CoordinatorLayout layout, TabLayout tabLayout, TabLayoutMediator.TabConfigurationStrategy tabConfigurationStrategy, FragmentStateAdapter adapter, ViewPager2.OnPageChangeCallback callback) {
        ViewPager2 pager = new ViewPager2(context);
        CoordinatorLayout.LayoutParams layoutParams = new CoordinatorLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT);
        layoutParams.setBehavior(new AppBarLayout.ScrollingViewBehavior());
        pager.setOverScrollMode(ViewPager2.OVER_SCROLL_NEVER);
        pager.setId(View.generateViewId());
        pager.setLayoutParams(layoutParams);
        pager.setAdapter(adapter);
        pager.registerOnPageChangeCallback(callback);
        layout.addView(pager);

        new TabLayoutMediator(tabLayout, pager, tabConfigurationStrategy)
            .attach();

        return pager;
    }

    /**
     * Call setColorFilter on a Drawable, passing in (int)Microsoft.Maui.FilterMode
     * Calls the appropriate methods for Android API 29/Q+
     * @param drawable android.graphics.Drawable
     * @param color android.graphics.Color
     * @param mode (int)Microsoft.Maui.FilterMode
     */
    public static void setColorFilter(@NonNull Drawable drawable, int color, int mode) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            drawable.setColorFilter(new BlendModeColorFilter(color, getBlendMode(mode)));
        } else {
            drawable.setColorFilter(color, getPorterMode(mode));
        }
    }

    @RequiresApi(api = Build.VERSION_CODES.Q)
    static BlendMode getBlendMode(int mode) {
        // NOTE: keep in sync with src/Core/src/Primitives/FilterMode.cs
        switch (mode) {
            case 0: return BlendMode.SRC_IN;
            case 1: return BlendMode.MULTIPLY;
            case 2: return BlendMode.SRC_ATOP;
            default: throw new RuntimeException("Invalid Mode");
        }
    }

    static PorterDuff.Mode getPorterMode(int mode) {
        // NOTE: keep in sync with src/Core/src/Primitives/FilterMode.cs
        switch (mode) {
            case 0: return PorterDuff.Mode.SRC_IN;
            case 1: return PorterDuff.Mode.MULTIPLY;
            case 2: return PorterDuff.Mode.SRC_ATOP;
            default: throw new RuntimeException("Invalid Mode");
        }
    }

    private static void prepare(RequestBuilder<Drawable> builder, Target<Drawable> target, Boolean cachingEnabled, ImageLoaderCallback callback) {
        // A special value to work around https://github.com/dotnet/maui/issues/6783 where targets
        // are actually re-used if all the variables are the same.
        // Adding this "error image" that will always load a null image makes each request unique,
        // but does not affect the various caching levels.
        builder = builder
            .error(callback);

        if (!cachingEnabled) {
            builder = builder
                .diskCacheStrategy(DiskCacheStrategy.NONE)
                .skipMemoryCache(true);
        }

        builder
            .into(target);
    }

    private static void loadInto(RequestBuilder<Drawable> builder, ImageView imageView, Boolean cachingEnabled, ImageLoaderCallback callback) {
        MauiCustomViewTarget target = new MauiCustomViewTarget(imageView, callback);
        prepare(builder, target, cachingEnabled, callback);
    }

    private static void load(RequestBuilder<Drawable> builder, Context context, Boolean cachingEnabled, ImageLoaderCallback callback) {
        MauiCustomTarget target = new MauiCustomTarget(context, callback);
        prepare(builder, target, cachingEnabled, callback);
    }

    public static void loadImageFromFile(ImageView imageView, String file, ImageLoaderCallback callback) {
        RequestBuilder<Drawable> builder = Glide
            .with(imageView)
            .load(file);
        loadInto(builder, imageView, true, callback);
    }

    public static void loadImageFromUri(ImageView imageView, String uri, Boolean cachingEnabled, ImageLoaderCallback callback) {
        Uri androidUri = Uri.parse(uri);
        if (androidUri == null) {
            callback.onComplete(false, null, null);
            return;
        }
        RequestBuilder<Drawable> builder = Glide
            .with(imageView)
            .load(androidUri);
        loadInto(builder, imageView, cachingEnabled, callback);
    }

    public static void loadImageFromStream(ImageView imageView, InputStream inputStream, ImageLoaderCallback callback) {
        RequestBuilder<Drawable> builder = Glide
            .with(imageView)
            .load(inputStream);
        loadInto(builder, imageView, false, callback);
    }

    public static void loadImageFromFont(ImageView imageView, @ColorInt int color, String glyph, Typeface typeface, float textSize, ImageLoaderCallback callback) {
        FontModel fontModel = new FontModel(color, glyph, textSize, typeface);
        RequestBuilder<Drawable> builder = Glide
            .with(imageView)
            .load(fontModel)
            .override(Target.SIZE_ORIGINAL, Target.SIZE_ORIGINAL);
        loadInto(builder, imageView, true, callback);
    }

    public static void loadImageFromFile(Context context, String file, ImageLoaderCallback callback) {
        RequestBuilder<Drawable> builder = Glide
            .with(context)
            .load(file);
        load(builder, context, true, callback);
    }

    public static void loadImageFromUri(Context context, String uri, Boolean cachingEnabled, ImageLoaderCallback callback) {
        Uri androidUri = Uri.parse(uri);
        if (androidUri == null) {
            callback.onComplete(false, null, null);
            return;
        }
        RequestBuilder<Drawable> builder = Glide
            .with(context)
            .load(androidUri);
        load(builder, context, cachingEnabled, callback);
    }

    public static void loadImageFromStream(Context context, InputStream inputStream, ImageLoaderCallback callback) {
        RequestBuilder<Drawable> builder = Glide
            .with(context)
            .load(inputStream);
        load(builder, context, false, callback);
    }

    public static void loadImageFromFont(Context context, @ColorInt int color, String glyph, Typeface typeface, float textSize, ImageLoaderCallback callback) {
        FontModel fontModel = new FontModel(color, glyph, textSize, typeface);
        RequestBuilder<Drawable> builder = Glide
            .with(context)
            .load(fontModel)
            .override(Target.SIZE_ORIGINAL, Target.SIZE_ORIGINAL);
        load(builder, context, true, callback);
    }

    public static ColorStateList getColorStateListForToolbarStyleableAttribute(Context context, int resId, int index) {
        TintTypedArray styledAttributes = TintTypedArray.obtainStyledAttributes(context, null, R.styleable.Toolbar, resId, 0);
        try {
            return styledAttributes.getColorStateList(index);
        } finally {
            styledAttributes.recycle();
        }
    }
    
    public static long measureAndGetWidthAndHeight(View view, int widthMeasureSpec, int heightMeasureSpec) {
        view.measure(widthMeasureSpec, heightMeasureSpec);
        int width = view.getMeasuredWidth();
        int height = view.getMeasuredHeight();
        return ((long)width << 32) | (height & 0xffffffffL);
    }

    @NonNull
    public static ColorStateList getDefaultColorStateList(int color)
    {
        return new ColorStateList(ColorStates.DEFAULT, new int[] { color });
    }

    @NonNull
    public static ColorStateList getEditTextColorStateList(int enabled, int disabled)
    {
        return new ColorStateList(ColorStates.getEditTextState(), new int[] { enabled, disabled });
    }

    @NonNull
    public static ColorStateList getCheckBoxColorStateList(int enabledChecked, int enabledUnchecked, int disabledChecked, int disabledUnchecked)
    {
        return new ColorStateList(ColorStates.getCheckBoxState(), new int[] { enabledChecked, enabledUnchecked, disabledChecked, disabledUnchecked });
    }

    @NonNull
    public static ColorStateList getSwitchColorStateList(int disabled, int on, int normal)
    {
        return new ColorStateList(ColorStates.getSwitchState(), new int[] { disabled, on, normal });
    }

    @NonNull
    public static ColorStateList getButtonColorStateList(int enabled, int disabled, int off, int pressed)
    {
        return new ColorStateList(ColorStates.getButtonState(), new int[] { enabled, disabled, off, pressed });
    }

    /**
     * Creates a ColorStateList for EditText if the existing colors do not match
     * @return a ColorStateList, or null if one is not needed
     */
    public static ColorStateList createEditTextColorStateList(ColorStateList colorStateList, int color)
    {
        if (colorStateList == null) {
            return getEditTextColorStateList(color, color);
        }
        int[][] editTextState = ColorStates.getEditTextState();
        for (int i = 0; i < editTextState.length; i++) {
            if (colorStateList.getColorForState(editTextState[i], color) != color) {
                return getEditTextColorStateList(color, color);
            }
        }
        return null;
    }

    /**
     * Sets many values at once on a Paint object
     * @param paint
     * @param strokeWidth
     * @param strokeJoin
     * @param strokeCap
     * @param strokeMiter
     * @param pathEffect
     */
    public static void setPaintValues(Paint paint, float strokeWidth, Paint.Join strokeJoin, Paint.Cap strokeCap, float strokeMiter, PathEffect pathEffect)
    {
        paint.setStrokeWidth(strokeWidth);
        paint.setStrokeJoin(strokeJoin);
        paint.setStrokeCap(strokeCap);
        paint.setStrokeMiter(strokeMiter);
        if (pathEffect != null) {
            paint.setPathEffect(pathEffect);
        }
    }

    /**
     * Calls canvas.saveLayer(), draws paths for clipPath & borderPaint, then canvas.restoreToCount()
     * @param drawable
     * @param canvas
     * @param width
     * @param height
     * @param clipPath
     * @param borderPaint
     */
    public static void drawMauiDrawablePath(PaintDrawable drawable, Canvas canvas, int width, int height, @NonNull Path clipPath, Paint borderPaint)
    {
        int saveCount = canvas.saveLayer(0, 0, width, height, null);

        Paint paint = drawable.getPaint();
        if (paint != null) {
            canvas.drawPath(clipPath, paint);
        }
        if (borderPaint != null) {
            canvas.drawPath(clipPath, borderPaint);
        }

        canvas.restoreToCount(saveCount);
    }

    /**
     * Gets the value of android.R.attr.windowBackground from the given Context
     * @param context
     * @return the color or -1 if not found
     */
    public static int getWindowBackgroundColor(Context context)
    {
        TypedValue value = new TypedValue();
        if (!context.getTheme().resolveAttribute(android.R.attr.windowBackground, value, true) && isColorType(value)) {
            return value.data;
        } else {
            return -1;
        }
    }

    /**
     * Needed because TypedValue.isColorType() is only API Q+
     * https://github.com/aosp-mirror/platform_frameworks_base/blob/1d896eeeb8744a1498128d62c09a3aa0a2a29a16/core/java/android/util/TypedValue.java#L266-L268
     * @param value
     * @return true if the TypedValue is a Color
     */
    private static boolean isColorType(TypedValue value)
    {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            return value.isColorType();
        } else {
            // Implementation from AOSP
            return (value.type >= TypedValue.TYPE_FIRST_COLOR_INT && value.type <= TypedValue.TYPE_LAST_COLOR_INT);
        }
    }

    /**
     * Sets the maxLength of an EditText
     * @param editText
     * @param maxLength
     */
    public static void updateMaxLength(@NonNull EditText editText, int maxLength)
    {
        setLengthFilter(editText, maxLength);

        if (maxLength < 0)
            return;

        Editable currentText = editText.getText();
        if (currentText.length() > maxLength) {
            editText.setText(currentText.subSequence(0, maxLength));
        }
    }

    /**
     * Updates the InputFilter[] of an EditText. Used for Entry and SearchBar.
     * @param editText
     * @param maxLength
     */
    public static void setLengthFilter(@NonNull EditText editText, int maxLength)
    {
        if (maxLength == -1)
            maxLength = Integer.MAX_VALUE;

        List<InputFilter> currentFilters = new ArrayList<>(Arrays.asList(editText.getFilters()));
        boolean changed = false;
        for (int i = 0; i < currentFilters.size(); i++) {
            InputFilter filter = currentFilters.get(i);
            if (filter instanceof InputFilter.LengthFilter) {
                currentFilters.remove(i);
                changed = true;
                break;
            }
        }

        if (maxLength >= 0) {
            currentFilters.add(new InputFilter.LengthFilter(maxLength));
            changed = true;
        }
        if (changed) {
            InputFilter[] newFilter = new InputFilter[currentFilters.size()];
            editText.setFilters(currentFilters.toArray(newFilter));
        }
    }

    /**
     * Computes the current WindowMetrics' bounds
     * @param activity
     * @return Rect value of the bounds
     */
    @NonNull
    public static Rect getCurrentWindowMetrics(Activity activity) {
        return WindowMetricsCalculator.Companion
            .getOrCreate()
            .computeCurrentWindowMetrics(activity)
            .getBounds();
    }

    /**
     * Gets font metrics based on the given context and default font size
     * @param context
     * @param defaultFontSize
     * @return FontMetrics object or null if context or display metrics is null
     */
    public static Paint.FontMetrics getFontMetrics(Context context, float defaultFontSize) {
        if (context == null)
            return null;

        DisplayMetrics metrics = context.getResources().getDisplayMetrics();
        if (metrics != null) {
            return new Paint() {{
                setTextSize(
                    TypedValue.applyDimension(
                        TypedValue.COMPLEX_UNIT_SP,
                        defaultFontSize,
                        metrics
                ));
            }}.getFontMetrics();
        } else {
            return null;
        }
    }

    private static class ColorStates
    {
        static final int[] EMPTY = new int[] { };
        static final int[][] DEFAULT = new int[][] { EMPTY };

        private static int[][] editTextState, checkBoxState, switchState, buttonState;

        static int[][] getEditTextState()
        {
            if (editTextState == null) {
                editTextState = new int[][] {
                  new int[] {  android.R.attr.state_enabled },
                  new int[] { -android.R.attr.state_enabled },
                };
            }
            return editTextState;
        }

        static int[][] getCheckBoxState()
        {
            if (checkBoxState == null) {
                checkBoxState = new int[][] {
                    new int[] {  android.R.attr.state_enabled,  android.R.attr.state_checked },
                    new int[] {  android.R.attr.state_enabled, -android.R.attr.state_checked },
                    new int[] { -android.R.attr.state_enabled,  android.R.attr.state_checked },
                    new int[] { -android.R.attr.state_enabled, -android.R.attr.state_pressed },
                };
            }
            return checkBoxState;
        }

        static int[][] getSwitchState()
        {
            if (switchState == null) {
                switchState = new int[][] {
                    new int[] { -android.R.attr.state_enabled },
                    new int[] {  android.R.attr.state_checked },
                    EMPTY,
                };
            }
            return switchState;
        }

        static int[][] getButtonState()
        {
            if (buttonState == null) {
                buttonState = new int[][] {
                    new int[] {  android.R.attr.state_enabled },
                    new int[] { -android.R.attr.state_enabled },
                    new int[] { -android.R.attr.state_checked },
                    new int[] {  android.R.attr.state_pressed },
                };
            }
            return buttonState;
        }
    }
}
