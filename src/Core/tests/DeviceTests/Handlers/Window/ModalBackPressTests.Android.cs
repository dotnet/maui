using System.Threading.Tasks;
using AndroidX.Activity;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public class ModalBackPressTests : TestBase
	{
		[Theory]
		[InlineData(CallbackMode.None, 0, 1, false)]
		[InlineData(CallbackMode.Disabled, 0, 1, false)]
		[InlineData(CallbackMode.Consumes, 1, 1, true)]
		[InlineData(CallbackMode.DispatchesAgain, 1, 1, false)]
		public async Task ModalBackPressRunsActivityCallbacks(CallbackMode mode, int expectedCallbackCount, int expectedHandlerCount, bool expectedConsumed)
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var activity = new MauiAppCompatActivity();
				activity.AddBackPressedCallback(null);
				var callback = new RecordingBackCallback(activity, mode);
				if (mode != CallbackMode.None)
					activity.OnBackPressedDispatcher.AddCallback(callback);

				var handlerCount = 0;
				var consumed = false;
				var dispatching = false;

				activity.DispatchModalBackPress(() =>
				{
					handlerCount++;
					consumed = activity.IsModalBackPressConsumed;
					dispatching = activity.IsDispatchingModalBackPress;
				});

				Assert.Equal(expectedCallbackCount, callback.Count);
				Assert.Equal(expectedHandlerCount, handlerCount);
				Assert.Equal(expectedConsumed, consumed);
				Assert.Equal(!expectedConsumed, dispatching);
				Assert.False(activity.IsModalBackPressConsumed);
				Assert.False(activity.IsDispatchingModalBackPress);
			});
		}

		public enum CallbackMode
		{
			None,
			Disabled,
			Consumes,
			DispatchesAgain,
		}

		class RecordingBackCallback : OnBackPressedCallback
		{
			readonly ComponentActivity _activity;
			readonly CallbackMode _mode;

			public RecordingBackCallback(ComponentActivity activity, CallbackMode mode) : base(mode != CallbackMode.Disabled)
			{
				_activity = activity;
				_mode = mode;
			}

			public int Count { get; private set; }

			public override void HandleOnBackPressed()
			{
				Count++;

				if (_mode == CallbackMode.DispatchesAgain)
				{
					Enabled = false;
					_activity.OnBackPressedDispatcher.OnBackPressed();
				}
			}
		}
	}
}
