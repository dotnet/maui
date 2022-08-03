package com.microsoft.maui.glide.font;

import androidx.annotation.NonNull;

import com.bumptech.glide.Priority;
import com.bumptech.glide.load.DataSource;
import com.bumptech.glide.load.data.DataFetcher;

public class FontModelDataFetcher implements DataFetcher<FontModel> {

    private final FontModel model;

    FontModelDataFetcher(FontModel fontModel)
    {
        model = fontModel;
    }


    @Override
    public void loadData(@NonNull Priority priority, @NonNull DataCallback<? super FontModel> callback) {
        callback.onDataReady(model);
    }

    @Override
    public void cleanup() {

    }

    @Override
    public void cancel() {

    }

    @NonNull
    @Override
    public Class<FontModel> getDataClass() {
        return FontModel.class;
    }

    @NonNull
    @Override
    public DataSource getDataSource() {
        return DataSource.LOCAL;
    }
}

