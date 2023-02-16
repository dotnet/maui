#nullable enable
using System;
using System.Threading.Tasks;
using Android.OS;
using AndroidX.AppCompat.App;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
{
	public abstract class MauiTestActivity : AppCompatActivity
	{
		public TaskCompletionSource<Bundle> TaskCompletionSource { get; } = new TaskCompletionSource<Bundle>();

		protected override void OnCreate(Bundle? savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Do the work on the background thread to avoid a keyDispatchingTimedOut ANR
			Task.Run(async () =>
			{
				try
				{
					var runner = MauiTestInstrumentation.Current.Services.GetRequiredService<HeadlessTestRunner>();

					var bundle = await runner.RunTestsAsync();

					TaskCompletionSource.TrySetResult(bundle);
				}
				catch (Exception ex)
				{
					TaskCompletionSource.TrySetException(ex);
				}

				Finish();
			});
		}
	}
}