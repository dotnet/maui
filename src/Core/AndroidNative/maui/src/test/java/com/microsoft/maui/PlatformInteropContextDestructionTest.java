package com.microsoft.maui;

import android.app.Activity;
import android.content.Context;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.mockito.Mock;
import org.mockito.junit.MockitoJUnitRunner;

import static org.mockito.Mockito.*;

/**
 * Unit tests for context destruction checks in PlatformInterop.
 * Tests the defensive measures to prevent Glide IllegalArgumentException crashes.
 * 
 * These tests verify that when contexts are destroyed, the image loading methods
 * return early and call the callback with false, preventing Glide crashes.
 */
@RunWith(MockitoJUnitRunner.class)
public class PlatformInteropContextDestructionTest {

    @Mock
    private Activity mockActivity;

    @Mock
    private ImageLoaderCallback mockCallback;

    @Test
    public void loadImageFromFile_withNullContext_callsCallbackWithFalse() {
        PlatformInterop.loadImageFromFile(null, "test.jpg", mockCallback);

        verify(mockCallback).onComplete(false, null, null);
        verifyNoMoreInteractions(mockCallback);
    }

    @Test
    public void loadImageFromFile_withDestroyedContext_callsCallbackWithFalse() {
        when(mockActivity.isDestroyed()).thenReturn(true);

        PlatformInterop.loadImageFromFile(mockActivity, "test.jpg", mockCallback);

        verify(mockCallback).onComplete(false, null, null);
        verifyNoMoreInteractions(mockCallback);
    }

    @Test
    public void loadImageFromFile_withFinishingContext_callsCallbackWithFalse() {
        when(mockActivity.isFinishing()).thenReturn(true);
        when(mockActivity.isDestroyed()).thenReturn(false);

        PlatformInterop.loadImageFromFile(mockActivity, "test.jpg", mockCallback);

        verify(mockCallback).onComplete(false, null, null);
        verifyNoMoreInteractions(mockCallback);
    }

    @Test
    public void loadImageFromUri_withNullContext_callsCallbackWithFalse() {
        PlatformInterop.loadImageFromUri(null, "https://example.com/image.jpg", true, mockCallback);

        verify(mockCallback).onComplete(false, null, null);
        verifyNoMoreInteractions(mockCallback);
    }

    @Test
    public void loadImageFromUri_withDestroyedContext_callsCallbackWithFalse() {
        when(mockActivity.isDestroyed()).thenReturn(true);

        PlatformInterop.loadImageFromUri(mockActivity, "https://example.com/image.jpg", true, mockCallback);

        verify(mockCallback).onComplete(false, null, null);
        verifyNoMoreInteractions(mockCallback);
    }

    @Test
    public void loadImageFromUri_withFinishingContext_callsCallbackWithFalse() {
        when(mockActivity.isFinishing()).thenReturn(true);
        when(mockActivity.isDestroyed()).thenReturn(false);

        PlatformInterop.loadImageFromUri(mockActivity, "https://example.com/image.jpg", true, mockCallback);

        verify(mockCallback).onComplete(false, null, null);
        verifyNoMoreInteractions(mockCallback);
    }

    @Test
    public void loadImageFromStream_withNullContext_callsCallbackWithFalse() {
        PlatformInterop.loadImageFromStream(null, null, mockCallback);

        verify(mockCallback).onComplete(false, null, null);
        verifyNoMoreInteractions(mockCallback);
    }

    @Test
    public void loadImageFromStream_withDestroyedContext_callsCallbackWithFalse() {
        when(mockActivity.isDestroyed()).thenReturn(true);

        PlatformInterop.loadImageFromStream(mockActivity, null, mockCallback);

        verify(mockCallback).onComplete(false, null, null);
        verifyNoMoreInteractions(mockCallback);
    }

    @Test
    public void loadImageFromStream_withFinishingContext_callsCallbackWithFalse() {
        when(mockActivity.isFinishing()).thenReturn(true);
        when(mockActivity.isDestroyed()).thenReturn(false);

        PlatformInterop.loadImageFromStream(mockActivity, null, mockCallback);

        verify(mockCallback).onComplete(false, null, null);
        verifyNoMoreInteractions(mockCallback);
    }

    @Test
    public void loadImageFromFont_withNullContext_callsCallbackWithFalse() {
        PlatformInterop.loadImageFromFont(null, 0xFF000000, "A", null, 16.0f, mockCallback);

        verify(mockCallback).onComplete(false, null, null);
        verifyNoMoreInteractions(mockCallback);
    }

    @Test
    public void loadImageFromFont_withDestroyedContext_callsCallbackWithFalse() {
        when(mockActivity.isDestroyed()).thenReturn(true);

        PlatformInterop.loadImageFromFont(mockActivity, 0xFF000000, "A", null, 16.0f, mockCallback);

        verify(mockCallback).onComplete(false, null, null);
        verifyNoMoreInteractions(mockCallback);
    }

    @Test
    public void loadImageFromFont_withFinishingContext_callsCallbackWithFalse() {
        when(mockActivity.isFinishing()).thenReturn(true);
        when(mockActivity.isDestroyed()).thenReturn(false);

        PlatformInterop.loadImageFromFont(mockActivity, 0xFF000000, "A", null, 16.0f, mockCallback);

        verify(mockCallback).onComplete(false, null, null);
        verifyNoMoreInteractions(mockCallback);
    }
}
}