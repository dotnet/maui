using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Bumptech.Glide;
using Bumptech.Glide.Request.Target;
using Bumptech.Glide.Request.Transition;
using Java.Lang;
using Microsoft.Maui.Storage;
using AImageView = Android.Widget.ImageView;

namespace Benchmarks.Droid;

// SimpleTarget is apparently obsolete?
#pragma warning disable CS0612

public class ImageBenchmark
{
	AImageView? imageView;
	Glide? glide;
	Handler? handler;
	Context? context;
	string? imageFilename;

	[GlobalSetup]
	public void GlobalSetup()
	{
		context = MainInstrumentation.Instance!.Context!;

		imageView = new AImageView(context);
		glide = Glide.Get(context);
		handler = new Handler(Looper.MainLooper!);

		var imageName = "dotnet_bot.png";
		var cacheDir = FileSystem.CacheDirectory;
		imageFilename = Path.Combine(cacheDir, imageName);

		using (var s = context!.Assets!.Open(imageName))
		using (var w = File.Create(Path.Combine(cacheDir, imageName)))
		{
			s.CopyTo(w);
		}
	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		imageView?.Dispose();
		imageView = null;
	}

	[Benchmark]
	public void SetImageResource() => imageView!.SetImageResource(Resource.Drawable.dotnet_bot);

	[Benchmark]
	public void SetImageDrawable() => imageView!.SetImageDrawable(MainInstrumentation.Instance!.Context!.GetDrawable(Resource.Drawable.dotnet_bot));

	[Benchmark]
	public async Task ImageHelperFromFile()
	{
		var callback = new Callback();

		handler!.Post(() =>
		{
			Microsoft.Maui.PlatformInterop.LoadImageFromFile(
				context!,
				imageFilename,
				callback);
		});

		await callback.SuccessTask;
	}

	class Callback : Java.Lang.Object, Microsoft.Maui.IImageLoaderCallback
	{
		readonly TaskCompletionSource<Drawable?> tcsDrawable = new TaskCompletionSource<Drawable?>();
		readonly TaskCompletionSource<bool> tcsSuccess = new TaskCompletionSource<bool>();

		public Task<Drawable?> DrawableTask => tcsDrawable.Task;
		public Task<bool> SuccessTask => tcsSuccess.Task;

		public void OnComplete(Java.Lang.Boolean? success, Drawable? drawable, IRunnable? dispose)
		{
			tcsSuccess.SetResult(success?.BooleanValue() ?? false);
			tcsDrawable.SetResult(drawable);
		}
	}
}