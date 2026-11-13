using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.View)]
	public class NativeViewPeerTests : TestBase
	{
		[Fact]
		public Task SuperviewReconstructsWrapperViewPeer() =>
			InvokeOnMainThreadAsync(AssertSuperviewReconstructsPeer<WrapperView>);

		[Fact]
		public Task SuperviewReconstructsContentViewPeer() =>
			InvokeOnMainThreadAsync(AssertSuperviewReconstructsPeer<ContentView>);

		[Fact]
		public Task SuperviewReconstructsLayoutViewPeer() =>
			InvokeOnMainThreadAsync(AssertSuperviewReconstructsPeer<LayoutView>);

		static void AssertSuperviewReconstructsPeer<T>() where T : UIView
		{
			using var nativeOwner = new UIView();
			using var child = new UIView();

			// Native initialization bypasses managed constructors, giving us a retained
			// UIView with no managed peer without depending on GC or toggle-reference timing.
			var allocated = Send(Class.GetHandle(typeof(T)), Selector.GetHandle("alloc"));
			var parentHandle = InitWithFrame(allocated, Selector.GetHandle("initWithFrame:"), CGRect.Empty);
			Assert.NotEqual(IntPtr.Zero, parentHandle);
			try
			{
				Assert.Null(Runtime.TryGetNSObject(parentHandle));
				Send((IntPtr)nativeOwner.Handle, Selector.GetHandle("addSubview:"), parentHandle);
				Send(parentHandle, Selector.GetHandle("addSubview:"), (IntPtr)child.Handle);

				using var reconstructedParent = child.Superview;
				Assert.IsType<T>(reconstructedParent);
				Assert.Equal(parentHandle, (IntPtr)reconstructedParent.Handle);
				Assert.Same(reconstructedParent, child.Superview);
			}
			finally
			{
				Send(parentHandle, Selector.GetHandle("release"));
			}
		}

		[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
		static extern IntPtr Send(IntPtr receiver, IntPtr selector);

		[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
		static extern void Send(IntPtr receiver, IntPtr selector, IntPtr argument);

		[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
		static extern IntPtr InitWithFrame(IntPtr receiver, IntPtr selector, CGRect frame);
	}
}
