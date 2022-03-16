using Android.Graphics.Drawables;
using Android.OS;
using Bumptech.Glide;
using Bumptech.Glide.Request.Target;
using Bumptech.Glide.Request.Transition;
using Java.Lang;
using AImageView = Android.Widget.ImageView;

namespace Benchmarks.Droid;

// SimpleTarget is apparently obsolete?
#pragma warning disable CS0612

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class ImageBenchmark
{
	AImageView? imageView;
	Glide? glide;
	Handler? handler;

	[GlobalSetup]
	public void GlobalSetup()
	{
		imageView = new AImageView(MainInstrumentation.Instance!.Context);
		glide = Glide.Get(MainInstrumentation.Instance!.Context);
		handler = new Handler(Looper.MainLooper!);
	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		imageView?.Dispose();
		imageView = null;
	}

	//[Benchmark]
	public void SetImageResource() => imageView!.SetImageResource(Resource.Drawable.dotnet_bot);

	//[Benchmark]
	public void SetImageDrawable() => imageView!.SetImageDrawable(MainInstrumentation.Instance!.Context!.GetDrawable(Resource.Drawable.dotnet_bot));

	[Benchmark]
	public void ImageHelperFromFile()
	{
		var callback = new Callback();

		handler!.Post(() => { 
			try
			{
				Microsoft.Maui.ImageLoader.LoadFromFile(
					imageView,
					"dotnet_bot.png",
					callback);
			}
			catch (System.Exception ex)
			{
				Log.Debug("DOTNET-BENCH", ex.ToString());
			}
		});

		Android.Util.Log.Debug("DOTNET-BENCH", "Waiting for Glide");
		callback.SuccessTask.GetAwaiter().GetResult();
		Android.Util.Log.Debug("DOTNET-BENCH", "Finsihed Glide");
	}

	class Callback : Java.Lang.Object, Microsoft.Maui.IImageLoaderCallback
	{
		readonly TaskCompletionSource<Drawable?> tcsDrawable = new TaskCompletionSource<Drawable?>();
		readonly TaskCompletionSource<bool> tcsSuccess = new TaskCompletionSource<bool>();

		public Task<Drawable?> DrawableTask => tcsDrawable.Task;
		public Task<bool> SuccessTask => tcsSuccess.Task;

		public void OnComplete(Java.Lang.Boolean? success, Drawable? drawable, IRunnable? dispose)
		{
			Android.Util.Log.Debug("DOTNET-BENCH", "Callback OnComplete");
			tcsSuccess.SetResult(success?.BooleanValue() ?? false);
			tcsDrawable.SetResult(drawable);
		}
	}
}