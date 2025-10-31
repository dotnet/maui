#if IOS || MACCATALYST
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using ObjCRuntime;

namespace Microsoft.Maui.Platform;

internal static class CALayerAutoresizeToSuperLayer
{
	static IntPtr _originalSetBoundsSelector;
	static IntPtr _subLayersSelector;
	static IntPtr _countSelector;
	static IntPtr _boundsSelector;
	static IntPtr _objectAtIndexSelector;
	static IntPtr _setFrameSelector;
	static readonly IntPtr _associatedObjectKey;
	static bool _initialized;
	static bool _useStret;

	static CALayerAutoresizeToSuperLayer()
	{
		// Create unique key for associated objects
		_associatedObjectKey = Marshal.AllocHGlobal(1);
		
		// Determine which calling convention to use based on architecture
		// ARM64 uses objc_msgSend for struct returns, x86_64 uses objc_msgSend_stret
		_useStret = RuntimeInformation.ProcessArchitecture == Architecture.X64;
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
			
			// Cache all selectors used in the hot path for fast access
			_originalSetBoundsSelector = Selector.GetHandle("maui_setBounds:");
			_subLayersSelector = Selector.GetHandle("sublayers");
			_countSelector = Selector.GetHandle("count");
			_boundsSelector = Selector.GetHandle("bounds");
			_objectAtIndexSelector = Selector.GetHandle("objectAtIndex:");
			_setFrameSelector = Selector.GetHandle("setFrame:");
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
		// Fast check: do we have sublayers?
		var sublayersArrayPtr = objc_msgSend(self, _subLayersSelector);
		
		if (sublayersArrayPtr != IntPtr.Zero)
		{
			var sublayerCount = objc_msgSend_IntPtr(sublayersArrayPtr, _countSelector);

			if (sublayerCount > 0)
			{
				var currentBounds = GetBounds(self);
				
				// Only proceed if bounds actually changed
				if (!currentBounds.Equals(bounds))
				{
					// Loop through sublayers once at native level
					for (nint i = 0; i < sublayerCount; i++)
					{
						var sublayerPtr = objc_msgSend_nint(sublayersArrayPtr, _objectAtIndexSelector, i);
						var flagPtr = objc_getAssociatedObject(sublayerPtr, _associatedObjectKey);
						
						// If this sublayer has autoresizing enabled, set its frame
						if (flagPtr != IntPtr.Zero)
						{
							objc_msgSend_CGRect(sublayerPtr, _setFrameSelector, bounds);
						}
					}
				}
			}
		}

		// Call the original setBounds: implementation
		objc_msgSend_CGRect(self, _originalSetBoundsSelector, bounds);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static CGRect GetBounds(IntPtr layerPtr)
	{
		// Use the appropriate calling convention based on architecture
		return _useStret 
			? objc_msgSend_stret_CGRect(layerPtr, _boundsSelector)
			: objc_msgSend_CGRect_ret(layerPtr, _boundsSelector);
	}

	internal static bool GetAutoresizeToSuperLayer(CALayer layer)
	{
		var valuePointer = objc_getAssociatedObject(layer.Handle, _associatedObjectKey);
		if (valuePointer == IntPtr.Zero)
		{
			return false;
		}

		var nsNumber = Runtime.GetNSObject<NSNumber>(valuePointer);
		return (nsNumber?.UInt32Value ?? 0) != 0;
	}

	internal static void SetAutoresizeToSuperLayer(CALayer layer, bool autoresize)
	{
		if (autoresize)
		{
			var maskNumber = NSNumber.FromUInt32(1);
			objc_setAssociatedObject(layer.Handle, _associatedObjectKey, maskNumber.Handle, AssociationPolicy.RETAIN_NONATOMIC);
		}
		else
		{
			// Remove the associated object by passing nil/IntPtr.Zero
			objc_setAssociatedObject(layer.Handle, _associatedObjectKey, IntPtr.Zero, AssociationPolicy.ASSIGN);
		}
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
	
	[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
	private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);
	
	[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
	private static extern IntPtr objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector);
	
	[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
	private static extern IntPtr objc_msgSend_nint(IntPtr receiver, IntPtr selector, nint arg1);
	
	// On ARM64 (iOS, Apple Silicon Macs), structs are returned via registers
	[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
	private static extern CGRect objc_msgSend_CGRect_ret(IntPtr receiver, IntPtr selector);
	
	// On x86_64 (Intel Macs), structs are returned via stret
	[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend_stret")]
	private static extern CGRect objc_msgSend_stret_CGRect(IntPtr receiver, IntPtr selector);

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

