using System;
using System.Threading.Tasks;
using UIKit;
using Xunit;

#if IOS || MACCATALYST

namespace Microsoft.Maui.DeviceTests.Memory
{
	// Set of tests to verify UIView subclasses do not leak
	[Category(TestCategory.Memory)]
	public class UIViewSubclassTests : TestBase
	{
#if IOS // MauiDoneAccessoryView is iOS only
		void DoAction() { }
		void DoAction(object data) { }

		[Fact]
		public async Task MauiDoneAccessoryView_Ctor()
		{
			WeakReference reference = null;
			Action action = DoAction;

			await InvokeOnMainThreadAsync(() =>
			{
				var accessory = new MauiDoneAccessoryView(action);
				reference = new(accessory);
			});

			await AssertionExtensions.WaitForGC(reference);
			Assert.False(reference.IsAlive, "MauiDoneAccessoryView should not be alive!");
		}

		[Fact]
		public async Task MauiDoneAccessoryView_SetDoneClicked()
		{
			WeakReference reference = null;
			Action<object> action = DoAction;

			await InvokeOnMainThreadAsync(() =>
			{
				var accessory = new MauiDoneAccessoryView();
				reference = new(accessory);
				accessory.SetDoneClicked(action);
			});

			await AssertionExtensions.WaitForGC(reference);
			Assert.False(reference.IsAlive, "MauiDoneAccessoryView should not be alive!");
		}
#endif

		[Fact]
		public async Task ResignFirstResponderTouchGestureRecognizer()
		{
			WeakReference viewReference = null;
			WeakReference recognizerReference = null;

			await InvokeOnMainThreadAsync(() =>
			{
				var view = new UIView();
				var recognizer = new ResignFirstResponderTouchGestureRecognizer(view);
				view.AddGestureRecognizer(recognizer);
				viewReference = new(view);
				recognizerReference = new(recognizer);
			});

			await AssertionExtensions.WaitForGC(viewReference, recognizerReference);
			Assert.False(viewReference.IsAlive, "UIView should not be alive!");
			Assert.False(recognizerReference.IsAlive, "ResignFirstResponderTouchGestureRecognizer should not be alive!");
		}
	}
}

#endif
