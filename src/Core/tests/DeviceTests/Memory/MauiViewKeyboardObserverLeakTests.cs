#if IOS || MACCATALYST

using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests.Memory
{
	// Regression test for https://github.com/dotnet/maui/issues/35386
	//
	// Root cause: UpdateKeyboardSubscription() guarded all logic with `if (Window != null)`.
	// When MovedToWindow fired with Window == null (view detached), NSNotificationCenter
	// observers for UIKeyboard.WillShow/WillHide notifications were never removed.
	// This created a native retain chain that kept the MauiView alive indefinitely:
	//   NSNotificationCenter → observer token → callback delegate → MauiView
	//
	// Fix: the else branch of UpdateKeyboardSubscription now always calls
	// UnsubscribeFromKeyboardNotifications(), including when Window == null.
	//
	// Test strategy: behavioral verification via reflection.
	// We check that _keyboardWillShowObserver is non-null after attachment
	// (proving subscription fired) and null after detachment (proving
	// unsubscription fired). This is more reliable than GC-based tests because
	// the native retain chain operates at the UIKit level, not the managed heap.
	[Category(TestCategory.Memory)]
	public class MauiViewKeyboardObserverLeakTests : TestBase
	{
		static readonly FieldInfo s_showObserverField =
			typeof(MauiView).GetField("_keyboardWillShowObserver", BindingFlags.NonPublic | BindingFlags.Instance)
			?? throw new InvalidOperationException("Could not find _keyboardWillShowObserver field on MauiView.");

		/// <summary>
		/// A layout stub that implements ISafeAreaView2 with SoftInput on all edges,
		/// which causes MauiView to subscribe to UIKeyboard notifications.
		/// </summary>
		class SoftInputLayoutStub : LayoutStub, ISafeAreaView2
		{
			Thickness ISafeAreaView2.SafeAreaInsets { set { } }

			SafeAreaRegions ISafeAreaView2.GetSafeAreaRegionsForEdge(int edge)
				=> SafeAreaRegions.SoftInput;
		}

		/// <summary>
		/// Regression test for #35386: verifies that keyboard observers are removed
		/// when a MauiView with SafeAreaEdges.SoftInput is detached from the window.
		/// Without the fix, _keyboardWillShowObserver remains non-null after detach
		/// because UpdateKeyboardSubscription's `if (Window != null)` guard blocks
		/// UnsubscribeFromKeyboardNotifications from running.
		/// </summary>
		[Fact]
		public async Task MauiView_WithSoftInputSafeAreaEdges_UnsubscribesObserversOnDetach()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var stub = new SoftInputLayoutStub();
				var view = new LayoutView();
				view.View = stub;
				view.CrossPlatformLayout = stub;

				// Before attach: no subscription yet.
				Assert.Null(s_showObserverField.GetValue(view));

				await view.AttachAndRun(() =>
				{
					// While attached to a live window: observer must be registered.
					Assert.NotNull(s_showObserverField.GetValue(view));
				});

				// After AttachAndRun calls RemoveFromSuperview (MovedToWindow(null)):
				// fix ensures UnsubscribeFromKeyboardNotifications runs → field is null.
				Assert.Null(s_showObserverField.GetValue(view));
			});
		}

		/// <summary>
		/// Control case: a plain layout (no SoftInput edges) must never subscribe
		/// and must remain uncollected after detach.
		/// </summary>
		[Fact]
		public async Task MauiView_WithoutSoftInputSafeAreaEdges_NeverSubscribes()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var stub = new LayoutStub();
				var view = new LayoutView();
				view.CrossPlatformLayout = stub;

				// No SoftInput → observer should never be registered.
				await view.AttachAndRun(() =>
				{
					Assert.Null(s_showObserverField.GetValue(view));
				});

				Assert.Null(s_showObserverField.GetValue(view));
			});
		}
	}
}

#endif
