#nullable enable
using Bumptech.Glide;
using Bumptech.Glide.Load;
using Bumptech.Glide.Load.Data;
using Bumptech.Glide.Load.Model;
using Bumptech.Glide.Signature;

namespace Microsoft.Maui.BumptechGlide
{
	public class PassThroughModelLoader<T> : Java.Lang.Object, IModelLoader
	{
		public ModelLoaderLoadData BuildLoadData(Java.Lang.Object model, int width, int height, Options options) =>
			new ModelLoaderLoadData(new ObjectKey(model.ToString()), new DataFetcher(model));

		public bool Handles(Java.Lang.Object model) =>
			model is T;

		public class DataFetcher : Java.Lang.Object, IDataFetcher
		{
			readonly Java.Lang.Object _model;

			public DataFetcher(Java.Lang.Object model)
			{
				_model = model;
			}

			public Java.Lang.Class DataClass =>
				Java.Lang.Class.FromType(typeof(T));

			public DataSource DataSource =>
				DataSource.Local;

			public void Cancel() { }

			public void Cleanup() =>
				_model?.Dispose();

			public void LoadData(Priority priority, IDataFetcherDataCallback callback) =>
				callback.OnDataReady(_model);
		}

		public class Factory : Java.Lang.Object, IModelLoaderFactory
		{
			public IModelLoader Build(MultiModelLoaderFactory factory) =>
				new PassThroughModelLoader<T>();

			public void Teardown() { }
		}
	}
}