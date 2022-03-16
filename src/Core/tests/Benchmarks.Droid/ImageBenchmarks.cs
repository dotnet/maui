using Android.Graphics.Drawables;
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

	[GlobalSetup]
	public void GlobalSetup() => imageView = new AImageView(MainInstrumentation.Instance!.Context);

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
	public async Task ImageHelperFromFile()
	{
		var tcsDone = new TaskCompletionSource<bool>();

		MainInstrumentation.Instance!.RunOnMainSync(() =>
		{
			try
			{
				Microsoft.Maui.ImageLoader.LoadFromUri(
					imageView,
					"https://user-images.githubusercontent.com/898335/146419825-23e0b47e-d14b-47d7-85cd-bd8299c2ae98.png", //"dotnet_bot.png",
					new Java.Lang.Boolean(true),
					new Callback(tcsDone.SetResult));
			}
			catch (System.Exception ex)
			{
				Log.Debug("DOTNET-BENCH", ex.ToString());
			}
		});

		Android.Util.Log.Debug("DOTNET-BENCH", "Waiting for Glide");
		await tcsDone.Task;
		Android.Util.Log.Debug("DOTNET-BENCH", "Finsihed Glide");
	}

	class Callback : Java.Lang.Object, Microsoft.Maui.IImageLoaderCallback
	{
		public Callback(Action<bool> handler)
			=> this.handler = handler;

		readonly Action<bool> handler;

		public void OnComplete(Java.Lang.Boolean? success, Drawable? drawable, IRunnable? dispose)
		{
			handler?.Invoke(success?.BooleanValue() ?? false);
		}
	}
}