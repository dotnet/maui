package com.microsoft.maui;

import android.content.Context;

import androidx.annotation.NonNull;
import androidx.appcompat.widget.AppCompatTextView;

public class PlatformAppCompatTextView extends AppCompatTextView {
    public PlatformAppCompatTextView(@NonNull Context context) {
        super(context);
    }

    private boolean isFormatted;

    /**
     * Sets isFormatted=true, used for FormattedString content
     * @param text
     * @param type
     */
    @Override
    public void setText(CharSequence text, BufferType type) {
        isFormatted = !(text instanceof String);
        super.setText(text, type);
    }

    @Override
    protected void onLayout(boolean changed, int left, int top, int right, int bottom) {
        super.onLayout(changed, left, top, right, bottom);

        if (isFormatted) {
            onLayoutFormatted(changed, left, top, right, bottom);
        }
    }

    /**
     * To be overridden from C#
     * @param changed
     * @param left
     * @param top
     * @param right
     * @param bottom
     */
    protected void onLayoutFormatted(boolean changed, int left, int top, int right, int bottom) { }
}
