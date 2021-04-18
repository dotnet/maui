using System;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Bumptech.Glide.Request.Target;
using Bumptech.Glide.Request.Transition;

namespace Microsoft.Maui.BumptechGlide
{
	public class TaskTarget : CustomTarget
	{
		public TaskTarget(TaskCompletionSource<Drawable> tcs)
		{
			TaskCompletionSource = tcs;
		}

		public TaskCompletionSource<Drawable> TaskCompletionSource { get; }

		public override void OnLoadCleared(Drawable placeholder)
		{
		}

		public override void OnLoadFailed(Drawable errorDrawable)
		{
		}

		public override void OnLoadStarted(Drawable placeholder)
		{
		}

		public override void OnResourceReady(Java.Lang.Object resource, ITransition transition)
		{
			try
			{
				TaskCompletionSource.TrySetResult((Drawable)resource);
			}
			catch (Exception ex)
			{
				TaskCompletionSource.TrySetException(ex);
			}
		}

		public override void OnStart()
		{
		}

		public override void OnStop()
		{
		}

		public override void OnDestroy()
		{
		}
	}
}