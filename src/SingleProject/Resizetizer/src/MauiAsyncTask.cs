// Based on: https://github.com/xamarin/xamarin-android-tools/blob/d92fc3e3a27e8240551baa813b15d6bf006a5620/src/Microsoft.Android.Build.BaseTasks/AndroidAsyncTask.cs

using System;
using static System.Threading.Tasks.TaskExtensions;

namespace Microsoft.Maui.Resizetizer
{
	public abstract class MauiAsyncTask : AsyncTask
	{
		/// <summary>
		/// Typically `ExecuteAsync` will be the preferred method to override instead of this one.
		/// </summary>
		public override bool Execute()
		{
			Yield();
			try
			{
				this.RunTask(() => ExecuteAsync())
					.Unwrap()
					.ContinueWith(Complete);

				// This blocks on AsyncTask.Execute, until Complete is called
				return base.Execute();
			}
			catch (Exception exc)
			{
				LogCodedError("MAUI0000", exc.ToString());
				return false;
			}
			finally
			{
				Reacquire();
			}
		}

		/// <summary>
		/// Override this method for simplicity of AsyncTask usage:
		/// * Yield / Reacquire is handled for you
		/// * RunTaskAsync is already on a background thread
		/// </summary>
		public virtual System.Threading.Tasks.Task ExecuteAsync() => System.Threading.Tasks.Task.CompletedTask;
	}
}