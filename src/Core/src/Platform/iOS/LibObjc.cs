using System;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;

namespace Microsoft.Maui.Platform.iOS
{
	// Internal Objective-C Selectors
	internal class LibObjc
	{
		const string libobjc = "/usr/lib/libobjc.dylib";
		const string objc_msgSend = "objc_msgSend";
		const string objc_msgSendSuper = "objc_msgSendSuper";

		[DllImport(libobjc)]
		extern public static IntPtr class_getClassMethod(IntPtr classHandle, IntPtr Selector);

		[DllImport(libobjc)]
		extern public static void method_setImplementation(IntPtr method, IntPtr imp);

		[DllImport(libobjc)]
		extern public static IntPtr method_getImplementation(IntPtr method);

		[DllImport(libobjc)]
		extern public static IntPtr class_getInstanceMethod(IntPtr classHandle, IntPtr Selector);
	}
}

