#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Runtime;
using Bumptech.Glide;
using Bumptech.Glide.Load;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request.Target;
using Java.Lang;

namespace Microsoft.Maui.BumptechGlide
{
	public static class RequestBuilderExtensions
	{
		public static Task<ImageSourceServiceResult?> SubmitAsync(this RequestBuilder requestBuilder, Context context, CancellationToken cancellationToken = default) =>
			requestBuilder.SubmitAsync(Glide.With(context), cancellationToken);

		public static async Task<ImageSourceServiceResult?> SubmitAsync(this RequestBuilder requestBuilder, RequestManager requestManager, CancellationToken cancellationToken = default)
		{
			var callback = new RequestListener();

			try
			{
				requestBuilder = requestBuilder.AddListener(callback);

				var target = requestBuilder.Submit();

				var drawable = await callback.Result;
				if (drawable == null)
					return null;

				return new ImageSourceServiceResult(drawable, () => requestManager.Clear(target));
			}
			finally
			{
				GC.KeepAlive(callback);
			}
		}

		class RequestListener : Java.Lang.Object, Bumptech.Glide.Request.IRequestListener
		{
			public RequestListener()
			{
				tcsDrawable = new TaskCompletionSource<Drawable>();
			}

			readonly TaskCompletionSource<Drawable> tcsDrawable;

			public Task<Drawable> Result => tcsDrawable.Task;

			public bool OnLoadFailed(GlideException exception, Java.Lang.Object model, ITarget target, bool isFirstResource)
			{
				tcsDrawable.TrySetException(exception);
				return false; // True would prevent target.OnLoadFailed from being called - not necessary here
			}

			public bool OnResourceReady(Java.Lang.Object result, Java.Lang.Object model, ITarget target, DataSource dataSource, bool isFirstResource)
			{
				tcsDrawable.TrySetResult(result.JavaCast<Drawable>());
				return false; // True would prevent target.OnResourceReady from being called - not necessary here
			}
		}
	}
}