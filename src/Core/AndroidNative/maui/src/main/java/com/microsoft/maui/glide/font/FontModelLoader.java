package com.microsoft.maui.glide.font;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.bumptech.glide.load.Options;
import com.bumptech.glide.load.model.ModelLoader;

public class FontModelLoader implements ModelLoader<FontModel, FontModel> {
    @Nullable
    @Override
    public LoadData<FontModel> buildLoadData(@NonNull FontModel fontModel, int width, int height, @NonNull Options options) {
        return new LoadData<>(fontModel.getCacheKey(), new FontModelDataFetcher(fontModel));
    }

    @Override
    public boolean handles(@NonNull FontModel fontModel) {
        return true;
    }
}

