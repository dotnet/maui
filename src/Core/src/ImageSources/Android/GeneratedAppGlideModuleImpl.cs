using System;
using System.Collections.Generic;
using Android.Content;
using Android.Runtime;
using Bumptech.Glide;
using Bumptech.Glide.Load;
using Bumptech.Glide.Load.Data;
using Bumptech.Glide.Load.Model;
using Bumptech.Glide.Signature;
using Java.IO;

namespace Microsoft.Maui
{
	// This class name is very specific as it is directly loaded.
	[Register("com.bumptech.glide.GeneratedAppGlideModuleImpl")]
	class GeneratedAppGlideModuleImpl : GeneratedAppGlideModule
	{
		public GeneratedAppGlideModuleImpl(Context context)
		{
		}

		public override void RegisterComponents(Context context, Glide glide, Registry registry) =>
			registry.Prepend(InputStreamModelLoader.InputStreamClass, InputStreamModelLoader.InputStreamClass, new InputStreamModelLoaderFactory());

		public override ICollection<Java.Lang.Class> ExcludedModuleClasses =>
			Array.Empty<Java.Lang.Class>();
	}

	class InputStreamModelLoader : Java.Lang.Object, IModelLoader
	{
		public static readonly Java.Lang.Class InputStreamClass = Java.Lang.Class.FromType(typeof(InputStream));

		public ModelLoaderLoadData BuildLoadData(Java.Lang.Object model, int width, int height, Options options) =>
			new ModelLoaderLoadData(new ObjectKey(Guid.NewGuid().ToString()), new InputStreamDataFetcher((InputStream)model));

		public bool Handles(Java.Lang.Object model) =>
			model is InputStream;
	}

	class InputStreamDataFetcher : Java.Lang.Object, IDataFetcher
	{
		readonly InputStream _inputStream;

		public InputStreamDataFetcher(InputStream inputStream)
		{
			_inputStream = inputStream;
		}

		public Java.Lang.Class DataClass =>
			InputStreamModelLoader.InputStreamClass;

		public DataSource DataSource =>
			DataSource.Local;

		public void Cancel() { }

		public void Cleanup() =>
			_inputStream?.Dispose();

		public void LoadData(Priority priority, IDataFetcherDataCallback callback) =>
			callback.OnDataReady(_inputStream);
	}

	class InputStreamModelLoaderFactory : Java.Lang.Object, IModelLoaderFactory
	{
		public IModelLoader Build(MultiModelLoaderFactory factory) =>
			new InputStreamModelLoader();

		public void Teardown() { }
	}
}