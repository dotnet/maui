#if IOS || MACCATALYST
using System;
using System.Runtime.InteropServices;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using ObjCRuntime;

namespace Microsoft.Maui.Platform;

internal static class CALayerAutoresizeToSuperLayer
{
	static IntPtr _originalSetBoundsSelector;
	static readonly IntPtr _associatedObjectKey;
	static bool _initialized;

	static CALayerAutoresizeToSuperLayer()
	{
		// Create a unique key for associated objects
		_associatedObjectKey = Marshal.AllocHGlobal(1);
	}

	internal static void EnsureInitialized()
	{
		if (_initialized)
		{
			return;
		}

		_initialized = true;

		// Get CALayer class handle
		var caLayerClass = Class.GetHandle(typeof(CALayer));
			
		// Get the original setBounds: method
		var setBoundsSelector = Selector.GetHandle("setBounds:");
		var originalSetBoundsMethod = class_getInstanceMethod(caLayerClass, setBoundsSelector);
			
		if (originalSetBoundsMethod == IntPtr.Zero)
		{
			return;
		}

		// Create our swizzled method implementation
		var swizzledSetBoundsImp = GetSwizzledSetBoundsImplementation();
			
		// Add our custom method to CALayer class
		var mauiSetBoundsSelector = Selector.GetHandle("maui_setBounds:");
			
		// Try to add the method first
		if (class_addMethod(caLayerClass, mauiSetBoundsSelector, swizzledSetBoundsImp, "v@:{CGRect={CGPoint=dd}{CGSize=dd}}"))
		{
			// Get the newly added method
			var swizzledMethod = class_getInstanceMethod(caLayerClass, mauiSetBoundsSelector);
				
			// Exchange implementations
			method_exchangeImplementations(originalSetBoundsMethod, swizzledMethod);
			
			// After swizzling, the original implementation is now at maui_setBounds:
			_originalSetBoundsSelector = Selector.GetHandle("maui_setBounds:");
		}
	}

	private static IntPtr GetSwizzledSetBoundsImplementation()
	{
		return Marshal.GetFunctionPointerForDelegate(new SetBoundsDelegate(Maui_SetBounds));
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void SetBoundsDelegate(IntPtr self, IntPtr selector, CGRect bounds);

	[MonoPInvokeCallback(typeof(SetBoundsDelegate))]
	private static void Maui_SetBounds(IntPtr self, IntPtr selector, CGRect bounds)
	{
		// Get the CALayer instance
		var layer = Runtime.GetNSObject<CALayer>(self);
			
		if (layer == null)
		{
			return;
		}

		// Only if size has changed
		var oldBounds = layer.Bounds;
		if (!oldBounds.Equals(bounds))
		{
			var sublayers = layer.Sublayers;
			if (sublayers != null)
			{
				foreach (var sublayer in sublayers)
				{
					if (GetAutoresizeToSuperLayer(sublayer))
					{
						sublayer.Frame = bounds;
					}
				}
			}
		}

		// Call the original setBounds: implementation
		objc_msgSend_CGRect(self, _originalSetBoundsSelector, bounds);
	}

	internal static bool GetAutoresizeToSuperLayer(CALayer layer)
	{
		var maskNumber = objc_getAssociatedObject(layer.Handle, _associatedObjectKey);
		if (maskNumber == IntPtr.Zero)
		{
			return false;
		}

		var nsNumber = Runtime.GetNSObject<NSNumber>(maskNumber);
		return (nsNumber?.UInt32Value ?? 0) != 0;
	}

	internal static void SetAutoresizeToSuperLayer(CALayer layer, bool autoresize)
	{
		var maskNumber = NSNumber.FromUInt32(autoresize ? 1u : 0u);
		objc_setAssociatedObject(layer.Handle, _associatedObjectKey, maskNumber.Handle, AssociationPolicy.RETAIN_NONATOMIC);
	}

	// Objective-C runtime P/Invoke declarations
	[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "class_getInstanceMethod")]
	private static extern IntPtr class_getInstanceMethod(IntPtr cls, IntPtr selector);

	[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "method_exchangeImplementations")]
	private static extern void method_exchangeImplementations(IntPtr method1, IntPtr method2);

	[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "class_addMethod")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool class_addMethod(IntPtr cls, IntPtr name, IntPtr imp, string types);

	[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_getAssociatedObject")]
	private static extern IntPtr objc_getAssociatedObject(IntPtr obj, IntPtr key);

	[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_setAssociatedObject")]
	private static extern void objc_setAssociatedObject(IntPtr obj, IntPtr key, IntPtr value, AssociationPolicy policy);

	[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
	private static extern void objc_msgSend_CGRect(IntPtr receiver, IntPtr selector, CGRect arg1);

	private enum AssociationPolicy
	{
		ASSIGN = 0,
		RETAIN_NONATOMIC = 1,
		COPY_NONATOMIC = 3,
		RETAIN = 01401,
		COPY = 01403
	}
}

// Extension methods to provide easy access to the autoresizing mask property
internal static class CALayerAutoresizeToSuperlayerExtensions
{
	internal static bool GetAutoresizeToSuperLayer(this CALayer layer)
	{
		return CALayerAutoresizeToSuperLayer.GetAutoresizeToSuperLayer(layer);
	}

	internal static void SetAutoresizeToSuperLayer(this CALayer layer, bool autoresize)
	{
		CALayerAutoresizeToSuperLayer.SetAutoresizeToSuperLayer(layer, autoresize);
	}
}
#endif

