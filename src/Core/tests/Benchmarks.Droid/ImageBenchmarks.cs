using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Bumptech.Glide;
using Java.Lang;
using Microsoft.Maui.Storage;
using AImageView = Android.Widget.ImageView;
using Path = System.IO.Path;

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
	Typeface? defaultTypeface;

	[GlobalSetup]
	public void GlobalSetup()
	{
		context = MainInstrumentation.Instance!.Context!;

		imageView = new AImageView(context);
		glide = Glide.Get(context);
		handler = new Handler(Looper.MainLooper!);
		defaultTypeface = Typeface.Default;

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

	[Benchmark]
	public async Task ImageHelperFromFont()
	{
		var callback = new Callback();

		handler!.Post(() =>
		{
			Microsoft.Maui.PlatformInterop.LoadImageFromFont(
				context,
				Color.Aquamarine,
				"A",
				defaultTypeface,
				24,
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