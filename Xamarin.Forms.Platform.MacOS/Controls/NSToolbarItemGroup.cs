using System;
using System.Runtime.InteropServices;
using AppKit;
using Foundation;
using ObjCRuntime;

[Register("NSToolbarItemGroup", true)]
// ReSharper disable once CheckNamespace
// ReSharper disable once InconsistentNaming
public class NSToolbarItemGroup : NSToolbarItem
{
	const string SelSetSubitems = "setSubitems:";
	const string SelSubitems = "subitems";
	const string SelInitWithItemIdentifier = "initWithItemIdentifier:";
	static readonly IntPtr s_selSetSubitemsHandle = Selector.GetHandle(SelSetSubitems);
	static readonly IntPtr s_selSubitemsHandle = Selector.GetHandle(SelSubitems);
	static readonly IntPtr s_selInitWithItemIdentifierHandle = Selector.GetHandle(SelInitWithItemIdentifier);
	static readonly IntPtr s_classPtr = Class.GetHandle("NSToolbarItemGroup");

	[Export("init")]
	public NSToolbarItemGroup() : base(NSObjectFlag.Empty)
	{
		InitializeHandle(
			IsDirectBinding
				? IntPtr_objc_msgSend(Handle, Selector.GetHandle("init"))
				: IntPtr_objc_msgSendSuper(SuperHandle, Selector.GetHandle("init")), "init");
	}

	[Export("initWithItemIdentifier:")]
	public NSToolbarItemGroup(string itemIdentifier)
		: base(NSObjectFlag.Empty)
	{
		NSApplication.EnsureUIThread();
		if (itemIdentifier == null)
			throw new ArgumentNullException(nameof(itemIdentifier));
		IntPtr nsitemIdentifier = NSString.CreateNative(itemIdentifier);

		InitializeHandle(
			IsDirectBinding
				? IntPtr_objc_msgSend_IntPtr(Handle, s_selInitWithItemIdentifierHandle, nsitemIdentifier)
				: IntPtr_objc_msgSendSuper_IntPtr(SuperHandle, s_selInitWithItemIdentifierHandle, nsitemIdentifier),
			"initWithItemIdentifier:");
		NSString.ReleaseNative(nsitemIdentifier);
	}

	protected internal NSToolbarItemGroup(IntPtr handle) : base(handle)
	{
	}

	protected NSToolbarItemGroup(NSObjectFlag t) : base(t)
	{
	}

	public override IntPtr ClassHandle => s_classPtr;

	public virtual NSToolbarItem[] Subitems
	{
		[Export(SelSubitems, ArgumentSemantic.Copy)]
		get
		{
			NSApplication.EnsureUIThread();
			NSToolbarItem[] ret =
				NSArray.ArrayFromHandle<NSToolbarItem>(IsDirectBinding
					? IntPtr_objc_msgSend(Handle, s_selSubitemsHandle)
					: IntPtr_objc_msgSendSuper(SuperHandle, s_selSubitemsHandle));
			return ret;
		}

		[Export(SelSetSubitems, ArgumentSemantic.Copy)]
		set
		{
			NSApplication.EnsureUIThread();
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			// ReSharper disable once CoVariantArrayConversion
			NSArray nsaValue = NSArray.FromNSObjects(value);

			if (IsDirectBinding)
				void_objc_msgSend_IntPtr(Handle, s_selSetSubitemsHandle, nsaValue.Handle);
			else void_objc_msgSendSuper_IntPtr(SuperHandle, s_selSetSubitemsHandle, nsaValue.Handle);
			nsaValue.Dispose();
		}
	}

	[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
	public static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector);

	[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
	public static extern IntPtr IntPtr_objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

	[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSendSuper")]
	public static extern IntPtr IntPtr_objc_msgSendSuper(IntPtr receiver, IntPtr selector);

	[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSendSuper")]
	public static extern IntPtr IntPtr_objc_msgSendSuper_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

	[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
	public static extern void void_objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

	[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSendSuper")]
	public static extern void void_objc_msgSendSuper_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);
}