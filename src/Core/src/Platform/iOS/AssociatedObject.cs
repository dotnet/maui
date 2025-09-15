using System;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;

namespace Microsoft.Maui.Platform;

// https://github.com/dotnet/macios/blob/0a8490c37222336ff5184a6bbeaa591706fb1c9f/tests/monotouch-test/ObjCRuntime/RuntimeTest.cs#L23-L29
internal enum AssociationPolicy { // uintptr_t
	Assign = 0,
	RetainNonAtomic = 1,
	CopyNonAtomic = 3,
	Retain = 0x301,
	Copy = 0x303,
}

internal static class AssociatedObject
{
	[DllImport(Constants.ObjectiveCLibrary)]
	static extern void objc_setAssociatedObject(IntPtr o, IntPtr key, IntPtr value, AssociationPolicy policy);

	[DllImport(Constants.ObjectiveCLibrary)]
	static extern IntPtr objc_getAssociatedObject(IntPtr o, IntPtr key);

	public static void Set(NSObject o, NSString key, NSObject value, AssociationPolicy policy = AssociationPolicy.RetainNonAtomic)
	{
		objc_setAssociatedObject(o.Handle, key.Handle, value.Handle, policy);
	}

	public static NSObject? Get(NSObject o, NSString key)
	{
		var obj = objc_getAssociatedObject(o.Handle, key.Handle);
		return Runtime.GetNSObject(obj);
	}
}